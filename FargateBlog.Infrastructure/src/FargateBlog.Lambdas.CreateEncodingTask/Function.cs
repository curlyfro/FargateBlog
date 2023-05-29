using Amazon.Lambda.Core;
using Amazon.Lambda.S3Events;
using FargateBlog.Core;
using FargateBlog.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;


[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace FargateBlog.Lambdas.CreateEncodingTask;

public class Function
{
    private readonly IEcsRepository _ecsRepository;

    public Function()
    {
        try
        {
            var serviceCollection = new ServiceCollection()
            .AddLogging()
            .ConfigureEcs();
            var serviceProvider = serviceCollection.BuildServiceProvider();

            _ecsRepository = serviceProvider.GetRequiredService<IEcsRepository>();
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task FunctionHandler(S3Event evnt, ILambdaContext context)
    {
        var privateSubnet1 = Environment.GetEnvironmentVariable("privateSubnet1") ?? "";
        var privateSubnet2 = Environment.GetEnvironmentVariable("privateSubnet2") ?? "";
        var vpcSecurityGroup = Environment.GetEnvironmentVariable("vpcSecurityGroup") ?? "";

        foreach (S3Event.S3EventNotificationRecord? message in evnt.Records)
        {
            await CreateEcsTaskAsync(privateSubnet1, privateSubnet2, vpcSecurityGroup, message, context);
        }
    }

    private async Task CreateEcsTaskAsync(
        string privateSubnet1,
        string privateSubnet2,
        string vpcSecurityGroup,
        S3Event.S3EventNotificationRecord message,
        ILambdaContext context)
    {
        try
        {
            var environment = new List<Amazon.ECS.Model.KeyValuePair>();
            var envEnableMetaData = new Amazon.ECS.Model.KeyValuePair() { Name = "ECS_ENABLE_CONTAINER_METADATA", Value = "true" };
            var envBuckeName = new Amazon.ECS.Model.KeyValuePair() { Name = "BuckeName", Value = message.S3.Bucket.Name };
            var envKey = new Amazon.ECS.Model.KeyValuePair() { Name = "ObjectKey", Value = Path.GetFileNameWithoutExtension(message.S3.Object.Key) };
            var envPrivateSubnet1 = new Amazon.ECS.Model.KeyValuePair() { Name = "privateSubnet1", Value = privateSubnet1 };
            var envPrivateSubnet2 = new Amazon.ECS.Model.KeyValuePair() { Name = "privateSubnet2", Value = privateSubnet2 };
            var envpcSecurityGroup = new Amazon.ECS.Model.KeyValuePair() { Name = "vpcSecurityGroup", Value = vpcSecurityGroup };
            environment.Add(envEnableMetaData);
            environment.Add(envBuckeName);
            environment.Add(envKey);
            environment.Add(envPrivateSubnet1);
            environment.Add(envPrivateSubnet2);
            environment.Add(envpcSecurityGroup);

            Console.WriteLine($"key.Value: {envKey.Value}");

            var encodingTaskDef = $"{Constants.AppName.ToHypenCase()}-encoding-task-def";
            var encodingContainer = $"{Constants.AppName.ToHypenCase()}-encoding-container";
            var response = await _ecsRepository.RunTaskAsync(encodingTaskDef, encodingContainer, environment); 
            
            context.Logger.LogInformation($"called run task...");

            if (response.HttpStatusCode != System.Net.HttpStatusCode.OK)
                throw new Exception(response.HttpStatusCode.ToString());

            if (response.Tasks == null)
                throw new Exception("Failed to create task");

            Amazon.ECS.Model.Task? task = response.Tasks.FirstOrDefault();
            if (task == null)
                throw new Exception("Failed to create task");

            var taskArn = task.TaskArn;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
            throw;
        }

        await Task.CompletedTask;
    }
}