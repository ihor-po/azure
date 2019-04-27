using System;
using Microsoft.WindowsAzure.Storage.Table;

#if CONSUMER
namespace MessageConsumer
#elif QUERY
namespace TableSearch.Services
#endif
{
    public class BadMessage : TableEntity
    {
        public string Text { get; set; }
    }
}
