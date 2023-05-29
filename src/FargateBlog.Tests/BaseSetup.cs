using Amazon;
using Amazon.DynamoDBv2;
using Amazon.ECS;
using Amazon.S3;
using FargateBlog.Core;
using FargateBlog.Core.Interfaces;
using FargateBlog.Core.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Tests;

[TestFixture]
public class BaseSetup
{
    protected ServiceProvider _serviceProvider;
    protected AmazonDynamoDBClient _dbClient;
    protected IS3Repository _s3Repository;
    protected IEcsRepository _ecsRepository;

    [SetUp]
    public void Setup()
    {
        var services = new ServiceCollection();
        _serviceProvider = services.BuildServiceProvider();

        var dynamoDbConfig = new AmazonDynamoDBConfig();
         _dbClient = new AmazonDynamoDBClient(dynamoDbConfig);
        var region = RegionEndpoint.GetBySystemName(Constants.Region);
        var s3Client = new AmazonS3Client(region);
        var ecsClient = new AmazonECSClient(region);

        _s3Repository = new S3Repository(s3Client, null);
        _ecsRepository = new EcsRepository(ecsClient, null);
    }
}
