namespace FargateBlog.Core.Interfaces;

public interface IS3Repository
{
    Task DeleteAllObjectsAsync(string bucketName);
    Task<bool> DoesS3ObjectExist(string bucketName, string key);
    string GetPreSignedUrl(string bucketName, string key, int expiresInMinutes);
    Task PutObjectAsync(string bucketName, string key, string body);
}