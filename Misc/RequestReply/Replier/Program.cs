using System;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Replier
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

            channel.QueueDeclare("rpc-queue", false, false, false);
            channel.BasicQos(0, 1, false);

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (s, e) =>
            {
                string response = null;

                var replyProps = channel.CreateBasicProperties();
                replyProps.CorrelationId = e.BasicProperties.CorrelationId;

                try
                {
                    var message = Encoding.UTF8.GetString(e.Body.ToArray());
                    int.TryParse(message, out int n);
                    Console.WriteLine("[.] Fibonacci({0})", n);
                    response = Fibonacci(n).ToString();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("[.] {0}", ex.Message);
                    response = string.Empty;
                }
                finally
                {
                    channel.BasicPublish(string.Empty, e.BasicProperties.ReplyTo, replyProps, Encoding.UTF8.GetBytes(response));
                    channel.BasicAck(e.DeliveryTag, false);
                }
            };
            channel.BasicConsume("rpc-queue", false, consumer);

            Console.WriteLine("Waiting for RPC requests... Press [enter] to exit.");
            Console.ReadKey();
        }

        private static int Fibonacci(int n)
        {
            if (n == 0 || n == 1)
            {
                return n;
            }

            return Fibonacci(n - 1) + Fibonacci(n - 2);
        }
    }
}
