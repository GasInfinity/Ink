using Ink.SourceGenerator.Block;
using Ink.SourceGenerator.Packet;
using Ink.SourceGenerator.Registry;
using System.CodeDom.Compiler; 
using System.Collections.Immutable;
using System.Text.Json;

namespace Ink.SourceGenerator;

public class Program // TODO Refactor this!!
{
    public static void Main()
    {
        // ProcessPackets();
        ProcessBlocks();
        //ProcessRegistries();

        GC.Collect(2);
        GCMemoryInfo info = GC.GetGCMemoryInfo();
        Console.WriteLine($"{info.HeapSizeBytes} / {info.TotalCommittedBytes} Bytes");
    }

    private static void ProcessPackets()
    {
        using FileStream packetsDataFile = File.OpenRead("packets_data.json");
        using FileStream packetsFile = File.OpenRead("packets.json");
        ImmutableDictionary<PacketKind, ImmutableDictionary<PacketSide, ImmutableDictionary<string, PacketDefinition>>> packets = JsonSerializer.Deserialize(packetsFile, InkGeneratorJsonContext.Default.ImmutableDictionaryPacketKindImmutableDictionaryPacketSideImmutableDictionaryStringPacketDefinition)!;
        ImmutableDictionary<PacketKind, ImmutableDictionary<PacketSide, ImmutableDictionary<string, PacketData?>>> packetsData = JsonSerializer.Deserialize(packetsDataFile, InkGeneratorJsonContext.Default.ImmutableDictionaryPacketKindImmutableDictionaryPacketSideImmutableDictionaryStringNullablePacketData)!;

        PacketProcessor.Process(packets, packetsData);
    }

    private static void ProcessRegistries()
    {
        using FileStream registriesFile = File.OpenRead("registries.json");
        ImmutableDictionary<string, RegistryData> registries = JsonSerializer.Deserialize(registriesFile, InkGeneratorJsonContext.Default.ImmutableDictionaryStringRegistryData)!;

        foreach (KeyValuePair<string, RegistryData> kv in registries)
        {
            string name = Generation.ToPascalCase(kv.Key);
            ProcessRegistry(kv.Value, "Ink.Data", name, name);
        }
    }

    private static void ProcessRegistry(RegistryData registry, string @namespace, string className, string outputFileName)
    {
        _ = Directory.CreateDirectory("Out/Registries");
        using FileStream outputFile = File.OpenWrite($"Out/Registries/{outputFileName}.g.cs");
        using StreamWriter writer = new(outputFile);
        using IndentedTextWriter indentedWriter = new(writer);

        // RegistryProcessor.WriteTo(indentedWriter, @namespace, className, registry);
    }

    private static void ProcessBlocks()
    {
        using FileStream blocksFile = File.OpenRead("blocks.json");
        ImmutableDictionary<string, BlockData> blocks = JsonSerializer.Deserialize(blocksFile, InkGeneratorJsonContext.Default.ImmutableDictionaryStringBlockData)!;

        BlocksProcessor.Process(blocks);
    }
}
