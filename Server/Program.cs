using System.Net;

namespace Server
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 55555);

            var server = new ChatServer(endPoint);
            await server.Run();
        }
    }
}
