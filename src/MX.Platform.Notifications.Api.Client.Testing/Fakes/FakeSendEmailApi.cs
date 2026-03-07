using System.Collections.Concurrent;
using System.Net;
using MX.Api.Abstractions;
using MX.Platform.Notifications.Abstractions.V1.Interfaces;
using MX.Platform.Notifications.Abstractions.V1.Models;

namespace MX.Platform.Notifications.Api.Client.Testing.Fakes;

/// <summary>
/// In-memory fake of <see cref="ISendEmailApi"/> for unit and integration tests.
/// Configure responses with <see cref="WithSuccessResponse"/> before exercising the code under test.
/// </summary>
public class FakeSendEmailApi : ISendEmailApi
{
    private readonly ConcurrentBag<SendEmailRequestDto> _sentEmails = [];
    private SendEmailResponseDto _successResponse = new()
    {
        MessageId = Guid.NewGuid().ToString(),
        Status = "Succeeded"
    };
    private bool _shouldFail;
    private HttpStatusCode _failureStatusCode = HttpStatusCode.InternalServerError;
    private string _failureErrorCode = "SEND_FAILED";
    private string _failureErrorMessage = "Fake email send failure";

    /// <summary>
    /// Configures the success response returned by <see cref="SendEmail"/>.
    /// </summary>
    public FakeSendEmailApi WithSuccessResponse(SendEmailResponseDto response)
    {
        _successResponse = response;
        return this;
    }

    /// <summary>
    /// Configures the fake to return an error response.
    /// </summary>
    public FakeSendEmailApi WithFailure(
        HttpStatusCode statusCode = HttpStatusCode.InternalServerError,
        string errorCode = "SEND_FAILED",
        string errorMessage = "Fake email send failure")
    {
        _shouldFail = true;
        _failureStatusCode = statusCode;
        _failureErrorCode = errorCode;
        _failureErrorMessage = errorMessage;
        return this;
    }

    /// <summary>
    /// Returns all email requests that were sent.
    /// </summary>
    public IReadOnlyCollection<SendEmailRequestDto> SentEmails => _sentEmails.ToArray();

    /// <summary>
    /// Clears all tracked emails and resets to default success behavior.
    /// </summary>
    public FakeSendEmailApi Reset()
    {
        while (_sentEmails.TryTake(out _)) { }
        _shouldFail = false;
        _successResponse = new SendEmailResponseDto
        {
            MessageId = Guid.NewGuid().ToString(),
            Status = "Succeeded"
        };
        return this;
    }

    public Task<IApiResult<SendEmailResponseDto>> SendEmail(SendEmailRequestDto request)
    {
        _sentEmails.Add(request);

        if (_shouldFail)
        {
            var errorResponse = new ApiResponse<SendEmailResponseDto>(
                new ApiError(_failureErrorCode, _failureErrorMessage));
            return Task.FromResult<IApiResult<SendEmailResponseDto>>(
                new ApiResult<SendEmailResponseDto>(_failureStatusCode, errorResponse));
        }

        var response = new ApiResponse<SendEmailResponseDto>(_successResponse);
        return Task.FromResult<IApiResult<SendEmailResponseDto>>(
            new ApiResult<SendEmailResponseDto>(HttpStatusCode.OK, response));
    }
}
