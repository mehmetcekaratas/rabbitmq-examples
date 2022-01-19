using System;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Consumer
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

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (s, e) =>
            {
                var message = Encoding.UTF8.GetString(e.Body.ToArray());
                Console.WriteLine("Message received: {0}", message);
            };

            channel.BasicConsume("log.info", true, consumer);
            channel.BasicConsume("log.error", true, consumer);
            channel.BasicConsume("log.warning", true, consumer);

            Console.WriteLine("Waiting for messages... Press any key to exit.");
            Console.ReadKey();
        }
    }
}
