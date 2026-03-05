using System.Text.RegularExpressions;
using ApplicationCore.Interfaces;
using ApplicationCore.Models;
using Serilog;
using SQLite;

namespace DAL.Services;

public class GpuSpecsService : IGpuSpecsService, IDisposable
{
    private readonly ILogger _logger;
    private readonly SQLiteConnection _connection;

    public GpuSpecsService(ILogger logger)
    {
        _logger = logger;
        _connection = new SQLiteConnection("GpuSpecs.db");
    }
    
    public IEnumerable<GpuSpecs> GetGpuSpecs(string gpuName)
    {
        try
        {
            var normalizedGpuName = NormalizeGpuName(gpuName);
            
            return _connection.Query<GpuSpecs>(
                @"SELECT g.* FROM GpuSpecs g 
                          WHERE g.Id IN (
                              SELECT rowid FROM GpuSearch 
                              WHERE GpuSearch MATCH ?
                          )",
                $"{normalizedGpuName}");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error occurred when querying gpu specifications");
        }
        
        return [];
    }
    
    private string NormalizeGpuName(string gpuName)
    {
        if (string.IsNullOrEmpty(gpuName))
            return gpuName;
        
        var normalized = gpuName.ToLowerInvariant();
        
        normalized = normalized
            .Replace("-", " ")
            .Replace("_", " ")
            .Replace("/", " ")
            .Replace("\\", " ")
            .Replace("(", " ")
            .Replace(")", " ");
        
        normalized = Regex.Replace(normalized, @"\s+", " ").Trim();
        
        normalized = normalized
            .Replace("geforce ", string.Empty)
            .Replace("radeon ", string.Empty)
            .Replace("nvidia ", string.Empty)
            .Replace("amd ", string.Empty);
            
        return normalized;
    }

    public void Dispose()
    {
        _connection.Dispose();
    }
}