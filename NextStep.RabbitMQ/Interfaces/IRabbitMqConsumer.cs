using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextStep.RabbitMQ.Interfaces
{
    public interface IRabbitMqConsumer
    {
        //Task PublishAsync<T>(T message);
        Task StartConsumingAsync<T>(Func<T, Task<bool>> onMessageReceived, CancellationToken cancellationToken);
        ValueTask DisposeAsync();
    }
}
