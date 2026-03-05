using System.Text;

namespace ApplicationCore.Utilities;

public static class StringBuilderPool
{
    private static readonly Queue<StringBuilder> Pool = new Queue<StringBuilder>();

    static StringBuilderPool()
    {
        Pool.Enqueue(new StringBuilder());
        Pool.Enqueue(new StringBuilder());
        Pool.Enqueue(new StringBuilder());
        Pool.Enqueue(new StringBuilder());
        Pool.Enqueue(new StringBuilder());
    }

    public static StringBuilder Rent()
    {
        if (!Pool.TryDequeue(out var sb))
        {
            sb = new StringBuilder();
        }

        return sb;
    }

    public static void Return(StringBuilder sb)
    {
        sb.Clear();
        Pool.Enqueue(sb);
    }
}