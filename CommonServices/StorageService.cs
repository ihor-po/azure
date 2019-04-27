using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

#if CONSUMER
namespace MessageConsumer.Services
#elif QUERY
namespace TableSearch.Services
#endif
{
    public class StorageService
    {
#if CONSUMER
        public CloudQueue Queue1 { get; }
#endif
        public CloudTable Table1 { get; }

        public StorageService()
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(@"DefaultEndpointsProtocol=https;AccountName=tymko;AccountKey=jj/AT7NPx6l5Oms+PLvrMkyM6YM6zI8hBTML78yTVwHqzQIcxxvR4XdmcZtVucRNppSERij/HEkSCjLsTgyJUg==;EndpointSuffix=core.windows.net");
            //CloudStorageAccount storageAccount = CloudStorageAccount.Parse(@"DefaultEndpointsProtocol=https;AccountName=itstep1511gen;AccountKey=dRV5C8hAj+QpKYW+7My5xzpd1C5ZLSmWs6Wgnj/sQnaUR670ofKPcOFOXs7EEh0GUyXgrnHgpPFUqKeBBhYkhA==;BlobEndpoint=https://itstep1511gen.blob.core.windows.net/;QueueEndpoint=https://itstep1511gen.queue.core.windows.net/;TableEndpoint=https://itstep1511gen.table.core.windows.net/;FileEndpoint=https://itstep1511gen.file.core.windows.net/;");

#if CONSUMER
            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
            Queue1 = queueClient.GetQueueReference("queue1");
            var queueCreationTask = Queue1.CreateIfNotExistsAsync();
#endif

            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            Table1 = tableClient.GetTableReference("table1");
            var tableCreationTask = Table1.CreateIfNotExistsAsync();

            Task.WaitAll(
#if CONSUMER
                queueCreationTask,
#endif
                tableCreationTask);

            ConditionalMethod();
        }

        [System.Diagnostics.Conditional("CONSUMER")]
        private static void ConditionalMethod()
        {
            Console.WriteLine("Conditional method");
        }
    }
}
