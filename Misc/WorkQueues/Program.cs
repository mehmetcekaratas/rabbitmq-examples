using System;
using System.Text;
using System.Threading;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace WorkQueues
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Write("Enter the name for this worker: ");
            var workerName = Console.ReadLine().Trim();

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
                int.TryParse(Encoding.UTF8.GetString(e.Body.ToArray()), out int durationInSeconds);
                Console.Write("[{0}] task will be competed after {1} second(s). ", workerName, durationInSeconds);
                Thread.Sleep(durationInSeconds * 1000);

                channel.BasicAck(e.DeliveryTag, false);
                Console.WriteLine("Completed.");
            };

            channel.BasicConsume("queue.ack", false, consumer);

            Console.WriteLine("Waiting for messages... Press any key to exit.");
            Console.ReadKey();

            channel.Close();
            connection.Close();
        }
    }
}
