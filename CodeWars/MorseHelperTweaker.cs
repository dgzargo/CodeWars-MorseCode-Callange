namespace CodeWars;

public class MorseHelperTweaker
{
	
	public static string DecodeMorse(int position)
	{
		const int maxLength = 7;
		Span<char> result = stackalloc char[maxLength];
		var length = 0;
		while (position != 0)
		{
			if (position % 2 == 0)
			{
				result[maxLength - length - 1] = '-';
				position -= 12;
			}
			else
			{
				result[maxLength - length - 1] = '.';
				position -= 5;
			}
			length++;
			position /= 2;
		}

		return new string(result[(maxLength - length)..]);
	}

	private static int GetMorseDictionaryLength(List<char> morseDictionary, int i, int j, int k, int l, int m)
	{
		foreach (var (key, value) in Preloaded.MORSE_CODE)
		{
			if (value.Length != 1) continue;
			if (key.Length > 5) continue;
			var position = i;
			
			// ReSharper disable once ForCanBeConvertedToForeach
			// ReSharper disable once LoopCanBeConvertedToQuery
			for (var index = 0; index < key.Length; index++)
			{
				var c = key[index];
				position = c == '.' ? position * j + k : position * l + m;
			}

			if (position<morseDictionary.Count && morseDictionary[position] != '\0')
			{
				return int.MaxValue;
			}

			if (position > 1000) return int.MaxValue;
			if (position >= morseDictionary.Count)
			{
				morseDictionary.AddRange(Enumerable.Repeat('\0', position - morseDictionary.Count + 1));
			}
			morseDictionary[position] = value[0];
		}

		return morseDictionary.Count;
	}

	private static int GetMorseDictionaryLength2(List<char> morseDictionary, int i, int j, int k, int l, int m)
	{
		foreach (var (key, value) in Preloaded.MORSE_CODE)
		{
			if (value.Length == 1 && key.Length <= 5) continue;
			var position = i;
			
			// ReSharper disable once ForCanBeConvertedToForeach
			// ReSharper disable once LoopCanBeConvertedToQuery
			for (var index = 0; index < key.Length && position >= 0; index++)
			{
				var c = key[index];
				position = c == '.' ? position * j + k : position * l + m;
			}

			if (position < 0 || position<morseDictionary.Count && morseDictionary[position] != '\0')
			{
				return int.MaxValue;
			}

			if (position > 1000) return int.MaxValue;
			if (position >= morseDictionary.Count)
			{
				morseDictionary.AddRange(Enumerable.Repeat('\0', position - morseDictionary.Count + 1));
			}
			morseDictionary[position] = value[0];
		}

		return morseDictionary.Count;
	}

	public static void TestEncoding()
	{
		var morseDictionary = new List<char>(1000);
		var bestLength = int.MaxValue;
		for (var i = 0; i < 13; i++)
		{
			for (var j = 1; j < 13; j++)
			{
				for (var k = 0; k < 13; k++)
				{
					for (var l = 1; l < 13; l++)
					{
						for (var m = 0; m < 13; m++)
						{
							morseDictionary.Clear();
							var zipLength = GetMorseDictionaryLength2(morseDictionary, i, j, k, l, m);
							if (zipLength < bestLength)
							{
								bestLength = zipLength;
								Console.WriteLine($"New best: {bestLength}. {i};{j};{k};{l};{m}");
							}
						}
					}
				}
			}
		}
	}
}