using System;
using System.Collections.Generic;
using System.Management;
using System.Reactive.Subjects;
using System.Runtime.InteropServices;
using Universal_x86_Tuning_Utility.Windows.Interfaces;

namespace Universal_x86_Tuning_Utility.Windows.Services;

public class ManagementEventService : IManagementEventService, IDisposable
{
    private readonly Dictionary<string, Subject<EventArrivedEventArgs>> _observers = new();
    private readonly Dictionary<string, ManagementEventWatcher> _eventWatchers = new();

    public IObservable<EventArrivedEventArgs> SubscribeToQuery(string query)
    {
        ref var observer = ref CollectionsMarshal.GetValueRefOrAddDefault(_observers, query, out var exists);
        
        if (!exists)
        {
            observer = new Subject<EventArrivedEventArgs>();
            var eventWatcher = new ManagementEventWatcher(query);
            eventWatcher.EventArrived += EventWatcherOnEventArrived;
            eventWatcher.Start();
            
            _eventWatchers.Add(query, eventWatcher);
        }
        
        return observer!;
    }

    private void EventWatcherOnEventArrived(object sender, EventArrivedEventArgs e)
    {
        if (sender is ManagementEventWatcher watcher)
        {
            var query = watcher.Query.QueryString;
            ref var observer = ref CollectionsMarshal.GetValueRefOrNullRef(_observers, query);
            if (observer != null)
            {
                if (observer.HasObservers)
                {
                    observer.OnNext(e);
                }
                else
                {
                    watcher.EventArrived -= EventWatcherOnEventArrived;
                    watcher.Stop();
                    watcher.Dispose();
                    observer.Dispose();
                    
                    _observers.Remove(query);
                    _eventWatchers.Remove(query);
                }
            }
        }
    }

    public void Dispose()
    {
        foreach (var observer in _observers)
        {
            var eventWatcher = _eventWatchers[observer.Key];
            
            eventWatcher.EventArrived -= EventWatcherOnEventArrived;
            eventWatcher.Stop();
            eventWatcher.Dispose();
            observer.Value.Dispose();
            
            _observers.Remove(observer.Key);
            _eventWatchers.Remove(observer.Key);
        }
    }
}