using System.Text.RegularExpressions;
using Accord.Math.Distances;
using ApplicationCore.Interfaces;
using craftersmine.SteamGridDBNet;

namespace DAL.Services;

public class ImageService : IImageService
{
    public async Task<string> GetIconImageUrl(string gameName)
    {
        string filePath = $@"\Assets\GameImages\{CleanFileName(gameName)}.jpeg";
        try
        {
            var client = new SteamGridDb("33006ae9737e547251b1cff96e9e6ec9");
            SteamGridDbGame[]? games = await client.SearchForGamesAsync(gameName);

            // Instantiate the string distance algorithm
            var levenshtein = new Levenshtein();
            var iconSizeThresholds = new[] { 256, 192, 128, 512, 1024, 96 };

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
        string illegalChars = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
        string pattern = "[" + Regex.Escape(illegalChars) + "]";
        return Regex.Replace(fileName, pattern, "_");
    }

    private async Task<string> GetGridImageUrl(string gameName, string filePath)
    {
        try
        {
            var client = new SteamGridDb("33006ae9737e547251b1cff96e9e6ec9");
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
                await using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                {
                    await stream.CopyToAsync(fileStream);
                }
            }
        }
    }
}