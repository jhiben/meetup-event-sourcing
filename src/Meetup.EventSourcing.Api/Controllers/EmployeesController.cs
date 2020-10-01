using Meetup.EventSourcing.Api.ViewModels;
using Meetup.EventSourcing.Domain;
using Meetup.EventSourcing.Domain.IntegrationEvents;
using Meetup.EventSourcing.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Meetup.EventSourcing.Api.Controllers
{
    [ApiController]
    [Route("api/employees")]
    public class EmployeesController : ControllerBase
    {
        private readonly IEventStore _eventStore;

        private readonly IReadModelService _readModelService;

        public EmployeesController(IEventStore eventStore, IReadModelService readModelService)
        {
            _eventStore = eventStore ?? throw new ArgumentNullException(nameof(eventStore));
            _readModelService = readModelService ?? throw new ArgumentNullException(nameof(readModelService));
        }

        [HttpPost]
        public async Task<ActionResult<EmployeeViewModel>> Create(EmployeeInput input)
        {
            var role = (Role)Enum.Parse(typeof(Role), input.Role);
            var employee = Employee.Create(input.Name, role);

            var events = ((IAggregate)employee).GetIntegrationEvents();
            await _eventStore.Publish("Employee-" + employee.Id, events);

            return Ok(new EmployeeViewModel
            {
                Id = employee.Id,
                Name = employee.Name,
                Role = employee.Role.ToString(),
                Salary = employee.Salary,
            });
        }

        [HttpGet]
        public async IAsyncEnumerable<EmployeeSummaryViewModel> Get()
        {
            await foreach (var item in _readModelService.GetAllSummaries())
            {
                yield return new EmployeeSummaryViewModel
                {
                    Name = item.Name,
                    Role = item.Role,
                };
            }
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<EmployeeViewModel>> Get(Guid id)
        {
            var employee = await FetchEmployee(id);

            return Ok(new EmployeeViewModel
            {
                Id = employee.Id,
                Name = employee.Name,
                Role = employee.Role.ToString(),
                Salary = employee.Salary,
            });
        }

        [HttpPut("{id:guid}/promote")]
        public async Task<IActionResult> Promote(Guid id)
        {
            var employee = await FetchEmployee(id);

            employee.Promote();

            var events = ((IAggregate)employee).GetIntegrationEvents();
            await _eventStore.Publish("Employee-" + employee.Id, events);

            return NoContent();
        }

        private async Task<Employee> FetchEmployee(Guid id)
        {
            var events = new List<StoreEvent>();
            await foreach (var e in _eventStore.GetEvents("Employee-" + id))
            {
                events.Add(e);
            }

            var integrationEvents = events.Select(MapEvent);

            return Employee.From(id, integrationEvents);
        }

        private IntegrationEvent MapEvent(StoreEvent e)
        {
            var data = JToken.FromObject(e.Data);

            return e.EventType switch
            {
                "Employee.Renamed" => data.ToObject<EmployeeRenamedIntegrationEvent>(),
                "Employee.Promoted" => data.ToObject<EmployeePromotedIntegrationEvent>(),
                _ => throw new InvalidOperationException("Unknow exception"),
            };
        }
    }
}
