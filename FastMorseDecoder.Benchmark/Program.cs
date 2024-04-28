using System.Collections.Immutable;
using BenchmarkDotNet.Running;

namespace FastMorseDecoder.Benchmark;

// todo: remove
// 7x9 grid
// Backpack-battles
// Cards build
// Make all 31 cads to be connected.
internal static class Program
{
    public static void Main(string[] args)
    {
        // BenchmarkRunner.Run<MorseDecodeBenchmark>(null, args);
        // var config = new Config();
        //BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).RunAllJoined(null, args);


        var max = Preloaded.MORSE_CODE.Where(pair => pair.Value.Length == 1).Max(x=>x.Key.Length);
        Span<char> memory = stackalloc char[Environment.SystemPageSize];
        var dictionary = Preloaded.MORSE_CODE.ToImmutableList();
        var configurator = new Configurator(dictionary, false);

        var chunkConfiguration = configurator.TestEncoding(memory, new ChunkConfiguration(1, max), Environment.SystemPageSize);
        //-----------------
        var bestChunkLength = chunkConfiguration?.ChunkLength ?? Environment.SystemPageSize;
        for (var i = 2; i < max; i++)
        {
            var chunkLength = 4;
            var chunkConfiguration2 = configurator.TestEncoding(memory, new ChunkConfiguration(1, i), bestChunkLength - 4);
            if (!chunkConfiguration2.HasValue) continue;
            chunkLength += chunkConfiguration2.Value.ChunkLength;
            if (chunkLength > bestChunkLength) continue;
            var chunkConfiguration3 = configurator.TestEncoding(memory, new ChunkConfiguration(i, max), bestChunkLength - chunkLength);
            if (!chunkConfiguration3.HasValue) continue;
            chunkLength += chunkConfiguration3.Value.ChunkLength;
            if (chunkLength < bestChunkLength)
            {
                bestChunkLength = chunkLength;
            }
        }
        //todo reverse
        for (var i = 2; i < max; i++)
        {
            for (var j = i; j < max; j++)
            {
                var chunkLength = 0;
                var chunkConfiguration2 = configurator.TestEncoding(memory, new ChunkConfiguration(1, i), bestChunkLength);
                if (!chunkConfiguration2.HasValue) continue;
                chunkLength += chunkConfiguration2.Value.ChunkLength;
                if (chunkLength > bestChunkLength) continue;
                var chunkConfiguration3 = configurator.TestEncoding(memory, new ChunkConfiguration(i, j), bestChunkLength - chunkLength);
                if (!chunkConfiguration3.HasValue) continue;
                chunkLength += chunkConfiguration3.Value.ChunkLength;
                var chunkConfiguration4 = configurator.TestEncoding(memory, new ChunkConfiguration(j, max), bestChunkLength - chunkLength);
                if (!chunkConfiguration4.HasValue) continue;
                chunkLength += chunkConfiguration4.Value.ChunkLength;
                if (chunkLength < bestChunkLength)
                {
                    bestChunkLength = chunkLength;
                }
            }
        }
        Console.WriteLine(bestChunkLength);
        configurator.PrintStopwatch();
    }
}