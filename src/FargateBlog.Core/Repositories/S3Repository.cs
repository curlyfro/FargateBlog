using Amazon.S3;
using Amazon.S3.Model;
using FargateBlog.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace FargateBlog.Core.Repositories;

public class S3Repository : IS3Repository
{
    private readonly AmazonS3Client _client;
    private readonly ILogger<S3Repository> _logger;

    public S3Repository(
        AmazonS3Client client,
        ILogger<S3Repository> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task PutObjectAsync(
        string bucketName,
        string key,
        string body)
    {
        try
        {
            PutObjectRequest putObjectRequest = new PutObjectRequest
            {
                BucketName = bucketName,
                Key = key,
                ContentBody = body,
                StorageClass = S3StorageClass.IntelligentTiering,
            };

            await _client.PutObjectAsync(putObjectRequest);
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task DeleteAllObjectsAsync(string bucketName)
    {
        ListObjectsV2Request request = new()
        {
            BucketName = bucketName,
            MaxKeys = 10
        };

        try
        {
            ListObjectsV2Response listResponse;
            do
            {
                listResponse = await _client.ListObjectsV2Async(request);

                foreach (S3Object obj in listResponse.S3Objects)
                {
                    await _client.DeleteObjectAsync(new DeleteObjectRequest
                    {
                        BucketName = obj.BucketName,
                        Key = obj.Key
                    });
                }
                request.ContinuationToken = listResponse.NextContinuationToken;
            } while (listResponse.IsTruncated);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error deleting S3 bucket: {bucketName}");
        }
    }

    public string GetPreSignedUrl(string bucketName, string key, int expiresInMinutes)
    {
        var preSignedUrl = string.Empty;
        Console.WriteLine($"bucketName: {bucketName}");
        Console.WriteLine($"key: {key}");
        try
        {
            var preSignedUrlRequest = new GetPreSignedUrlRequest
            {
                BucketName = bucketName,
                Key = $"{key}",
                Expires = DateTime.Now.AddMinutes(expiresInMinutes)
            };
            preSignedUrl =  _client.GetPreSignedURL(preSignedUrlRequest);
        }
        catch (Exception ex)
        {
            throw ex;
        }

        return preSignedUrl;
    }

    public async Task<bool> DoesS3ObjectExist(string bucketName, string key)
    {
        GetObjectMetadataRequest request = new GetObjectMetadataRequest()
        {
            BucketName = bucketName,
            Key = key
        };

        try
        {
            await _client.GetObjectMetadataAsync(request);
            return true;
        }
        catch (AmazonS3Exception exception)
        {
            return false;
        }
    }
}
