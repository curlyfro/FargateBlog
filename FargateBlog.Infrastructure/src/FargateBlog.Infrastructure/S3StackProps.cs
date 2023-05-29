using Amazon.CDK;
using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.IAM;

namespace FargateBlog.Infrastructure;

public class S3StackProps : StackProps
{
    public Role LambdaRole { get; internal set; }
    public Vpc Vpc { get; internal set; }
    public SecurityGroup VpcSecurityGroup { get; internal set; }
}
