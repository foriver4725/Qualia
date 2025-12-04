namespace MyScripts.Common.Extension;

internal static class CollectionExtension
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void ShuffleSelf<T>(this Span<T> collection)
    {
        int n = collection.Length;
        for (int i = 0; i < n - 1; i++)
        {
            int j = Random.Range(i, n);
            (collection[i], collection[j]) = (collection[j], collection[i]);
        }
    }
}
