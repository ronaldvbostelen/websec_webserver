using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Client_tech_resversi_api.Models.Non_DB_models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using reCAPTCHA.AspNetCore;
using ReversiServer.Assets;
using ReversiServer.Assets.Interface;

namespace ReversiServer.Pages
{
    [Authorize(Roles = "User")]
    public class WelcomeModel : PageModel
    {
        private readonly IReversiApi _api;
        private readonly ILogger<WelcomeModel> _logger;
        private readonly IRecaptchaService _recaptcha;
        private readonly IPasswordValidator _passwordValidator;

        public ChangeUserPassword ChangeUserPassword { get; set; }

        [BindProperty(Name = "g-recaptcha-response")]
        public string ReCaptcha { get; set; }

        [BindProperty]
        public string ReCaptchaDeux { get; set; }

        [BindProperty]
        public UserProfile User { get; set; }

        [BindProperty]
        public UserProfile UpdatedUser { get; set; }

        [BindProperty]
        public string OldPw { get; set; }

        [BindProperty]
        public string FirstPw { get; set; }

        [BindProperty]
        public string SecondPw { get; set; }
        
        public WelcomeModel(IReversiApi api, ILogger<WelcomeModel> logger, IRecaptchaService recaptchaService, IPasswordValidator passwordValidator)
        {
            _passwordValidator = passwordValidator;
            _recaptcha = recaptchaService;
            _api = api;
            _logger = logger;
        }

        public async Task<ActionResult> OnGet()
        {
            WriteLogInfoLine();

            var userId = (from userClaim in HttpContext.User.Claims
                where userClaim.Type == "UserId"
                select userClaim.Value).FirstOrDefault();

            if (userId == null)
            {
                TempData["ErrorMsg"] = "Something went wrong. Unable to retrieve your userprofile. Please login again.";
                RedirectToPage("Error");
            }

            var response = await _api.FetchUserProfileAsync(int.Parse(userId));

            if (response.IsSuccessStatusCode)
            {
                User = JsonConvert.DeserializeObject<UserProfile>(await response.Content.ReadAsStringAsync());

                return Page();
            }

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                return RedirectToPage("Reject");
            }


            TempData["ErrorMsg"] = "Something went wrong. Unable to retrieve your userprofile. Please login again.";
            return RedirectToPage("Error");

        }

        public async Task<ActionResult> OnPost()
        {
            if (string.IsNullOrWhiteSpace(UpdatedUser.ScreenName) || string.IsNullOrWhiteSpace(UpdatedUser.ScreenName))
            {
                ViewData["ErrorMsg"] = "Something went wrong. Please try again.";
                return await OnGet();
            }
            
            if (!await ValidateCaptcha())
            {
                return await OnGet();
            }

            UpdatedUser.UserId = int.Parse((from claim in HttpContext.User.Claims where claim.Type == "UserId" select claim.Value).FirstOr("0"));

            var response = await _api.UpdateUserAsync(UpdatedUser.UserId, UpdatedUser);

            if (!response.IsSuccessStatusCode)
            {
                ViewData["ErrorMsg"] = "Something went wrong. Unable to update your userprofile. Please try again.";
                
            }
            else if (response.StatusCode == HttpStatusCode.AlreadyReported)
            {
                ViewData["ErrorMsg"] = "No update. You didn't change a thing.";
            }
            else
            {
                ViewData["SuccesMsg"] = "Update succesfull";
            }

            return await OnGet();
        }

        public async Task<ActionResult> OnPostPassword()
        {
            if (!await ValidateCaptcha())
            {
                return await OnGet();
            }

            if (FirstPw != SecondPw || !_passwordValidator.StrongPassword(FirstPw))
            {
                ModelState.AddModelError("FirstPw", "Please supply a stronger/correct password");
                return await OnGet();
            }

            var userId = int.Parse((from claim in HttpContext.User.Claims where claim.Type == "UserId" select claim.Value).FirstOr("0"));

            var response = await _api.ChangePasswordAsync(userId, new ChangeUserPassword {UserId = userId, CurrentPassword = OldPw, PasswordOne = FirstPw, PasswordTwo = SecondPw});

            if (response.IsSuccessStatusCode)
            {
                ViewData["SuccesMsgPw"] = "Update succesfull";
            }
            else if (response.StatusCode == HttpStatusCode.InternalServerError)
            {
                ViewData["ErrorMsgPw"] = "Uppss.. Someone screwed it up. Please try again later";
            }
            else
            {
                ViewData["ErrorMsgPw"] = "Unable to change your password. Please try again.";
            }

            return await OnGet();
        }
        
        private void WriteLogInfoLine(string msg = null)
        {
            _logger.LogInformation($"{HttpContext.Request.Method}   {HttpContext.Request.Path}  {HttpContext.Connection.RemoteIpAddress}        {msg}");
        }

        private async Task<bool> ValidateCaptcha()
        {
            ReCaptcha = ReCaptcha ?? ReCaptchaDeux;
            
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
    }
}