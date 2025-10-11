using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using UnityEditor;
using VRChatContentManagerConnect.Editor.Services;

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

        var provider = services.BuildServiceProvider();
        
        Instance = new ConnectEditorApp(provider);

        EditorApplication.delayCall += () => {
            Task.Run(async () => {
                var appLifetimeService = provider.GetRequiredService<EditorAppLifetimeService>();
                await appLifetimeService.StartAsync();
            });
        };
    }
}