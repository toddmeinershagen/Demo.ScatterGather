using Demo.ScatterGather.Core;
using MassTransit;
using StructureMap;

namespace Demo.ScatterGather.Claims.CptValidator
{
    public class ValidatorRegistry : Registry
    {
        public ValidatorRegistry()
        {
            Scan(scan =>
            {
                scan.AssembliesFromApplicationBaseDirectory();
                scan.SingleImplementationsOfInterface();
            });

            var provider = new ServiceBusProvider();

            For<IBusControl>()
                .Use(ctx => provider.GetBusWith<ClaimValidationRequestedConsumer>(ctx,
                    "demo-scattergather-claims-cpt-rule"));
        }
    }
}