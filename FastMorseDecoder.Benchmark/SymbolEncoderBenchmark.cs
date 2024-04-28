using BenchmarkDotNet.Attributes;

namespace FastMorseDecoder.Benchmark;

[MemoryDiagnoser]
public class SymbolEncoderBenchmark
{
	private Dictionary<string, string>? _dictionary;

	[GlobalSetup]
	public void Setup()
	{
		_dictionary = Preloaded.MORSE_CODE;
	}

	[Benchmark]
	public string TestDictionaryCalculationWithStringCreation()
	{
		var morseHelper = new SignDecoder(stackalloc char[SignDecoder.SpanLength], _dictionary!, true);
		return morseHelper.CalculatedDictionary;
	}
}