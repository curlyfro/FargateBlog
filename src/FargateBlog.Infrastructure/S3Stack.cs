using Amazon.CDK;
using Constructs;
using Amazon.CDK.AWS.S3;
using FargateBlog.Core;
using Amazon.CDK.AWS.Lambda;
using Amazon.CDK.AWS.Lambda.EventSources;
using System.Collections.Generic;
using Amazon.CDK.AWS.EC2;

namespace FargateBlog.Infrastructure;

public class S3Stack : Stack
{
    internal S3Stack(Construct scope, string id, S3StackProps props = null) : base(scope, id, props)
    {
        var originalsBucketName = $"{Constants.AppName.ToHypenCase()}-originals-{props.Env.Account}-{props.Env.Region.ToHypenCase()}";
        var originalsBucket = new Bucket(this, originalsBucketName, new BucketProps
        {
            BucketName = originalsBucketName
        });
        props.OriginalsBucketName = originalsBucketName;

        var encodedBucketName = $"{Constants.AppName.ToHypenCase()}-encoded-{props.Env.Account}-{props.Env.Region.ToHypenCase()}";
        var encodedBucke = new Bucket(this, encodedBucketName, new BucketProps
        {
            BucketName = encodedBucketName
        });
        props.EncodedBucketName = encodedBucketName;

        var s3PutEventSource = new S3EventSource(originalsBucket, new S3EventSourceProps
        {
            Events = new[] { EventType.OBJECT_CREATED }
        });

        var s3TriggerFunction = S3TriggerFunction(props);
        s3TriggerFunction.AddEventSource(s3PutEventSource);

        var s3GatewayEndpointName = $"{Constants.AppName.ToHypenCase()}-s3-endpoint";
        var s3GatewayEndpoint = props.Vpc.AddGatewayEndpoint(s3GatewayEndpointName, new GatewayVpcEndpointOptions
        {
            Service = GatewayVpcEndpointAwsService.S3,
        });
    }

    private Function S3TriggerFunction(S3StackProps props)
    {
        var environment = new Dictionary<string, string> {
            { "privateSubnet1", props.Vpc.PrivateSubnets[0].SubnetId },
            { "privateSubnet2", props.Vpc.PrivateSubnets[1].SubnetId },
            { "vpcSecurityGroup", props.VpcSecurityGroup.SecurityGroupId },
            { "originalsBucketName", props.OriginalsBucketName },
            { "encodedBucketName", props.EncodedBucketName }
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
            Environment = environment,
            MemorySize = 512
        });
        return encodeLambdaFunction;
    }
}