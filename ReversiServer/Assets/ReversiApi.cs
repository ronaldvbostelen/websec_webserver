using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Client_tech_resversi_api.Models.Non_DB_models;
using Client_tech_resversi_api.Models.Principal;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Extensions.Logging;
using MimeKit.Encodings;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ReversiServer.Assets.Interface;
using ReversiServer.Models;

namespace ReversiServer.Assets
{
    public class ReversiApi : IReversiApi 
    {
        private readonly ILogger<ReversiApi> _logger;
        private readonly HttpContext _context;
        private readonly HttpClient client;
        private readonly string baseAddress = "http://localhost:5000/api/";
        private CookieContainer cookieContainer;

        private Dictionary<string, string> apiEndpoints = new Dictionary<string, string>
        {
            {"Player","Players/"},
            {"Login","Login/"},
            {"Create","CreateUser/"},
            {"UserAccount","UserAccount/"},
            {"Reset", "Reset/"},
            {"User", "User/" },
            {"UserOverview", "UserOverview/" },
            {"Admin", "Admin/" },
            {"ChangeEmail", "ChangeEmail/" },
            {"ChangePassword", "ChangePassword/"},
            {"Ban","Ban/" },
            {"UnBan","Unban/" },
            {"UserRole","Roles/" },
            {"UserClaim","Claims/" },
            {"Totp","Totp/" }
        };

        public ReversiApi(IHttpContextAccessor contextAccessor, ILogger<ReversiApi> logger)
        {
            _context = contextAccessor.HttpContext;
            _logger = logger;

            cookieContainer = new CookieContainer();

            if (contextAccessor.HttpContext.Request.Cookies.TryGetValue("whoami", out var value))
            {
                cookieContainer.Add(new Cookie
                {
                    Name = "whoami",
                    Value = value,
                    Domain = "localhost",
                    Expires = DateTime.Now.Add(TimeSpan.FromMinutes(30)),
                    Secure = false,
                    HttpOnly = true
                });
            }
            
            client = new HttpClient(new HttpClientHandler
            {
                CookieContainer = cookieContainer,
//                ClientCertificateOptions = ClientCertificateOption.Manual,
//                ServerCertificateCustomValidationCallback = (a, b, c, d) => true
            });
        }

        public async Task<HttpResponseMessage> CreateUserAsync(RegisterUser user)
        {
            WriteLogInfoLine("Creating user");
            return await client.PostAsync(baseAddress + apiEndpoints["Create"], new StringContent(JsonConvert.SerializeObject(user), Encoding.UTF8, "application/json"));
        }

        public async Task<HttpResponseMessage> LoginAsync(LoginUser user)
        {
            WriteLogInfoLine("Login");
            return await client.PostAsync(baseAddress + apiEndpoints["Login"], new StringContent(JsonConvert.SerializeObject(user), Encoding.UTF8, "application/json"));
        }

        public async Task<HttpResponseMessage> ValidateActivationCodeAsync(int userId, string code)
        {
            WriteLogInfoLine("Validate activationcode");
            return await client.PostAsync(baseAddress + apiEndpoints["UserAccount"] + userId, new StringContent(JsonConvert.SerializeObject(code),Encoding.UTF8,"application/json"));
        }

        public async Task<HttpResponseMessage> NewActivationCodeAsync(int userId)
        {
            WriteLogInfoLine("New activationcode");
            return await client.GetAsync(baseAddress + apiEndpoints["UserAccount"] + userId);
        }

        public async Task<HttpResponseMessage> RequestResetCodeAsync(string emailOrUsername)
        {
            WriteLogInfoLine("Request logincode");
            return await client.PostAsync(baseAddress + apiEndpoints["Reset"], new StringContent(JsonConvert.SerializeObject(emailOrUsername), Encoding.UTF8, "application/json"));
        }

        public async Task<HttpResponseMessage> ResetPasswordAsync(ResetUserPassword user)
        {
            WriteLogInfoLine("Reset password");
            return await client.PutAsync(baseAddress + apiEndpoints["Reset"], new StringContent(JsonConvert.SerializeObject(user),Encoding.UTF8,"application/json"));
        }

        public async Task<HttpResponseMessage> FetchUserProfileAsync(int userId)
        {
            WriteLogInfoLine("Fetch user profile");
            return await client.GetAsync(baseAddress + apiEndpoints["User"] + userId);
        }

        public async Task<HttpResponseMessage> UpdateUserAsync(int userId, UserProfile user)
        {
            WriteLogInfoLine("Update user profile");
            return await client.PatchAsync(baseAddress + apiEndpoints["User"] + userId, new StringContent(JsonConvert.SerializeObject(user), Encoding.UTF8, "application/json"));
        }

        public async Task<HttpResponseMessage> ChangePasswordAsync(int userId, ChangeUserPassword changeUserPassword)
        {
            WriteLogInfoLine("Update user password");
            return await client.PostAsync(baseAddress + apiEndpoints["User"] + userId, new StringContent(JsonConvert.SerializeObject(changeUserPassword), Encoding.UTF8, "application/json"));
        }

        public async Task<HttpResponseMessage> GetAllUsersAsync()
        {
            WriteLogInfoLine("Fetch all users");
            return await client.GetAsync(baseAddress + apiEndpoints["UserOverview"]);
        }

        public async Task<HttpResponseMessage> GetUserAsync(int userId)
        {
            WriteLogInfoLine("Fetch user");
            return await client.GetAsync(baseAddress + apiEndpoints["UserOverview"] + userId);
        }

        public async Task<HttpResponseMessage> ChangeEmailAsync(int userId, string emailaddress)
        {
            WriteLogInfoLine("change email for user:" + userId);
            return await client.PutAsync(baseAddress + apiEndpoints["ChangeEmail"] + userId, new StringContent(JsonConvert.SerializeObject(emailaddress), Encoding.UTF8, "application/json"));
        }

        public async Task<HttpResponseMessage> ChangePasswordAsync(int userId, string password)
        {
            WriteLogInfoLine("change password for user:" + userId);
            return await client.PutAsync(baseAddress + apiEndpoints["ChangePassword"] + userId, new StringContent(JsonConvert.SerializeObject(password), Encoding.UTF8, "application/json"));
        }

        public async Task<HttpResponseMessage> BanUserAsync(int userId)
        {
            WriteLogInfoLine("ban user:" + userId);
            return await client.PutAsync(baseAddress + apiEndpoints["Ban"] + userId, null);
        }

        public async Task<HttpResponseMessage> UnBanUserAsync(int userId)
        {
            WriteLogInfoLine("unban user:" + userId);
            return await client.DeleteAsync(baseAddress + apiEndpoints["Ban"] + userId);
        }

        public async Task<HttpResponseMessage> AddUserRoleAsync(UserRole userRole)
        {
            WriteLogInfoLine($"set role: {userRole.Role} user:" + userRole.UserId);
            return await client.PostAsync(baseAddress + apiEndpoints["UserRole"], new StringContent(JsonConvert.SerializeObject(userRole), Encoding.UTF8, "application/json"));
        }

        public async Task<HttpResponseMessage> RevokeUserRoleAsync(int roleId)
        {
            WriteLogInfoLine($"revoke role with id: {roleId}");
            return await client.DeleteAsync(baseAddress + apiEndpoints["UserRole"] + roleId);
        }

        public async Task<HttpResponseMessage> AddUserClaimAsync(UserClaim claim)
        {
            WriteLogInfoLine($"set userClaim: {claim.Claim} {claim.Value} user:" + claim.UserId);
            return await client.PostAsync(baseAddress + apiEndpoints["UserClaim"], new StringContent(JsonConvert.SerializeObject(claim), Encoding.UTF8, "application/json"));
        }

        public async Task<HttpResponseMessage> RevokeUserClaimAsync(int claimId)
        {
            WriteLogInfoLine($"revoke role with id: {claimId}");
            return await client.DeleteAsync(baseAddress + apiEndpoints["UserClaim"] + claimId);
        }

        public async Task<HttpResponseMessage> EnableTwoFaAsync(int userId)
        {
            WriteLogInfoLine($"enable QR 2FA user: {userId}");
            return await client.PostAsync(baseAddress + apiEndpoints["Totp"] + userId, null);
        }

        public async Task<HttpResponseMessage> DisableTwoFaAsync(int userId)
        {
            WriteLogInfoLine($"disable QR 2FA user: {userId}");
            return await client.DeleteAsync(baseAddress + apiEndpoints["Totp"] + userId);
        }

        public async Task<HttpResponseMessage> ConfirmTwoFaAsync(TwoFaUser twoFaUser)
        {
            WriteLogInfoLine($"confirm 2FA user: {twoFaUser.UserId}");
            return await client.PutAsync(baseAddress + apiEndpoints["Totp"], new StringContent(JsonConvert.SerializeObject(twoFaUser), Encoding.UTF8, "application/json"));
        }

        public async Task<HttpResponseMessage> LoginTwoFaAsync(TwoFaLoginUser twoFaLoginUser)
        {
            WriteLogInfoLine($"login 2FA user: {twoFaLoginUser.Name}");
            return await client.PatchAsync(baseAddress + apiEndpoints["Login"], new StringContent(JsonConvert.SerializeObject(twoFaLoginUser), Encoding.UTF8, "application/json"));
        }

        private void WriteLogInfoLine(string nameOfAction)
        {
            _logger.LogInformation($"{_context.Request.Method}   {_context.Request.Path}  {_context.Connection.RemoteIpAddress}        {_context.User.Identity.Name} : {nameOfAction}");
        }
    }
}
