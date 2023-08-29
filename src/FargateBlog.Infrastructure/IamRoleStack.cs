using Amazon.CDK;
using Amazon.CDK.AWS.IAM;
using Constructs;
using FargateBlog.Core;

namespace FargateBlog.Infrastructure;

public class IamRoleStack : Stack
{
    public Role LambdaRole { get; set; }
    public Role EcsTaskExecutionRole { get; set; }

    internal IamRoleStack(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
    {
        var lambdaRoleName = $"{Constants.AppName.ToHypenCase()}-{Constants.Region}-lambda-role";
        var lambdaRole = new Role(this, lambdaRoleName, new RoleProps
        {
            RoleName = lambdaRoleName,
            AssumedBy = new ServicePrincipal("lambda.amazonaws.com"),
            ManagedPolicies = new[]
            {
                ManagedPolicy.FromAwsManagedPolicyName("CloudWatchFullAccess"),
                ManagedPolicy.FromAwsManagedPolicyName("AWSLambdaExecute"),
                ManagedPolicy.FromAwsManagedPolicyName("AmazonS3FullAccess"),
                ManagedPolicy.FromAwsManagedPolicyName("AmazonECS_FullAccess")
            }
        });
        LambdaRole = lambdaRole;

        var ecsTaskExecutionRoleName = $"{Constants.AppName.ToHypenCase()}-{Constants.Region}-task-execution-role";
        var ecsTaskExecutionRole = new Role(this, ecsTaskExecutionRoleName, new RoleProps
        {
            RoleName = ecsTaskExecutionRoleName,
            AssumedBy = new ServicePrincipal("ecs-tasks.amazonaws.com"),
            ManagedPolicies = new[]
            {
                ManagedPolicy.FromAwsManagedPolicyName("CloudWatchFullAccess"),
                ManagedPolicy.FromAwsManagedPolicyName("AmazonS3FullAccess"),
                ManagedPolicy.FromAwsManagedPolicyName("AmazonECS_FullAccess"),
                ManagedPolicy.FromAwsManagedPolicyName("AWSLambdaExecute"),
               // ManagedPolicy.FromAwsManagedPolicyName("service-role/AmazonECSTaskExecutionRolePolicy"),
            }
        });
        EcsTaskExecutionRole = ecsTaskExecutionRole;
    }
}