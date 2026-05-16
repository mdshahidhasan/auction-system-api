using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using AuctionSystem.Core.Interfaces.ExternalServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace AuctionSystem.Infra.Services.External;

public sealed class CloudflareR2StorageService : IStorageService, IDisposable
{
    private readonly IAmazonS3 _s3Client;
    private readonly string _bucket;
    private readonly ILogger<CloudflareR2StorageService> _logger;

    public CloudflareR2StorageService(IConfiguration configuration, ILogger<CloudflareR2StorageService> logger)
    {
        _logger = logger;

        string accessKey = configuration["CloudflareR2:AccessKeyId"] ?? throw new InvalidOperationException("CloudflareR2:AccessKeyId is not configured.");
        string secret = configuration["CloudflareR2:SecretAccessKey"] ?? throw new InvalidOperationException("CloudflareR2:SecretAccessKey is not configured.");
        _bucket = configuration["CloudflareR2:Bucket"] ?? throw new InvalidOperationException("CloudflareR2:Bucket is not configured.");

        var endpoint = configuration["CloudflareR2:Endpoint"];
        if (string.IsNullOrWhiteSpace(endpoint))
        {
            var accountId = configuration["CloudflareR2:AccountId"] ?? throw new InvalidOperationException("CloudflareR2:AccountId or Endpoint must be configured.");
            endpoint = $"https://{accountId}.r2.cloudflarestorage.com";
        }

        var creds = new BasicAWSCredentials(accessKey, secret);
        var s3Config = new AmazonS3Config
        {
            ServiceURL = endpoint,
            ForcePathStyle = true
        };

        _s3Client = new AmazonS3Client(creds, s3Config);
    }

    public async Task UploadAsync(string key, Stream content, string contentType)
    {
        if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("Key is required.", nameof(key));
        if (content is null) throw new ArgumentNullException(nameof(content));

        var request = new PutObjectRequest
        {
            BucketName = _bucket,
            Key = key,
            InputStream = content,
            ContentType = contentType
        };

        var response = await _s3Client.PutObjectAsync(request);
        if (response.HttpStatusCode != System.Net.HttpStatusCode.OK && response.HttpStatusCode != System.Net.HttpStatusCode.NoContent)
        {
            _logger.LogError("Failed to upload object {Key} to bucket {Bucket}. StatusCode: {StatusCode}", key, _bucket, response.HttpStatusCode);
            throw new InvalidOperationException($"Failed to upload object. Status code: {response.HttpStatusCode}");
        }
    }

    public async Task UploadManyAsync(IEnumerable<(string key, Stream content, string contentType)> items, int maxParallelism = 4, CancellationToken cancellationToken = default)
    {
        if (items is null) throw new ArgumentNullException(nameof(items));
        if (maxParallelism <= 0) maxParallelism = 4;

        var semaphore = new SemaphoreSlim(maxParallelism);
        var exceptions = new ConcurrentQueue<Exception>();

        var tasks = new List<Task>();

        foreach (var (key, content, contentType) in items)
        {
            await semaphore.WaitAsync(cancellationToken);

            tasks.Add(Task.Run(async () =>
            {
                try
                {
                    await UploadAsync(key, content, contentType);
                }
                catch (Exception ex)
                {
                    exceptions.Enqueue(ex);
                }
                finally
                {
                    semaphore.Release();
                }
            }, cancellationToken));
        }

        await Task.WhenAll(tasks);

        if (!exceptions.IsEmpty)
        {
            throw new AggregateException(exceptions);
        }
    }


    public async Task DeleteAsync(string key)
    {
        if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("Key is required.", nameof(key));

        var response = await _s3Client.DeleteObjectAsync(new DeleteObjectRequest { BucketName = _bucket, Key = key });
        if (response.HttpStatusCode != System.Net.HttpStatusCode.NoContent && response.HttpStatusCode != System.Net.HttpStatusCode.OK)
        {
            _logger.LogError("Failed to delete object {Key} from bucket {Bucket}. StatusCode: {StatusCode}", key, _bucket, response.HttpStatusCode);
            throw new InvalidOperationException($"Failed to delete object. Status code: {response.HttpStatusCode}");
        }
    }

    public async Task DeleteManyAsync(IEnumerable<string> keys, int maxParallelism = 4, CancellationToken cancellationToken = default)
    {
        if (keys is null) throw new ArgumentNullException(nameof(keys));
        if (maxParallelism <= 0) maxParallelism = 4;

        var semaphore = new SemaphoreSlim(maxParallelism);
        var exceptions = new ConcurrentQueue<Exception>();
        var tasks = new List<Task>();

        foreach (var key in keys)
        {
            await semaphore.WaitAsync(cancellationToken);

            tasks.Add(Task.Run(async () =>
            {
                try
                {
                    await DeleteAsync(key);
                }
                catch (Exception ex)
                {
                    exceptions.Enqueue(ex);
                }
                finally
                {
                    semaphore.Release();
                }
            }, cancellationToken));
        }

        await Task.WhenAll(tasks);

        if (!exceptions.IsEmpty)
        {
            throw new AggregateException(exceptions);
        }
    }

    public Task<Uri> GetPresignedUrlAsync(string key, TimeSpan expires)
    {
        if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("Key is required.", nameof(key));

        var request = new GetPreSignedUrlRequest
        {
            BucketName = _bucket,
            Key = key,
            Expires = DateTime.UtcNow.Add(expires)
        };

        string url = _s3Client.GetPreSignedURL(request);
        return Task.FromResult(new Uri(url));
    }

    public void Dispose()
    {
        _s3Client?.Dispose();
    }
}