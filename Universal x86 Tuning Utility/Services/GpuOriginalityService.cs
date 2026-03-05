using System.Collections.Generic;
using System.Linq;
using ApplicationCore.Interfaces;
using ApplicationCore.Models;

namespace Universal_x86_Tuning_Utility.Services;

public class GpuOriginalityService : IGpuOriginalityService
{
    private readonly IGpuSpecsService _gpuSpecsService;
    private readonly INvidiaGpuService _nvidiaGpuService;

    public GpuOriginalityService(IGpuSpecsService gpuSpecsService, INvidiaGpuService nvidiaGpuService)
    {
        _gpuSpecsService = gpuSpecsService;
        _nvidiaGpuService = nvidiaGpuService;
    }
    
    public (IEnumerable<CheckIsGpuOriginalResult> results, IEnumerable<string> notFoundNames) CheckIsGpusOriginal()
    {
        var results = new List<CheckIsGpuOriginalResult>();
        var notFoundNames = new List<string>();
        
        var data = _nvidiaGpuService.Gpus.ToList();
        
        for (int i = 0; i < data.Count; i++)
        {
            var gpu = data[i];
            var expectedSpecs = _gpuSpecsService.GetGpuSpecs(gpu.Name).ToList();

            if (expectedSpecs.Count == 1)
            {
                var expectedSpec = expectedSpecs[0];
                results.Add(new CheckIsGpuOriginalResult()
                {
                    GpuName = gpu.Name,
                    GpuNumber = i + 1,
                    IsGpuOriginal = gpu.RopCount == expectedSpec.RopCount &&
                                    gpu.TmusCount == expectedSpec.TmusCount &&
                                    gpu.ShadersCount == expectedSpec.ShadersCount &&
                                    gpu.MemorySize == expectedSpec.MemorySize,
                });
            }
            else if (expectedSpecs.Count > 1)
            {
                bool isValid = false;
                foreach (var expectedSpec in expectedSpecs)
                {
                    if (gpu.RopCount == expectedSpec.RopCount &&
                        gpu.TmusCount == expectedSpec.TmusCount &&
                        gpu.ShadersCount == expectedSpec.ShadersCount &&
                        gpu.MemorySize == expectedSpec.MemorySize)
                    {
                        results.Add(new CheckIsGpuOriginalResult()
                        {
                            GpuName = gpu.Name,
                            GpuNumber = i + 1,
                            IsGpuOriginal = true
                        });
                        isValid = true;
                        break;
                    }
                }

                if (!isValid)
                {
                    results.Add(new CheckIsGpuOriginalResult()
                    {
                        GpuName = gpu.Name,
                        GpuNumber = i + 1,
                        IsGpuOriginal = false
                    });
                }
            }
            else
            {
                notFoundNames.Add(gpu.Name);
            }
        }
        
        return (results, notFoundNames);
    }
}