using System.Net.Sockets;

namespace SumireCraft.Maui.UITests;

internal static class AppiumServer
{
    public static Uri Uri { get; } = new("http://127.0.0.1:4723/");

    public static string StartHint =>
        $"No Appium server answered at {Uri}. Start one in another terminal with: appium --address 127.0.0.1";

    public static bool IsListening
    {
        get
        {
            try
            {
                using var client = new TcpClient();

                // Short on purpose. A server that is running answers a local connection
                // immediately.
                return client.ConnectAsync(Uri.Host, Uri.Port).Wait(TimeSpan.FromSeconds(2));
            }
            catch (SocketException)
            {
                return false;
            }
            catch (AggregateException)
            {
                return false;
            }
        }
    }
}
