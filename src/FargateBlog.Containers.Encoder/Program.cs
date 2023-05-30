using Xabe.FFmpeg;
using Amazon;
using Amazon.Lambda.Core;
using Amazon.S3.Model;
using Amazon.S3;
using FargateBlog.Core;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

var region = RegionEndpoint.GetBySystemName(Constants.Region);
var s3Client = new AmazonS3Client(region);

var envBuckeName = Environment.GetEnvironmentVariable("BuckeName") ?? "";
var envObjectKey = Environment.GetEnvironmentVariable("ObjectKey") ?? "";

Console.WriteLine($"in Program **********************");

Console.WriteLine($"envBuckeName: {envBuckeName}");
Console.WriteLine($"envObjectKey: {envObjectKey}");

var preSignedUrlRequest = new GetPreSignedUrlRequest
{
    BucketName = envBuckeName,
    Key = $"{envObjectKey}.mp4",
    Expires = DateTime.Now.AddMinutes(30)
};
var preSignedUrl = s3Client.GetPreSignedURL(preSignedUrlRequest);

Console.WriteLine($"got presigned url: {preSignedUrl}");

var outputFile = @$"/home/{envObjectKey}.mp4";
var argument = @$"-protocol_whitelist file,http,https,tcp,tls -i {preSignedUrl} {outputFile}";
var conversion = await FFmpeg.Conversions.New().Start(argument);

var files = new List<string>(Directory.EnumerateFiles("/home"));

for (int i = 0; i < files.Count; i++)
{
    string? file = files[i];
    Console.WriteLine($"about to put object: {file}");

    var putObjectRequest = new PutObjectRequest
    {
        BucketName = $"{Constants.AppName.ToHypenCase()}-encoded",
        Key = @$"{envObjectKey}.mp4",
        FilePath = file,
        ContentType = "video/mp4",
        StorageClass = S3StorageClass.IntelligentTiering
    };

    PutObjectResponse response = await s3Client.PutObjectAsync(putObjectRequest);

    Console.WriteLine($"response: {response.HttpStatusCode}");
}