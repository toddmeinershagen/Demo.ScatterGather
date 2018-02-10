using System;
using System.Threading;
using System.Web.Http;
using MassTransit;
using MassTransit.Util;
using Microsoft.Owin;
using Microsoft.Owin.BuilderProperties;
using Owin;
using WebApi.StructureMap;

[assembly: OwinStartup(typeof(Demo.ScatterGather.Aggregator.Service.Startup))]

namespace Demo.ScatterGather.Aggregator.Service
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var config = new HttpConfiguration();
            WebApiConfig.Register(config);

            config.UseStructureMap<ValidatorRegistry>();
            app.UseWebApi(config);

            var bus = config.DependencyResolver.GetService<IBusControl>();
            var busHandle = TaskUtil.Await(() => bus.StartAsync());
            var properties = new AppProperties(app.Properties);

            if (properties.OnAppDisposing != CancellationToken.None)
            {
                properties.OnAppDisposing.Register(() => busHandle.Stop(TimeSpan.FromSeconds(30)));
            }
        }
    }
}
