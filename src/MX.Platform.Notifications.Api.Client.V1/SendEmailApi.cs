using Microsoft.Extensions.Logging;

using MX.Api.Abstractions;
using MX.Api.Client;
using MX.Api.Client.Auth;
using MX.Api.Client.Extensions;
using MX.Platform.Notifications.Abstractions.V1.Interfaces;
using MX.Platform.Notifications.Abstractions.V1.Models;

using RestSharp;

namespace MX.Platform.Notifications.Api.Client.V1;

/// <summary>
/// HTTP client implementation for sending emails via the Platform Notifications API.
/// Registered automatically by <c>AddNotificationsApiClient()</c>.
/// </summary>
public class SendEmailApi : BaseApi<NotificationsApiClientOptions>, ISendEmailApi
{
    /// <inheritdoc />
    public SendEmailApi(
        ILogger<BaseApi<NotificationsApiClientOptions>> logger,
        IApiTokenProvider? apiTokenProvider,
        IRestClientService restClientService,
        NotificationsApiClientOptions options)
        : base(logger, apiTokenProvider, restClientService, options)
    {
    }

    /// <inheritdoc />
    public async Task<IApiResult<SendEmailResponseDto>> SendEmail(SendEmailRequestDto request)
    {
        try
        {
            var restRequest = await CreateRequestAsync("v1/email/send", Method.Post);
            restRequest.AddJsonBody(request);

            var response = await ExecuteAsync(restRequest);

            var result = response.ToApiResult<SendEmailResponseDto>();
            return result;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            var errorResponse = new ApiResponse<SendEmailResponseDto>(
                new ApiError("CLIENT_ERROR", "Failed to send email"));
            return new ApiResult<SendEmailResponseDto>(System.Net.HttpStatusCode.InternalServerError, errorResponse);
        }
    }
}
