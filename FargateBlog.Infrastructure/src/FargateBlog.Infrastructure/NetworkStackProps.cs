using Amazon.CDK;
using Amazon.CDK.AWS.IAM;

namespace FargateBlog.Infrastructure;

public class NetworkStackProps : StackProps
{
    public Role EcsTaskExecutionRole { get; internal set; }
}
