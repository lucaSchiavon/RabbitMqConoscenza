using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NextStep.RabbitMQ.Interfaces;
using NextStep.RabbitMQ.Models;
using NextStep.RabbitMQ.Services;


namespace Subscriber
{
    internal class Program
    {
        static async Task Main(string[] args)
        {

            #region creazione di una sottoscrizione (senza l'uso della libreria di questa soluzione, direttamente sulla pagina) 
            //var factory = new ConnectionFactory() { HostName = "localhost" };
            ////istruzioni costose in termini di risorse percui uso using per dispose
            //using IConnection connection = await factory.CreateConnectionAsync();
            //using IChannel channel = await connection.CreateChannelAsync();
            ////string nomeCoda = "NMiaCoda";
            //string nomeCoda = "CodaDirect";

            //await channel.QueueDeclareAsync(
            //   //queue: "NMiaCoda",
            //   queue: nomeCoda,
            //  durable: true, //tutti i messaggi sopravviveranno a riavvio broker
            //  exclusive: false, //la coda è esclusiva per la connessione che la ha preparata, solo la connessione che la ha generata può utilizzarla
            //  autoDelete: false, //la coda verra eliminata dopo che l'ultimo consumer ha annullato l'iscrizione
            //  arguments: null);

            //var consumer = new AsyncEventingBasicConsumer(channel);



            //using CancellationTokenSource cts = new CancellationTokenSource();
            //{
            //    CancellationToken token = cts.Token;

            //    consumer.ReceivedAsync += async (sender, eventArgs) =>
            //    {
            //        //if (token.IsCancellationRequested)
            //        //{
            //        //    Console.WriteLine("Elaborazione annullata.");
            //        //    return;
            //        //}

            //        byte[] body = eventArgs.Body.ToArray();
            //        string message = Encoding.UTF8.GetString(body);
            //        Prodotto prodotto = JsonSerializer.Deserialize<Prodotto>(message);
            //        Console.WriteLine($"Received {prodotto.Id}");

            //        if (!string.IsNullOrEmpty(message))
            //        {
            //            //effettiva consegna al broker, il messaggio verrà rimosso dalla coda
            //            //multiple false significa che daremo conferma solo del messaggio corrente

            //            //todo:mettere e testare una logica per cui se avviene qualcosa il messaggio viene ackato altrimenti viene rigettato, togliere l'else qui

            //            //se fosse impostato l'autoack non ci sarebbe bisogno di questa riga di codice di conferma manuale della eliminazione messaggio dalla coda
            //            await ((AsyncEventingBasicConsumer)sender).Channel.BasicAckAsync(eventArgs.DeliveryTag, multiple: false, token);
            //            Thread.Sleep(3000);
            //        }
            //        else
            //        {
            //            //la differenza tra nack e reject è che nack può gestire anche più messaggi in batch (impostando multiple=true).


            //            //nack requeue=false Scarta ed invia a DLX
            //            await ((AsyncEventingBasicConsumer)sender).Channel.BasicNackAsync(eventArgs.DeliveryTag, true, false, token);

            //            ////nack requeue = true Ritenta rimettendo in coda dal punto che ha tentato di scodare quindi se il messaggio era C e la coda era ABC
            //            ////riaccoda ad AB il C in questo modo ABC e quindi devo prevedere dei meccanismi per non frullare come un matto
            //            //await ((AsyncEventingBasicConsumer)sender).Channel.BasicNackAsync(eventArgs.DeliveryTag, true, true, token);

            //            ////nack requeue = false Scarta o invia a DLX (exchange speciale a cui vengono inviati i messaggi “morti”)
            //            //await ((AsyncEventingBasicConsumer)sender).Channel.BasicRejectAsync(eventArgs.DeliveryTag, false, token);

            //            ////reject requeue = true ...........rimette in coda in questo modo CAB
            //            //await ((AsyncEventingBasicConsumer)sender).Channel.BasicRejectAsync(eventArgs.DeliveryTag, true, token);

            //            ////reject requeue = false ..........scarta o invia a DLX 
            //            //await ((AsyncEventingBasicConsumer)sender).Channel.BasicRejectAsync(eventArgs.DeliveryTag, false, token);
            //        }

            //    };
            //    //vogliamo manualmente confermare l'avvenuto invio del messaggio      
            //    //ATTENZIONE, QUI SI IMPOSTA L'AUTOACK O L'ACK MANUALE in questo caso si vuole confermare manualmente quindi autoack:false
            //    var consumeTag = await channel.BasicConsumeAsync(nomeCoda, autoAck: false, consumer, token);

            //    //se premi quit si interrompe il consumo
            //    while (!token.IsCancellationRequested)
            //    {
            //        var key = Console.ReadKey(intercept: true);
            //        if (key.Key == ConsoleKey.Q)
            //        {
            //            cts.Cancel();
            //            break;
            //        }
            //    }


            //    // Cancella il consumer in modo ordinato
            //    //await channel.BasicCancelAsync(consumeTag,true,token);
            //    //await Task.WhenAll(consumeTag);


            //    //Console.ReadLine();
            //}
            //Console.WriteLine("Consumer cancellato. Uscita.");
            #endregion

            #region Creazione sottoscrizione con libreria NextStep.RabbitMQ
            IRabbitMqConsumer client = null;
            try
            {
                var config = new ConfigurationBuilder()
               .SetBasePath(AppContext.BaseDirectory) // Imposta la directory base
               .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true) // Carica il file JSON
               .Build();

                var rabbitMqSettings = config.GetSection("RabbitMq").Get<RabbitMqSettings>();

                var serviceCollection = new ServiceCollection()
                .AddSingleton(rabbitMqSettings)
                 //.AddSingleton<IProdottoService, ProdottoService>()
                 .AddSingleton<IRabbitMqConsumer>(sp =>
                 {
                     var settings = rabbitMqSettings;

                     // factory async → blocco in fase di registrazione
                     // (si può anche fare in modo completamente async, ma richiede un approccio diverso)
                     return RabbitMqConsumer.CreateAsync(settings).GetAwaiter().GetResult();
                 });

               
                var serviceProvider = serviceCollection.BuildServiceProvider();
                client = serviceProvider.GetRequiredService<IRabbitMqConsumer>();


                    using var cts = new CancellationTokenSource();

                //due versioni aspettando e non, se non si aspetta la chiusura del servizio va gestita con  await Task.WhenAll(task);
                //si veda sotto
                //await client.StartConsumingAsync<Prodotto>(async prodotto =>
                var task = client.StartConsumingAsync<Prodotto>(async prodotto =>
                    {
                        //qui inserire la logica di validazione o meno del dato
                        //se qualcosa va storto far ritornare false (verrà effettuato il nack con eliminazione della risorsa
                        Console.WriteLine($"Ricevuto: {prodotto.Id}");
                        await Task.Delay(3000); // Simula elaborazione
                        return true;
                    }, cts.Token);

                    // Aspetta che l'utente prema Q per uscire
                    while (!cts.Token.IsCancellationRequested)
                    {
                        var key = Console.ReadKey(true);
                        if (key.Key == ConsoleKey.Q)
                            cts.Cancel();
                    }

                // Ora è sicuro chiudere
                //prima di interrompere aspetta comunque che il servizio finisca di consumare le code
                await Task.WhenAll(task);

                //+++++++++++
                //}




                //while (true)
                //{
                //    Console.Write("Inserisci il nome del prodotto (esci per uscire): ");
                //    string NomeProdotto = Console.ReadLine();

                //    if (NomeProdotto.ToUpper() == "ESCI")
                //        break;

                //    Console.Write("Inserisci il prezzo del prodotto: ");
                //    if (!double.TryParse(Console.ReadLine(), out double Prezzo))
                //    {
                //        Console.WriteLine("Prezzo non valido. Riprova.");
                //        continue;
                //    }
                //    //pubblica il messaggio
                //    await prodottoService.InviaProdottoAsync(new Prodotto { Id = Guid.NewGuid(), Nome = "pere" });
                //}


            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore: {ex.Message}");
            }
            finally
            {
                await client.DisposeAsync();
                ////eseguire sempre questo
                //if (prodottoService != null)
                //    await prodottoService.DisposeAsync();
            }

            #endregion
        }
    }
}
