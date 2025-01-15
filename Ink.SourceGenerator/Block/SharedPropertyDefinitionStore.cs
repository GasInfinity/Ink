using System.CodeDom.Compiler;
using System.Collections.Immutable;
using Ink.SourceGenerator.Util;

namespace Ink.SourceGenerator.Block;

public sealed class SharedPropertyDefinitionStore
{
    const string ClassName = nameof(SharedPropertyDefinitionStore);
    const string EmptyPropertyIndexVariable = "FrozenDictionary<string, int>.Empty";
    const string EmptyPropertyDefinitionsVariable = $"[]";

    private readonly List<ImmutableArray<string>> knownPropertyNames = [];
    private readonly List<string> sharedPropertyIndexVariables = [];
    private int propertyIndexVariableCount = 0;

    private readonly List<ImmutableArray<(PropertyKind, int, int)>> knownPropertyDefinitions = [];
    private readonly List<string> sharedPropertyDefinitionsVariables = [];
    private int propertyDefinitionsVariableCount = 0;

    public (string IndexVariableName, string DefinitionsVariableName) GetOrAddPropertyDefinitions(ImmutableDictionary<string, ImmutableArray<string>> properties)
    {
        if (properties == null || properties.IsEmpty)
            return (EmptyPropertyIndexVariable, EmptyPropertyDefinitionsVariable);

        string indexVariableName = string.Empty;
        IEnumerable<string> propertyNames = properties.Keys;
        int namesIndex = IndexOfPropertyNames(propertyNames);
        
        if (namesIndex != -1)
        {
            indexVariableName = sharedPropertyIndexVariables[namesIndex];
        }
        else
        {
            knownPropertyNames.Add([.. propertyNames]);
            sharedPropertyIndexVariables.Add(indexVariableName = GetPropertyIndexVariableName(propertyIndexVariableCount));
            ++propertyIndexVariableCount;
        }

        string definitionsVariableName = string.Empty;
        IEnumerable<ImmutableArray<string>> definitionValues = properties.Values;
        int definitionsIndex = IndexOfPropertyDefinitions(definitionValues);

        if(definitionsIndex != -1)
        {
            definitionsVariableName = sharedPropertyDefinitionsVariables[definitionsIndex];
        }
        else
        {
            knownPropertyDefinitions.Add(definitionValues.Select(static t => ParsePropertyDefinition(t)).ToImmutableArray());
            sharedPropertyDefinitionsVariables.Add(definitionsVariableName = GetPropertyDefinitionsVariableName(propertyDefinitionsVariableCount));
            ++propertyDefinitionsVariableCount;
        }

        return ($"{ClassName}.{indexVariableName}", $"{ClassName}.{definitionsVariableName}");
    }

    public void WriteTo(IndentedTextWriter writer)
    {
        writer.WrtLine($"private static class {ClassName}")
              .WrtLine('{')
              .Ind();

        for (int i = 0; i < propertyIndexVariableCount; ++i)
        {
            writer.Write($"public static readonly FrozenDictionary<string, int> {sharedPropertyIndexVariables[i]} = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase) {{ ");

            ImmutableArray<string> propertyNames = knownPropertyNames[i];
            for (int j = 0; j < propertyNames.Length; ++j)
            {
                writer.Write($"{{ \"{propertyNames[j]}\", {j} }}");

                if (j + 1 < propertyNames.Length)
                    writer.Write(", ");
            }

            writer.WriteLine(" }.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);");
        }

        for (int i = 0; i < propertyDefinitionsVariableCount; ++i)
        {
            string arrayName = this.sharedPropertyDefinitionsVariables[i];
            string rawArrayName = $"Raw{arrayName}";

            writer.Write($"private static readonly PropertyDefinition[] {rawArrayName} = [");
            ImmutableArray<(PropertyKind, int, int)> propertyDefinitions = knownPropertyDefinitions[i];
            for (int j = 0; j < propertyDefinitions.Length; ++j)
            {
                (PropertyKind kind, int offset, int max) = propertyDefinitions[j];
                writer.Write($"new PropertyDefinition({nameof(PropertyKind)}.{kind}, {offset}, {max})");

                if (j + 1 < propertyDefinitions.Length)
                    writer.Write(", ");
            }
            writer.WriteLine("];");
            writer.WriteLine($"public static ImmutableArray<PropertyDefinition> {arrayName} => ImmutableCollectionsMarshal.AsImmutableArray({rawArrayName});");
        }

        writer.Unind()
              .WrtLine('}');
    }

    private int IndexOfPropertyNames(IEnumerable<string> propertyNames)
    {
        for (int i = 0; i < knownPropertyNames.Count; ++i)
        {
            if (knownPropertyNames[i].SequenceEqual(propertyNames))
                return i;
        }

        return -1;
    }

    private int IndexOfPropertyDefinitions(IEnumerable<ImmutableArray<string>> propertyDefinitions)
    {
        for (int i = 0; i < knownPropertyDefinitions.Count; ++i)
        {
            if (knownPropertyDefinitions[i].SequenceEqual(propertyDefinitions.Select(static t => ParsePropertyDefinition(t))))
                return i;
        }

        return -1;
    }

    private static (PropertyKind, int, int) ParsePropertyDefinition(ImmutableArray<string> propertyValues)
    {
        if (int.TryParse(propertyValues[0], out _))
        {
            int min = propertyValues.Min(static v => int.Parse(v));
            return (PropertyKind.Integer, min, propertyValues.Length);
        }

        if (propertyValues.Length == 2 && (propertyValues[0] == "true" || propertyValues[0] == "false"))
            return (PropertyKind.Boolean, 0, 1);

        return (PropertyKind.Integer, 0, propertyValues.Length);
    }

    private static string GetPropertyIndexVariableName(int index)
        => $"SharedPropertyKeyIndex{index}";

    private static string GetPropertyDefinitionsVariableName(int index)
        => $"SharedPropertyDefinitions{index}";
}
