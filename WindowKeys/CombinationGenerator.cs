namespace WindowKeys;

internal static class CombinationGenerator
{
    public static List<string> Generate(int count, char[] letters)
    {
        if (count == 0) return [];
        var length = Math.Max(1, (int)Math.Ceiling(Math.Log(count) / Math.Log(letters.Length)));

        var current = new char[length];
        var results = new List<string>();
        Fill(current, 0, results, count, letters);
        return results;
    }

    private static void Fill(char[] current, int position, ICollection<string> results, int desired, char[] letters)
    {
        if (results.Count == desired) return;
        if (position == current.Length)
        {
            results.Add(new string(current));
            return;
        }
        foreach (var l in letters)
        {
            current[position] = l;
            Fill(current, position + 1, results, desired, letters);
        }
    }
}
