using Ink.SourceGenerator.Util;

namespace Ink.SourceGenerator.Packet;

public sealed class ArrayFieldType(IPacketFieldType ElementType, ArrayFieldType.Kind ArrayKind) : IPacketFieldType
{
    public enum Kind
    {
        Unbounded = -1,
        LengthPrefixed,
    }

    public readonly IPacketFieldType ElementType = ElementType;
    public readonly Kind ArrayKind = ArrayKind;

    public void AppendTypename(IndentingStringBuilder writer)
    {
        ElementType.AppendTypename(writer);
        writer.Write("[]");
    }

    public void AppendWriting(IndentingStringBuilder writer, string fieldName)
    {
        if(ArrayKind == Kind.LengthPrefixed)
        {
            writer.WriteLine($"writer.WriteVarInteger({fieldName}.Length);");
        }

        if(ElementType is BinaryFieldType b && b.BinaryKind is BinaryFieldType.Kind.UInt8 or BinaryFieldType.Kind.Int8 or BinaryFieldType.Kind.Bool)
        {
            switch(b.BinaryKind)
            {
                case BinaryFieldType.Kind.UInt8:
                    {
                        writer.WriteLine($"writer.Write({fieldName});");
                        break;
                    }
                case BinaryFieldType.Kind.Int8:
                case BinaryFieldType.Kind.Bool:
                    {
                        writer.WriteLine($"writer.Write(MemoryMarshal.Cast<{b.BinaryKind}, UInt8>({fieldName}));");
                        break;
                    }
            }
        }
        else
        {
            writer.WriteLine($"for(int i = 0; i < {fieldName}.Length; ++i)");
            using(writer.EnterBlock())
            {
                ElementType.AppendTypename(writer);
                writer.WriteLine($" {fieldName}Element = {fieldName}[i];");
                ElementType.AppendWriting(writer, $"{fieldName}Element");
            }
        }
    }

    public void AppendReading(IndentingStringBuilder writer, string fieldName)
    {
        using(writer.EnterBlock())
        {
            if(ArrayKind == Kind.LengthPrefixed)
            {
                writer.Write(true, $$"""
                        if(VarInteger<uint>.TryDecode(payload, out int bytesRead{{fieldName}}, out uint length{{fieldName}}) != OperationStatus.Done)
                        {
                            result = default;
                            return false;
                        }
                        
                        payload = payload[bytesRead{{fieldName}}..];
                        {{fieldName}} = GC.AllocateUninitializedArray<
                        """);
                ElementType.AppendTypename(writer);
                writer.WriteLine($">((int)length{fieldName});");
            }
            else
            {
                writer.Write($"{fieldName} = GC.AllocateUninitializedArray<");
                ElementType.AppendTypename(writer);
                writer.WriteLine($">(payload.Length);");
            }

            if(ElementType is BinaryFieldType b && b.BinaryKind is BinaryFieldType.Kind.UInt8 or BinaryFieldType.Kind.Int8 or BinaryFieldType.Kind.Bool)
            {
                if(ArrayKind == Kind.LengthPrefixed)
                {
                    writer.WriteLine(true, $$"""
                            if(payload.Length < {{fieldName}}.Length)
                            {
                                result = default;
                                return false;
                            }
                            """);
                }

                switch(b.BinaryKind)
                {
                    case BinaryFieldType.Kind.UInt8:
                        {
                            writer.WriteLine($"_ = payload[..{fieldName}.Length].TryCopyTo({fieldName});");
                            break;
                        }
                    case BinaryFieldType.Kind.Int8:
                    case BinaryFieldType.Kind.Bool:
                        {
                            writer.WriteLine($"_ = payload[..{fieldName}.Length].TryCopyTo(MemoryMarshal.Cast<{b.BinaryKind}, UInt8>({fieldName}));");
                            break;
                        }
                }

                if(ArrayKind == Kind.Unbounded)
                {
                    writer.WriteLine("payload = default;"); 
                }
                else
                {
                    writer.WriteLine($"payload = payload[{fieldName}.Length..];");
                }
            }
            else
            {
                writer.WriteLine($"for(int i = 0; i < {fieldName}.Length; ++i)");
                using (writer.EnterBlock())
                {
                    ElementType.AppendTypename(writer);
                    writer.WriteLine($" {fieldName}Element;");
                    ElementType.AppendReading(writer, $"{fieldName}Element");
                    writer.WriteLine($"{fieldName}[i] = {fieldName}Element;");
                }
            }
        }
    }

    public static ArrayFieldType Parse(IPacketFieldType elementType, ReadOnlySpan<char> typeDescription)
    {
        if(typeDescription[1] == '?')
        {
            if(elementType is not BinaryFieldType binary
            || (binary.BinaryKind != BinaryFieldType.Kind.Bool
            &&  binary.BinaryKind != BinaryFieldType.Kind.UInt8
            &&  binary.BinaryKind != BinaryFieldType.Kind.Int8))
                throw new NotSupportedException($"Unsupported unbounded element type {elementType}]");

            return new ArrayFieldType(elementType, Kind.Unbounded);
        }

        return new ArrayFieldType(elementType, Kind.LengthPrefixed);
    }
}
