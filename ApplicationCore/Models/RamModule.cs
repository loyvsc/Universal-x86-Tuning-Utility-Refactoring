namespace ApplicationCore.Models;

public class RamModule
{
    public string Producer { get; set; }
    public string Model { get; set; }
    /// <summary>
    /// In gigabytes
    /// </summary>
    public double Capacity { get; set; }
    public int Speed { get; set; }
}