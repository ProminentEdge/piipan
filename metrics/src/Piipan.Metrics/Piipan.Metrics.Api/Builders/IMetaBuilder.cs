using Piipan.Metrics.Models;

namespace Piipan.Metrics.Api.Builders
{
    public interface IMetaBuilder
    {
        Meta Build();
        IMetaBuilder SetPage(int page);
        IMetaBuilder SetPerPage(int perPage);
        IMetaBuilder SetState(string state);
    }
}