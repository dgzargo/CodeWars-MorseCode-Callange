namespace FastMorseDecoder;

public readonly ref struct SignDecoder
{
	private readonly ReadOnlySpan<char> _morseDictionary;
	private readonly Dictionary<string, string>? _morseDictionaryLongSignals;
	public string CalculatedDictionary => new (_morseDictionary);
	public static int SpanLength => 111; //todo
	private readonly ChunkConfiguration _shortConfig = (0, 2, 1, 2, 2, 0);
	private readonly ChunkConfiguration _longConfig = (0, 1, 1, 2, 2, 63);

	public SignDecoder(Span<char> span, Dictionary<string, string> morseDictionary, bool encodeLongSignals)
	{
        CalculateMorseDictionary(span, morseDictionary);
		_morseDictionary = span;
		if (encodeLongSignals && morseDictionary.Any(pair => pair.Key.Length > 1))
		{
			_morseDictionaryLongSignals = morseDictionary.Where(pair => pair.Key.Length > 1).ToDictionary();
		}
	}

	public SignDecoder(string calculatedDictionary)
	{
		_morseDictionary = calculatedDictionary;
	}
	
	private void CalculateMorseDictionary(Span<char> span, Dictionary<string, string> morseDictionary)
	{
		if (span.Length != SpanLength) //todo
		{
			throw new Exception();
		}

		//63 for key.Length < 5
		//48 for key.Length >= 5
		foreach (var (key, value) in morseDictionary)
		{
			if (value.Length != 1) continue;
			var index = EncodeSign(key) ?? throw new ApplicationException();
			span[index] = value[0];
		}
	}

	private int? EncodeSign(ReadOnlySpan<char> morse)
	{
		// todo: configuration
		ChunkConfiguration chunkConfiguration;
		switch (morse.Length)
		{
			case <= 5:
				chunkConfiguration = _shortConfig;
				break;
			case <= 7:
				chunkConfiguration = _longConfig;
				break;
			default:
				return null;
		}
		return SignMisc.EncodeSign(morse, chunkConfiguration);
	}

	public ReadOnlySpan<char> DecodeSign(ReadOnlySpan<char> morseChar)
	{
		if (morseChar.IsEmpty || _morseDictionary.IsEmpty)
		{
			return default;
		}
		
		if (morseChar.Length == 9 && morseChar[0] == '.' && morseChar[1] == '.' && morseChar[2] == '.' &&
		    morseChar[3] == '-' && morseChar[4] == '-' && morseChar[5] == '-' && morseChar[6] == '.' &&
		    morseChar[7] == '.' && morseChar[8] == '.')
		{
			return "SOS"; // todo: long symbol dictionary
		}
		
		var index = EncodeSign(morseChar) ?? throw new ApplicationException(); //todo
		if (index >= _morseDictionary.Length)
		{
			return default;
		}
		return _morseDictionary[index] == '\0' ? default : _morseDictionary[index..(index+1)]; // todo: allow long values
	}
}