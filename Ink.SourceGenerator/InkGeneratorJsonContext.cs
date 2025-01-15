using Ink.SourceGenerator.Packet;
using Ink.SourceGenerator.Block;
using Ink.SourceGenerator.Registry;
using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace Ink.SourceGenerator;

[JsonSourceGenerationOptions(IncludeFields = true)]
[JsonSerializable(typeof(ImmutableDictionary<string, BlockData>))]
[JsonSerializable(typeof(BlockData))]
[JsonSerializable(typeof(DefinedBlockState))]
[JsonSerializable(typeof(ImmutableDictionary<string, RegistryData>))]
[JsonSerializable(typeof(ImmutableDictionary<PacketKind, ImmutableDictionary<PacketSide, ImmutableDictionary<string, PacketData?>>>))]
[JsonSerializable(typeof(ImmutableDictionary<PacketKind, ImmutableDictionary<PacketSide, ImmutableDictionary<string, PacketDefinition>>>))]
public sealed partial class InkGeneratorJsonContext : JsonSerializerContext
{
}
