using IdentityService.Api.Application.Models;
using System.Security.Claims;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;

namespace IdentityService.Api.Application.Services
{
    public class IdentityService : IIdentityService
    {


        public Task<LoginResponseModel> Login(LoginRequestModel loginRequestModel)
        {
            //DB user işlemlerini burada kontrol et.

            var claims = new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, loginRequestModel.UserName),
                new Claim(ClaimTypes.Name, "Furkan Bekereci"),
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("TechBuddySecretKeyShouldBeLong"));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expiry = DateTime.Now.AddDays(10);

            var token = new JwtSecurityToken(claims: claims, expires: expiry, signingCredentials: credentials, notBefore: DateTime.Now);
            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(token);

            LoginResponseModel response = new LoginResponseModel
            {
                UserToken = encodedJwt,
                UserName = loginRequestModel.UserName,
            };
            return Task.FromResult(response);
        }
    }
}
