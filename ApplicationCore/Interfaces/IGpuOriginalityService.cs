using ApplicationCore.Models;

namespace ApplicationCore.Interfaces;

public interface IGpuOriginalityService
{
    public (IEnumerable<CheckIsGpuOriginalResult> results, IEnumerable<string> notFoundNames) CheckIsGpusOriginal();
}