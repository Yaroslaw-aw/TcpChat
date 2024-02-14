using System.Net;
using System.Net.Sockets;

namespace Server
{
    internal class ChatServer
    {
        TcpListener listener;

        HashSet<TcpClient> senders = new HashSet<TcpClient>(); // добавляет только уникальных пользователей

        public ChatServer(IPEndPoint endPoint)
        {
            listener = new TcpListener(endPoint);
        }

        /// <summary>
        /// Запуск сервера
        /// </summary>
        /// <returns></returns>
        public async Task Run()
        {
            try
            {
                listener.Start();

                await Console.Out.WriteLineAsync("Запущен");

                while (true)
                {
                    TcpClient? tcpClient = await listener.AcceptTcpClientAsync();

                    senders.Add(tcpClient);

                    Console.WriteLine("Успешно подключен");

                    Task entry = Task.Run(() => ProcessClient(tcpClient));
                }
            }
            catch (Exception ex)
            {
                await Console.Out.WriteLineAsync(ex.Message);
            }
        }

        /// <summary>
        /// Прослушивание входящих и отправка всем остальным клиентам сообщений
        /// </summary>
        /// <param name="client">присоединяющийся клиент</param>
        /// <returns></returns>
        public async Task ProcessClient(TcpClient client)
        {
            try
            {
                using var reader = new StreamReader(client.GetStream());
                string? message;

                while (!string.IsNullOrEmpty(message = await reader.ReadLineAsync())) // если сообщение не пустое
                {
                    Console.WriteLine($"Получено сообщение: {message}");

                    foreach (var sender in senders)
                    {
                        if (sender != client)
                        {
                            try
                            {
                                // Создаем StreamWriter для отправки, не сохраняя его
                                var writer = new StreamWriter(sender.GetStream(), leaveOpen: true);
                                await writer.WriteLineAsync(message);
                                await writer.FlushAsync(); // Убедимся, что сообщение отправлено немедленно
                            }
                            catch
                            {
                                // Обработка ошибок отправки
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при обработке клиента: {ex.Message}");
            }
            finally
            {
                senders.Remove(client); // высвобождаем ресурсы
                client.Close();
            }
        }
    }
}