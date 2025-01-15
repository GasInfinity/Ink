using Ink.SourceGenerator.Util;
using System.Collections.Immutable;
using System.Diagnostics;

namespace Ink.SourceGenerator.Packet;

public sealed class ConditionalFieldType(IPacketFieldType Type, string FieldCondition, ConditionalFieldType.ConditionKind Condition, ImmutableHashSet<string> ConditionValues) : IPacketFieldType
{
    public enum ConditionKind
    {
        Equals,
        NotEquals,
        Bitfield,
    }

    public readonly IPacketFieldType Type = Type;
    public readonly string FieldCondition = FieldCondition;
    public readonly ConditionKind Condition = Condition;
    public readonly ImmutableHashSet<string> ConditionValues = ConditionValues;

    public void AppendTypename(IndentingStringBuilder writer)
    { 
        Type.AppendTypename(writer);
        writer.Write("?");
    }

    public void AppendWriting(IndentingStringBuilder writer, string fieldName)
    {
        writer.Write("if(");
        int i = 0;
        foreach(string value in ConditionValues)
        {
            switch(Condition)
            {
                case ConditionKind.Equals:
                    {
                        writer.Write($"{FieldCondition} == {value}"); 
                        break;
                    }
                case ConditionKind.NotEquals:
                    {
                        writer.Write($"{FieldCondition} != {value}"); 
                        break;
                    }
                case ConditionKind.Bitfield:
                    {
                        writer.Write($"({FieldCondition} & {value}) == {value}"); 
                        break;
                    }
            }

            if(++i == ConditionValues.Count - 1)
                writer.Write(" || ");
        }
        writer.WriteLine(")");
        using(writer.EnterBlock())
        {
            Type.AppendTypename(writer);
            writer.WriteLine($" value{fieldName} = {fieldName}!.Value;");
            Type.AppendWriting(writer, $"value{fieldName}");
        }
    }

    public void AppendReading(IndentingStringBuilder writer, string fieldName)
    {
        writer.Write("if(");
        int i = 0;
        foreach(string value in ConditionValues)
        {
            switch(Condition)
            {
                case ConditionKind.Equals:
                    {
                        writer.Write($"{FieldCondition} == {value}"); 
                        break;
                    }
                case ConditionKind.NotEquals:
                    {
                        writer.Write($"{FieldCondition} != {value}"); 
                        break;
                    }
                case ConditionKind.Bitfield:
                    {
                        writer.Write($"({FieldCondition} & {value}) == {value}"); 
                        break;
                    }
            }

            if(++i == ConditionValues.Count - 1)
                writer.Write(" || ");
        }
        writer.WriteLine(")");

        using (writer.EnterBlock())
            Type.AppendReading(writer, fieldName);

        writer.WriteLine("else");

        using(writer.EnterBlock())
            writer.WriteLine($"{fieldName} = null;");
    }

    public static ConditionalFieldType Parse(IPacketFieldType elementType, ReadOnlySpan<char> typeDescription)
    {
        int i = 1;

        while(char.IsLetterOrDigit(typeDescription[i]))
            ++i;

        string variable = typeDescription.Slice(1, i - 1).ToString();
        ConditionKind kind;
        ImmutableHashSet<string> conditionValues;

        if(typeDescription[i..].Trim()[0] == '}')
        {
            kind = ConditionKind.Equals;
            conditionValues = ["true"];
        }
        else
        {
            typeDescription = typeDescription.Slice(i).Trim();
            kind = typeDescription[0] switch
            {
                '=' => ConditionKind.Equals,
                '!' => ConditionKind.NotEquals,
                '|' => ConditionKind.Bitfield,
                _ => throw new UnreachableException($"Found {typeDescription[0]}")
            };

            ReadOnlySpan<char> values = typeDescription[1..^1].Trim();
            conditionValues = values.ToString().Split("/").ToImmutableHashSet(); // TODO: Lazy af, we can not allocate three times if we want....
        }

        return new (elementType, variable, kind, conditionValues); // FIXME
    }
}
