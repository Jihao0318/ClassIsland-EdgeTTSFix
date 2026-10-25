using ClassIsland.Core.Abstractions;
using ClassIsland.Core.Attributes;
using ClassIsland.Core.Extensions.Registry;
using EdgeTTSFix.Controls.SpeechProviderSettingsControls;
using EdgeTTSFix.Services.EdgeTts;
using EdgeTTSFix.Services.SpeechService;
using EdgeTTSFix.Shared;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace EdgeTTSFix;

[PluginEntrance]
public class Plugin : PluginBase
{
    public override void Initialize(HostBuilderContext context, IServiceCollection services)
    {
        services.AddSingleton<EdgeTtsClient>();
        services.AddSpeechProvider<EdgeTtsFixSpeechService, EdgeTtsFixSettingsControl>();
        GlobalConstants.PluginConfigFolder = PluginConfigFolder;
    }
}
