namespace ApplicationCore.Models;

public class DisplayResolution
{
    public int Width { get; }
    public int Height { get; }

    public DisplayResolution(int width, int height)
    {
        Width = width;
        Height = height;
    }

    public override bool Equals(object? obj)
    {
        if (obj is DisplayResolution otherDisplayResolution)
        {
            return Width == otherDisplayResolution.Width && Height == otherDisplayResolution.Height;
        }

        return false;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Width, Height);
    }
}