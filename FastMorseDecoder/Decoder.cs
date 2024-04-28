namespace FastMorseDecoder;

public static class Decoder
{
    public static string DecodeBitsToText(string bits)
    {
        var morseHelper = new SignDecoder(stackalloc char[SignDecoder.SpanLength], Preloaded.MORSE_CODE, true);

        return DecodeBitsToText(bits, morseHelper);
    }
    
    public static string DecodeBitsToText(string bits, SignDecoder signDecoder)
    {
        Span<char> morse = stackalloc char[bits.Length / 2 + 2]; // todo
        DecodeBitsToMorse(bits, ref morse);

        var text = morse; // reuse
        DecodeMorseToText(morse, signDecoder, ref text);
        
        return new string(text);
    }
    
    private static void DecodeBitsToMorse(string bits, ref Span<char> result)
    {
        var readOnlySpan = bits.AsSpan();
        readOnlySpan = TrimZeros(readOnlySpan);
        
        Span<int> statistics = stackalloc int[readOnlySpan.Length]; // todo
        CollectStatistics(readOnlySpan, ref statistics);
        
        // var onesPoint = Cluster2(onesStatistics);
        var (point1, point2) = Cluster3(statistics);

        DecodeBits(readOnlySpan, point1, point2, ref result);
    }

    private static ReadOnlySpan<char> TrimZeros(ReadOnlySpan<char> span)
    {
        var start = 0;
        for (var index = 0; index < span.Length; index++)
        {
            var c = span[index];
            if (c != '0')
            {
                start = index;
                break;
            }
        }

        var end = -1;
        for (var index = span.Length - 1; index >= 0; index--)
        {
            var c = span[index];
            if (c != '0')
            {
                end = index;
                break;
            }
        }

        return span.Slice(start, end - start + 1);
    }

    private static void CollectStatistics(ReadOnlySpan<char> span, ref Span<int> statistics)
    {
        var count = 0;
        var s = '1';
        var len = 0;
        var i = 0;

        do
        {
            var c = i < span.Length ? span[i] : '\0';

            if (c == s)
            {
                len++;
            }
            else if (len != 0)
            {
                var indexOfAny = statistics.IndexOfAny(len, 0);
                statistics[indexOfAny] = len;
                count = Math.Max(count, indexOfAny + 1);

                s = c;
                len = 1;
            }

            i++;
        } while (i <= span.Length);

        statistics = statistics[..count];
    }

    private static (int, int) Cluster3(ReadOnlySpan<int> statisticsSpan)
    {
        if (statisticsSpan.Length == 0)
        {
            return (0, 0);
        }
        if (statisticsSpan.Length == 1)
        {
            return (statisticsSpan[0] + 1, statisticsSpan[0] * 5);
        }
        if (statisticsSpan.Length == 2)
        {
            var (min, max) = GetMinMax(statisticsSpan);
            if (max / min > 5)
            {
                var avg = (min + max) / 2;
                return ((min + avg) / 2, (avg + max) / 2);
            }
            var point1 = (min + max) / 2;
            return (point1, point1 * 5 / 2);
        }

        if (statisticsSpan.Length == 3)
        {
            var v1 = statisticsSpan[0];
            var v2 = statisticsSpan[1];
            var v3 = statisticsSpan[2];
            switch (v1 < v2, v2 < v3, v1 < v3)
            {
                case (true, true, true): // 1-2-3
                    return ((v1 + v2)/2,(v2 + v3)/2);
                case (true, false, true): // 1-3-2
                    return ((v1 + v3)/2,(v3 + v2)/2);
                case (true, false, false): // 3-1-2
                    return ((v3 + v1)/2,(v1 + v2)/2);
                case (false, true, true): // 2-1-3
                    return ((v2 + v1)/2,(v1 + v3)/2);
                case (false, true, false): // 2-3-1
                    return ((v2 + v3)/2,(v3 + v1)/2);
                case (false, false, false): // 3-2-1
                    return ((v3 + v2)/2,(v2 + v1)/2);
                
                
                case (true, true, false):
                case (false, false, true):
                    throw new Exception();
            }
        }

        // k - means clustering
        var (center1, center3) = GetMinMax(statisticsSpan);
        var center2 = (center1 + center3) / 2;

        Span<int> cluster1 = stackalloc int[statisticsSpan.Length-2];
        Span<int> cluster2 = stackalloc int[statisticsSpan.Length-2];
        Span<int> cluster3 = stackalloc int[statisticsSpan.Length-2];

        while (true)
        {
            Assign3(cluster1, cluster2, cluster3, statisticsSpan, center1, center2, center3);
            var newCenter1 = GetCenter(cluster1);
            var newCenter2 = GetCenter(cluster2);
            var newCenter3 = GetCenter(cluster3);
                
            if (center1 == newCenter1 && center2 == newCenter2 && center3 == newCenter3)
            {
                break;
            }
            else
            {
                center1 = newCenter1;
                center2 = newCenter2;
                center3 = newCenter3;
                cluster1.Clear();
                cluster2.Clear();
                cluster3.Clear();
            }
        }

        // calculate border accurately
        var (min1, max1) = GetMinMax(cluster1);
        var (min2, max2) = GetMinMax(cluster2);
        var (min3, max3) = GetMinMax(cluster3);

        switch (max1 < max2, max2 < max3, max1 < max3)
        {
            case (true, true, true): // 1-2-3
                return ((max1 + min2)/2,(max2 + min3)/2);
            case (true, false, true): // 1-3-2
                return ((max1 + min3)/2,(max3 + min2)/2);
            case (true, false, false): // 3-1-2
                return ((max3 + min1)/2,(max1 + min2)/2);
            case (false, true, true): // 2-1-3
                return ((max2 + min1)/2,(max1 + min3)/2);
            case (false, true, false): // 2-3-1
                return ((max2 + min3)/2,(max3 + min1)/2);
            case (false, false, false): // 3-2-1
                return ((max3 + min2)/2,(max2 + min1)/2);
                
                
            case (true, true, false):
            case (false, false, true):
                throw new Exception();
        }
    }

    private static void Assign3(Span<int> cluster1, Span<int> cluster2, Span<int> cluster3, ReadOnlySpan<int> statisticsSpan, int center1, int center2, int center3)
    {
        foreach (var i in statisticsSpan)
        {
            ref var cluster = ref Math.Abs(center1 - i) < Math.Abs(center2 - i)
                ? ref Math.Abs(center1 - i) < Math.Abs(center3 - i) ? ref cluster1 : ref cluster3
                : ref Math.Abs(center2 - i) < Math.Abs(center3 - i) ? ref cluster2 : ref cluster3;
            // clever way to make Span act as a List
            cluster[0] = i;
            cluster = cluster[1..];
        }
    }

    private static int GetCenter(ReadOnlySpan<int> cluster)
    {
        var sum = 0;
        var count = 0; // can't be zero
        for (var index = 0; index < cluster.Length; index++)
        {
            var i = cluster[index];
            sum += i;
            if (i == 0)
            {
                if (index == 0)
                {
                    // no items in cluster
                    return 0;
                }
                count = index;
                break;
            }
        }

        if (count == 0)
        {
            // cluster contains all elements
            count = cluster.Length;
        }

        return sum / count;
    }

    private static (int min, int max) GetMinMax(ReadOnlySpan<int> cluster)
    {
        var min = int.MaxValue;
        var max = 0;

        foreach (var i in cluster)
        {
            if (i == 0)
            {
                break;
            }

            if (i < min)
            {
                min = i;
            }

            if (i > max)
            {
                max = i;
            }
        }

        return (min, max);
    }

    private static void DecodeBits(ReadOnlySpan<char> span, int point1, int point2, ref Span<char> result)
    {
        if (span.Length == 0)
        {
            result = Span<char>.Empty;
            return;
        }
        var resultLength = 0;
        var currentChar = span[0];
        var currentLength = 1;
        for (var index = 1; index < span.Length; index++)
        {
            var c = span[index];
            if (c == currentChar)
            {
                currentLength++;
            }
            else
            {
                AddChar(result, ref resultLength, currentChar, currentLength, point1, point2);
                currentChar = c;
                currentLength = 1;
            }
        }

        AddChar(result, ref resultLength, currentChar, currentLength, point1, point2);
        result = result[..resultLength];
    }

    private static void AddChar(Span<char> result, ref int resultLength, char currentChar, int currentLength, int point1, int point2)
    {
        var isSpacer = currentChar == '0';
        var isLong = isSpacer ? currentLength > point2 : currentLength > point1;
        var isShort = currentLength <= point1;
        switch (isSpacer, isShort, isLong)
        {
            case (true, true, true): // impossible
                break;
            case (true, true, false):
                // signal spacer
                break;
            case (true, false, true):
                // word spacer
                result[resultLength] = ' ';
                result[resultLength + 1] = ' ';
                result[resultLength + 2] = ' ';
                resultLength += 3;
                break;
            case (true, false, false):
                // char spacer
                result[resultLength] = ' ';
                resultLength++;
                break;
            case (false, false, true):
                result[resultLength] = '-';
                resultLength++;
                break;
            case (false, true, false):
                result[resultLength] = '.';
                resultLength++;
                break;
            case (false, true, true): // impossible
            case (false, false, false):
                break;
        }
    }

    private static void DecodeMorseToText(ReadOnlySpan<char> morse, SignDecoder signDecoder, ref Span<char> result)
    {
        var resultLength = 0;
        var offset = 0;
        int indexOfSpace;
        do
        {
            indexOfSpace = morse[offset..].IndexOf(' ');
            if (indexOfSpace == -1)
            {
                indexOfSpace = morse.Length;
            }
            else
            {
                indexOfSpace += offset;
            }

            var morseSymbol = morse[offset..indexOfSpace];
            var decodedMorse = signDecoder.DecodeSign(morseSymbol);
            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < decodedMorse.Length; i++)
            {
                result[resultLength] = decodedMorse[i];
                resultLength++;
            }
            
            if (indexOfSpace + 1 < morse.Length && morse[indexOfSpace + 1] == ' ')
            {
                offset = indexOfSpace + 3;
                result[resultLength] = ' ';
                resultLength++;
            }
            else
            {
                offset = indexOfSpace + 1;
            }
        } while (indexOfSpace != morse.Length);
        
        result = result[..resultLength];
    }
}