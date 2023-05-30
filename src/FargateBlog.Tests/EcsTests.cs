using System;
using Tests;
using FargateBlog.Core;

namespace FargateBlog.Tests;

public class EcsTests : BaseSetup
{
    [TestCase("fargate-blog-originals", "test_1.mp4", "subnet-09bd32636ad8f0064", "subnet-01450578db17c4866", "sg-061d1edaff3dde3ba")]
    public async Task can_run_task(
        string bucketName,
        string key,
        string privateSubnet1,
        string privateSubnet2,
        string vpcSecurityGroup
        )
    {
        try
        {
            var environment = new List<Amazon.ECS.Model.KeyValuePair>();
            var envEnableMetaData = new Amazon.ECS.Model.KeyValuePair() { Name = "ECS_ENABLE_CONTAINER_METADATA", Value = "true" };
            var envBuckeName = new Amazon.ECS.Model.KeyValuePair() { Name = "BuckeName", Value = bucketName };
            var envKey = new Amazon.ECS.Model.KeyValuePair() { Name = "ObjectKey", Value = key };
            var envPrivateSubnet1 = new Amazon.ECS.Model.KeyValuePair() { Name = "privateSubnet1", Value = privateSubnet1 };
            var envPrivateSubnet2 = new Amazon.ECS.Model.KeyValuePair() { Name = "privateSubnet2", Value = privateSubnet2 };
            var envpcSecurityGroup = new Amazon.ECS.Model.KeyValuePair() { Name = "vpcSecurityGroup", Value = vpcSecurityGroup };
            environment.Add(envEnableMetaData);
            environment.Add(envBuckeName);
            environment.Add(envKey);
            environment.Add(envPrivateSubnet1);
            environment.Add(envPrivateSubnet2);
            environment.Add(envpcSecurityGroup);

            var encodingTaskDef = $"{Constants.AppName.ToHypenCase()}-encoding-task-def";
            var encodingContainer = $"{Constants.AppName.ToHypenCase()}-encoding-container";
            var response = await _ecsRepository.RunTaskAsync(encodingTaskDef, encodingContainer, environment);
        }
        catch (Exception)
        {
            throw;
        }
    }

    [TestCase("ThisisATest")]
    [TestCase("ThisIsATest")]
    [TestCase("FargateBlog")]
    [TestCase("FargateBlog-repository")]
    [TestCase("Fargate-Blog-repository")]
    [TestCase("Fargate-blog-repository")]
    [TestCase("fargate-blog-repository")]
    public void can_convert_to_hypen_case(string str)
    {
        var output = str.ToHypenCase();
        Console.WriteLine(output);
    }
}