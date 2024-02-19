﻿using System.Net;

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

            Console.WriteLine($"Main : {Thread.CurrentThread.ManagedThreadId}");

            stopper.Set();

            Console.WriteLine("Сервер\n");

            Console.WriteLine("Q - остановка сервера\n" + new string('-', 21));

            while (true)
            {
                string operation = Console.ReadKey(true).KeyChar.ToString().ToUpper();

                if (operation == "Q" || operation == "Й")
                {
                    handle.Unregister(stopper);
                    break;
                }
            }
        }
    }
}
