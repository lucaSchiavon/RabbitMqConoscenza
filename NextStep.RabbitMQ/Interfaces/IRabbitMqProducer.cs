using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextStep.RabbitMQ.Interfaces
{
    public interface IRabbitMqProducer
    {
        Task PublishAsync<T>(T message);
        ValueTask DisposeAsync();
    }
}
