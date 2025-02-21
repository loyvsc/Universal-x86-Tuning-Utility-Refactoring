namespace ApplicationCore.Interfaces;

public interface IImageService
{
    public Task<string> GetIconImageUrl(string gameName);
    public string CleanFileName(string fileName);
}