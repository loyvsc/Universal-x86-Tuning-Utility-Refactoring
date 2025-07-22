using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using ApplicationCore.Enums;
using ApplicationCore.Interfaces;
using ApplicationCore.Models;
using ApplicationCore.Models.LaptopInfo;
using ApplicationCore.Utilities;
using DesktopNotifications;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using Universal_x86_Tuning_Utility.Extensions;
using Universal_x86_Tuning_Utility.Properties;

namespace Universal_x86_Tuning_Utility.ViewModels;

public class PremadePresetsViewModel : ReactiveObject
{
    private readonly ILogger<PremadePresetsViewModel> _logger;
    private readonly ISystemInfoService _systemInfoService;
    private readonly IPremadePresets _premadePresets;
    private readonly IRyzenAdjService _ryzenAdjService;
    private readonly INotificationManager _notificationManager;
    public ICommand ApplyPresetCommand { get; }

    public string Header
    {
        get => _header;
        set => this.RaiseAndSetIfChanged(ref _header, value);
    }

    public bool IsCertifiedBadgeVisible
    {
        get => _isCertifiedBadgeVisible;
        set => this.RaiseAndSetIfChanged(ref _isCertifiedBadgeVisible, value);
    }

    public PremadePreset? CurrentPreset
    {
        get => _currentPreset;
        set => this.RaiseAndSetIfChanged(ref _currentPreset, value);
    }

    public EnhancedObservableCollection<PremadePreset> AvailablePresets
    {
        get => _availablePresets;
        set => this.RaiseAndSetIfChanged(ref _availablePresets, value);
    }

    private PremadePreset? _currentPreset;
    private EnhancedObservableCollection<PremadePreset> _availablePresets;
    private string _header;
    private bool _isCertifiedBadgeVisible;

    public PremadePresetsViewModel(ILogger<PremadePresetsViewModel> logger,
                                   ISystemInfoService systemInfoService,
                                   IPremadePresets premadePresets,
                                   IRyzenAdjService ryzenAdjService,
                                   INotificationManager notificationManager)
    {
        _logger = logger;
        _systemInfoService = systemInfoService;
        _premadePresets = premadePresets;
        _ryzenAdjService = ryzenAdjService;
        _notificationManager = notificationManager;
        
        AvailablePresets = new EnhancedObservableCollection<PremadePreset>(_premadePresets.PremadePresetsList);
        ApplyPresetCommand = ReactiveCommand.CreateFromTask(ApplyPreset);

        Header = "Premade Presets";
    }

    private async Task ApplyPreset(CancellationToken cancellationToken)
    {
        if (CurrentPreset == null) return;
        try
        {
            ReloadValue();

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

    private void ReloadValue()
    {
        try
        {
            if (_systemInfoService.Cpu.ProcessorType is ProcessorType.Apu or ProcessorType.Desktop)
            {
                if (_systemInfoService.LaptopInfo is FrameworkLaptopInfo frameworkLaptopInfo)
                {
                    if (frameworkLaptopInfo.CpuSeries == "7040")
                    {
                        if (frameworkLaptopInfo.LaptopSeries == 16)
                        {
                            Header = "Premade Presets - Framework Laptop 16 (AMD Ryzen 7040HS Series)";
                            IsCertifiedBadgeVisible = true;
                        }
                        else if (frameworkLaptopInfo.LaptopSeries == 13)
                        {
                            Header = "Premade Presets - Framework Laptop 13 (AMD Ryzen 7040U Series)";
                            IsCertifiedBadgeVisible = true;
                        }
                    }
                }
                else
                {
                    IsCertifiedBadgeVisible = false;
                }
                
                _premadePresets.InitializePremadePresets();
    
                int selectedPreset = Settings.Default.premadePreset;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update Premade page");
        }
    }
}