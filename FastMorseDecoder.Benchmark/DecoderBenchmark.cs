using BenchmarkDotNet.Attributes;

namespace FastMorseDecoder.Benchmark;

[MemoryDiagnoser]
public class DecoderBenchmark
{
	private string? _morseHelperDictionary;

	[GlobalSetup]
	public void Setup()
	{
		var morseHelper = new SignDecoder(stackalloc char[SignDecoder.SpanLength], Preloaded.MORSE_CODE, true);
		_morseHelperDictionary = morseHelper.CalculatedDictionary;
	}

	[Benchmark]
	public string Test1()
	{
		var morseHelper = new SignDecoder(_morseHelperDictionary!);
		return Decoder.DecodeBitsToText("0000000011011010011100000110000001111110100111110011111100000000000111011111111011111011111000000101100011111100000111110011101100000100000", morseHelper);
	}
}