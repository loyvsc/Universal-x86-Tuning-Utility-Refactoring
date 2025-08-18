using System;
using System.Management;

namespace Universal_x86_Tuning_Utility.Windows.Interfaces;

public interface IManagementEventService
{
    public IObservable<EventArrivedEventArgs> SubscribeToQuery(string query);
}