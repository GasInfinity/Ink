using System.CodeDom.Compiler;
using System.Collections.Immutable;

namespace Ink.SourceGenerator.Block;

public sealed class SharedPropertyValueStore
{
    const string ClassName = nameof(SharedPropertyValueStore);
    const string EmptyPropertyValuesVariable = $"[]";
    const int MaxClassVariables = 512;

    private readonly List<ImmutableArray<int>> knownPropertyValues = [];
    private readonly List<string> sharedPropertyValuesVariables = [];
    private int propertyValuesVariableCount = 0;

    public string GetOrAddPropertyValues(ImmutableDictionary<string, string> propertyValues, ImmutableDictionary<string, ImmutableArray<string>> properties)
    {
        if (propertyValues == null || propertyValues.IsEmpty)
            return EmptyPropertyValuesVariable;

        int propertyValuesIndex = IndexOfPropertyValues(propertyValues, properties);
        string valuesVariableName = string.Empty;
        int classIndex = 0;

        if(propertyValuesIndex != -1)
        {
            valuesVariableName = sharedPropertyValuesVariables[propertyValuesIndex];
            classIndex = propertyValuesIndex / MaxClassVariables;
        }
        else
        {
            knownPropertyValues.Add(propertyValues.Select(kv => properties.TryGetValue(kv.Key, out ImmutableArray<string> possibleValues) ? GetIntegerValue(kv.Value, possibleValues) : 0).ToImmutableArray());
            sharedPropertyValuesVariables.Add(valuesVariableName = GetPropertyIndexVariableName(this.propertyValuesVariableCount));
            classIndex = this.propertyValuesVariableCount++ / MaxClassVariables;
        }

        return $"{ClassName}{classIndex}.{valuesVariableName}";
    }

    public void WriteTo(IndentedTextWriter writer)
    {
        int classCount = (this.propertyValuesVariableCount / MaxClassVariables) + 1;
        for(int c = 0; c < classCount; ++c)
        {
            writer.WrtLine($"private static class {ClassName}{c}")
                  .WrtLine('{')
                  .Ind();

            int start = c * MaxClassVariables;
            int max = int.Min(this.propertyValuesVariableCount - start, MaxClassVariables);
            for(int i = 0; i < max; ++i)
            {
                int index = start + i;

                string arrayName = this.sharedPropertyValuesVariables[index];
                string rawArrayName = $"Raw{arrayName}";

                writer.Write($"private static readonly Property[] {rawArrayName} = [");

                ImmutableArray<int> propertyValues = this.knownPropertyValues[index];
                for (int j = 0; j < propertyValues.Length; ++j)
                {
                    writer.Write($"new Property({propertyValues[j]})");

                    if (j + 1 < propertyValues.Length)
                        writer.Write(", ");
                }
                writer.WriteLine("];");
                writer.WriteLine($"public static ImmutableArray<Property> {arrayName} => ImmutableCollectionsMarshal.AsImmutableArray({rawArrayName});");
            }

            writer.Unind()
                  .WrtLine('}');
        }
    }

    private int IndexOfPropertyValues(ImmutableDictionary<string, string> propertyValues, ImmutableDictionary<string, ImmutableArray<string>> properties) // FIXME: Optimize this?
    {
        for(int i = 0; i < this.knownPropertyValues.Count; ++i)
        {
            if (this.knownPropertyValues[i].SequenceEqual(propertyValues.Select(kv => properties.TryGetValue(kv.Key, out ImmutableArray<string> possibleValues) ? GetIntegerValue(kv.Value, possibleValues) : 0)))
                return i;
        }

        return -1;
    }

    private static int GetIntegerValue(string propertyValue, ImmutableArray<string> possibleValues)
    {
        if (int.TryParse(propertyValue, out int value))
            return value;

        return propertyValue switch
        {
            "true" => 1,
            "false" => 0,
            _ => possibleValues.IndexOf(propertyValue)
        };
    }

    private static string GetPropertyIndexVariableName(int index)
        => $"SharedPropertyValues{index}";
}
