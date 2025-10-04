using ApplicationCore.Utilities;

namespace Universal_x86_Tuning_Utility.Helpers;

public static class SizesConverter
{
    public static string ToString(double size)
    {
        var sb = StringBuilderPool.Rent();
        
        if (size < 1024)
        {
            sb.Append(size);
            sb.Append(" KB");
            return sb.ToString();
        }

        sb.Append((size / 1024).ToString("0.##"));
        sb.Append(" MB");
        var value = sb.ToString();
        StringBuilderPool.Return(sb);
        return value;
    }
}