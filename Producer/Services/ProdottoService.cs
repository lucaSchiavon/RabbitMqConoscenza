using NextStep.RabbitMQ.Interfaces;
using NextStep.RabbitMQ.Services;
using System.Threading.Tasks;


namespace Producer.Services
{
    public class ProdottoService : IProdottoService, IAsyncDisposable
    {
        private readonly IRabbitMqProducer _producer;
        private bool _disposed = false;

        public ProdottoService(IRabbitMqProducer producer)
        {
            _producer = producer;
        }

        public async Task InviaProdottoAsync(Prodotto prodotto)
        {
            //var prodotto = new Prodotto { Id = Guid.NewGuid(), Nome = "pere" };
            await _producer.PublishAsync(prodotto);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);

        }
        protected virtual async Task Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
              await  _producer.DisposeAsync();
            }


            _disposed = true;
        }

        public async ValueTask DisposeAsync()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
