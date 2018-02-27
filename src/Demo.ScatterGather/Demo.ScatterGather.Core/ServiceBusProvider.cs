using System;
using Demo.ScatterGather.Core.Events;
using GreenPipes;
using MassTransit;
using MassTransit.AzureServiceBusTransport;
using MassTransit.AzureServiceBusTransport.Configuration;
using Microsoft.ServiceBus;
using StructureMap;

namespace Demo.ScatterGather.Core
{
    public class ServiceBusProvider
    {
        public IBusControl GetBusWith<TConsumer>(IContext context, string queueName) where TConsumer : class, IConsumer
        {
            return GetBus(context, (cfg, host, workerLimit) =>
            {
                cfg.ReceiveEndpoint(host, queueName, ep =>
                {
                    ep.PrefetchCount = workerLimit;
                    ep.MaxConcurrentCalls = workerLimit;
                    ep.Consumer(context.GetInstance<TConsumer>,
                        c =>
                        {
                            c.UseConcurrencyLimit(workerLimit);
                        });
                });
            });
        }

        public IBusControl GetBus(IContext context)
        {
            return GetBus(context, null);
        }

        private IBusControl GetBus(IContext context,
            Action<IServiceBusBusFactoryConfigurator, IServiceBusHost, int> configureReceiveEndpoint = null)
        {
            var appSettings = context.GetInstance<IAppSettings>();
            var serviceNamespace = appSettings.Get("Microsoft.ServiceBus.Namespace", true);
            var sharedAccessKey = appSettings.Get("Microsoft.ServiceBus.SharedAccessKey", true);
            var workerLimit = appSettings.Get("WorkerLimit").Parse<int>() ?? 10;

            var busControl = Bus.Factory.CreateUsingAzureServiceBus(cfg =>
            {
                var serviceUri =
                    ServiceBusEnvironment.CreateServiceUri("sb", serviceNamespace, "CommonGround.Scrape.Service");
                var tokenProvider =
                    TokenProvider.CreateSharedAccessSignatureTokenProvider("RootManageSharedAccessKey",
                        sharedAccessKey);

                var host = ServiceBusBusFactoryConfiguratorExtensions.Host(cfg, serviceUri, h =>
                {
                    h.TokenProvider = tokenProvider;
                    h.OperationTimeout = TimeSpan.FromMinutes(4);
                    h.RetryLimit = 1;
                });

                cfg.MaxConcurrentCalls = workerLimit;
                cfg.PrefetchCount = workerLimit;
                cfg.UseConcurrencyLimit(workerLimit);

                configureReceiveEndpoint?.Invoke(cfg, host, workerLimit);
            });

            var observer = new ReceiveObserver();
            busControl.ConnectReceiveObserver(observer);

            return busControl;
        }
    }
}