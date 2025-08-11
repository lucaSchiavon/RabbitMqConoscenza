using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Producer
{
    internal class Program
    {
        static async Task Main(string[] args)
        {

            #region "creazione producer modo più semplice: un solo producer ed una sola coda"

            //var factory = new ConnectionFactory() { HostName = "localhost" };
            ////istruzioni costose in termini di risorse percui uso using per dispose
            //using IConnection connection = await factory.CreateConnectionAsync();
            //using IChannel channel = await connection.CreateChannelAsync();


            ////channel è il modo di interagire con il nostro broker
            ////posso associarmi ad una coda, dichiarare una coda, eliminare una coda
            ////channel.QueueBindAsync
            ////channel.QueueDeleteAsync ecc

            ////posso lavorare con gli exchange
            ////channel.ExchangeBindAsync ecc

            ////poi abbiamo il supporto per lacknolwledge
            ////channel.BasicAckAsync


            ////dichiariamo una coda
            //await channel.QueueDeclareAsync(
            //    queue: "NMiaCoda",
            //    durable: true, //tutti i messaggi sopravviveranno a riavvio broker
            //    exclusive: false, //la coda è esclusiva per la connessione che la ha preparata, solo la connessione che la ha generata può utilizzarla
            //    autoDelete: false, //la coda verra eliminata dopo che l'ultimo consumer ha annullato l'iscrizione
            //    arguments: null);

            //for (int i = 0; i < 40; i++)
            //{
            //    //creazione del payload da inviare...
            //    var prod = new Prodotto()
            //    { Id = Guid.NewGuid(), Nome = "pere" };
            //    var prodSer = JsonSerializer.Serialize(prod);
            //    var body = Encoding.UTF8.GetBytes(prodSer);


            //    //pubblicazione della coda che verrà gestitat dall'exchange di default
            //    //modo semplice di gestione in caso di solo producer, sola coda e nessuna logica di
            //    //routing complessa
            //    await channel.BasicPublishAsync(
            //        exchange: string.Empty, //questo significa che semplifichiamo e pubblichiamo sull'exchange di default
            //                                //Questo exchange ha un comportamento speciale: ogni coda dichiarata viene automaticamente "bindata"
            //                                //alla coda mediante il routingKey
            //        routingKey: "NMiaCoda",
            //        mandatory: true, //il messaggio deve essere consegnato ad una coda legata al routingkey
            //        basicProperties: new BasicProperties() { Persistent = true }, //il messaggio sarà salvato su disco
            //        body);


            //    Console.WriteLine($"sent {prod.Id}");

            //    await Task.Delay(100);

            #endregion


            var factory = new ConnectionFactory() { HostName = "localhost" };
            using var connection = await factory.CreateConnectionAsync();
            using var channel = await connection.CreateChannelAsync();

            string exchangeName = "MioExchange";
            string queueName = "MiaCoda";
            string routingKey = "chiave.pere";

            // 1️ Dichiarazione dell'exchange
            await channel.ExchangeDeclareAsync(
                exchange: exchangeName,
                type: "direct", // può essere direct, fanout, topic, headers
                durable: true,
                autoDelete: false,
                arguments: null);

            // 2️ Dichiarazione della coda
            await channel.QueueDeclareAsync(
                queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            // 3️ Binding della coda all'exchange con routingKey
            await channel.QueueBindAsync(
                queue: queueName,
                exchange: exchangeName,
                routingKey: routingKey);

            // 4️⃣ Invio dei messaggi
            for (int i = 0; i < 10; i++)
            {
                var prodotto = new { Id = Guid.NewGuid(), Nome = "pere" };
                var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(prodotto));

                await channel.BasicPublishAsync(
                    exchange: exchangeName,
                    routingKey: routingKey,
                    mandatory: true,
                    basicProperties: new BasicProperties() { Persistent = true },
                    body);

                Console.WriteLine($"Messaggio inviato: {prodotto.Id}");
                await Task.Delay(100);
            }
        }

        }
    }
}
