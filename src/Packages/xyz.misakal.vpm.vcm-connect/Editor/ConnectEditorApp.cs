using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using UnityEditor;
using VRChatContentManagerConnect.Editor.Services;
using VRChatContentManagerConnect.Editor.Services.Rpc;

namespace VRChatContentManagerConnect.Editor;

internal class ConnectEditorApp {
    internal static ConnectEditorApp? Instance { get; private set; }

    internal ServiceProvider ServiceProvider { get; private set; }

    public ConnectEditorApp(ServiceProvider serviceProvider) {
        ServiceProvider = serviceProvider;
    }

    [InitializeOnLoadMethod]
    private static void Initialize() {
        var services = new ServiceCollection();

        services.AddTransient<IRpcClientSessionProvider, AppRpcClientSessionProvider>();
        services.AddTransient<IRpcClientIdProvider, AppRpcClientIdProvider>();
        services.AddSingleton<RpcClientService>();
        services.AddSingleton<EditorAppLifetimeService>();
        services.AddSingleton<AppSettingsService>();

        var provider = services.BuildServiceProvider();

        Instance = new ConnectEditorApp(provider);

        EditorApplication.delayCall += () => {
            Task.Run(async () => {
                var appLifetimeService = provider.GetRequiredService<EditorAppLifetimeService>();
                await appLifetimeService.StartAsync();
            });
        };

    #if !VCCM_WORLDS_PACKAGE_EXIST && !VCCM_AVATARS_PACKAGE_EXIST
        EditorUtility.DisplayDialog("VRChat Content Manager Connect",
            "Warning: Required VRChat Content Manager packages are missing.\n" +
            "Please ensure the 'VRChat Content Manager Connect - Worlds' or 'VRChat Content Manager Connect - Avatars' packages are installed.",
            "OK");
    #endif
    }
}