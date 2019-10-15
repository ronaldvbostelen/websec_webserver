using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ReversiServer.Assets
{
    public interface IUserIdentity
    {
        IUserIdentity SetAuthenticationScheme(string authenticationScheme);
        IUserIdentity CreateClaimsIdentity(IEnumerable<Claim> claims);
        IUserIdentity CreateClaimsIdentity(IEnumerable<Claim> claims, IEnumerable<Claim> roles);
        Task<ClaimsPrincipal> SignInUserAsync(bool persistent);
    }
}