using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Threading.Tasks;

namespace RabbitmqExchange
{
    public class Subscriber
    {
        private string _name;

        public Subscriber(string name)
        {
            _name = name;
            var factory = new ConnectionFactory() { Uri = Configuration.RabbitmqUri };

            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                var queueName = channel.QueueDeclare(queue: Configuration.QueueName, durable: false, autoDelete: true, exclusive: false, arguments: null);
                var consumer = new EventingBasicConsumer(channel);
                channel.BasicConsume(queue: queueName, autoAck: false, consumer: consumer);

                consumer.Received += (model, ea) =>
                {
                    Request response = null;

                    var body = ea.Body;
                    var props = ea.BasicProperties;
                    var replyProps = channel.CreateBasicProperties();
                    replyProps.CorrelationId = props.CorrelationId;
                    //var replyQueueName = channel.QueueDeclare(props.ReplyTo, exclusive: false, autoDelete: true);

                    try
                    {
                        var request = JsonConvert.DeserializeObject<Request>(Encoding.UTF8.GetString(body));
                        Console.WriteLine("[" + _name + "] writes: From [" + request.From + "]" + request.Message);
                        response = request;
                        response.From = _name;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(" [.] " + e.Message);
                        response = null;
                    }
                    finally
                    {
                        var responseBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(response));
                        channel.BasicPublish(exchange: "", routingKey: props.ReplyTo,
                          basicProperties: replyProps, body: responseBytes);
                        channel.BasicAck(deliveryTag: ea.DeliveryTag,
                          multiple: false);
                    }
                };

                Console.WriteLine(" Press [enter] to exit.");
                Console.ReadLine();
            };

        }
    }
}
