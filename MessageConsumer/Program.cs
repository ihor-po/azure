
using MessageConsumer.Services;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MessageConsumer
{
    class Program
    {
        static async Task Main(string[] args)
        {
            //await ProcessQueueAsync();
            Task.Run(() => ProcessQueueAsync());
            Console.ReadKey();
        }

        //private static async void ProcessQueueAsync()
        private static async Task ProcessQueueAsync()
        {
            StorageService storageService = new StorageService();
            var queue = storageService.Queue1;
            var table = storageService.Table1;

            while (true)
            {
                var message = await queue.GetMessageAsync();
                if (message != null)
                {
                    string messageString = message.AsString;
                    if (message.DequeueCount > 2)
                    {
                        var badMessage = new BadMessage
                        {
                            PartitionKey = "BadMessages",
                            RowKey = Guid.NewGuid().ToString(),
                            Text = messageString
                        };
                        var insertOperation = TableOperation.Insert(badMessage);

                        await table.ExecuteAsync(insertOperation);

                        await queue.DeleteMessageAsync(message);
                        Console.WriteLine($"Message \"{messageString}\" deleted");
                    }
                    else
                    {
                        try
                        {
                            var requestMessage = JsonConvert.DeserializeObject<ServiceRequestMessage>(messageString);
                            Console.WriteLine($"Broken item: {requestMessage.ItemName}{Environment.NewLine}Problem description: {requestMessage.Problem}");
                            await queue.DeleteMessageAsync(message);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Tried to process bad message");
                        }
                    }
                }
                else
                    await Task.Delay(TimeSpan.FromSeconds(10));
            }
        }
    }

    public class ServiceRequestMessage
    {
        public string ItemName { get; set; }
        public string Problem { get; set; }
    }
}
