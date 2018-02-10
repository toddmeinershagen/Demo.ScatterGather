using System;
using Demo.ScatterGather.Core;
using StructureMap;
using Topshelf;

namespace Demo.ScatterGather.EyeValidator
{
    class Program
    {
        static void Main(string[] args)
        {
            Func<IContainer> containerFactory = () => new Container(_ =>
            {
                _.AddRegistry<ValidatorRegistry>();
            });

            HostFactory.Run(x =>
            {
                x.Service(() => new ValidatorService(containerFactory));
            });
        }
    }
}
