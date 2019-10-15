using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Client_tech_resversi_api.Models.Non_DB_models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using ReversiServer.Assets;
using ReversiServer.Assets.Interface;

namespace ReversiServer.Pages
{
    public class ActivateModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly IReversiApi _api;
        private readonly IUserIdentity _userIdentity;
        private readonly IJArrayToClaimsList _jArrayToCLaimsList;

        [BindProperty]
        public string ActivationCode { get; set; }

        [BindProperty]
        public bool ActivationSucces { get; set; }

        [BindProperty]
        public bool Login { get; set; }

        public ActivateModel(ILogger<IndexModel> logger, IReversiApi api, IUserIdentity userIdentity, IJArrayToClaimsList jArrayToClaimsList)
        {
            _jArrayToCLaimsList = jArrayToClaimsList;
            _userIdentity = userIdentity;
            _api = api;
            _logger = logger;
        }

        public async Task<ActionResult> OnGet(string newCode)
        {
            if (HttpContext.User.IsInRole("User"))
            {
                TempData["ErrorMsg"] = "You are logged in. You can't activate your account.";
                WriteLogInfoLine("already logged in");
                return RedirectToPage("Error");
            }

            if (!string.IsNullOrWhiteSpace(newCode))
            {
                var response = await _api.NewActivationCodeAsync(HttpContext.Session.GetInt32("newUser") ?? 0);

                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == HttpStatusCode.InternalServerError)
                    {
                        TempData["ErrorMsg"] = "Sorry, something went wrong inside our server. Please try again :). Probably John pressed the wrong button again..";
                        return Page();
                    }

                    TempData["ErrorMsg"] = "Your account has been activated, please login or if you forgot your credentials request a reset";
                    WriteLogInfoLine("already activated");
                    return RedirectToPage("Error");
                }
            }

            Login = !HttpContext.Session.TryGetValue("newUser", out var id);
            
            WriteLogInfoLine();
            return Page();
        }

        public async Task<ActionResult> OnPost()
        {
            int? userId;

            if ((userId = HttpContext.Session.GetInt32("newUser")) == null)
            {
                Login = true;
                return Page();
            }

            var response = await _api.ValidateActivationCodeAsync((int)userId, ActivationCode);

            if (response.IsSuccessStatusCode)
            {
                WriteLogInfoLine($"Activation succes for: {userId}");

                if (HttpContext.Session.TryGetValue("username", out var usernameBytes) && HttpContext.Session.TryGetValue("password", out var passwordBytes))
                {
                    ActivationSucces = true;

                    var loginresponse = await _api.LoginAsync(new LoginUser {Name = Encoding.UTF8.GetString(usernameBytes), Password = Encoding.UTF8.GetString(passwordBytes)});

                    if (loginresponse.IsSuccessStatusCode)
                    {
                        var claimsList = _jArrayToCLaimsList.CreateClaimsList(JArray.Parse(await loginresponse.Content.ReadAsStringAsync()));

                        await _userIdentity
                            .SetAuthenticationScheme("Identity.Application")
                            .CreateClaimsIdentity(claimsList)
                            .SignInUserAsync(false);
                    }
                }

                Login = HttpContext.User.IsInRole("User");
                HttpContext.Response.Cookies.Delete("id");

                return Page();
            }

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                TempData["ErrorMsg"] = "Your account has been activated, please login or if you forgot your credentials request a reset";
                WriteLogInfoLine("already activated");
                return RedirectToPage("Error");
            }


            WriteLogInfoLine($"Activation failed for: {userId}");
            TempData["ErrorMsg"] = "That code doesn't seems to work. Please try again.";
            return Page();
        }

        private void WriteLogInfoLine(string msg = null)
        {
            _logger.LogInformation($"{HttpContext.Request.Method}   {HttpContext.Request.Path}  {HttpContext.Connection.RemoteIpAddress}        {msg}");
        }
    }
}