using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using ApplicationCore.Interfaces;
using ApplicationCore.Models;
using ApplicationCore.Utilities;
using DesktopNotifications;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using Universal_x86_Tuning_Utility.Extensions;
using Universal_x86_Tuning_Utility.Properties;

namespace Universal_x86_Tuning_Utility.ViewModels;

public class PremadePresetsViewModel : NotifyPropertyChangedBase
{
    private readonly ILogger<PremadePresetsViewModel> _logger;
    private readonly IPremadePresets _premadePresets;
    private readonly IRyzenAdjService _ryzenAdjService;
    private readonly INotificationManager _notificationManager;
    public ICommand ApplyPresetCommand { get; }

    public PremadePreset? CurrentPreset
    {
        get => _currentPreset;
        set => SetValue(ref _currentPreset, value);
    }

    public EnhancedObservableCollection<PremadePreset> AvailablePresets
    {
        get => _availablePresets;
        set => SetValue(ref _availablePresets, value);
    }

    private PremadePreset? _currentPreset;
    private EnhancedObservableCollection<PremadePreset> _availablePresets;

    public PremadePresetsViewModel(ILogger<PremadePresetsViewModel> logger,
                                   IPremadePresets premadePresets,
                                   IRyzenAdjService ryzenAdjService,
                                   INotificationManager notificationManager)
    {
        _logger = logger;
        _premadePresets = premadePresets;
        _ryzenAdjService = ryzenAdjService;
        _notificationManager = notificationManager;
        
        AvailablePresets = new EnhancedObservableCollection<PremadePreset>(_premadePresets.PremadePresetsList);
        ApplyPresetCommand = ReactiveCommand.CreateFromTask(ApplyPreset);
    }

    private async Task ApplyPreset(CancellationToken cancellationToken)
    {
        if (CurrentPreset == null) return;
        try
        {
            //ReloadValue();

            await _ryzenAdjService.Translate(CurrentPreset.RyzenAdjParameters);

            await _notificationManager.ShowTextNotification(title: $"{CurrentPreset.Name} Preset Applied!",
                text: $"The {CurrentPreset.Name.ToLower()} premade power preset has been applied!",
                cancellationToken: cancellationToken);

            Settings.Default.CommandString = CurrentPreset.RyzenAdjParameters;
            Settings.Default.premadePreset =
                _premadePresets.PremadePresetsList.FindIndex(x => x.Name == CurrentPreset.Name);
            Settings.Default.Save();
        }
        catch (Exception ex)
        {
            await _notificationManager.ShowTextNotification(title: "Error", 
                                                            text: "Error occured while applying preset",
                                                            notificationType: NotificationManagerExtensions.NotificationType.Error,
                                                            cancellationToken: cancellationToken);
            _logger.LogError(ex, "Error while applying preset");
        }
    }

    // private void ReloadValue()
    // {
    //     try
    //     {
    //         if (_systemInfoService.CpuInfo.AmdProcessorType is AmdProcessorType.Apu or AmdProcessorType.Desktop)
    //         {
    //             var cpuName = _systemInfoService.CpuInfo.Name.Replace("AMD", null)
    //                                                          .Replace("with", null)
    //                                                          .Replace("Mobile", null)
    //                                                          .Replace("Ryzen", null)
    //                                                          .Replace("Radeon", null)
    //                                                          .Replace("Graphics", null)
    //                                                          .Replace("Vega", null)
    //                                                          .Replace("Gfx", null);
    //
    //             var productName = _systemInfoService.Product.ToLower();
    //             var manufacturer = _systemInfoService.Manufacturer.ToLower();
    //
    //             if (productName.Contains("laptop 16 (amd ryzen 7040") &&
    //                manufacturer.Contains("framework"))
    //             {
    //                 tbxMessage.Text = "Premade Presets - Framework Laptop 16 (AMD Ryzen 7040HS Series)";
    //                 bdgCertified.Visibility = Visibility.Visible;
    //             }
    //             else if (productName.Contains("laptop 13 (amd ryzen 7040") &&
    //                     manufacturer.Contains("framework"))
    //             {
    //                 tbxMessage.Text = "Premade Presets - Framework Laptop 13 (AMD Ryzen 7040U Series)";
    //                 bdgCertified.Visibility = Visibility.Visible;
    //             }
    //             else bdgCertified.Visibility = Visibility.Collapsed;
    //             
    //             _premadePresets.InitializePremadePresets();
    //
    //             int selectedPreset = Settings.Default.premadePreset;
    //         }
    //     }
    //     catch (Exception ex)
    //     {
    //         _logger.LogError(ex, "Failed to update Premade page");
    //     }
    // }
}