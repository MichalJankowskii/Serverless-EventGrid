namespace RegistrationApp
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.Http;
    using Microsoft.Extensions.Logging;
    using Microsoft.WindowsAzure.Storage.Table;
    using Models;

    public static class RequestStatusCheck
    {
        [FunctionName("RequestStatusCheck")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequest req,
            [Table("processingStatus", Connection = "registrationstorage_STORAGE")]CloudTable processingStatusTable,
            ILogger log)
        {
            string guid = req.Query["guid"];

            log.LogInformation($"Status check : {guid}");

            TableQuery<ProcessingStatus> searchQuery = new TableQuery<ProcessingStatus>()
                .Where(TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, guid));

            ProcessingStatus status = (await processingStatusTable.ExecuteQuerySegmentedAsync(searchQuery, null)).FirstOrDefault();

            if (status == null)
            {
                return new HttpResponseMessage(HttpStatusCode.BadRequest)
                    { Content = new StringContent("Please pass a correct guid on the query string") };
            }

            switch (status.ProcessingState)
            {
                case "Accepted":
                    return new HttpResponseMessage(HttpStatusCode.OK);
                case "Received":
                    return new HttpResponseMessage(HttpStatusCode.Accepted)
                    {
                        Headers =
                        {
                            Location = new Uri(Environment.GetEnvironmentVariable("RequestStatusCheckUrl", EnvironmentVariableTarget.Process) + guid)
                        }
                    };
                case "Error":
                    return new HttpResponseMessage(HttpStatusCode.BadRequest)
                        { Content = new StringContent(status.Message) };
                default:
                    return new HttpResponseMessage(HttpStatusCode.BadRequest);
            }
        }
    }
}
