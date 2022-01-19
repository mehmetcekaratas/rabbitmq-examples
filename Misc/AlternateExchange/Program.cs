using System;
using System.Buffers;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Text;
using RabbitMQ.Client;

namespace AlternateExchange
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

            channel.QueueDeclare("queue.video", false, false, true);
            channel.QueueDeclare("queue.image", false, false, true);
            channel.QueueDeclare("queue.unrouted", false, false, true);

            channel.ExchangeDeclare("ex.fanout", "fanout", true, false);
            channel.ExchangeDeclare("ex.direct", "direct", true, false, new Dictionary<string, object>
            {
                {"alternate-exchange", "ex.fanout"}
            });

            channel.QueueBind("queue.video", "ex.direct", "video");
            channel.QueueBind("queue.image", "ex.direct", "image");
            channel.QueueBind("queue.unrouted", "ex.fanout", string.Empty);

            channel.BasicPublish("ex.direct", "video", false, null, Encoding.UTF8.GetBytes("This is video message."));
            channel.BasicPublish("ex.direct", "image", false, null, Encoding.UTF8.GetBytes("This is an image message."));
            channel.BasicPublish("ex.direct", "image", false, null, Encoding.UTF8.GetBytes("This is a text message."));
        }
    }
}
