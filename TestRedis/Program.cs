using System;
using System.Threading.Tasks;
using StackExchange.Redis;

class Program
{
    static async Task Main()
    {
        var cs = "hot-squid-178214.upstash.io:6379,password=AcOAAAAAAArgmMAtgcDtA2JA2OGFkMjF1ZDk8MThjYWZlMWQ2MTdlZDk4M2Q87c,ssl=True,abortConnect=False,sslProtocols=tls12";
        try
        {
            Console.WriteLine("Connecting...");
            var muxer = await ConnectionMultiplexer.ConnectAsync(cs);
            var db = muxer.GetDatabase();
            await db.PingAsync();
            Console.WriteLine("SUCCESS!");
        }
        catch (Exception ex)
        {
            Console.WriteLine("FAILED: " + ex.Message);
        }
    }
}
