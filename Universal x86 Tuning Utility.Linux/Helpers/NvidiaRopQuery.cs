using System;
using System.Runtime.InteropServices;

namespace Universal_x86_Tuning_Utility.Linux.Helpers;

unsafe class NvidiaRopQuery
{
    // ===== libc bindings =====
    private const string LIBC = "libc";

    [DllImport(LIBC, SetLastError = true)]
    private static extern int openat(int dirfd, string pathname, int flags);

    [DllImport(LIBC, SetLastError = true)]
    private static extern int ioctl(int fd, ulong request, void* argp);

    // ===== private constants =====
    private const int AT_FDCWD = -100;
    private const int O_RDWR = 0x0002;
    private const int O_CLOEXEC = 0x80000;

    private const byte NV_IOCTL_MAGIC = (byte)'F';
    private const uint NV_IOCTL_BASE = 200;
    private const uint NV_ESC_REGISTER_FD = NV_IOCTL_BASE + 1;
    private const uint NV_ESC_RM_CONTROL = 0x2A;
    private const uint NV_ESC_RM_ALLOC = 0x2B;

    private const uint NV01_DEVICE_0 = 0x80;
    private const uint NV20_SUBDEVICE_0 = 0x2080;

    private const int CMD_SUBDEVICE_CTRL_GR_GET_ROP_INFO = 0x20801213;

    // ===== ioctl helpers =====
    private static ulong _IOC(uint dir, uint type, uint nr, uint size)
    {
        return (dir << 30) | (type << 8) | (nr << 0) | (size << 16);
    }

    private const uint _IOC_READ = 2;
    private const uint _IOC_WRITE = 1;

    // ===== Structs =====
    [StructLayout(LayoutKind.Sequential)]
    private struct NVOS21_PARAMETERS
    {
        public uint hRoot;            
        public uint hObjectParent;    
        public uint hObjectNew;       
        public int hClass;            
        public IntPtr pAllocParms;    // pointer to allocation parameters
        public uint paramsSize;    
        public int status;            
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct NVOS64_PARAMETERS
    {
        public uint hRoot;            
        public uint hObjectParent;    
        public uint hObjectNew;       
        public int hClass;            
        public IntPtr pAllocParms;    // pointer to allocation parameters
        public IntPtr pRightsRequested;  // pointer to rights requested
        public uint paramsSize;    
        public uint flags;         
        public int status;            
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct NVOS54_PARAMETERS
    {
        public uint hClient;          
        public uint hObject;          
        public int cmd;               
        public uint flags;         
        public IntPtr paramsPtr;      // pointer to parameters
        public uint paramsSize;    
        public int status;            
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct NV0080_ALLOC_PARAMETERS
    {
        public uint deviceId;      
        public uint hClientShare;     
        public uint hTargetClient;    
        public uint hTargetDevice;    
        public int flags;             
        public ulong vaSpaceSize;     
        public ulong vaStartInternal; 
        public ulong vaLimitInternal; 
        public int vaMode;            
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct NV2080_ALLOC_PARAMETERS
    {
        public uint subDeviceId;   
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct NV2080_CTRL_GR_GET_ROP_INFO_PARAMS
    {
        public uint ropUnitCount;      
        public uint ropOperationsFactor;
        public uint ropOperationsCount;
    }
    
    /// <exception cref="Exception">Throws on initialization fails</exception>
    public static uint GetRopCount(uint deviceHandle, uint subDeviceHandle)
    {
        int nvidiactl = openat(AT_FDCWD, "/dev/nvidiactl", O_RDWR);
        if (nvidiactl < 0)
            throw new Exception("Failed to open /dev/nvidiactl");

        NVOS21_PARAMETERS clientReq = new();
        ulong alloc21 = _IOC(_IOC_READ | _IOC_WRITE, NV_IOCTL_MAGIC, NV_ESC_RM_ALLOC, (uint)Marshal.SizeOf<NVOS21_PARAMETERS>());
        
        // Allocate client and check status
        if (ioctl(nvidiactl, alloc21, &clientReq) != 0 || clientReq.status != 0)
            throw new Exception("Failed to allocate client");

        uint hClient = clientReq.hObjectNew;

        int nvidia0 = openat(AT_FDCWD, "/dev/nvidia0", O_RDWR | O_CLOEXEC);
        if (nvidia0 < 0)
            throw new Exception("Failed to open /dev/nvidia0");

        // Register file descriptor for the opened nvidia device
        ioctl(nvidia0, _IOC(_IOC_READ | _IOC_WRITE, NV_IOCTL_MAGIC, NV_ESC_REGISTER_FD, (uint)sizeof(int)), &nvidiactl);

        uint hDevice = deviceHandle; // Assuming device handle (could be different)
        NV0080_ALLOC_PARAMETERS devParams = new();

        NVOS64_PARAMETERS devReq = new()
        {
            hRoot = hClient,
            hObjectParent = hClient,
            hObjectNew = hDevice,
            hClass = (int)NV01_DEVICE_0,
            pAllocParms = Marshal.AllocHGlobal(Marshal.SizeOf<NV0080_ALLOC_PARAMETERS>()),
            paramsSize = 0
        };

        Marshal.StructureToPtr(devParams, devReq.pAllocParms, false);
        ulong alloc64 = _IOC(_IOC_READ | _IOC_WRITE, NV_IOCTL_MAGIC, NV_ESC_RM_ALLOC, (uint)Marshal.SizeOf<NVOS64_PARAMETERS>());
        ioctl(nvidiactl, alloc64, &devReq);

        uint hSubDevice = subDeviceHandle; // Assuming subdevice handle (could be different)
        NV2080_ALLOC_PARAMETERS subParams = new();

        devReq.hObjectParent = hDevice;
        devReq.hObjectNew = hSubDevice;
        devReq.hClass = (int)NV20_SUBDEVICE_0;
        devReq.pAllocParms = Marshal.AllocHGlobal(Marshal.SizeOf<NV2080_ALLOC_PARAMETERS>());
        Marshal.StructureToPtr(subParams, devReq.pAllocParms, false);

        ioctl(nvidiactl, alloc64, &devReq);

        NV2080_CTRL_GR_GET_ROP_INFO_PARAMS rop = new();
        IntPtr ropPtr = Marshal.AllocHGlobal(Marshal.SizeOf<NV2080_CTRL_GR_GET_ROP_INFO_PARAMS>());
        Marshal.StructureToPtr(rop, ropPtr, false);

        NVOS54_PARAMETERS ctrl = new()
        {
            hClient = hClient,
            hObject = hSubDevice,
            cmd = CMD_SUBDEVICE_CTRL_GR_GET_ROP_INFO,
            paramsPtr = ropPtr,
            paramsSize = (uint)Marshal.SizeOf<NV2080_CTRL_GR_GET_ROP_INFO_PARAMS>()
        };

        ulong ctrlIoctl = _IOC(_IOC_READ | _IOC_WRITE, NV_IOCTL_MAGIC, NV_ESC_RM_CONTROL, (uint)Marshal.SizeOf<NVOS54_PARAMETERS>());
        ioctl(nvidiactl, ctrlIoctl, &ctrl);

        rop = Marshal.PtrToStructure<NV2080_CTRL_GR_GET_ROP_INFO_PARAMS>(ropPtr);

        return rop.ropUnitCount;
    }
}