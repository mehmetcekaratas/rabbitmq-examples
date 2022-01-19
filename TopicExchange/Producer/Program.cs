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

            channel.ExchangeDeclare("ex.topic", "topic", false, true);

            channel.QueueDeclare("log.error", false, false, true);
            channel.QueueDeclare("logs.all", false, false, true);
            channel.QueueDeclare("all.warnings", false, false, true);

            channel.QueueBind("log.error", "ex.topic", "log.error");
            channel.QueueBind("logs.all", "ex.topic", "log.*");
            channel.QueueBind("all.warnings", "ex.topic", "#.warning");

            channel.BasicPublish("ex.topic", "log", null, Encoding.UTF8.GetBytes("This is a log message"));
            channel.BasicPublish("ex.topic", "log.error", null, Encoding.UTF8.GetBytes("This is an error message"));
            channel.BasicPublish("ex.topic", "log.info", null, Encoding.UTF8.GetBytes("This is an info message"));
            channel.BasicPublish("ex.topic", "test.warning", null, Encoding.UTF8.GetBytes("This is a test warning message"));
            channel.BasicPublish("ex.topic", "warning", null, Encoding.UTF8.GetBytes("This is a warning message"));

            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();

            channel.QueueDelete("log.error");
            channel.QueueDelete("logs.all");
            channel.QueueDelete("all.warnings");
            channel.ExchangeDelete("ex.topic");

            channel.Close();
            connection.Close();
        }
    }
}
