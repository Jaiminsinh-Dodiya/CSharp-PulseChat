using Microsoft.AspNet.SignalR;
using Microsoft.Owin.Cors;
using Owin;

namespace PulseChatServer
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // Enable CORS so clients from different processes can connect
            app.UseCors(CorsOptions.AllowAll);

            // Increase limits for image transfer (default WebSocket max is only 64KB)
            GlobalHost.Configuration.MaxIncomingWebSocketMessageSize = 10 * 1024 * 1024; // 10 MB
            GlobalHost.Configuration.DefaultMessageBufferSize = 500;

            // Configure SignalR
            var hubConfiguration = new HubConfiguration
            {
                EnableDetailedErrors = true,
                EnableJSONP = true
            };

            // Map SignalR hubs to /signalr endpoint
            app.MapSignalR(hubConfiguration);
        }
    }
}
