using System;
using System.Text;
using System.Threading;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Worker
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

            channel.BasicQos(0, 1, false);

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (s, e) =>
            {
                var message = Encoding.UTF8.GetString(e.Body.ToArray());
                Console.WriteLine("Message received: {0}", message);
                Thread.Sleep(1000);
                channel.BasicAck(e.DeliveryTag, false);
            };

            var consumerTag = channel.BasicConsume("queue.priorities", false, consumer);

            Console.WriteLine("Subscribed to the queue. Press any key to exit.");
            Console.ReadKey();

            channel.BasicCancel(consumerTag);

            channel.Close();
            connection.Close();
        }
    }
}
