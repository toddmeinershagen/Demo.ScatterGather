using MassTransit;

namespace Demo.ScatterGather.Core
{
    public interface IAppSettings
    {
        string Get(string key, bool required = false);
    }
}
