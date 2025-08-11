using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Subscriber
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            //istruzioni costose in termini di risorse percui uso using per dispose
            using IConnection connection = await factory.CreateConnectionAsync();
            using IChannel channel = await connection.CreateChannelAsync();

            await channel.QueueDeclareAsync(
              queue: "NMiaCoda",
              durable: true, //tutti i messaggi sopravviveranno a riavvio broker
              exclusive: false, //la coda è esclusiva per la connessione che la ha preparata, solo la connessione che la ha generata può utilizzarla
              autoDelete: false, //la coda verra eliminata dopo che l'ultimo consumer ha annullato l'iscrizione
              arguments: null);

            var consumer = new AsyncEventingBasicConsumer(channel);

         

               

           

            using CancellationTokenSource cts = new CancellationTokenSource();
            {
                CancellationToken token = cts.Token;

                consumer.ReceivedAsync += async (sender, eventArgs) =>
                {
                    //if (token.IsCancellationRequested)
                    //{
                    //    Console.WriteLine("Elaborazione annullata.");
                    //    return;
                    //}

                    byte[] body = eventArgs.Body.ToArray();
                    string message = Encoding.UTF8.GetString(body);
                    Prodotto prodotto = JsonSerializer.Deserialize<Prodotto>(message);
                    Console.WriteLine($"Received {prodotto.Id}");

                    if (!string.IsNullOrEmpty(message))
                    {
                        //effettiva consegna al broker, il messaggio verrà rimosso dalla coda
                        //multiple false significa che daremo conferma solo del messaggio corrente
                        
                        //todo:mettere e testare una logica per cui se avviene qualcosa il messaggio viene ackato altrimenti viene rigettato, togliere l'else qui

                        //se fosse impostato l'autoack non ci sarebbe bisogno di questa riga di codice di conferma manuale della eliminazione messaggio dalla coda
                        await ((AsyncEventingBasicConsumer)sender).Channel.BasicAckAsync(eventArgs.DeliveryTag, multiple: false, token);
                        Thread.Sleep(3000);
                    }
                    else
                    {
                        //la differenza tra nack e reject è che nack può gestire anche più messaggi in batch (impostando multiple=true).


                        //nack requeue=false Scarta ed invia a DLX
                        await ((AsyncEventingBasicConsumer)sender).Channel.BasicNackAsync(eventArgs.DeliveryTag, true,false, token);
                        ////nack requeue = true Ritenta rimettendo in coda dal punto che ha tentato di scodare quindi se il messaggio era C e la coda era ABC
                        ////riaccoda ad AB il C in questo modo ABC e quindi devo prevedere dei meccanismi per non frullare come un matto
                        //await ((AsyncEventingBasicConsumer)sender).Channel.BasicNackAsync(eventArgs.DeliveryTag, true, true, token);

                        ////nack requeue = false Scarta o invia a DLX (exchange speciale a cui vengono inviati i messaggi “morti”)
                        //await ((AsyncEventingBasicConsumer)sender).Channel.BasicRejectAsync(eventArgs.DeliveryTag, false, token);

                        ////reject requeue = true ...........rimette in coda in questo modo CAB
                        //await ((AsyncEventingBasicConsumer)sender).Channel.BasicRejectAsync(eventArgs.DeliveryTag, true, token);

                        ////reject requeue = false ..........scarta o invia a DLX 
                        //await ((AsyncEventingBasicConsumer)sender).Channel.BasicRejectAsync(eventArgs.DeliveryTag, false, token);
                    }

                };
                //vogliamo manualmente confermare l'avvenuto invio del messaggio      
                //ATTENZIONE, QUI SI IMPOSTA L'AUTOACK O L'ACK MANUALE in questo caso si vuole confermare manualmente quindi autoack:false
                var consumeTag  =await  channel.BasicConsumeAsync("NMiaCoda", autoAck: false, consumer, token);


                while (!token.IsCancellationRequested)
                {
                    var key = Console.ReadKey(intercept: true);
                    if (key.Key == ConsoleKey.Q)
                    {
                        cts.Cancel();
                        break;
                    }
                }
                
              
                // Cancella il consumer in modo ordinato
                //await channel.BasicCancelAsync(consumeTag,true,token);
                //await Task.WhenAll(consumeTag);
               
                
                //Console.ReadLine();
            }
            Console.WriteLine("Consumer cancellato. Uscita.");

        }
    }
}
