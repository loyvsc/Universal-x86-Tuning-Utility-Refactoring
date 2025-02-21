namespace ApplicationCore.Interfaces;

public interface IRtssService
{
    /// <summary>
    /// 0 - fps limit is disabled
    /// </summary>
    public int FpsLimit { get; set; }

    public void Start();
    public void Stop();
    public bool IsRTSSRunning();
}