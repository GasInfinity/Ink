using System.CodeDom.Compiler;

namespace Ink.SourceGenerator.Registry;

public static class RegistryProcessor
{
    public static void WriteTo(IndentedTextWriter writer, string @namespace, string className, RegistryData data)
    {
        writer.WrtLine($"namespace {@namespace};")
              .WrtLine()
              .WrtLine($"public static partial class {className}")
              .WrtLine('{')
              .Ind();

        foreach(KeyValuePair<string, RegistryElement> kv in data.Entries)
            writer.WriteLine($"public const int {Generation.ToPascalCase(kv.Key)} = {kv.Value.ProtocolId};");

        writer.Unind()
              .WrtLine('}');
    }
}
