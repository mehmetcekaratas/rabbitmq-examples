using System;
using System.Text;
using RabbitMQ.Client;

namespace Producer
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

            channel.ExchangeDeclare("ex.direct", "direct", true, true);

            channel.QueueDeclare("log.error", true, false, false);
            channel.QueueDeclare("log.warning", true, false, false);
            channel.QueueDeclare("log.info", true, false, false);

            channel.QueueBind("log.error", "ex.direct", "error");
            channel.QueueBind("log.warning", "ex.direct", "warning");
            channel.QueueBind("log.info", "ex.direct", "info");

            channel.BasicPublish("ex.direct", "info", false, null, Encoding.UTF8.GetBytes("This is an info message."));
            channel.BasicPublish("ex.direct", "error", false, null, Encoding.UTF8.GetBytes("This is an error message."));
            channel.BasicPublish("ex.direct", "warning", false, null, Encoding.UTF8.GetBytes("This is a warning message."));

            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();

            channel.QueueDelete("log.error");
            channel.QueueDelete("log.info");
            channel.QueueDelete("log.warning");
            channel.ExchangeDelete("ex.direct");

            channel.Close();
            connection.Close();
        }
    }
}
