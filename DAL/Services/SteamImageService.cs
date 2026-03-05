using System.Text.RegularExpressions;
using Accord.Math.Distances;
using ApplicationCore.Interfaces;
using ApplicationCore.Utilities;
using craftersmine.SteamGridDBNet;

namespace DAL.Services;

public class ImageService : IImageService
{
    private const string SteamApiKey = "33006ae9737e547251b1cff96e9e6ec9";
    private const string GameImagesDirectory = @"Assets\GameImages";
    
    public async Task<string> GetIconImageUrl(string gameName)
    {
        string filePath = $@"Assets\GameImages\{CleanFileName(gameName)}.jpeg";
        try
        {
            if (File.Exists(filePath))
            {
                return filePath;
            }
            
            using var client = new SteamGridDb(SteamApiKey);
            SteamGridDbGame[]? games = await client.SearchForGamesAsync(gameName);

            // Instantiate the string distance algorithm
            var levenshtein = new Levenshtein();
            var iconSizeThresholds = new[] { 256, 192, 128, 512, 96 };

            foreach (var result in games)
            {
                double distance = levenshtein.Distance(gameName.ToLower(), result.Name.ToLower());

                if (distance <= 3)
                {
                    SteamGridDbIcon[]? icons = await client.GetIconsByGameIdAsync(result.Id);

                    // Use Task.WhenAny to fetch icons in parallel and return the first matching one
                    var iconTasks = icons
                        .Where(hero => iconSizeThresholds.Any(size => hero.Width >= size))
                        .Select(async hero =>
                        {
                            var iconUrl = hero.FullImageUrl;
                            if (!string.IsNullOrEmpty(iconUrl))
                            {
                                return iconUrl;
                            }

                            return null;
                        })
                        .ToList(); // Store the tasks in a list

                    while (iconTasks.Any())
                    {
                        var completedTask = await Task.WhenAny(iconTasks);
                        iconTasks.Remove(completedTask); // Remove the completed task after awaiting it

                        if (completedTask.Result != null)
                        {
                            await DownloadImage(completedTask.Result, filePath);
                            return filePath;
                        }
                    }
                }
            }

            return await GetGridImageUrl(gameName, filePath);
        }
        catch
        {
            return await GetGridImageUrl(gameName, filePath);
        }
    }
    
    public string CleanFileName(string fileName)
    {
        var sb = StringBuilderPool.Rent();
        
        sb.Append(Path.GetInvalidFileNameChars());
        sb.Append(Path.GetInvalidPathChars());

        var escape = Regex.Escape(sb.ToString());

        sb.Clear();

        sb.Append('[');
        sb.Append(escape);
        sb.Append(']');

        string pattern = sb.ToString();
        
        StringBuilderPool.Return(sb);
        return Regex.Replace(fileName, pattern, "_");
    }

    private async Task<string> GetGridImageUrl(string gameName, string filePath)
    {
        try
        {
            using var client = new SteamGridDb(SteamApiKey);
            SteamGridDbGame[]? games = await client.SearchForGamesAsync(gameName);

            // Instantiate the string distance algorithm
            var levenshtein = new Levenshtein();
            var iconSizeThresholds = new[] { 1024, 512 };

            foreach (var result in games)
            {
                double distance = levenshtein.Distance(gameName.ToLower(), result.Name.ToLower());

                if (distance <= 3)
                {
                    SteamGridDbGrid[]? icons = await client.GetGridsByGameIdAsync(result.Id);

                    // Use Task.WhenAny to fetch icons in parallel and return the first matching one
                    var iconTasks = icons
                        .Where(hero => iconSizeThresholds.Any(size => hero.Width == size))
                        .Select(async hero =>
                        {
                            var iconUrl = hero.FullImageUrl;
                            if (!string.IsNullOrEmpty(iconUrl))
                            {
                                return iconUrl;
                            }

                            return null;
                        })
                        .ToList(); // Store the tasks in a list

                    while (iconTasks.Any())
                    {
                        var completedTask = await Task.WhenAny(iconTasks);
                        iconTasks.Remove(completedTask); // Remove the completed task after awaiting it

                        if (completedTask.Result != null)
                        {
                            await DownloadImage(completedTask.Result, filePath);
                            return filePath;
                        }
                    }
                }
            }
        }
        catch
        {
            return filePath;
        }

        return string.Empty;
    }

    private async Task DownloadImage(string url, string filePath)
    {
        using (var client = new HttpClient())
        {
            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            await using (var stream = await response.Content.ReadAsStreamAsync())
            {
                if (!Directory.Exists(GameImagesDirectory))
                {
                    Directory.CreateDirectory(GameImagesDirectory);
                }
                await using (var fileStream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write))
                {
                    await stream.CopyToAsync(fileStream);
                }
            }
        }
    }
}