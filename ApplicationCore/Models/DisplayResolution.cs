using System.Collections.ObjectModel;

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
}