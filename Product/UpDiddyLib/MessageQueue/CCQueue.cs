using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace UpDiddyLib.MessageQueue
{


    public class CCQueue
    {
        string _connectionString = string.Empty;
        CloudStorageAccount _storageAccount = null;
        CloudQueueClient _queueClient = null;
        string _queueName = string.Empty;
        CloudQueue _queue = null;

        public CCQueue(string QueueName, string ConnectionString)
        {
            _connectionString = ConnectionString;
            _queueName = QueueName;
            _storageAccount = CloudStorageAccount.Parse(
                 "DefaultEndpointsProtocol=https;AccountName=ccmqenrollment;AccountKey=Q1seibI3vnxGzsC3zqw/KYAcjJQap8nKSiYWKfldGdfJnPuQ6M47CT9EaRk53bGofWkct4LC+N+DYginPWV53Q==;EndpointSuffix=core.windows.net");

            // Create the queue client.
            _queueClient = _storageAccount.CreateCloudQueueClient();

            // Retrieve a reference to a container.
            _queue = _queueClient.GetQueueReference("ccmessagequeue");

            // Create the queue if it doesn't already exist
            _queue.CreateIfNotExistsAsync();

        }

        public void EnQueue<T>(T QueueItem )
        {
            var Json = Newtonsoft.Json.JsonConvert.SerializeObject(QueueItem);
            // Create a message and add it to the queue.
            CloudQueueMessage Message = new CloudQueueMessage(Json);
            _queue.AddMessageAsync(Message);
        }

        /* Not neded with Microservices 
        public T DeQueue<T>()
        {
            T Rval = (T)Convert.ChangeType(null, typeof(T));
 
            // Get the next message
            Task<Microsoft.WindowsAzure.Storage.Queue.CloudQueueMessage> QueueGetTask = _queue.GetMessageAsync();
            CloudQueueMessage  QueueMessage = QueueGetTask.Result;

            if (QueueMessage == null)
                return Rval;

            var Json = QueueMessage.AsString;
            // Create a message and add it to the queue.
            CloudQueueMessage Message = new CloudQueueMessage(Json);

            //Delete the message TODO Uncomment
           //  _queue.DeleteMessageAsync(QueueMessage);

     
       
            return Rval;

        }
        */

 



    }
}








 
