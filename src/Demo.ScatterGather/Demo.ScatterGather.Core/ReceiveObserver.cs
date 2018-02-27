using System;
using System.Threading.Tasks;
using MassTransit;

namespace Demo.ScatterGather.Core
{
    public class ReceiveObserver : IReceiveObserver
    {
        public async Task PreReceive(ReceiveContext context)
        {

        }

        public async Task PostReceive(ReceiveContext context)
        {
        }

        public async Task PostConsume<T>(ConsumeContext<T> context, TimeSpan duration, string consumerType)
            where T : class
        {
            await Console.Out.WriteLineAsync($"Request received for {consumerType}.");
        }

        public async Task ConsumeFault<T>(ConsumeContext<T> context, TimeSpan duration, string consumerType,
            Exception exception) where T : class
        {

        }

        public async Task ReceiveFault(ReceiveContext context, Exception exception)
        {
        }
    }
}