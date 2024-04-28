using System.Collections.Immutable;
using System.Diagnostics;

namespace FastMorseDecoder;

public class Configurator
{
	private readonly ImmutableArray<KeyValuePair<string, char>> _dictionary;
	private readonly ImmutableArray<int> _index;
	private readonly Stopwatch _stopwatch1;
	private readonly Stopwatch _stopwatch2;

	public Configurator(IEnumerable<KeyValuePair<string, string>> morseDictionary, bool allowProsigns)
	{
		_dictionary = morseDictionary.Where(pair => pair.Value.Length == 1).Select(pair => new KeyValuePair<string,char>(pair.Key, pair.Value[0])).OrderBy(pair => pair.Key.Length).ToImmutableArray();
		var indexTable = _dictionary.GroupBy(pair => pair.Key.Length).OrderBy(grouping => grouping.Key).Select(grouping => grouping.Count()).ToList();
		for (var i = indexTable.Count - 1 - 1; i >= 0; i--)
		{
			for (var j = indexTable.Count - 1; j > i; j--)
			{
				indexTable[j] += indexTable[i];
			}
		}
		indexTable.Insert(0, 0);
		_index = indexTable.ToImmutableArray();
		_stopwatch1 = new Stopwatch();
		_stopwatch2 = new Stopwatch();
	}
	public static Configuration Create()
	{
		throw new NotImplementedException();
	}

	private (int minPosition, int maxPosition)? CalculateDictionaryLength(Span<char> memory, in ChunkConfiguration chunkConfiguration, in int bestChunkLength, in Span<int> fastClearCache, out int fastClearCacheCounter)
	{
		Debug.Assert(memory.Length == Environment.SystemPageSize);
		var minPosition = int.MaxValue;
		var maxPosition = int.MinValue;
		var (minKeyLength, maxKeyLength) = chunkConfiguration;

		fastClearCacheCounter = 0;
		
		// do not iterate through unwanted values
		for (var index = _index[minKeyLength - 1]; index < _index[maxKeyLength - 1]; index++)
		{
			var (key, value) = _dictionary[index];

			var position = SignMisc.EncodeSign(key, chunkConfiguration);
			if (position >= Environment.SystemPageSize || memory[position] != '\0')
			{
				return null; // index is too large
			}

			memory[position] = value; // todo: write down
			fastClearCache[fastClearCacheCounter] = position;
			fastClearCacheCounter++;

			minPosition = Math.Min(position, minPosition);
			maxPosition = Math.Max(position, maxPosition);

			if (maxPosition - minPosition > bestChunkLength)
			{
				return null; // config is poor
			}
		}
		
		return (minPosition, maxPosition);
	}

	public void PrintStopwatch()
	{
		if (_stopwatch1.IsRunning || _stopwatch2.IsRunning)
		{
			Console.WriteLine("One of stopwatches still running!");
		}
		Console.WriteLine($"SW1: {_stopwatch1.Elapsed}, SW2: {_stopwatch2.Elapsed}");
	}

	public ChunkConfiguration? TestEncoding(Span<char> memory, ChunkConfiguration chunkConfiguration, int bestChunkLength)
	{
		_stopwatch1.Start();
		Span<int> fastClearCache = stackalloc int[_dictionary.Length];
		ChunkConfiguration? bestConfiguration = null;
		do
		{
			var dictionaryLength = CalculateDictionaryLength(memory, chunkConfiguration, bestChunkLength, fastClearCache, out var fastClearCacheCounter);
			for (var index = 0; index < fastClearCacheCounter; index++)
			{
				memory[fastClearCache[index]] = '\0';
			}

			if (dictionaryLength is var (minPosition, maxPosition))
			{
				var chunkLength = maxPosition - minPosition;
				if (chunkLength < bestChunkLength)
				{
					bestChunkLength = chunkLength;
					bestConfiguration = chunkConfiguration with
					{
						ChunkLength = chunkLength,
						Offset = - minPosition,
					};
				}
			}
		} while (chunkConfiguration.Next());
		_stopwatch1.Stop();

		return bestConfiguration;
	}
	
	private struct CalculationResult
	{
		public enum Result
		{
			Some,
			SameIndexError,
			TooLargeIndexError,
		}
		public struct Some
		{
			public int MinPosition;
			public int MaxPosition;
		}
	}
}