using Microsoft.Extensions.Logging;
using FargateBlog.Core.Interfaces;
using Amazon.ECS.Model;
using Amazon.ECS;

namespace FargateBlog.Core.Repositories;

public class EcsRepository : IEcsRepository
{
    private readonly AmazonECSClient _client;
    private readonly ILogger<EcsRepository> _logger;

    public EcsRepository(
        AmazonECSClient client,
        ILogger<EcsRepository> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<RunTaskResponse> RunTaskAsync(
     string taskDefinition,
     string containerName,
     List<Amazon.ECS.Model.KeyValuePair> environment,
     string body
     )
    {
        RunTaskResponse response = null;

        try
        {
            Console.WriteLine($"in RunTaskAsync  **********************");

            var clusterName = $"{Constants.AppName.ToHypenCase()}-cluster";

            var privateSubnet1 = environment.First(x => x.Name == "privateSubnet1").Value;
            var privateSubnet2 = environment.First(x => x.Name == "privateSubnet2").Value;
            var vpcSecurityGroup = environment.First(x => x.Name == "vpcSecurityGroup").Value;

            Console.WriteLine($"privateSubnet1: {privateSubnet1}");
            Console.WriteLine($"privateSubnet2: {privateSubnet2}");
            Console.WriteLine($"vpcSecurityGroup: {vpcSecurityGroup}");
            var envBuckeName = environment.First(x => x.Name == "BuckeName").Value;
            var envKey = environment.First(x => x.Name == "ObjectKey").Value;
            Console.WriteLine($"envBuckeName: {envBuckeName}");
            Console.WriteLine($"envKey: {envKey}");

            var runTask = new RunTaskRequest()
            {
                LaunchType = LaunchType.FARGATE,
                TaskDefinition = taskDefinition,
                Cluster = clusterName,
                Count = 1,
                StartedBy = "SYSTEM",
                EnableECSManagedTags = true,

                NetworkConfiguration = new NetworkConfiguration()
                {
                    AwsvpcConfiguration = new AwsVpcConfiguration()
                    {
                        Subnets = $"{privateSubnet1}, {privateSubnet2}".Split(',').ToList(),
                        AssignPublicIp = AssignPublicIp.ENABLED,
                        SecurityGroups = new List<string>() { vpcSecurityGroup }
                    }
                },
                Overrides = new TaskOverride()
                {
                    TaskRoleArn = @"arn:aws:iam::676229420717:role/ecsTaskExecutionRole",
                    ExecutionRoleArn = @"arn:aws:iam::676229420717:role/ecsTaskExecutionRole",
                    ContainerOverrides = new List<ContainerOverride>()
                    {
                       new ContainerOverride()
                       {
                          Name = containerName,
                          Environment = environment
                       }
                    }
                }
            };

            Console.WriteLine($"completed configuring");

            response = await _client.RunTaskAsync(runTask);

            Console.WriteLine($"############# RAN RunTaskAsync **********************");

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error*********************");
            Console.WriteLine(ex.Message);
        }

        return response;
    }

    public async Task<StopTaskResponse> StopTaskAsync(string taskId)
    {
        var stopTask = new StopTaskRequest
        {
            Cluster = Constants.EncodingClusterName,
            Task = taskId
        };

        StopTaskResponse response = await _client.StopTaskAsync(stopTask);
        return response;
    }

    public async Task<DescribeTasksResponse> DescribeTasksAsync(string taskArn)
    {
        var describeTasksRequest = new DescribeTasksRequest()
        {
            Cluster = Constants.EncodingClusterName,
            Tasks = new List<string> { taskArn },
        };

        var describeTasksResponse = await _client.DescribeTasksAsync(describeTasksRequest);
        return describeTasksResponse;
    }
}
