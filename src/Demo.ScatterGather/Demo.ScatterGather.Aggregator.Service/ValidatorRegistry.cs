using Demo.ScatterGather.Core;
using MassTransit;
using Microsoft.ServiceBus.Messaging;
using StructureMap;

namespace Demo.ScatterGather.Aggregator.Service
{
    public class ValidatorRegistry : Registry
    {
        public ValidatorRegistry()
        {
            Scan(scan =>
            {
                scan.SingleImplementationsOfInterface();
                scan.AssembliesFromApplicationBaseDirectory();
            });

            ForSingletonOf<IBusControl>()
                .Use(ctx => new ServiceBusProvider().GetBus(ctx));
            For<IBus>()
                .Use(c => c.GetInstance<IBusControl>());

        }
    }
}