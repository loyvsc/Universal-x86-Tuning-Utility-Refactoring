namespace ApplicationCore.Interfaces;

public interface IIconExtractor
{
    public Task<string> ExtractIcon(string pathToExecutable, string directory);
    public Task<string> ExtractIcon(Stream dataStream, string name, string directory);
}