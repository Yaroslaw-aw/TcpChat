using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;

namespace Server
{
    internal class ChatServer : IDisposable
    {
        TcpListener? listener;

        CuncurrentHashSet<TcpClient> clients = new CuncurrentHashSet<TcpClient>(); // добавляет только уникальных пользователей

        public ChatServer(IPEndPoint? endPoint)
        {
            if (endPoint != null)
                listener = new TcpListener(endPoint);
        }

        public void Dispose()
        {
            if (listener != null)
            {
                listener.Stop();
                listener.Server.Dispose();
            }
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Запуск сервера
        /// </summary>
        /// <returns></returns>
        public void Run(object? state, bool timeOut)
        {
            Console.Out.WriteLineAsync("Сервер\n" + new string('-', 6));
            try
            {
                if (listener != null)
                    listener.Start();

                Console.Out.WriteLineAsync("Запущен");

                if (listener != null)
                    while (true)
                    {

                        TcpClient? tcpClient = listener.AcceptTcpClient();

                        clients.Add(tcpClient);

                        Task entry = Task.Run(() => ProcessClient(tcpClient));

                        Console.WriteLine("Успешно подключен");

                        using (StreamWriter writer = new StreamWriter(tcpClient.GetStream(), leaveOpen: true))
                        {
                            writer.WriteLineAsync("Сервер: соединение установленно");
                            writer.FlushAsync();
                        };
                    }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                Dispose();
            }

            /// <summary>
            /// Прослушивание входящих и отправка всем остальным клиентам сообщений
            /// </summary>
            /// <param name="client">присоединяющийся клиент</param>
            /// <returns></returns>
            async Task ProcessClient(TcpClient client)
            {
                try
                {
                    using var reader = new StreamReader(client.GetStream());

                    string? message;

                    while (!string.IsNullOrEmpty(message = await reader.ReadLineAsync())) // если сообщение не пустое
                    {
                        Console.WriteLine($"{message}");

                        foreach (var sender in clients)
                        {
                            // Создаем StreamWriter для отправки, не сохраняя его
                            using var writer = new StreamWriter(sender.GetStream(), leaveOpen: true);

                            if (sender != client)
                            {
                                try
                                {
                                    await writer.WriteLineAsync(message);
                                    await writer.FlushAsync(); // Убедимся, что сообщение отправлено немедленно
                                }
                                catch
                                {
                                    // Обработка ошибок отправки
                                }
                            }
                            else
                            {
                                await writer.WriteLineAsync("Сообщение доставлено.");
                                await writer.FlushAsync();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при обработке клиента: {ex.Message}");
                }
                finally // высвобождаем ресурсы
                {
                    lock (clients)
                    {
                        clients.Remove(client);
                    }
                    client.GetStream().Close();
                    client.Close();
                }
            }
        }
    }
}
