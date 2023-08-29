namespace FargateBlog.Core;

public static class Constants
{
    public static readonly string AppName = "FargateBlog";
    public static readonly string Region = Environment.GetEnvironmentVariable("AWS_REGION") ?? "us-east-1";
    public static string EncodingClusterName { get; set; } = "encoding-cluster";
    public static string EncodingQueueName { get; set; } = "encoding-queue";
}

public static class ExtensionMethods
{
    public static string ToHypenCase(this string str)
    {
        return string.Concat(str.Select((x, i) => i > 0 && char.IsUpper(x) ? "-" + x.ToString() : x.ToString())).ToLower();
    }
}

