using System;
using System.Diagnostics;
using CastelloBranco.AvaloniaMessageBox;
using Microsoft.Extensions.Logging;
using Splat;

namespace Universal_x86_Tuning_Utility.Helpers;

public class RxAppObservableExceptionHandler : IObserver<Exception>
{
    private readonly ILogger<App> _logger;
    
    public RxAppObservableExceptionHandler()
    {
        _logger = Locator.Current.GetService<ILogger<App>>()!;
    }
    
    public void OnNext(Exception value)
    {
        if (Debugger.IsAttached) Debugger.Break();

        _logger.LogError(value, "RxApp unhandled exception");
        
        ExceptionMessageBox.ShowExceptionDialogAsync(null, value);
    }

    public void OnError(Exception error) => OnNext(error);

    public void OnCompleted()
    {
        // Ignored
    }
}