using System;
using System.Diagnostics;
using CastelloBranco.AvaloniaMessageBox;
using Splat;
using ILogger = Serilog.ILogger;

namespace Universal_x86_Tuning_Utility.Helpers;

public class RxAppObservableExceptionHandler : IObserver<Exception>, IEnableLogger
{
    private readonly ILogger _logger;
    
    public RxAppObservableExceptionHandler()
    {
        _logger = Locator.Current.GetService<ILogger>()!;
    }
    
    public void OnNext(Exception value)
    {
        if (Debugger.IsAttached) Debugger.Break();

        _logger.Fatal(value, "RxApp unhandled exception");
        
        ExceptionMessageBox.ShowExceptionDialogAsync(null, value);
    }

    public void OnError(Exception error) => OnNext(error);

    public void OnCompleted()
    {
        // Ignored
    }
}