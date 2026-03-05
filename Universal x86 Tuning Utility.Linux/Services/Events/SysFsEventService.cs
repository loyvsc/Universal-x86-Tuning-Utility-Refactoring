using System;
using System.Collections.Generic;
using System.IO;
using System.Reactive.Subjects;
using System.Runtime.InteropServices;
using Universal_x86_Tuning_Utility.Linux.Interfaces;

namespace Universal_x86_Tuning_Utility.Linux.Services.Events;

public class SysFsEventService : ISysFsEventService, IDisposable
{
    private readonly Dictionary<string, Subject<FileSystemEventArgs>> _observers = new();
    private readonly Dictionary<string, FileSystemWatcher> _eventWatchers = new();

    public IObservable<FileSystemEventArgs> SubscribeToPath(string path)
    {
        ref var observer = ref CollectionsMarshal.GetValueRefOrAddDefault(_observers, path, out var exists);

        if (!exists)
        {
            observer = new Subject<FileSystemEventArgs>();
            var eventWatcher = new FileSystemWatcher(path);

            if (Path.EndsInDirectorySeparator(path))
            {
                eventWatcher.Created += EventWatcherOnEventArrived;
                eventWatcher.Deleted += EventWatcherOnEventArrived;
            }
            else
            {
                eventWatcher.Changed += EventWatcherOnEventArrived;
            }
            
            _eventWatchers.Add(path, eventWatcher);
        }
        
        return observer!;
    }

    private void EventWatcherOnEventArrived(object sender, FileSystemEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(e.Name)) return;
        
        foreach (var keyValuePair in _observers)
        {
            if (keyValuePair.Key.EndsWith(e.Name))
            {
                if (keyValuePair.Value.HasObservers)
                {
                    keyValuePair.Value.OnNext(e);
                }
                else
                {
                    ref var watcher = ref CollectionsMarshal.GetValueRefOrNullRef(_observers, keyValuePair.Key);
                    
                    watcher.Dispose();
                    keyValuePair.Value.Dispose();
                    
                    _observers.Remove(keyValuePair.Key);
                    _eventWatchers.Remove(keyValuePair.Key);
                }
                
                return;
            }
        }
    }

    public void Dispose()
    {
        foreach (var observer in _observers)
        {
            ref var eventWatcher = ref CollectionsMarshal.GetValueRefOrNullRef(_observers, observer.Key);
            
            eventWatcher.Dispose();
            observer.Value.Dispose();
            
            _observers.Remove(observer.Key);
            _eventWatchers.Remove(observer.Key);
        }
    }
}