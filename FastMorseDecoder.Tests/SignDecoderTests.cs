namespace FastMorseDecoder.Tests;

public class SignDecoderTests
{
	[Test]
	public void Creation()
	{
		var morseHelper = new SignDecoder(stackalloc char[SignDecoder.SpanLength], Preloaded.MORSE_CODE, true);
		Assert.IsNotEmpty(morseHelper.CalculatedDictionary);
		Assert.That(morseHelper.CalculatedDictionary.Length, Is.LessThanOrEqualTo(SignDecoder.SpanLength));
	}
	
	[TestCase(true, TestName = "Can encode any signal")]
	[TestCase(false, TestName = "Can encode short signals")]
	public void CanEncode(bool encodeLongSignals)
	{
		var morseHelper = new SignDecoder(stackalloc char[SignDecoder.SpanLength], Preloaded.MORSE_CODE, encodeLongSignals);
		foreach (var (key, value) in Preloaded.MORSE_CODE)
		{
			if (value.Length > 1 && !encodeLongSignals) continue;
			Assert.AreEqual(value, morseHelper.DecodeSign(key).ToString());
		}
	}
}