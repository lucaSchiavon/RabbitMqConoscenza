using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextStep.RabbitMQ.Models
{
    public class RabbitMqSettings
    {
        public string HostName { get; set; } 
        public string ExchangeName { get; set; } 
        public string QueueName { get; set; } 
        public string RoutingKey { get; set; } 
        public string ExchangeType { get; set; } 
    }
}
