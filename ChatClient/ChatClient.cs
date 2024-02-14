using System.Net.Sockets;
using System.Net;

namespace Client
{
    internal class ChatClient
    {
        private TcpClient client;
        private IPEndPoint endPoint;

        public ChatClient(IPEndPoint endPoint)
        {
            this.client = new TcpClient();
            this.endPoint = endPoint;
        }

        /// <summary>
        /// Запуск клиента
        /// </summary>
        /// <returns></returns>
        public async Task Run()
        {
            try
            {
                await client.ConnectAsync(endPoint); // Асинхронное подключение
                Console.WriteLine("Соединён");

                var stream = client.GetStream();
                var writer = new StreamWriter(stream);
                var reader = new StreamReader(stream);

                var receiveTask = ReceiveMessages(reader); // Задача для получения сообщений

                // Отправка сообщений в цикле
                while (true)
                {
                    string? message = Console.ReadLine();
                    if (string.IsNullOrEmpty(message)) continue; // Если ввод пустой, делаем вид, что ничего не произошло

                    await writer.WriteLineAsync(message);
                    await writer.FlushAsync();
                }

                //client.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Прослушивание сообщений от сервера
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        private async Task ReceiveMessages(StreamReader reader)
        {
            try
            {
                string? message;
                while (!string.IsNullOrEmpty(message = await reader.ReadLineAsync())) // если сообщение не пустое
                {
                    Console.WriteLine(message);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при получении сообщений: {ex.Message}");
            }
        }
    }

}
