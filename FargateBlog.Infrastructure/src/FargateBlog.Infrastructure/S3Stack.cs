using Amazon.CDK;
using Constructs;
using Amazon.CDK.AWS.S3;
using FargateBlog.Core;
using Amazon.CDK.AWS.Lambda;
using Amazon.CDK.AWS.Lambda.EventSources;
using System.Collections.Generic;

namespace FargateBlog.Infrastructure;

public class S3Stack : Stack
{
    internal S3Stack(Construct scope, string id, S3StackProps props = null) : base(scope, id, props)
    {
        var originalsName = $"{Constants.AppName.ToHypenCase()}-originals";
        var originalsBucket = new Bucket(this, originalsName, new BucketProps
        {
            BucketName = originalsName
        });

        var s3PutEventSource = new S3EventSource(originalsBucket, new S3EventSourceProps
        {
            Events = new[] { EventType.OBJECT_CREATED }
        });

        var s3TriggerFunction = S3TriggerFunction(props);
        s3TriggerFunction.AddEventSource(s3PutEventSource);

        var encodedName = $"{Constants.AppName.ToHypenCase()}-encoded";
        var bucket = new Bucket(this, encodedName, new BucketProps
        {
            BucketName = encodedName
        });
    }

    private Function S3TriggerFunction(S3StackProps props)
    {
        var environment = new Dictionary<string, string> {
            { "privateSubnet1", props.Vpc.PrivateSubnets[0].SubnetId },
            { "privateSubnet2", props.Vpc.PrivateSubnets[1].SubnetId },
            { "vpcSecurityGroup", props.VpcSecurityGroup.SecurityGroupId }
        };

        var functionName = $"{Constants.AppName.ToHypenCase()}-encoding-lambda";
        var asset = $"src/{Constants.AppName}.Lambdas.CreateEncodingTask/bin/Release/net6.0/publish";
        var handler = $"{Constants.AppName}.Lambdas.CreateEncodingTask::{Constants.AppName}.Lambdas.CreateEncodingTask.Function::FunctionHandler";
        var encodeLambdaFunction = new Function(this, functionName, new FunctionProps
        {
            FunctionName = functionName,
            Role = props.LambdaRole,
            Runtime = Runtime.DOTNET_6,
            Code = Code.FromAsset(asset),
            Handler = handler,
            Environment = environment
        });
        return encodeLambdaFunction;
    }
}