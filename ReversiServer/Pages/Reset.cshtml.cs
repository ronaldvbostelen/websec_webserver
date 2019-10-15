using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Client_tech_resversi_api.Models.Non_DB_models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using reCAPTCHA.AspNetCore;
using ReversiServer.Assets;
using ReversiServer.Assets.Interface;

namespace ReversiServer.Pages
{
    public class ResetModel : PageModel
    {
        private readonly IUserIdentity _userIdentity;
        private readonly IRecaptchaService _recaptcha;
        private readonly ILogger<ResetModel> _logger;
        private readonly IReversiApi _api;
        private readonly IPasswordValidator _passwordValidator;
        private readonly IJArrayToClaimsList _jArrayToClaimsList;

        [BindProperty(Name = "g-recaptcha-response")]
        public string ReCaptcha { get; set; }

        [BindProperty]
        public string ReCaptcha2 { get; set; }

        [BindProperty]
        public string ReCaptcha3 { get; set; }

        [BindProperty]
        public string FirstPw { get; set; }

        [BindProperty]
        public string SecondPw { get; set; }

        [BindProperty]
        public string EmailOrUsername { get; set; }

        [BindProperty]
        public string ResetCode { get; set; }

        [BindProperty]
        public bool DisableEmail { get; set; }

        [BindProperty]
        public bool ResetSuccesfull { get; set; }

        [BindProperty]
        public string SecurityHash { get; set; }

        [BindProperty]
        public bool NotActivated { get; set; }

        public ResetModel(IRecaptchaService recaptcha, ILogger<ResetModel> logger, IReversiApi api, 
            IUserIdentity userIdentity, IPasswordValidator passwordValidator, IJArrayToClaimsList jArrayToClaimsList)
        {
            _jArrayToClaimsList = jArrayToClaimsList;
            _passwordValidator = passwordValidator;
            _userIdentity = userIdentity;
            _api = api;
            _recaptcha = recaptcha;
            _logger = logger;
        }

        public async Task<ActionResult> OnPost()
        {
            if (!await ValidateCaptcha())
            {
                return Page();
            }

            var responseOne = await _api.RequestResetCodeAsync(EmailOrUsername);

            if (responseOne.IsSuccessStatusCode)
            {
                var responseBody = await responseOne.Content.ReadAsStringAsync();
                var anonObj = new { id = 0, securityHash = "" };

                var json = JsonConvert.DeserializeAnonymousType(responseBody, anonObj);
                SecurityHash = json.securityHash;

                HttpContext.Session.SetInt32("userId", json.id);
                DisableEmail = true;
            }
            else if (responseOne.StatusCode == HttpStatusCode.Forbidden)
            { 
                HttpContext.Session.SetInt32("newUser", int.Parse(await responseOne.Content.ReadAsStringAsync()));
                NotActivated = true;
            }
            else
            {
                TempData["Msg"] = "Something went wrong. Please try again. You are sure that your email address or username is correct?";
            }

            return Page();
        }

        public async Task<ActionResult> OnPostFinal()
        {
            if (!await ValidateCaptcha())
            {
                return Page();
            }

            if (FirstPw != SecondPw || !_passwordValidator.StrongPassword(FirstPw))
            {
                ModelState.AddModelError("FirstPw", "Please supply a stronger password");
                return Page();
            }

            var responseTwo = await _api.ResetPasswordAsync(new ResetUserPassword {UserId = HttpContext.Session.GetInt32("userId") ?? 0, PassOne = FirstPw, PassTwo = SecondPw, ResetCode = ResetCode});

            if (responseTwo.IsSuccessStatusCode)
            {
                HttpContext.Response.Cookies.Delete("id");
                HttpContext.Response.Cookies.Delete("whoami");
                ResetSuccesfull = true;
            }
            else
            {
                TempData["Msg"] = "Something went wrong. Please try again. Are you sure that you entered the right email address or username?";
            }

            //login user after succesfull resert (based on OWASP)
            try
            {
                var loginresponse = await _api.LoginAsync(new LoginUser { Name = await responseTwo.Content.ReadAsStringAsync(), Password = FirstPw });

                var claimsList = _jArrayToClaimsList.CreateClaimsList(JArray.Parse(await loginresponse.Content.ReadAsStringAsync()));

                await _userIdentity
                    .SetAuthenticationScheme("Identity.Application")
                    .CreateClaimsIdentity(claimsList)
                    .SignInUserAsync(false);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                TempData["Msg"] = "Something went wrong while finalizing your reset. Please try to login on our homepage.";
            }
            
            return Page();
        }

        private async Task<bool> ValidateCaptcha()
        {
            ReCaptcha = ReCaptcha ?? ReCaptcha2 ?? ReCaptcha3;

            if (ReCaptcha == null)
            {
                ModelState.AddModelError("ReCaptcha", "There was an error validating reCAPTCHA. Please try again!");
                return false;
            }

            var recaptcha = await _recaptcha.Validate(ReCaptcha);

            if (recaptcha.score < 0.8m)
            {
                WriteLogInfoLine($"reCAPTCHA failed score {recaptcha.score}");
                ModelState.AddModelError("ReCaptcha", "There was an error validating reCAPTCHA. Please try again!");
            }

            return recaptcha.score > 0.8m;
        }

        private void WriteLogInfoLine(string msg = null)
        {
            _logger.LogInformation($"{HttpContext.Request.Method}   {HttpContext.Request.Path}  {HttpContext.Connection.RemoteIpAddress}        {msg}");
        }
    }
}