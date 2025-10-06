using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using ApplicationCore.Enums;
using ApplicationCore.Interfaces;
using Avalonia.Threading;
using DesktopNotifications;
using ReactiveUI;
using Universal_x86_Tuning_Utility.Extensions;
using Universal_x86_Tuning_Utility.Interfaces;

namespace Universal_x86_Tuning_Utility.ViewModels;

public class FanControlViewModel : ReactiveObject
{
    public ICommand EnableFanControlCommand { get; }
    public ICommand DisableFanControlCommand { get; }
    public ICommand ReloadCommand { get; }
    public ICommand TestFanCurveCommand { get; }
    public ICommand SetFanSpeedCommand { get; }
    public ICommand CopyCommand { get; }
    
    public string ConfigName
    {
        get => _configName;
        set => this.RaiseAndSetIfChanged(ref _configName, value);
    }
    
    public string Status
    {
        get => _status;
        set => this.RaiseAndSetIfChanged(ref _status, value);
    }
    
    public bool IsFanControlEnabled
    {
        get => _isFanControlEnabled;
        set => this.RaiseAndSetIfChanged(ref _isFanControlEnabled, value);
    }

    public decimal FanSpeed
    {
        get => _fanSpeed;
        set => this.RaiseAndSetIfChanged(ref _fanSpeed, value);
    }

    private string _configName;
    private string _status;
    private bool _isFanControlEnabled;
    private decimal _fanSpeed;
    
    private readonly DispatcherTimer _timer;
    
    private readonly Serilog.ILogger _logger;
    private readonly IFanControlService _fanControlService;
    private readonly ISystemInfoService _systemInfoService;
    private readonly INotificationManager _notificationManager;
    private readonly ISensorsService _sensorsService;
    private readonly IPlatformServiceAccessor _platformServiceAccessor;

    public FanControlViewModel(Serilog.ILogger logger,
                               IFanControlService fanControlService,
                               ISystemInfoService systemInfoService,
                               INotificationManager notificationManager,
                               ISensorsService sensorsService,
                               IPlatformServiceAccessor platformServiceAccessor)
    {
        _logger = logger;
        _fanControlService = fanControlService;
        _systemInfoService = systemInfoService;
        _notificationManager = notificationManager;
        _sensorsService = sensorsService;
        _platformServiceAccessor = platformServiceAccessor;

        ReloadCommand = ReactiveCommand.CreateFromTask(Reload);
        TestFanCurveCommand = ReactiveCommand.CreateFromTask(TestFanCurve);
        EnableFanControlCommand = ReactiveCommand.CreateFromTask(EnableFanControl);
        DisableFanControlCommand = ReactiveCommand.CreateFromTask(DisableFanControl);
        SetFanSpeedCommand = ReactiveCommand.CreateFromTask(SetFanSpeed);
        CopyCommand = ReactiveCommand.CreateFromTask(Copy);

        FanSpeed = 50;
        ConfigName = $"{_systemInfoService.Manufacturer.Value.ToUpper()}_{_systemInfoService.Product.Value.ToUpper()}.json";
        
        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(2.5)
        };
        _timer.Tick += Timer_Tick;
        _fanControlService.UpdateAddresses();
    }

    private async Task Copy()
    {
        await _platformServiceAccessor.Clipboard.SetTextAsync(_configName);
    }

    private async Task SetFanSpeed()
    {
        _fanControlService.SetFanSpeed((int)FanSpeed);
    }

    private async Task EnableFanControl()
    {
        try
        {
            _fanControlService.EnableFanControl();
            IsFanControlEnabled = true;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to enable FanControl");
            await _notificationManager.ShowTextNotification("Failed to enable FanControl", $"Failed to enable FanControl: {ex.Message}");
        }
    }

    private async Task DisableFanControl()
    {
        try
        {
            _timer.Stop();
            Status = "Disabled";

            _fanControlService.DisableFanControl();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to disable FanControl");
            await _notificationManager.ShowTextNotification("Failed to disable FanControl", $"Failed to disable FanControl: {ex.Message}");
        }
    }

    private async Task Reload()
    {
        try
        {
            _fanControlService.UpdateAddresses();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to reload");
            await _notificationManager.ShowTextNotification("Failed to reload", $"Failed to reload: {ex.Message}");
        }
    }
    
    private async Task TestFanCurve()
    {
        if (_timer.IsEnabled)
        {
            _timer.Stop();
            Status = "Disabled";
        }
        else
        {
            _timer.Start();
        }
    }
    
    private int Interpolate(int[] yValues, int[] xValues, int x)
    {
        int i = Array.FindIndex(xValues, t => t >= x);

        return i switch
        {
            -1 or 0 => yValues[0], // temperature is lower than the first input point
            _ => i == xValues.Length 
                ? yValues[xValues.Length - 1] // temperature is higher than the last input point
                : Interpolate(yValues[i - 1], xValues[i - 1], yValues[i], xValues[i], x) // interpolate between two closest input points
        };
    }

    private int Interpolate(int y1, int x1, int y2, int x2, int x)
    {
        return (y1 * (x2 - x) + y2 * (x - x1)) / (x2 - x1);
    }

    private async void Timer_Tick(object? sender, EventArgs e)
    {
        try
        {
            int[] temps = { 25, 35, 45, 55, 65, 75, 85, 95 };
            int[] speeds = { 0, 5, 15, 25, 40, 55, 70, 100 };

            int cpuTemperature = await GetCpuTemperature();

            var fanSpeed = Interpolate(speeds, temps, cpuTemperature);

            if (_fanControlService.IsFanControlEnabled)
            {
                _fanControlService.SetFanSpeed(fanSpeed);
            }

            Status = $"Enabled - {fanSpeed}% - {cpuTemperature}°C";
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to get cpu temperature");
            await _notificationManager.ShowTextNotification("Error occurred", "Failed to get cpu temperature", NotificationManagerExtensions.NotificationType.Error);
        }
    }

    private async Task<int> GetCpuTemperature()
    {
        try
        {
            return (int) _sensorsService.GetCPUInfo(SensorType.Temperature, _systemInfoService.Cpu.Manufacturer == Manufacturer.Intel ? "Package" : "Core");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to get cpu temperature");
            await _notificationManager.ShowTextNotification("Error occurred", "Failed to get cpu temperature", NotificationManagerExtensions.NotificationType.Error);
            return 0;
        }
    }
}