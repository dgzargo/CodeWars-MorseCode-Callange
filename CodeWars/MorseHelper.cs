namespace CodeWars;

public ref struct MorseHelper
{
	private ReadOnlySpan<char> _morseDictionary;
	public string CalculatedDictionary => new (_morseDictionary);
	public static int SpanLength => 111; //todo

	public MorseHelper(Span<char> span, Dictionary<string, string> morseDictionary)
	{
		_morseDictionary = ReadOnlySpan<char>.Empty;
		CalculateMorseDictionary(span, morseDictionary);
	}
	
	public MorseHelper(string calculatedDictionary)
	{
		_morseDictionary = calculatedDictionary;
	}

	public char? DecodeMorse(ReadOnlySpan<char> morseChar)
	{
		if (morseChar.Length == 0)
		{
			return null;
		}
		var index = EncodeMorse(morseChar);
		if (index >= _morseDictionary.Length)
		{
			return null;
		}
		var c = _morseDictionary[index];
		return c == '\0' ? null : c;
	}
	
	private void CalculateMorseDictionary(Span<char> span, Dictionary<string, string> morseDictionary)
	{
		if (span.Length != SpanLength)
		{
			throw new Exception();
		}

		if (!_morseDictionary.IsEmpty)
		{
			_morseDictionary.CopyTo(span);
			return;
		}

		//63
		foreach (var (key, value) in morseDictionary)
		{
			if (value.Length != 1 || key.Length > 5) continue;
			span[EncodeMorse(key)] = value[0];
		}

		//48
		foreach (var (key, value) in morseDictionary)
		{
			if (value.Length != 1 || key.Length <= 5) continue;
			span[EncodeMorse(key)] = value[0];
		}

		_morseDictionary = span;
	}

	private static int EncodeMorse(string morse)
	{
		return EncodeMorse(morse.AsSpan());
	}

	private static int EncodeMorse(ReadOnlySpan<char> morse)
	{
		var (i, j, k, l, m, offset) = morse.Length <= 5 ? (0, 2, 1, 2, 2, 0) : (0, 1, 1, 2, 2, 63);
		var result = i;
		// ReSharper disable once ForCanBeConvertedToForeach
		// ReSharper disable once LoopCanBeConvertedToQuery
		for (var index = 0; index < morse.Length; index++)
		{
			var c = morse[index];
			result = c == '.' ? result * j + k : result * l + m;
		}
		return result + offset;
	}

	// todo
	public string DecodeMorseLong(ReadOnlySpan<char> morseChar)
	{
		if (morseChar.Length == 9 && morseChar[0] == '.' && morseChar[1] == '.' && morseChar[2] == '.' &&
		    morseChar[3] == '-' && morseChar[4] == '-' && morseChar[5] == '-' && morseChar[6] == '.' &&
		    morseChar[7] == '.' && morseChar[8] == '.')
		{
			return "SOS";
		}
		return string.Empty;
	}
}