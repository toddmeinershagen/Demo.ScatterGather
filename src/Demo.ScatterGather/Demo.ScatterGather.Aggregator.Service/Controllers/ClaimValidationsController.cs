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
    [Route("api/ClaimValidations")]
    public class ClaimValidationsController : ApiController
    {
        private readonly IBus _bus;

        public ClaimValidationsController(IBus bus)
        {
            _bus = bus;
        }

        public async Task<IHttpActionResult> Post([FromBody] Claim claim)
        {
            var requestId = Guid.NewGuid();

            var serviceNamespace = ConfigurationManager.AppSettings["Microsoft.ServiceBus.Namespace"];
            var requestTimeout = TimeSpan.FromSeconds(30);

            var results = new List<ClaimValidated>();
            var validators = ConfigurationManager.AppSettings["ClaimValidators"].Split(',');

            await Task.Run(() =>
            {
                Parallel.ForEach(validators, validator =>
                {
                    var serviceUri = ServiceBusEnvironment.CreateServiceUri("sb", serviceNamespace, $"CommonGround.Scrape.Service/{validator}");
                    var client = _bus.CreateRequestClient<ClaimValidationRequested, ClaimValidated>(serviceUri, requestTimeout);
                    var result = client.Request(new ClaimValidationRequested {RequestId = requestId, Claim = claim}).Result;
                    results.Add(result);
                });
            });

            return await Task.FromResult(Created(Request.RequestUri + requestId.ToString(),
                new ClaimValidationResult { RequestId = requestId, Results = results }));
        }
    }
}
