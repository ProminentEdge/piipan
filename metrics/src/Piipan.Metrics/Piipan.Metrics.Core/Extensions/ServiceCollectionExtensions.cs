using Microsoft.Extensions.DependencyInjection;
using Piipan.Metrics.Api;
using Piipan.Metrics.Core.DataAccessObjects;
using Piipan.Metrics.Core.Services;

namespace Piipan.Metrics.Core.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void RegisterCoreServices(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<IParticipantUploadDao, ParticipantUploadDao>();
            serviceCollection.AddTransient<IParticipantUploadApi, ParticipantUploadService>();
        }
    }
}