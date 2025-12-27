using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using UnityEditor;
using VRChatContentPublisherConnect.Editor.Services;
using VRChatContentPublisherConnect.Editor.Services.Rpc;

namespace VRChatContentPublisherConnect.Editor;

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
        services.AddSingleton<MenuItemService>();

        var provider = services.BuildServiceProvider();

        Instance = new ConnectEditorApp(provider);

        EditorApplication.delayCall += () => {
            Task.Run(async () => {
                var appLifetimeService = provider.GetRequiredService<EditorAppLifetimeService>();
                await appLifetimeService.StartAsync();
            });
        };

    #if !VCCM_WORLDS_PACKAGE_EXIST && !VCCM_AVATARS_PACKAGE_EXIST
        EditorUtility.DisplayDialog("VRChat Content Publisher Connect",
            "Warning: Required VRChat Content Publisher packages are missing.\n" +
            "Please ensure the 'VRChat Content Publisher Connect - Worlds' or 'VRChat Content Publisher Connect - Avatars' packages are installed.",
            "OK");
    #endif
    }
}