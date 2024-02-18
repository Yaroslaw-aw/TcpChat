using System.Net;

namespace Server
{
    internal class Program
    {
        static void Main(string[] args)
        {
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 55555);
            ChatServer tcpServer = new ChatServer(endPoint);

            AutoResetEvent stopper = new AutoResetEvent(false);
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
