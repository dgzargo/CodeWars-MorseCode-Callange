namespace CodeWars;

#nullable enable

// 7x9 grid
// Backpack-battles
// Cards build
// Make all 31 cads to be connected.
internal static class Program
{
    public static void Main()
    {
        var morse = decodeBitsAdvanced("0000000011011010011100000110000001111110100111110011111100000000000111011111111011111011111000000101100011111100000111110011101100000100000");
        var message = decodeMorse(morse);
        Console.WriteLine(message);
    }



    public static string decodeBitsAdvanced(string bits)
    {
        return bits;
    }
    
    // ReSharper disable once InconsistentNaming
    private static void decodeBitsAdvanced2(string bits, ref Span<char> result)
    {
        var readOnlySpan = bits.AsSpan();
        readOnlySpan = TrimZeros(readOnlySpan);
        
        Span<int> zerosStatistics = stackalloc int[readOnlySpan.Length];
        Span<int> onesStatistics = stackalloc int[readOnlySpan.Length];
        CollectStatistics2(readOnlySpan, ref zerosStatistics, ref onesStatistics);
        
        var onesPoint = Clasterize2(onesStatistics);
        var (zerosPoint1, zerosPoint2) = Clasterize3(zerosStatistics);

        DecodeBits(readOnlySpan, onesPoint, zerosPoint1, zerosPoint2, ref result);

        static ReadOnlySpan<char> TrimZeros(ReadOnlySpan<char> span)
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

            var end = 0;
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

        static void CollectStatistics2(ReadOnlySpan<char> span, ref Span<int> zerosStatistics, ref Span<int> onesStatistics)
        {
            var zerosStatisticCount = 0;
            var onesStatisticCount = 0;
            var s = '1';
            var len = 0;
            foreach (var c in span)
            {
                if (c == s)
                {
                    len++;
                    continue;
                }

                ref var statistics = ref s == '0' ? ref zerosStatistics : ref onesStatistics;
                ref var count = ref s == '0' ? ref zerosStatisticCount : ref onesStatisticCount;
                
                var indexOfAny = statistics.IndexOfAny(len, 0);
                statistics[indexOfAny] = len;
                count = Math.Max(count, indexOfAny + 1);

                s = c;
                len = 1;
            }

            zerosStatistics = zerosStatistics[..zerosStatisticCount];
            onesStatistics = onesStatistics[..onesStatisticCount];
        }

        static int Clasterize2(ReadOnlySpan<int> statisticsSpan)
        {
            if (statisticsSpan.Length < 2)
            {
                throw new Exception();
            }

            if (statisticsSpan.Length == 2)
            {
                return (statisticsSpan[0] + statisticsSpan[1]) / 2;
            }

            // k - means clustering
            var (center1, center2) = GetMinMax(statisticsSpan);

            if (center1 == center2)
            {
                throw new Exception();
            }

            Span<int> cluster1 = stackalloc int[statisticsSpan.Length];
            Span<int> cluster2 = stackalloc int[statisticsSpan.Length];

            while (true)
            {
                Assign(cluster1, cluster2, statisticsSpan);
                var newCenter1 = GetCenter(cluster1);
                var newCenter2 = GetCenter(cluster2);
                
                if (center1 == newCenter1 && center2 == newCenter2)
                {
                    break;
                }
                else
                {
                    center1 = newCenter1;
                    center2 = newCenter2;
                    cluster1.Clear();
                    cluster2.Clear();
                }
            }

            // calculate border accurately
            var (min1, max1) = GetMinMax(cluster1);
            var (min2, max2) = GetMinMax(cluster2);

            return (max1 < min2 ? max1 + min2 : max2 + min1) / 2;

            void Assign(Span<int> cluster1, Span<int> cluster2, ReadOnlySpan<int> statisticsSpan)
            {
                foreach (var i in statisticsSpan)
                {
                    ref var cluster = ref Math.Abs(center1 - i) < Math.Abs(center2 - i) ? ref cluster1 : ref cluster2;
                    // clever way to make Span act as a List
                    cluster[0] = i;
                    cluster = cluster[1..];
                }
            }
        }

        static (int, int) Clasterize3(ReadOnlySpan<int> statisticsSpan)
        {
            if (statisticsSpan.Length < 3)
            {
                throw new Exception();
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

            Span<int> cluster1 = stackalloc int[statisticsSpan.Length-1];
            Span<int> cluster2 = stackalloc int[statisticsSpan.Length-1];
            Span<int> cluster3 = stackalloc int[statisticsSpan.Length-1];

            while (true)
            {
                Assign(cluster1, cluster2, cluster3, statisticsSpan);
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

            void Assign(Span<int> cluster1, Span<int> cluster2, Span<int> cluster3, ReadOnlySpan<int> statisticsSpan)
            {
                foreach (var i in statisticsSpan)
                {
                    ref var cluster = ref Math.Abs(center1 - i) < Math.Abs(center2 - i)
                        ? ref Math.Abs(center1 - i) < Math.Abs(center3 - i) ? ref cluster1 : ref cluster3
                        : ref Math.Abs(center2 - i) < Math.Abs(center3 - i) ? ref cluster2 : ref cluster3;
                    // clever way to make Span act as a List
                    cluster[0] = i;
                    cluster = cluster[1..]; // todo: fix
                }
            }
        }

        static int GetCenter(ReadOnlySpan<int> cluster)
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

        static (int min, int max) GetMinMax(ReadOnlySpan<int> cluster)
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

        static void DecodeBits(ReadOnlySpan<char> span, int onesPoint, int zerosPoint1, int zerosPoint2, ref Span<char> result)
        {
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
                    AddChar(result, ref resultLength, currentChar, currentLength, zerosPoint1, zerosPoint2, onesPoint);
                    currentChar = c;
                    currentLength = 1;
                }
            }

            AddChar(result, ref resultLength, currentChar, currentLength, zerosPoint1, zerosPoint2, onesPoint);
            result = result[..resultLength];

            static void AddChar(Span<char> result, ref int resultLength, char currentChar, int currentLength, int zerosPoint1, int zerosPoint2, int onesPoint)
            {
                var isSpacer = currentChar == '0';
                var isLong = isSpacer ? currentLength > zerosPoint2 : currentLength > onesPoint;
                var isShort = isSpacer ? currentLength <= zerosPoint1 : currentLength <= onesPoint;
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
        }
    }

    // ReSharper disable once InconsistentNaming
    public static string decodeMorse (string bits)
    {
        Span<char> morse = stackalloc char[bits.Length / 4];
        decodeBitsAdvanced2(bits, ref morse);
        
        Span<char> morseDictionary = stackalloc char[MorseHelper.SpanLength];
        MorseHelper.Zip(morseDictionary);

        var result = morse; // reuse
        var resultLength = 0;
        var offset = 0;
        var indexOfSpace = morse.IndexOf(' ');
        while (indexOfSpace != -1)
        {
            indexOfSpace += offset;
            var morseChar = morse[offset..indexOfSpace];
            var c = morseDictionary[MorseHelper.EncodeMorse(morseChar)];
            result[resultLength] = c;
            resultLength++;
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
            indexOfSpace = morse[offset..].IndexOf(' ');
        }
        result[resultLength] = morseDictionary[MorseHelper.EncodeMorse(morse[offset..])];
        resultLength++;
        return new string(result[..resultLength]);
    }
}

public static class MorseHelper
{
    private static string? _zip;
    public static int SpanLength => 708;
    public static void Zip(Span<char> span)
    {
        if (span.Length != SpanLength)
        {
            throw new Exception();
        }

        if (_zip is not null)
        {
            _zip.CopyTo(span);
            return;
        }

        foreach (var (key, value) in Preloaded.MORSE_CODE)
        {
            if (value.Length != 1) continue;
            span[EncodeMorse(key)] = value[0];
        }

        _zip = new string(span);
    }

    public static int EncodeMorse(string morse)
    {
        return EncodeMorse(morse.AsSpan());
    }

    public static int EncodeMorse(ReadOnlySpan<char> morse)
    {
        var result = 0;
        // ReSharper disable once ForCanBeConvertedToForeach
        // ReSharper disable once LoopCanBeConvertedToQuery
        for (var index = 0; index < morse.Length; index++)
        {
            var c = morse[index];
            result = c == '.' ? result * 2 + 5 : result * 2 + 12;
        }
        return result;
    }

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

    public static int GetZipLength(int i, int j, int k, int l, int m)
    {
        var morseDictionary = new List<char>(1000);
        foreach (var (key, value) in Preloaded.MORSE_CODE)
        {
            if (value.Length != 1) continue;
            var position = key.Aggregate(i, (position, c) => c == '.' ? position * j + k : position * l + m);
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
}

static class Preloaded
{
    // ReSharper disable once InconsistentNaming
    public static Dictionary<string, string> MORSE_CODE { get; }

    static Preloaded()
    {
        MORSE_CODE = new Dictionary<string, string>
        {
            { ".-", "A" },
            { "-...", "B" },
            { "-.-.", "C" },
            { "-..", "D" },
            { ".", "E" },
            { "..-.", "F" },
            { "--.", "G" },
            { "....", "H" },
            { "..", "I" },
            { ".---", "J" },
            { "-.-", "K" },
            { ".-..", "L" },
            { "--", "M" },
            { "-.", "N" },
            { "---", "O" },
            { ".--.", "P" },
            { "--.-", "Q" },
            { ".-.", "R" },
            { "...", "S" },
            { "-", "T" },
            { "..-", "U" },
            { "...-", "V" },
            { ".--", "W" },
            { "-..-", "X" },
            { "-.--", "Y" },
            { "--..", "Z" },
            { "-----", "0" },
            { ".----", "1" },
            { "..---", "2" },
            { "...--", "3" },
            { "....-", "4" },
            { ".....", "5" },
            { "-....", "6" },
            { "--...", "7" },
            { "---..", "8" },
            { "----.", "9" },
            { ".-.-.-", "." },
            { "--..--", "," },
            { "..--..", "?" },
            { ".----.", "'" },
            { "-.-.--", "!" },
            { "-..-.", "/" },
            { "-.--.", "(" },
            { "-.--.-", ")" },
            { ".-...", "&" },
            { "---...", ":" },
            { "-.-.-.", ";" },
            { "-...-", "=" },
            { ".-.-.", "+" },
            { "-....-", "-" },
            { "..--.-", "_" },
            { ".-..-.", "\"" },
            { "...-..-", "$" },
            { ".--.-.", "@" },
            { "...---...", "SOS" },
        };
    }
}

