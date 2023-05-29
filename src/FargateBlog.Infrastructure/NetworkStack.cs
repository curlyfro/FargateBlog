using Amazon.CDK;
using Constructs;
using Amazon.CDK.AWS.EC2;
using FargateBlog.Core;
using Amazon.CDK.AWS.ECR;
using Amazon.CDK.AWS.ECS;

namespace FargateBlog.Infrastructure;

public class NetworkStack : Stack
{
    public Vpc Vpc { get; private set; }
    public SecurityGroup VpcSecurityGroup { get; private set; }

    internal NetworkStack(Construct scope, string id, NetworkStackProps props = null) : base(scope, id, props)
    {
        var vpnName = $"{Constants.AppName.ToHypenCase()}-vpc";
        Vpc = new Vpc(this, vpnName, new VpcProps
        {
            VpcName = vpnName,
            IpAddresses = IpAddresses.Cidr("10.0.0.0/16"),
            MaxAzs = 2,
            NatGateways = 1,
            SubnetConfiguration = new[]
            {
                new SubnetConfiguration
                {
                    Name = $"{Constants.AppName.ToHypenCase()}-public-",
                    CidrMask = 24,
                    SubnetType = SubnetType.PUBLIC,
                },
                new SubnetConfiguration
                {
                    Name = $"{Constants.AppName.ToHypenCase()}-private-",
                    CidrMask = 24,
                    SubnetType = SubnetType.PRIVATE_WITH_EGRESS,
                }
            }
        });

        var vpcSecurityGroupName = $"{Constants.AppName.ToHypenCase()}-vpc-security-group";
        var vpcSecurityGroup = new SecurityGroup(this, vpcSecurityGroupName, new SecurityGroupProps
        {
            Vpc = Vpc,
            SecurityGroupName = vpcSecurityGroupName,
            AllowAllOutbound = true,
        });
        VpcSecurityGroup = vpcSecurityGroup;

        var repositoryName = $"{Constants.AppName.ToHypenCase()}-repository";
        var repository = new Repository(this, repositoryName, new RepositoryProps
        {
            RepositoryName = repositoryName,
        });

        var clusterName = $"{Constants.AppName.ToHypenCase()}-cluster";
        new Cluster(this, clusterName, new ClusterProps
        {
            ClusterName = clusterName,
            Vpc = Vpc
        });

        var taskDefinitionName = $"{Constants.AppName.ToHypenCase()}-encoding-task-def";
        var taskDefinition = new FargateTaskDefinition(this, taskDefinitionName, new FargateTaskDefinitionProps
        {
            Family = taskDefinitionName,
            RuntimePlatform = new RuntimePlatform
            {
                CpuArchitecture = CpuArchitecture.X86_64,
                OperatingSystemFamily = OperatingSystemFamily.LINUX
            },
            TaskRole = props.EcsTaskExecutionRole,
            ExecutionRole = props.EcsTaskExecutionRole,
            Cpu = 4096,
            MemoryLimitMiB = 8192,
        });

        try
        {
            var accountId = System.Environment.GetEnvironmentVariable("CDK_DEPLOY_ACCOUNT") ??
                           System.Environment.GetEnvironmentVariable("CDK_DEFAULT_ACCOUNT");
            var containerName = $"{Constants.AppName.ToHypenCase()}-encoding-container";
            taskDefinition.AddContainer(containerName, new ContainerDefinitionOptions
            {
                Image = ContainerImage.FromRegistry($"{accountId}.dkr.ecr.{Constants.Region.ToLower()}.amazonaws.com/{repositoryName}:latest"),
            });
        }
        catch (System.Exception)
        {
        }
    }
}