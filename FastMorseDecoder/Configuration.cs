namespace FastMorseDecoder;

public readonly struct Configuration
{
	public int RequiredMemory { get; }
	public readonly List<ChunkConfiguration> Chunks { get; }
}

public record struct ChunkConfiguration
{
	public override string ToString()
	{
		return $"{Initial} {DotMultiplier} {DotDelta} {DashMultiplier} {DashDelta} Chunk length: {ChunkLength}";
	}

	public void Deconstruct(out int initial, out int dotMultiplier, out int dotDelta, out int dashMultiplier, out int dashDelta, out int offset)
	{
		initial = Initial;
		dotMultiplier = DotMultiplier;
		dotDelta = DotDelta;
		dashMultiplier = DashMultiplier;
		dashDelta = DashDelta;
		offset = Offset;
	}
	
	public void Deconstruct(out int minKeyLength, out int maxKeyLength)
	{
		minKeyLength = MinKeyLength;
		maxKeyLength = MaxKeyLength;
	}

	public static implicit operator ChunkConfiguration((int initial, int dotMultiplier, int dotDelta, int dashMultiplier, int dashDelta, int offset) tuple)
	{
		return new ChunkConfiguration
		{
			Initial = tuple.initial,
			DotMultiplier = tuple.dotMultiplier,
			DotDelta = tuple.dotDelta,
			DashMultiplier = tuple.dashMultiplier,
			DashDelta = tuple.dashDelta,
			Offset = tuple.offset,
		};
	}

	public ChunkConfiguration(int minKeyLength, int maxKeyLength)
	{
		MinKeyLength = minKeyLength;
		MaxKeyLength = maxKeyLength;
		DashMultiplier = 1;
		DashMultiplier = 1;
		DashDelta = 1;
	}

	public bool Next()
	{
		DashDelta++;
		
		if (DashDelta > 14)
		{
			DashMultiplier++;
			DashDelta = 0;
			return true;
		}

		if (DashMultiplier > 14)
		{
			DotDelta++;
			DashMultiplier = 0;
			DashDelta = 0;
			return true;
		}

		if (DotDelta > 14)
		{
			DotMultiplier++;
			DotDelta = 0;
			DashMultiplier = 0;
			DashDelta = 0;
			return true;
		}

		if (DotMultiplier > 14)
		{
			Initial++;
			DotMultiplier = 0;
			DotDelta = 0;
			DashMultiplier = 0;
			DashDelta = 0;
			return true;
		}

		if (Initial > 4)
		{
			return false;
		}

		return true;
	}

	public int Initial { get; set; }
	public int DotMultiplier { get; set; }
	public int DotDelta { get; set; }
	public int DashMultiplier { get; set; }
	public int DashDelta { get; set; }
	public int Offset { get; set; }
	
	public int MinKeyLength { get; set; }
	public int MaxKeyLength { get; set; }
	public int ChunkLength { get; set; }
}