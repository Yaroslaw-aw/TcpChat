using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Reflection;
using System.Text;

namespace Server
{
    internal class ChatServer : IDisposable
    {
        TcpListener? listener;

        CuncurrentHashSet<TcpClient> clients; // добавляет только уникальных пользователей

        public ChatServer(IPEndPoint? endPoint)
        {
            if (endPoint != null)
                listener = new TcpListener(endPoint);

            clients = new CuncurrentHashSet<TcpClient>();
        }        

        /// <summary>
        /// Запуск сервера
        /// </summary>
        /// <returns></returns>
        public void Run(object? state, bool timeOut)
        {
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

                        Task entry = ProcessClient(tcpClient);

                        Console.WriteLine($"Клиент {tcpClient.GetHashCode()} Успешно подключен");

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
        }

        /// <summary>
        /// Прослушивание входящих и отправка всем остальным клиентам сообщений
        /// </summary>
        /// <param name="producer">присоединяющийся клиент</param>
        /// <returns></returns>
        async Task ProcessClient(TcpClient producer)
        {
            try
            {
                using var reader = new StreamReader(producer.GetStream());

                string? message;

                while (!string.IsNullOrEmpty(message = await reader.ReadLineAsync())) // если сообщение не пустое
                {
                    Console.WriteLine($"{message}");

                    foreach (var consumer in clients)
                    {
                        // Создаем StreamWriter для отправки, не сохраняя его
                        using var writer = new StreamWriter(consumer.GetStream(), leaveOpen: true);

                        if (consumer != producer)
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
                    clients.Remove(producer);
                }
                producer.GetStream().Close();
                producer.Close();
            }
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
    }
}
