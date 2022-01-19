using System;
using System.Text;
using System.Threading;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace PushPull
{
    class Program
    {
        static void Main(string[] args)
        {
            var factory = new ConnectionFactory()
            {
                HostName = "localhost",
                VirtualHost = "/",
                Port = 5672,
                UserName = "guest",
                Password = "guest"
            };
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            // Push Model
            //ReadMessagesViaPushModel(channel); 

            // Pull Model
            ReadMessagesViaPullModel(channel);

            channel.Close();
            connection.Close();
        }

        private static void ReadMessagesViaPushModel(IModel channel)
        {
            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (s, e) =>
            {
                var message = Encoding.UTF8.GetString(e.Body.ToArray());
                Console.WriteLine("Message received: {0}", message);
            };

            var consumerTag = channel.BasicConsume("queue.one", true, consumer);

            Console.WriteLine("Subsribed. Press any key to unsubscribe.");
            Console.ReadKey();

            channel.BasicCancel(consumerTag);
        }

        private static void ReadMessagesViaPullModel(IModel channel)
        {
            Console.WriteLine("Reading messages from queue via pull model. Press [e] to exit.");

            while (true)
            {
                var result = channel.BasicGet("queue.one", true);
                if (result != null)
                {
                    var message = Encoding.UTF8.GetString(result.Body.ToArray());
                    Console.WriteLine("Message received: {0}", message);
                }
                if (Console.KeyAvailable)
                {
                    var keyInfo = Console.ReadKey();
                    if (keyInfo.KeyChar == 'e' || keyInfo.KeyChar == 'E')
                    {
                        return;
                    }
                }
                Thread.Sleep(5000);
            }
        }
    }
}
