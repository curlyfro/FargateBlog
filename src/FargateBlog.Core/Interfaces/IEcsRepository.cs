using Amazon.ECS.Model;

namespace FargateBlog.Core.Interfaces;

public interface IEcsRepository
{
    public Task<DescribeTasksResponse> DescribeTasksAsync(string taskArn);

    public Task<RunTaskResponse> RunTaskAsync(
        string taskDefinition, 
        string containerName, 
        List<Amazon.ECS.Model.KeyValuePair> environment = null, 
        string body = null);
    
    public Task<StopTaskResponse> StopTaskAsync(string taskId);
}