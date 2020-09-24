using Meetup.EventSourcing.Api.Models;
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

        private readonly IReadModelService _readModel;

        public EmployeesController(IEventStore eventStore, IReadModelService readModel)
        {
            _eventStore = eventStore ?? throw new ArgumentNullException(nameof(eventStore));
            _readModel = readModel ?? throw new ArgumentNullException(nameof(readModel));
        }

        [HttpGet]
        public async IAsyncEnumerable<EmployeeSummaryViewModel> GetEmployees()
        {
            await foreach (var employee in _readModel.GetEmployees())
            {
                yield return new EmployeeSummaryViewModel
                {
                    Id = employee.Id,
                    Name = employee.Name,
                    Role = employee.Role,
                };
            }
        }

        [HttpPost]
        public async Task<ActionResult<EmployeeViewModel>> CreateEmployee(EmployeeModel model)
        {
            try
            {
                var role = (Role)Enum.Parse(typeof(Role), model.Role);
                var employee = Employee.Create(model.Name, role);

                var events = ((IAggregate)employee).GetIntegrationEvents();
                await _eventStore.PublishEvents(employee.GetStreamName(), events);

                return Ok(new EmployeeViewModel
                {
                    Id = employee.Id,
                    Name = employee.Name,
                    Role = employee.Role.ToString(),
                    Salary = employee.Salary,
                });
            }
            catch (Exception e)
            {
                return BadRequest(new { e.Message });
            }
        }

        [HttpPut("{id:guid}/promote")]
        public async Task<IActionResult> Promote(Guid id)
        {
            var employee = await FetchEmployeeEvents(id);
            employee.Promote();
            var events = ((IAggregate)employee).GetIntegrationEvents();
            await _eventStore.PublishEvents(employee.GetStreamName(), events);

            return NoContent();
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<EmployeeViewModel>> GetEmployee(Guid id)
        {
            try
            {
                var employee = await FetchEmployeeEvents(id);

                return Ok(new EmployeeViewModel
                {
                    Id = employee.Id,
                    Name = employee.Name,
                    Role = employee.Role.ToString(),
                    Salary = employee.Salary,
                });
            }
            catch (Exception e)
            {
                return BadRequest(new { e.Message });
            }
        }

        private async Task<Employee> FetchEmployeeEvents(Guid id)
        {
            string stream = $"Employee-" + id;
            var events = new List<StoreEvent>();

            await foreach (var e in _eventStore.GetEvents(stream))
            {
                events.Add(e);
            }

            var integrationEvents = events.Select(ParseEvent);

            return Employee.From(id, integrationEvents);
        }

        private IntegrationEvent ParseEvent(StoreEvent e)
        {
            var data = JToken.FromObject(e.Data);
            return e.EventType switch
            {
                "Employee.Renamed" => data.ToObject<EmployeeRenamedIntegrationEvent>(),
                "Employee.Promoted" => data.ToObject<EmployeePromotedIntegrationEvent>(),
                _ => throw new InvalidOperationException("Unknown event"),
            };
        }
    }
}
