using System;

namespace RabbitmqExchange
{
    public static class Configuration
    {
        public static string QueueName = "rpc_queue";
        public static Uri RabbitmqUri = new Uri("amqp://rabbit:rabbit@InnovaEnv:5672/");
    }
}
