using System;
using Universal_x86_Tuning_Utility.Extensions;
using MsBox.Avalonia;
using Splat;
using ILogger = Serilog.ILogger;

namespace Universal_x86_Tuning_Utility.Helpers;

public class RxAppObservableExceptionHandler : IObserver<Exception>
{
    private readonly ILogger _logger;
    
    public RxAppObservableExceptionHandler()
    {
        _logger = Locator.Current.GetService<ILogger>()!;
    }
    
    public void OnNext(Exception value)
    {
        _logger.Fatal(value, "RxApp unhandled exception");

        MessageBoxManager.GetMessageBoxStandard("Error", value.ToString())
            .ShowDialogAsync();
    }

    public void OnError(Exception error) => OnNext(error);

    public void OnCompleted()
    {
        // Ignored
    }
}