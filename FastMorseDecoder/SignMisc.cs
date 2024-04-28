namespace FastMorseDecoder;

public static class SignMisc
{
	public static int EncodeSign(ReadOnlySpan<char> morse, ChunkConfiguration chunkConfiguration)
	{
		var (result, dotMultiplier, dotDelta, dashMultiplier, dashDelta, offset) = chunkConfiguration;
		// ReSharper disable once ForCanBeConvertedToForeach
		// ReSharper disable once LoopCanBeConvertedToQuery
		for (var index = 0; index < morse.Length; index++)
		{
			var c = morse[index];
			result = c == '.' ? result * dotMultiplier + dotDelta : result * dashMultiplier + dashDelta;
		}
		return result + offset;
	}
}