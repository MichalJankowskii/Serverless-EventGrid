namespace RegistrationApp
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.Http;
    using Microsoft.Extensions.Logging;
    using Microsoft.WindowsAzure.Storage.Queue;
    using Microsoft.WindowsAzure.Storage.Table;
    using Models;
    using Newtonsoft.Json;

    public static class SignCustomer
    {
        [FunctionName("SignCustomer")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post")]
            Customer customer,
            [Table("processingStatus", Connection = "registrationstorage_STORAGE")]
            CloudTable processingStatusTable,
            [Queue("requestreceived", Connection = "registrationstorage_STORAGE")]
            CloudQueue requestReceivedQueue,
            ILogger log)
        {
            log.LogInformation($"Data received: {customer.Name} - {customer.Surname}");

            var processingStatus = new ProcessingStatus
            {
                ProcessingState = ProcessingState.Received.ToString()
            };

            customer.RowKey = processingStatus.RowKey;

            TableOperation insertOperation = TableOperation.Insert(processingStatus);
            await processingStatusTable.ExecuteAsync(insertOperation);

            string serializeCustomer = JsonConvert.SerializeObject(customer);
            await requestReceivedQueue.AddMessageAsync(new CloudQueueMessage(serializeCustomer));

            return new HttpResponseMessage(HttpStatusCode.Accepted)
            {
                Headers =
                {
                    Location = new Uri(
                        Environment.GetEnvironmentVariable("RequestStatusCheckUrl", EnvironmentVariableTarget.Process) +
                        processingStatus.RowKey)
                }
            };
        }
    }
}