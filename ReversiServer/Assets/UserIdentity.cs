using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

namespace ReversiServer.Assets
{
    public class UserIdentity : IUserIdentity
    {
        private string AuthenticationScheme;

        private ClaimsIdentity claimsIdentity;
        private HttpContext httpContext;


        public UserIdentity(IHttpContextAccessor httpContextAccessor)
        {
            httpContext = httpContextAccessor.HttpContext;
        }

        public IUserIdentity SetAuthenticationScheme(string authenticationScheme)
        {
            AuthenticationScheme = authenticationScheme;

            return this;
        }

        public IUserIdentity CreateClaimsIdentity(IEnumerable<Claim> claims) => CreateClaimsIdentity(claims, new Claim[0]);

        public IUserIdentity CreateClaimsIdentity(IEnumerable<Claim> claims, IEnumerable<Claim> roles)
        {
            if (string.IsNullOrWhiteSpace(AuthenticationScheme))
            {
                return null;
            }

            var claimsList = new List<Claim>();

            claimsList.AddRange(claims);
            claimsList.AddRange(roles);

            claimsIdentity = new ClaimsIdentity(claimsList,AuthenticationScheme);

            return this;
        }

        public async Task<ClaimsPrincipal> SignInUserAsync(bool persistent)
        {
            if (string.IsNullOrWhiteSpace(AuthenticationScheme))
            {
                return null;
            }

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = persistent,
                AllowRefresh = true,
                IssuedUtc = DateTimeOffset.Now,
                RedirectUri = "/"
            };

            await httpContext.SignInAsync(
                AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            return httpContext.User;
        }
    }
}
