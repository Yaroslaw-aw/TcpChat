using System.Net;

namespace Server
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            AutoResetEvent stopper = new AutoResetEvent(false);

            IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 55555);
            ChatServer tcpServer = new ChatServer(endPoint);

            RegisteredWaitHandle handle = ThreadPool.RegisterWaitForSingleObject(stopper, tcpServer.Run, null, Timeout.Infinite, true);

            string operation = "S";

            Console.WriteLine("Q - выход");

            while (true)
            {
                if (operation == "S")
                {
                    stopper.Set();
                }
                if (operation == "Q" || operation == "Й")
                {
                    handle.Unregister(stopper);
                    break;
                }

                operation = Console.ReadKey(true).KeyChar.ToString().ToUpper();
            }
        }
    }
}
