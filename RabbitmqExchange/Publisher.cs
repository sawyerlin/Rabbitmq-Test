using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Concurrent;
using System.Text;
using System.Threading.Tasks;

namespace RabbitmqExchange
{
    public class Publisher
    {
        private string _name;
        private ConnectionFactory _factory;
        private BlockingCollection<string> _respQueue = new BlockingCollection<string>();
        private readonly bool _useExchange = false;

        public Publisher(string name)
        {
            _name = name;
            _factory = new ConnectionFactory() { Uri = Configuration.RabbitmqUri };
        }

        public void SendMessage(int index, string message)
        {
            Task.Run(() =>
            {
                var request = new Request(message, _name);
                using (var connection = _factory.CreateConnection())
                using (var channel = connection.CreateModel())
                {
                    var replyQueueName = channel.QueueDeclare("rpc_queue_reply", exclusive: false, autoDelete: true);
                    var consumer = new EventingBasicConsumer(channel);
                    var props = channel.CreateBasicProperties();
                    props.CorrelationId = request.Id.ToString();
                    props.ReplyTo = replyQueueName;
                    consumer.Received += (model, ea) =>
                    {
                        var response = JsonConvert.DeserializeObject<Request>(Encoding.UTF8.GetString(ea.Body));
                        if (ea.BasicProperties.CorrelationId == request.Id.ToString())
                        {
                            _respQueue.Add("[Provider (" + _name + ")] write: From [" + response.From + "]" + response.Message);
                        }
                    };

                    var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(request));
                    if (_useExchange)
                    {
                        var exchangeName = "exchange" + request.Id;
                        channel.ExchangeDeclare(exchange: exchangeName, type: "fanout");
                        channel.QueueBind(queue: Configuration.QueueName, exchange: exchangeName, routingKey: "");
                        channel.BasicPublish(exchange: exchangeName,
                                             routingKey: "",
                                             basicProperties: props,
                                             body: body);
                    }
                    else
                    {
                        channel.BasicPublish(exchange: "",
                                             routingKey: Configuration.QueueName,
                                             basicProperties: props,
                                             body: body);
                    }

                    channel.BasicConsume(
                        consumer: consumer,
                        queue: replyQueueName,
                        autoAck: true
                        );
                    Console.WriteLine(_respQueue.Take());
                }
            });
        }
    }
}
