using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;
using System.Web.Http;
using Demo.ScatterGather.Aggregator.Service.Models;
using Demo.ScatterGather.Core.Events;
using MassTransit;
using Microsoft.ServiceBus;

namespace Demo.ScatterGather.Aggregator.Service.Controllers
{
    [Route("api/PersonValidations")]
    public class PersonValidationsController : ApiController
    {
        private readonly IBus _bus;

        public PersonValidationsController(IBus bus)
        {
            _bus = bus;
        }

        public async Task<IHttpActionResult> Post([FromBody] Person request)
        {
            var requestId = Guid.NewGuid();

            var serviceNamespace = ConfigurationManager.AppSettings["Microsoft.ServiceBus.Namespace"];
            var requestTimeout = TimeSpan.FromSeconds(30);

            var results = new List<PersonValidated>();
            var validators = ConfigurationManager.AppSettings["Validators"].Split(',');

            Parallel.ForEach(validators, validator =>
            {
                var serviceUri = ServiceBusEnvironment.CreateServiceUri("sb", serviceNamespace, $"CommonGround.Scrape.Service/{validator}");
                var client = _bus.CreateRequestClient<PersonValidationRequested, PersonValidated>(serviceUri, requestTimeout);
                var result = client.Request(new PersonValidationRequested {RequestId = requestId, Person = request}).Result;
                results.Add(result);
            });

            return await Task.FromResult(Created(Request.RequestUri + requestId.ToString(),
                new PersonValidationResult { RequestId = requestId, Results = results }));
        }
    }
}
