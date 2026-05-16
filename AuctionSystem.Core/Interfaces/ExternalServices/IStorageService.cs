namespace AuctionSystem.Core.Interfaces.ExternalServices;

public interface IStorageService
{
    Task UploadAsync(string key, Stream content, string contentType);
    Task UploadManyAsync(IEnumerable<(string key, Stream content, string contentType)> items, int maxParallelism = 4, CancellationToken cancellationToken = default);
    Task DeleteAsync(string key);
    Task DeleteManyAsync(IEnumerable<string> keys, int maxParallelism = 4, CancellationToken cancellationToken = default);
    Task<Uri> GetPresignedUrlAsync(string key, TimeSpan expires);
}