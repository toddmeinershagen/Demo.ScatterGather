using System;
using MassTransit;
using StructureMap;
using Topshelf;

namespace Demo.ScatterGather.Core
{
    public class ValidatorService : ServiceControl
    {
        private readonly Func<IContainer> _containerFactory;
        private IContainer _container;
        private IBusControl _bus;

        public ValidatorService(Func<IContainer> containerFactory)
        {
            Guard.AgainstNull(containerFactory, nameof(containerFactory));
            _containerFactory = containerFactory;
        }

        public bool Start(HostControl hostControl)
        {
            _container = _containerFactory();
            _bus = _container.GetInstance<IBusControl>();
            _bus.Start();

             return true;
        }

        public bool Stop(HostControl hostControl)
        {
            _bus?.Stop(TimeSpan.FromSeconds(15));
            _container?.Dispose();

            return true;
        }
    }
}