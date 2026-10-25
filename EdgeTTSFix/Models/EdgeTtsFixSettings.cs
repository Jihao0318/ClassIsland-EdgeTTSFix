using CommunityToolkit.Mvvm.ComponentModel;

namespace EdgeTTSFix.Models;

public class EdgeTtsFixSettings : ObservableRecipient
{
    public const string DefaultVoice = "zh-CN-XiaoxiaoNeural";
    public const string DefaultRate = "+0%";
    public const string DefaultVolume = "+0%";
    public const string DefaultPitch = "+0Hz";

    public static readonly IReadOnlyList<string> VoiceOptions =
    [
        "zh-CN-XiaoxiaoNeural",
        "zh-CN-XiaoyiNeural",
        "zh-CN-YunjianNeural",
        "zh-CN-YunxiNeural",
        "zh-CN-YunxiaNeural",
        "zh-CN-YunyangNeural",
        "zh-CN-liaoning-XiaobeiNeural",
        "zh-CN-shaanxi-XiaoniNeural",
        "zh-HK-HiuGaaiNeural",
        "zh-HK-HiuMaanNeural",
        "zh-HK-WanLungNeural",
        "zh-TW-HsiaoChenNeural",
        "zh-TW-HsiaoYuNeural",
        "zh-TW-YunJheNeural",
        "en-US-AriaNeural",
        "en-US-GuyNeural",
        "en-US-JennyNeural",
        "en-US-AvaNeural",
        "en-US-AndrewNeural",
        "en-US-EmmaNeural",
        "en-US-BrianNeural",
        "en-US-ChristopherNeural",
        "en-US-EricNeural",
        "en-US-MichelleNeural",
        "en-US-RogerNeural",
        "en-US-SteffanNeural",
        "ja-JP-NanamiNeural",
        "ja-JP-KeitaNeural",
        "ko-KR-SunHiNeural",
        "ko-KR-InJoonNeural"
    ];

    public static readonly IReadOnlyList<string> RateOptions =
    [
        "-50%", "-30%", "-20%", "-10%", "+0%", "+10%", "+20%", "+30%", "+50%", "+100%"
    ];

    public static readonly IReadOnlyList<string> VolumeOptions =
    [
        "-50%", "-20%", "+0%", "+20%", "+50%"
    ];

    public static readonly IReadOnlyList<string> PitchOptions =
    [
        "-50Hz", "-20Hz", "+0Hz", "+20Hz", "+50Hz"
    ];

    private string _voice = DefaultVoice;
    private string _rate = DefaultRate;
    private string _volume = DefaultVolume;
    private string _pitch = DefaultPitch;

    public string Voice
    {
        get => _voice;
        set => SetProperty(ref _voice, value);
    }

    public string Rate
    {
        get => _rate;
        set => SetProperty(ref _rate, value);
    }

    public string Volume
    {
        get => _volume;
        set => SetProperty(ref _volume, value);
    }

    public string Pitch
    {
        get => _pitch;
        set => SetProperty(ref _pitch, value);
    }

    public static void Normalize(EdgeTtsFixSettings settings)
    {
        if (string.IsNullOrWhiteSpace(settings.Voice))
        {
            settings.Voice = DefaultVoice;
        }

        if (string.IsNullOrWhiteSpace(settings.Rate))
        {
            settings.Rate = DefaultRate;
        }

        if (string.IsNullOrWhiteSpace(settings.Volume))
        {
            settings.Volume = DefaultVolume;
        }

        if (string.IsNullOrWhiteSpace(settings.Pitch))
        {
            settings.Pitch = DefaultPitch;
        }
    }
}
