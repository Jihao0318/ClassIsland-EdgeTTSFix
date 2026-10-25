using System.ComponentModel;
using Avalonia.Interactivity;
using ClassIsland.Core.Abstractions.Controls;
using ClassIsland.Shared.Helpers;
using EdgeTTSFix.Models;
using EdgeTTSFix.Services.SpeechService;
using EdgeTTSFix.Shared;

namespace EdgeTTSFix.Controls.SpeechProviderSettingsControls;

public partial class EdgeTtsFixSettingsControl : SpeechProviderControlBase
{
    private static string SettingsPath =>
        Path.Combine(GlobalConstants.PluginConfigFolder, "Settings.json");

    public EdgeTtsFixSettings Settings { get; private set; } = new();

    public IReadOnlyList<string> VoiceOptions => EdgeTtsFixSettings.VoiceOptions;

    public IReadOnlyList<string> RateOptions => EdgeTtsFixSettings.RateOptions;

    public IReadOnlyList<string> VolumeOptions => EdgeTtsFixSettings.VolumeOptions;

    public IReadOnlyList<string> PitchOptions => EdgeTtsFixSettings.PitchOptions;

    public string CacheFolderPath => EdgeTtsFixSpeechService.CacheFolderPath;

    public EdgeTtsFixSettingsControl()
    {
        InitializeComponent();
        LoadSettings();
    }

    private void LoadSettings()
    {
        Settings = ConfigureFileHelper.LoadConfig<EdgeTtsFixSettings>(SettingsPath);
        EdgeTtsFixSettings.Normalize(Settings);
        Settings.PropertyChanged += SettingsOnPropertyChanged;
        DataContext = this;
    }

    private void SettingsOnPropertyChanged(object? sender, PropertyChangedEventArgs args)
    {
        ConfigureFileHelper.SaveConfig(SettingsPath, Settings);
    }

    private void ClearCache_Click(object? sender, RoutedEventArgs args)
    {
        try
        {
            if (Directory.Exists(CacheFolderPath))
            {
                Directory.Delete(CacheFolderPath, true);
            }
        }
        catch
        {
            // 缓存目录可能正被占用，忽略清理失败。
        }
    }
}
