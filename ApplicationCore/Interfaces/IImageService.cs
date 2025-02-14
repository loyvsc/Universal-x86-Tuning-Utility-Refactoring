namespace ApplicationCore.Interfaces;

public interface IImageService
{
    public Task<string> GetIconImageUrl(string gameName);
}