using MX.Api.Abstractions;
using MX.Platform.Notifications.Abstractions.V1.Models;

namespace MX.Platform.Notifications.Abstractions.V1.Interfaces;

public interface ISendEmailApi
{
    Task<IApiResult<SendEmailResponseDto>> SendEmail(SendEmailRequestDto request);
}
