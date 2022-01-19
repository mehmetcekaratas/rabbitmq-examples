using System;
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

            channel.QueueDeclare("queue.one", false, false, true);
            channel.QueueDeclare("queue.two", false, false, true);

            channel.QueueBind("queue.one", "ex.fanout", string.Empty);
            channel.QueueBind("queue.two", "ex.fanout", string.Empty);

            while (true)
            {
                Console.Write("Enter message to be sent: ");
                var message = Console.ReadLine();

                if (message.ToLower() == "exit")
                {
                    break;
                }

                channel.BasicPublish("ex.fanout", string.Empty, null, Encoding.UTF8.GetBytes(message));
            }

            channel.Close();
            connection.Close();
        }
    }
}
