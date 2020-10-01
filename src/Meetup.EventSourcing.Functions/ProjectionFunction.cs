using Meetup.EventSourcing.Infrastructure;
using Microsoft.Azure.Documents;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Meetup.EventSourcing.Functions
{
    public static class ProjectionFunction
    {

        [FunctionName("ChangeFeedFunction")]
        public static async Task Run(
            [CosmosDBTrigger(
                databaseName: "event-store",
                collectionName: "events-meetup",
                ConnectionStringSetting = "DatabaseConnection",
                LeaseCollectionName = "leases")]IReadOnlyList<Document> input,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            if (input?.Count > 0)
            {
                foreach (var item in input)
                {
                    await starter.StartNewAsync("ProjectionFunction", item);
                }
            }
        }

        [FunctionName("ProjectionFunction")]
        public static async Task RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var document = context.GetInput<Document>();
            var e = JsonConvert.DeserializeObject<EventData>(document.ToString());

            var data = new EmployeeEventData
            {
                EmployeeId = e.Data["employeeId"].ToObject<Guid>(),
                Data = e
            };

            await context.CallActivityAsync<EmployeeEventData>("ProjectSummary", data);
        }

        [FunctionName("ProjectSummary")]
        public static void SayHello(
            [ActivityTrigger] EmployeeEventData document,
            [CosmosDB(
                databaseName: "event-store",
                collectionName: "summaries-meetup",
                ConnectionStringSetting = "DatabaseConnection",
                Id = "{document.EmployeeId}",
                PartitionKey = "{document.EmployeeId}")]EmployeeSummary employee,
            [CosmosDB(
                databaseName: "event-store",
                collectionName: "summaries-meetup",
                ConnectionStringSetting = "DatabaseConnection")] out EmployeeSummary output,
            ILogger log)
        {
            employee ??= new EmployeeSummary { Id = document.EmployeeId };

            switch (document.Data.EventType)
            {
                case "Employee.Renamed":
                    employee.Name = document.Data.Data["name"].ToString();
                    break;
                case "Employee.Promoted":
                    employee.Role = document.Data.Data["role"].ToString();
                    break;
            }

            output = employee;
        }
    }
}
