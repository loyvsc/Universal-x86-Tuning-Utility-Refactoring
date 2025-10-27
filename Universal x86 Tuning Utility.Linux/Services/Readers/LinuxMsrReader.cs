using System;
using System.IO;

namespace Universal_x86_Tuning_Utility.Linux.Services.Readers;

public class LinuxMsrReader : IDisposable
{
    private FileStream? _stream;
    private const string Path = "/dev/cpu/{0}/msr";
    private readonly byte[] _buffer = new byte[8];

    public bool TryOpen(int cpuId = 0)
    {
        try
        {
            if (_stream == null || !_stream.CanRead)
            {
                _stream = new FileStream(string.Format(Path, cpuId), FileMode.Open, FileAccess.Read);
            }
            return true;
        }
        catch
        {
            return false;
        }
    }

    public uint Read(ulong msrAddr)
    {
        if (_stream == null)
            throw new Exception("Stream not opened");
        
        _stream.Seek((long)msrAddr, SeekOrigin.Begin);

        try
        {
            int read = _stream.Read(_buffer, 0, 8);
            if (read != 8)
                throw new InvalidDataException();
            return BitConverter.ToUInt32(_buffer, 0);
        }
        finally
        {
            Array.Clear(_buffer);
        }
    }
    
    public void Dispose()
    {
        _stream?.Dispose();
    }
}