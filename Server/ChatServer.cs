using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;

namespace Server
{
    internal class ChatServer
    {
        TcpListener listener;

        HashSet<TcpClient> senders = new HashSet<TcpClient>(); // добавляет только уникальных пользователей

        public ChatServer(IPEndPoint? endPoint)
        {
            listener = new TcpListener(endPoint);
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
                listener.Start();

                Console.Out.WriteLineAsync("Запущен");

                while (true)
                {
                    TcpClient? tcpClient = listener.AcceptTcpClient();

                    senders.Add(tcpClient);

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
                Console.Out.WriteLineAsync(ex.Message);
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

                        foreach (var sender in senders)
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
                finally
                {
                    senders.Remove(client); // высвобождаем ресурсы
                    client.Close();
                }
            }

            void Dispose() // У меня вопрос: правильный ли вообще этот метод?
            {
                listener.Stop();
                listener.Dispose();
                GC.SuppressFinalize(this);
            }
        }
    }
}
