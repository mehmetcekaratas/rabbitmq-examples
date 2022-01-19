using System;
using System.Collections.Concurrent;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Requester
{
    public class RpcClient
    {
        private readonly IConnection connection;
        private readonly IModel channel;
        private readonly string replyQueueName;
        private readonly EventingBasicConsumer consumer;
        private readonly BlockingCollection<string> responseQueue = new BlockingCollection<string>();
        private readonly IBasicProperties props;

        public RpcClient()
        {
            var factory = new ConnectionFactory()
            {
                HostName = "localhost",
                VirtualHost = "/",
                Port = 5672,
                UserName = "guest",
                Password = "guest"
            };
            connection = factory.CreateConnection();
            channel = connection.CreateModel();
            replyQueueName = channel.QueueDeclare().QueueName;
            consumer = new EventingBasicConsumer(channel);

            props = channel.CreateBasicProperties();
            props.CorrelationId = Guid.NewGuid().ToString();
            props.ReplyTo = replyQueueName;

            consumer.Received += (s, e) =>
            {
                var response = Encoding.UTF8.GetString(e.Body.ToArray());
                if (e.BasicProperties.CorrelationId == props.CorrelationId)
                {
                    responseQueue.Add(response);
                }
            };

            channel.BasicConsume(replyQueueName, true, consumer);
        }

        public string Call(string message)
        {
            channel.BasicPublish(string.Empty, "rpc-queue", false, props, Encoding.UTF8.GetBytes(message));
            return responseQueue.Take();
        }

        public void Close()
        {
            connection.Close();
        }
    }
}