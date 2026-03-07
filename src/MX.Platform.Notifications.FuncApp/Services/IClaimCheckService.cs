namespace MX.Platform.Notifications.FuncApp.Services;

/// <summary>
/// Service for storing and retrieving large email bodies using blob storage (claim-check pattern).
/// </summary>
public interface IClaimCheckService
{
    /// <summary>
    /// Stores a large message body in blob storage and returns a reference key.
    /// </summary>
    /// <param name="content">The content to store.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The blob reference key.</returns>
    Task<string> StoreAsync(string content, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a message body from blob storage by reference key.
    /// </summary>
    /// <param name="reference">The blob reference key.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The stored content.</returns>
    Task<string> RetrieveAsync(string reference, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a stored message body from blob storage.
    /// </summary>
    /// <param name="reference">The blob reference key.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task DeleteAsync(string reference, CancellationToken cancellationToken = default);
}
