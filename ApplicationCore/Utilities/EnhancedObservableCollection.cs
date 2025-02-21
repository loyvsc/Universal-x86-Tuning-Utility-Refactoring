using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace ApplicationCore.Utilities;

public class EnhancedObservableCollection<T> : ObservableCollection<T>
{
    public EnhancedObservableCollection()
    {
        
    }
    
    public EnhancedObservableCollection(IEnumerable<T> collection) : base(collection)
    {
        
    }
    
    public virtual void AddRange(IEnumerable<T> items)
    {
        ((List<T>)items).AddRange(items);
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add));
    }
}