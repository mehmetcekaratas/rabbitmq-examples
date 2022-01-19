using System;
using System.Text;
using RabbitMQ.Client;

namespace ExchangeToExchangeBinding
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

            channel.ExchangeDeclare("ex.dog", "direct", false, true);
            channel.ExchangeDeclare("ex.rabbit", "direct", false, true);

            channel.QueueDeclare("queue-dog", false, false, true);
            channel.QueueDeclare("queue-rabbit", false, false, true);

            channel.ExchangeBind("ex.rabbit", "ex.dog", "route.rabbit");

            channel.QueueBind("queue-dog", "ex.dog", "route.dog");
            channel.QueueBind("queue-rabbit", "ex.rabbit", "route.rabbit");

            channel.BasicPublish("ex.dog", "route.dog", false, null, Encoding.UTF8.GetBytes("This is a message from dog."));
            channel.BasicPublish("ex.dog", "route.rabbit", false, null, Encoding.UTF8.GetBytes("This is a message from rabbit."));

            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();

            channel.QueueDelete("queue-dog");
            channel.QueueDelete("queue-rabbit");

            channel.ExchangeDelete("ex.dog");
            channel.ExchangeDelete("ex.rabbit");

            channel.Close();
            connection.Close();
        }
    }
}
