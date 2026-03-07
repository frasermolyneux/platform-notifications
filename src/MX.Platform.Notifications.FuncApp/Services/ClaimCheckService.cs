using System.Text;

using Azure.Storage.Blobs;

using Microsoft.Extensions.Logging;

namespace MX.Platform.Notifications.FuncApp.Services;

/// <summary>
/// Implements claim-check pattern using Azure Blob Storage for large email bodies.
/// </summary>
public class ClaimCheckService : IClaimCheckService
{
    private const string ContainerName = "email-attachments";
    private readonly BlobServiceClient? _blobServiceClient;
    private readonly ILogger<ClaimCheckService> _logger;

    public ClaimCheckService(ILogger<ClaimCheckService> logger, BlobServiceClient? blobServiceClient = null)
    {
        _logger = logger;
        _blobServiceClient = blobServiceClient;
    }

    public async Task<string> StoreAsync(string content, CancellationToken cancellationToken = default)
    {
        if (_blobServiceClient is null)
        {
            throw new InvalidOperationException("Blob storage is not configured for claim-check pattern.");
        }

        var containerClient = _blobServiceClient.GetBlobContainerClient(ContainerName);
        await containerClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

        var blobName = $"{Guid.NewGuid()}.json";
        var blobClient = containerClient.GetBlobClient(blobName);

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
        await blobClient.UploadAsync(stream, overwrite: true, cancellationToken: cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("Stored claim-check blob: {BlobName}", blobName);
        return blobName;
    }

    public async Task<string> RetrieveAsync(string reference, CancellationToken cancellationToken = default)
    {
        if (_blobServiceClient is null)
        {
            throw new InvalidOperationException("Blob storage is not configured for claim-check pattern.");
        }

        var containerClient = _blobServiceClient.GetBlobContainerClient(ContainerName);
        var blobClient = containerClient.GetBlobClient(reference);

        var response = await blobClient.DownloadContentAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
        return response.Value.Content.ToString();
    }

    public async Task DeleteAsync(string reference, CancellationToken cancellationToken = default)
    {
        if (_blobServiceClient is null)
        {
            _logger.LogWarning("Blob storage not configured; skipping claim-check delete for {Reference}", reference);
            return;
        }

        var containerClient = _blobServiceClient.GetBlobContainerClient(ContainerName);
        var blobClient = containerClient.GetBlobClient(reference);

        await blobClient.DeleteIfExistsAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
        _logger.LogInformation("Deleted claim-check blob: {BlobName}", reference);
    }
}
