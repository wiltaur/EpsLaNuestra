using EpsLaNuestra.Application.Utilities;
using MediatR;

namespace EpsLaNuestra.Application.Features.Authentication.Queries;

public record GetTokenAuthenticateQuery : IRequest<ApiResponseUtility<string>>
{
    public string UserId { get; }
    public GetTokenAuthenticateQuery(string userId) => UserId = userId;
}