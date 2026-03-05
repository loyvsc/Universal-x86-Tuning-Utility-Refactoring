using System;
using System.Runtime.InteropServices;

namespace Universal_x86_Tuning_Utility.Linux.Services.GPUs.Native;

internal static class AmdSmiNative
{
    private const string LIB_AMD_SMI_X64 = "librocm_smi64.so";

    [DllImport(LIB_AMD_SMI_X64)]
    internal static extern int amdsmi_init(uint flags);

    [DllImport(LIB_AMD_SMI_X64)]
    internal static extern int amdsmi_shut_down();
    
    [DllImport(LIB_AMD_SMI_X64)]
    internal static extern int amdsmi_get_socket_handles(out uint num_devices, IntPtr socket_handles);
    
    [DllImport(LIB_AMD_SMI_X64)]
    internal static extern int amdsmi_get_processor_handles_by_type(IntPtr socket_handle, int processor_type, IntPtr processor_handles, uint processor_count);
    
    [DllImport(LIB_AMD_SMI_X64)]
    internal static extern int amdsmi_get_gpu_metrics_info(IntPtr processor_handle, IntPtr pgpu_metrics);
}

public static class AmiSmiWrapper
{
    private static void CheckRet(int libStatus)
    {
        if ((LibStatus)libStatus != LibStatus.AMDSMI_STATUS_SUCCESS)
            throw new Exception("AMISMI returned error: " + libStatus);
    }
    
    public static LibStatus Initialize(InitFlags initFlags = InitFlags.AMDSMI_INIT_AMD_APUS)
    {
        return (LibStatus)AmdSmiNative.amdsmi_init((uint)initFlags);
    }

    public static LibStatus Free()
    {
        return (LibStatus)AmdSmiNative.amdsmi_shut_down();
    }

    public static GpuMetricsInfo GetGpuMetrics(int gpuId)
    {
        var sockets = GetProcessorHandles();

        var ret = AmdSmiNative.amdsmi_get_processor_handles_by_type(socket_handle: sockets[gpuId], 
            processor_type: (int) ProcessorType.AMDSMI_PROCESSOR_TYPE_AMD_GPU,
            processor_handles: Marshal.UnsafeAddrOfPinnedArrayElement(sockets, 0), 
            processor_count: (uint) sockets.Length);
        CheckRet(ret);

        if (sockets.Length < gpuId)
        {
            var gpuMetricsInfo = new GpuMetricsInfo();
            var gpuMetricsInfoPtr = Marshal.AllocHGlobal(Marshal.SizeOf<GpuMetricsInfo>());
            Marshal.StructureToPtr(gpuMetricsInfo, gpuMetricsInfoPtr, false);

            CheckRet(AmdSmiNative.amdsmi_get_gpu_metrics_info(sockets[gpuId], gpuMetricsInfoPtr));
            Marshal.FreeHGlobal(gpuMetricsInfoPtr);
            
            return gpuMetricsInfo;
        }
        
        throw new Exception($"Invalid {nameof(gpuId)}: {gpuId}");
    }
    
    private static IntPtr[] GetProcessorHandles()
    {
        IntPtr[] handles = new IntPtr[10];
        if (AmdSmiNative.amdsmi_get_socket_handles(out var deviceCount, Marshal.UnsafeAddrOfPinnedArrayElement(handles, 0)) != 0)
            return Array.Empty<IntPtr>();

        return handles;
    }
}

internal static class Sizes
{
    public const int AMDSMI_MAX_NUM_XCC = 8;
    public const int AMDSMI_MAX_NUM_JPEG = 8 * 4;
    public const int AMDSMI_MAX_NUM_JPEG_ENG_V1 = 40;
    public const int AMDSMI_MAX_NUM_VCN = 4;
    public const int AMDSMI_MAX_NUM_XGMI_LINKS = 8;
    public const int AMDSMI_MAX_NUM_GFX_CLKS = 8;
    public const int AMDSMI_MAX_NUM_CLKS = 4;
    public const int AMDSMI_MAX_NUM_XCP = 8;
}

public enum LibStatus : uint
{
    AMDSMI_STATUS_SUCCESS = 0, //!< Call succeeded

    // Library usage errors
    AMDSMI_STATUS_INVAL = 1, //!< Invalid parameters
    AMDSMI_STATUS_NOT_SUPPORTED = 2, //!< Command not supported
    AMDSMI_STATUS_NOT_YET_IMPLEMENTED = 3, //!< Not implemented yet
    AMDSMI_STATUS_FAIL_LOAD_MODULE = 4, //!< Fail to load lib
    AMDSMI_STATUS_FAIL_LOAD_SYMBOL = 5, //!< Fail to load symbol
    AMDSMI_STATUS_DRM_ERROR = 6, //!< Error when call libdrm
    AMDSMI_STATUS_API_FAILED = 7, //!< API call failed
    AMDSMI_STATUS_TIMEOUT = 8, //!< Timeout in API call
    AMDSMI_STATUS_RETRY = 9, //!< Retry operation
    AMDSMI_STATUS_NO_PERM = 10, //!< Permission Denied
    AMDSMI_STATUS_INTERRUPT = 11, //!< An interrupt occurred during execution of function
    AMDSMI_STATUS_IO = 12, //!< I/O Error
    AMDSMI_STATUS_ADDRESS_FAULT = 13, //!< Bad address
    AMDSMI_STATUS_FILE_ERROR = 14, //!< Problem accessing a file
    AMDSMI_STATUS_OUT_OF_RESOURCES = 15, //!< Not enough memory
    AMDSMI_STATUS_INTERNAL_EXCEPTION = 16, //!< An internal exception was caught
    AMDSMI_STATUS_INPUT_OUT_OF_BOUNDS = 17, //!< The provided input is out of allowable or safe range
    AMDSMI_STATUS_INIT_ERROR = 18, //!< An error occurred when initializing internal data structures
    AMDSMI_STATUS_REFCOUNT_OVERFLOW = 19, //!< An internal reference counter exceeded INT32_MAX
    AMDSMI_STATUS_DIRECTORY_NOT_FOUND = 20, //!< Error when a directory is not found, maps to ENOTDIR

    // Processor related errors
    AMDSMI_STATUS_BUSY = 30, //!< Processor busy
    AMDSMI_STATUS_NOT_FOUND = 31, //!< Processor Not found
    AMDSMI_STATUS_NOT_INIT = 32, //!< Processor not initialized
    AMDSMI_STATUS_NO_SLOT = 33, //!< No more free slot
    AMDSMI_STATUS_DRIVER_NOT_LOADED = 34, //!< Processor driver not loaded

    // Data and size errors
    AMDSMI_STATUS_MORE_DATA = 39, //!< There is more data than the buffer size the user passed
    AMDSMI_STATUS_NO_DATA = 40, //!< No data was found for a given input
    AMDSMI_STATUS_INSUFFICIENT_SIZE = 41, //!< Not enough resources were available for the operation
    AMDSMI_STATUS_UNEXPECTED_SIZE = 42, //!< An unexpected amount of data was read
    AMDSMI_STATUS_UNEXPECTED_DATA = 43, //!< The data read or provided to function is not what was expected

    //esmi errors
    AMDSMI_STATUS_NON_AMD_CPU = 44, //!< System has different cpu than AMD
    AMDSMI_STATUS_NO_ENERGY_DRV = 45, //!< Energy driver not found
    AMDSMI_STATUS_NO_MSR_DRV = 46, //!< MSR driver not found
    AMDSMI_STATUS_NO_HSMP_DRV = 47, //!< HSMP driver not found
    AMDSMI_STATUS_NO_HSMP_SUP = 48, //!< HSMP not supported
    AMDSMI_STATUS_NO_HSMP_MSG_SUP = 49, //!< HSMP message/feature not supported
    AMDSMI_STATUS_HSMP_TIMEOUT = 50, //!< HSMP message timed out
    AMDSMI_STATUS_NO_DRV = 51, //!< No Energy and HSMP driver present
    AMDSMI_STATUS_FILE_NOT_FOUND = 52, //!< file or directory not found
    AMDSMI_STATUS_ARG_PTR_NULL = 53, //!< Parsed argument is invalid
    AMDSMI_STATUS_AMDGPU_RESTART_ERR = 54, //!< AMDGPU restart failed
    AMDSMI_STATUS_SETTING_UNAVAILABLE = 55, //!< Setting is not available
    AMDSMI_STATUS_CORRUPTED_EEPROM = 56, //!< EEPROM is corrupted

    // General errors
    AMDSMI_STATUS_MAP_ERROR = 0xFFFFFFFE, //!< The internal library error did not map to a status code
    AMDSMI_STATUS_UNKNOWN_ERROR = 0xFFFFFFFF //!< An unknown error occurred
}

public enum InitFlags : uint
{
    AMDSMI_INIT_ALL_PROCESSORS = 0xFFFFFFFF, // Initialize all processors
    AMDSMI_INIT_AMD_CPUS = 1 << 0, // Initialize AMD CPUS
    AMDSMI_INIT_AMD_GPUS = 1 << 1, // Initialize AMD GPUS
    AMDSMI_INIT_NON_AMD_CPUS = 1 << 2, // Initialize Non-AMD CPUS
    AMDSMI_INIT_NON_AMD_GPUS = 1 << 3, // Initialize Non-AMD GPUS
    AMDSMI_INIT_AMD_APUS = AMDSMI_INIT_AMD_CPUS | AMDSMI_INIT_AMD_GPUS // Initialize AMD CPUS and GPUS (Default option)
}

[StructLayout(LayoutKind.Sequential)]
public struct GpuMetricsInfo
{
    public ushort temperature_edge;
    public ushort temperature_hotspot;
    public ushort temperature_mem;
    public ushort temperature_vrgfx;
    public ushort temperature_vrsoc;
    public ushort temperature_vrmem;
    public ushort average_gfx_activity;
    public ushort average_umc_activity;
    public ushort average_mm_activity;
    public ushort current_gfxclk;
    public ushort current_socclk;
    public ushort current_uclk;
    public ushort current_vclk0;
    public ushort current_dclk0;
    public ushort current_vclk1;
    public ushort current_dclk1;
    public uint throttle_status;
    public ushort current_fan_speed;
    public ushort pcie_link_width;
    public ushort pcie_link_speed;
    public uint gfx_activity_acc;
    public uint mem_activity_acc;
    public ushort temperature_hbm;
    public ulong firmware_timestamp;
    public ushort voltage_soc;
    public ushort voltage_gfx;
    public ushort voltage_mem;
    public ulong indep_throttle_status;
    public ushort current_socket_power;
    public ushort vcn_activity;
    public uint gfxclk_lock_status;
    public ushort xgmi_link_width;
    public ushort xgmi_link_speed;
    public ulong pcie_bandwidth_acc;
    public ulong pcie_bandwidth_inst;
    public ulong pcie_l0_to_recov_count_acc;
    public ulong pcie_replay_count_acc;
    public ulong pcie_replay_rover_count_acc;
    public ulong[] xgmi_read_data_acc;
    public ulong[] xgmi_write_data_acc;
    public ushort[] current_gfxclks;
    public ushort[] current_socclks;
    public ushort[] current_vclk0s;
    public ushort[] current_dclk0s;
    public ushort[] jpeg_activity;
    public uint pcie_nak_sent_count_acc;
    public uint pcie_nak_rcvd_count_acc;
    public ulong accumulation_counter;
    public ulong prochot_residency_acc;
    public ulong ppt_residency_acc;
    public ulong socket_thm_residency_acc;
    public ulong vr_thm_residency_acc;
    public ulong hbm_thm_residency_acc;
    public ulong num_partition;
    public GpuXcpMetrics[] xcp_stats;
    public uint pcie_lc_perf_other_end_recovery;
    public ulong vram_max_bandwidth;
    public ushort[] xgmi_link_status;
}

[StructLayout(LayoutKind.Sequential)]
public struct GpuXcpMetrics
{
    /**
     * @brief v1.6 additions
     * The max uint32_t will be used if that information is N/A
     */
    public uint[] gfx_busy_inst;      //!< Utilization Instantaneous in %
    public ushort[] jpeg_busy;  //!< Utilization Instantaneous in % (UPDATED: to 40 in v1.8)
    public ushort[] vcn_busy;           //!< Utilization Instantaneous in %
    public ulong[] gfx_busy_acc;       //!< Utilization Accumulated in %

    /**
     * @brief v1.7 additions
     */
    /* Total App Clock Counter Accumulated */
    public ulong[] gfx_below_host_limit_acc; //!< Total App Clock Counter Accumulated

    /**
     * @brief v1.8 additions
     */
    /* Total App Clock Counter Accumulated */
    public ulong[] gfx_below_host_limit_ppt_acc;
    public ulong[] gfx_below_host_limit_thm_acc;
    public ulong[] gfx_low_utilization_acc;
    public ulong[] gfx_below_host_limit_total_acc;
}

public enum ProcessorType
{
    AMDSMI_PROCESSOR_TYPE_UNKNOWN = 0,  //!< Unknown processor type
    AMDSMI_PROCESSOR_TYPE_AMD_GPU,      //!< AMD Graphics processor type
    AMDSMI_PROCESSOR_TYPE_AMD_CPU,      //!< AMD CPU processor type
    AMDSMI_PROCESSOR_TYPE_NON_AMD_GPU,  //!< Non-AMD Graphics processor type
    AMDSMI_PROCESSOR_TYPE_NON_AMD_CPU,  //!< Non-AMD CPU processor type
    AMDSMI_PROCESSOR_TYPE_AMD_CPU_CORE, //!< AMD CPU-Core processor type
    AMDSMI_PROCESSOR_TYPE_AMD_APU
}