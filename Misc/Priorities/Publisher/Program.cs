using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using RabbitMQ.Client;

namespace Publisher
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

            channel.ExchangeDeclare("ex.fanout", "fanout", false, true);

            channel.QueueDeclare("queue.priorities", false, false, true, new Dictionary<string, object>
{
    {"x-max-priority", 2}
});

            channel.QueueBind("queue.priorities", "ex.fanout", string.Empty);

            SendMessageWithPriority(channel, "This a test message.", 1);
            SendMessageWithPriority(channel, "This a test message.", 1);
            SendMessageWithPriority(channel, "This a test message.", 1);

            SendMessageWithPriority(channel, "This a test message.", 2);
            SendMessageWithPriority(channel, "This a test message.", 2);

            Console.Write("Press any key to exit.");
            Console.ReadKey();

            channel.QueueDelete("queue.priorities");
            channel.ExchangeDelete("ex.fanout");

            channel.Close();
            connection.Close();
        }

        private static void SendMessageWithPriority(IModel channel, string message, byte priority)
        {
            var basicProperties = channel.CreateBasicProperties();
            basicProperties.Priority = priority;
            channel.BasicPublish("ex.fanout", string.Empty, basicProperties, Encoding.UTF8.GetBytes(message));
            Console.WriteLine("Message '{0}' sent with {1} priority.", message, priority);
        }
    }
}
