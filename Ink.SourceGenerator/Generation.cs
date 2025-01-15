namespace Ink.SourceGenerator;

public static class Generation
{
    public static string ToPascalCase(string value)
    {
        ReadOnlySpan<char> path = value.StartsWith("minecraft:") ? value.AsSpan()["minecraft:".Length..] : value.AsSpan();
        Span<char> pascalPath = stackalloc char[path.Length]; // TODO? Allocate with ArrayPool<char/byte>?
        _ = path.TryCopyTo(pascalPath);

        pascalPath[0] = char.ToUpperInvariant(pascalPath[0]);
        for (int i = 1; i < pascalPath.Length; ++i)
        {
            if (i + 1 >= path.Length)
                break;

            char currentChar = pascalPath[i];
            char nextChar = pascalPath[i + 1];

            if (!char.IsLetter(currentChar))
                pascalPath[i + 1] = char.ToUpperInvariant(nextChar);
        }

        pascalPath.Replace('_', '\0');
        pascalPath.Replace('/', '_');
        pascalPath.Replace('.', '_');

        int lengthToRemove = 0;
        int index = pascalPath.IndexOf('\0');
        while (index != -1)
        {
            ++lengthToRemove;
            for (int i = index; i < pascalPath.Length; ++i)
            {
                if (i + 1 >= pascalPath.Length)
                    break;

                char nextChar = pascalPath[i + 1];
                pascalPath[i] = nextChar;
            }

            index = pascalPath.IndexOf('\0');
        }

        return new string(pascalPath[..^lengthToRemove]);
    }
}
