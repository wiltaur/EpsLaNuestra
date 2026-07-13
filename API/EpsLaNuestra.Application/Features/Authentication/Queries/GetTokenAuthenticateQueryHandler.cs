using EpsLaNuestra.Application.Utilities;
using EpsLaNuestra.Domain.Persistence;
using MediatR;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace EpsLaNuestra.Application.Features.Authentication.Queries;
public class GetTokenAuthenticateQueryHandler(IOptions<AuthenticationSettings> options) : IRequestHandler<GetTokenAuthenticateQuery, ApiResponseUtility<string>>
{
    /// <summary>
    /// This method is responsible for generating a token to authenticate and consume the APIs.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>Token to consume the APIs.</returns>
    public Task<ApiResponseUtility<string>> Handle(GetTokenAuthenticateQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.Value.AuthKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, request.UserId),
            };

            var token = new JwtSecurityToken(options.Value.AuthIssuer,
                options.Value.AuthAudience,
                claims,
                expires: DateTime.Now.AddMinutes(options.Value.MinExpire),
                signingCredentials: credentials);

            var response = new ApiResponseUtility<string>(new JwtSecurityTokenHandler().WriteToken(token))
            {
                IsSuccess = true,
                ReturnMessage = "The token has been successfully generated."
            };

            return Task.FromResult(response);
        }
        catch (Exception ex)
        {
            var response = new ApiResponseUtility<string>(ex.InnerException == null ? ex.Message : ex.InnerException.Message)
            {
                IsSuccess = false,
                ReturnMessage = "Error getting token."
            };
            
            return Task.FromResult(response);
        }
    }
}