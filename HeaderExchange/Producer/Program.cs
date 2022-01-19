using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualBasic;
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

            channel.ExchangeDeclare("ex.headers", "headers", false, true);

            channel.QueueDeclare("queue.one", false, false, true);
            channel.QueueDeclare("queue.two", false, false, true);
            channel.QueueDeclare("queue.three", false, false, true);

            channel.QueueBind("queue.one", "ex.headers", string.Empty, new Dictionary<string, object>
{
    {"x-match", "all"},
    {"op", "convert"},
    {"format", "jpeg"}
});
            channel.QueueBind("queue.two", "ex.headers", string.Empty, new Dictionary<string, object>
{
    {"x-match", "all"},
    {"op", "convert"},
    {"format", "png"}
});
            channel.QueueBind("queue.three", "ex.headers", string.Empty, new Dictionary<string, object>
{
    {"x-match", "any"},
    {"op", "convert"},
    {"format", "bitmap"}
});

            var props = channel.CreateBasicProperties();
            props.Headers = new Dictionary<string, object>()
{
    {"op", "convert"},
    {"format", "jpeg"}
};
            channel.BasicPublish("ex.headers", string.Empty, false, props, Encoding.UTF8.GetBytes("This is a jpeg convert operation."));

            props = channel.CreateBasicProperties();
            props.Headers = new Dictionary<string, object>
{
    {"op", "convert"},
    {"format", "gif"}
};
            channel.BasicPublish("ex.headers", string.Empty, props, Encoding.UTF8.GetBytes("This is a gif convert operation."));

            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();

            channel.QueueDelete("queue.one");
            channel.QueueDelete("queue.two");
            channel.QueueDelete("queue.three");
            channel.ExchangeDelete("ex.headers");

            channel.Close();
            connection.Close();
        }
    }
}
