using System;

namespace RabbitmqExchange
{
    public class Request
    {
        public Guid Id { get; set; }
        public string From { get; set; }
        public string Message { get; set; }

        public Request(string message, string from)
        {
            Id = Guid.NewGuid();
            From = from;
            Message = message;
        }
    }
}
