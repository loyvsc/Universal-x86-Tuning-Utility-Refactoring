using ApplicationCore.Models;

namespace ApplicationCore.Interfaces;

public interface IGpuSpecsService
{
    public IEnumerable<GpuSpecs> GetGpuSpecs(string gpuName);
}