using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MessageProducer.Services
{
    public class QueueService
    {
        public CloudQueue Queue1 { get; set; }

        public QueueService()
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(@"DefaultEndpointsProtocol=https;AccountName=tymko;AccountKey=jj/AT7NPx6l5Oms+PLvrMkyM6YM6zI8hBTML78yTVwHqzQIcxxvR4XdmcZtVucRNppSERij/HEkSCjLsTgyJUg==;EndpointSuffix=core.windows.net");

            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
            Queue1 = queueClient.GetQueueReference("queue1");
            Queue1.CreateIfNotExistsAsync().Wait();
        }
    }
}
