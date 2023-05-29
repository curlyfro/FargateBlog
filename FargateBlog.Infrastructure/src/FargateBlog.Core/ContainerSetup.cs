using Amazon;
using Amazon.ECS;
using Amazon.S3;
using FargateBlog.Core.Interfaces;
using FargateBlog.Core.Repositories;
using Microsoft.Extensions.DependencyInjection;


namespace FargateBlog.Core;

public static class ContainerSetup
{
    public static IServiceCollection ConfigureS3(
      this IServiceCollection services)
    {
        try
        {
            var region = RegionEndpoint.GetBySystemName(Constants.Region);
            services.AddSingleton<AmazonS3Client>(new AmazonS3Client(region));
            services.AddTransient<IS3Repository, S3Repository>();

            return services;
        }
        catch (Exception)
        {
            throw;
        }
    }

    public static IServiceCollection ConfigureEcs(
        this IServiceCollection services)
    {
        try
        {
            var region = RegionEndpoint.GetBySystemName(Constants.Region);
            services.AddSingleton<AmazonECSClient>(new AmazonECSClient(region));
            services.AddTransient<IEcsRepository, EcsRepository>();

            return services;
        }
        catch (Exception)
        {
            throw;
        }
    }
}
