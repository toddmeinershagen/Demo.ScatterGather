using Demo.ScatterGather.Core;
using MassTransit;
using StructureMap;

namespace Demo.ScatterGather.BirthDateValidator
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
                .Use(ctx => provider.GetBusWith<PersonValidationRequestedConsumer>(ctx, "demo-scattergather-birthdate"));
        }
    }
}