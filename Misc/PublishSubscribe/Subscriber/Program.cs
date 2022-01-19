using System;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Subscriber
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Write("Enter the queue name: ");
            var queueName = Console.ReadLine();

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
            consumer.Received += (sender, args) =>
            {
                var message = Encoding.UTF8.GetString(args.Body.ToArray());
                Console.WriteLine("Message received. [{0}]: {1}", queueName, message);
            };

            channel.BasicConsume(queueName, true, consumer);

            Console.WriteLine("Subscribed to the queue '{0}'. Press any key to exit.", queueName);
            Console.ReadKey();

            channel.Close();
            connection.Close();
        }
    }
}
