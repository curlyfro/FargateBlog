using Amazon.CDK;
using FargateBlog.Core;

namespace FargateBlog.Infrastructure;

sealed class Program
{
    public static void Main(string[] args)
    {
        var app = new App();
        var env = MakeEnv();

        var iamRoleStack = new IamRoleStack(app, $"{Constants.AppName.ToHypenCase()}-iam-role", new StackProps { Env = env });
        var networkStack = new NetworkStack(app, $"{Constants.AppName.ToHypenCase()}-network", new NetworkStackProps
        {
            Env = env,
            EcsTaskExecutionRole = iamRoleStack.EcsTaskExecutionRole
        });

        var s3Stack = new S3Stack(app, $"{Constants.AppName.ToHypenCase()}-s3", new S3StackProps
        {
            Env = env,
            Vpc = networkStack.Vpc,
            VpcSecurityGroup = networkStack.VpcSecurityGroup,
            LambdaRole = iamRoleStack.LambdaRole
        });

        app.Synth();
    }

    private static Environment MakeEnv(string account = null, string region = null)
    {
        return new Environment
        {
            Account = account ??
                System.Environment.GetEnvironmentVariable("CDK_DEPLOY_ACCOUNT") ??
                System.Environment.GetEnvironmentVariable("CDK_DEFAULT_ACCOUNT"),
            Region = region ??
                System.Environment.GetEnvironmentVariable("CDK_DEPLOY_REGION") ??
                System.Environment.GetEnvironmentVariable("CDK_DEFAULT_REGION")
        };
    }
}
