namespace ApplicationCore.Interfaces;

public interface INavigationService
{
    public void Navigate(Type type);
    public void NavigateFromContext(object dataContext);
}