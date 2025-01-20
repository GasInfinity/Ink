﻿using Ink.Blocks.State;
using Ink.SourceGenerator.Util;
using System.Collections.Immutable;

namespace Ink.SourceGenerator.Block;

public static class BlocksProcessor // TODO: Refactor this some day, make it a netstandard2.0 C# SourceGenerator?
{
    const string Namespace = "Ink.Blocks.State";
    const string ClassName = "BlockStates";

    public static void Process(ImmutableDictionary<string, BlockData> blocks)
    {
        PropertyStore propertyStore = new();
        PropertyNamesStore propertyNamesStore = new();
        Dictionary<string, (List<int>, int)> blockPropertyCache = new();
        int stateCount = 0;

        foreach ((string location, BlockData block) in blocks)
        {
            if(block.Properties != null)
            {
                List<int> propertyCache = new();

                foreach((string propertyName, ImmutableArray<string> propertyValues) in block.Properties)
                    propertyCache.Add(propertyStore.Add(propertyName, propertyValues));

                int namesStoreIndex = propertyNamesStore.Add(block.Properties.Keys);
                blockPropertyCache.Add(location, (propertyCache, namesStoreIndex));
            }

            stateCount += block.States.Length;
        }

        _ = Directory.CreateDirectory("Out/Blocks/State/");

        IndentingStringBuilder builder = new();
        WriteBlockStateRoots(builder, stateCount, propertyStore, blockPropertyCache, blocks);

        IndentingStringBuilder propertyStoreBuilder = new();
        propertyStore.WriteTo(propertyStoreBuilder);

        IndentingStringBuilder propertyNamesStoreBuilder = new();
        propertyNamesStore.WriteTo(propertyNamesStoreBuilder);

        File.WriteAllText("Out/Blocks/State/BlockStates.g.cs", builder.ToString());
        File.WriteAllText("Out/Blocks/State/Properties.g.cs", propertyStoreBuilder.ToString());
        File.WriteAllText("Out/Blocks/State/PropertyNamesStore.g.cs", propertyNamesStoreBuilder.ToString());
    }

    // HACK: Could be optimized a lot more...
    private static void WriteBlockStateRoots(IndentingStringBuilder builder, int stateCount, PropertyStore propertyStore, Dictionary<string, (List<int>, int)> blockPropertyCache, ImmutableDictionary<string, BlockData> blocks)
    {
        builder.WriteLine($$"""
                // <auto-generated />

                using System.Collections.Frozen;
                using System.Collections.Immutable;
                using System.Runtime.InteropServices;
                using Ink.Registries;
                using Ink.Util;

                namespace {{Namespace}};

                public static partial class {{ClassName}}
                """);
        using(builder.EnterBlock())
        {
            builder.WriteLine($"public const int StateCount = {stateCount};");
            builder.WriteLine($"public const byte MaxStateBits = {Utilities.BitSize(stateCount)};");
            builder.WriteLine();
            builder.WriteLine($"static {ClassName}()");
            using(builder.EnterBlock())
            {
                builder.WriteLine($"ImmutableArray<(BlockStateRoot, int)>.Builder allStates = ImmutableArray.CreateBuilder<(BlockStateRoot, int)>({stateCount});");
                builder.WriteLine($"allStates.Count = {stateCount};");
                foreach((string locationStr, BlockData block) in blocks)
                {
                    Identifier location = Identifier.Parse(locationStr, null);

                    builder.WriteLine($"AddRoot(allStates, {Generation.ToPascalCase(location.Path)}.Root);");
                }
                builder.WriteLine($"AllStates = allStates.MoveToImmutable();");
            }

            foreach ((string locationStr, BlockData block) in blocks)
            {
                Identifier location = Identifier.Parse(locationStr, null);

                if(!blockPropertyCache.TryGetValue(locationStr, out (List<int> PropertyStoreIndexes, int PropertyNamesIndex) cache))
                {
                    builder.WriteLine($"public static class {Generation.ToPascalCase(location.Path)}");

                    using (builder.EnterBlock())
                        builder.WriteLine($"public static readonly BlockStateRoot Root = new(Identifier.Vanilla(\"{location.Path}\"), FrozenDictionary<string, int>.Empty, [], (new Dictionary<int, int>() {{ {{ 0, {block.States[0].Id} }} }}).ToFrozenDictionary(), 0);"); 
                    continue;
                }

                builder.WriteLine($"public static class {Generation.ToPascalCase(location.Path)}");
                using(builder.EnterBlock())
                {
                    builder.Write($$"""
                            public static readonly BlockStateRoot Root = new(Identifier.Vanilla("{{location.Path}}"), {{nameof(PropertyNamesStore)}}.CachedNames{{cache.PropertyNamesIndex}}, [
                            """);

                    int currentBitOffset = 0;
                    for(int i = 0; i < block.Properties!.Count; ++i)
                    {
                        string propertyName = block.Properties.Keys.ElementAt(i);
                        Property property = propertyStore.GetProperty(propertyName, cache.PropertyStoreIndexes[i]);
                        int bitsUsed = property.BitsUsed;

                        builder.Write($"(Properties.{propertyStore.GetName(propertyName, cache.PropertyStoreIndexes[i])}, {bitsUsed}, {currentBitOffset})");
                        currentBitOffset += bitsUsed;

                        if(i != block.Properties!.Count - 1)
                            builder.Write(", ");
                    }

                    builder.Write("], (new Dictionary<int, int>() { ");

                    int defaultStateIndex = 0;
                    for(int i = 0; i < block.States.Length; ++i)
                    {
                        DefinedBlockState state = block.States[i]; 

                        int builtStateIndex = 0;
                        int propertyIndex = 0;
                        int bitOffset = 0;
                        foreach((string propertyName, string propertyValue) in state.Properties)
                        {
                            Property property = propertyStore.GetProperty(propertyName, cache.PropertyStoreIndexes[propertyIndex++]);
                            int valueIndex = block.Properties[propertyName].IndexOf(propertyValue);

                            builtStateIndex |= (valueIndex << bitOffset);

                            bitOffset += property.BitsUsed;
                        }

                        if(state.IsDefault)
                            defaultStateIndex = builtStateIndex;

                        builder.Write($"{{ {builtStateIndex}, {state.Id} }}");

                        if(i != block.States!.Length - 1)
                            builder.Write(", ");
                    }

                    builder.WriteLine($" }}).ToFrozenDictionary(), {defaultStateIndex});");
                }
            }
        }
    }
}
