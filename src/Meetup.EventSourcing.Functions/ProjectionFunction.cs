using Meetup.EventSourcing.Infrastructure;
using Microsoft.Azure.Documents;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Meetup.EventSourcing.Functions
{
    public static class ProjectionFunction
    {
        [FunctionName("CosmosChangeFeedFunction")]
        public static async Task CosmosChangeFeedTrigger(
            [CosmosDBTrigger(
                databaseName: "event-store",
                collectionName: "events",
                ConnectionStringSetting = "DatabaseConnection",
                LeaseCollectionName = "leases")]IReadOnlyList<Document> input,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            if (input?.Count > 0)
            {
                foreach (var d in input)
                {
                    await starter.StartNewAsync("EventProjectionOrchestratorFunction", d);
                }
            }
        }

        [FunctionName("EventProjectionOrchestratorFunction")]
        public static Task RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var input = context.GetInput<Document>();
            var json = JToken.Parse(input.ToString());
            var data = new EmployeeEventData
            {
                EmployeeId = json["data"]["employeeId"].ToObject<Guid>(),
                Event = json.ToObject<EventData>(),
            };

            return context.CallActivityAsync<string>("EmployeeSummaryProcessorFunction", data);
        }

        [FunctionName("EmployeeSummaryProcessorFunction")]
        public static void ProcessEventToEmployeeSummary(
            [ActivityTrigger] EmployeeEventData document,
            [CosmosDB(
                databaseName: "event-store",
                collectionName: "summaries",
                ConnectionStringSetting = "DatabaseConnection",
                Id = "{document.EmployeeId}",
                PartitionKey = "{document.EmployeeId}")]EmployeeSummary employee,
            [CosmosDB(
                databaseName: "event-store",
                collectionName: "summaries",
                ConnectionStringSetting = "DatabaseConnection")]out EmployeeSummary output,
            ILogger log)
        {
            employee ??= new EmployeeSummary { Id = document.EmployeeId };

            switch (document.Event.EventType)
            {
                case "Employee.Renamed":
                    employee.Name = document.Event.Data["name"].ToString();
                    break;
                case "Employee.Promoted":
                    employee.Role = document.Event.Data["role"].ToString();
                    break;
            }

            output = employee;
        }
    }
}
