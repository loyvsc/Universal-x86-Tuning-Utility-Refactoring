using System;
using System.IO;

namespace Universal_x86_Tuning_Utility.Linux.Interfaces;

public interface ISysFsEventService
{
    public IObservable<FileSystemEventArgs> SubscribeToPath(string path);
}