using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using MessageProducer.Services;

namespace MessageProducer.Controllers
{
    [Route("api/[controller]")]
    public class MessageController : Controller
    {
        private readonly QueueService queueService;

        public MessageController(QueueService queueService)
        {
            this.queueService = queueService;
        }

        [HttpGet]
        public string Get() => "Running";

        // POST api/message
        [HttpPost]
        public async Task Post([FromBody]dynamic value)
        {
            var message = new CloudQueueMessage(value.ToString());
            await queueService.Queue1.AddMessageAsync(message);
        }
    }
}
