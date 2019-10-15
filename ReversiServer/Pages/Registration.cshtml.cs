using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Client_tech_resversi_api.Models.Non_DB_models;
using Client_tech_resversi_api.Models.Principal;
using MailKit;
using MailKit.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using MimeKit;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using reCAPTCHA.AspNetCore;
using ReversiServer.Assets;
using ReversiServer.Assets.Interface;
using ReversiServer.Models;

namespace ReversiServer.Pages
{
    public class RegistrationModel : PageModel
    {
        private readonly ILogger<RegistrationModel> _logger;
        private readonly IRecaptchaService _recaptcha;
        private readonly IReversiApi _api;
        private readonly IPasswordValidator _passwordValidator;

        [BindProperty]
        public RegisterUser User { get; set; }

        [BindProperty(Name = "g-recaptcha-response")]
        public string ReCaptcha { get; set; }

        public RegistrationModel(IRecaptchaService recaptcha, ILogger<RegistrationModel> logger, IReversiApi api, IPasswordValidator passwordValidator)
        {
            _passwordValidator = passwordValidator;
            _api = api;
            _recaptcha = recaptcha;
            _logger = logger;
        }
        
        public void OnGet()
        {
            WriteLogInfoLine();
        }

        public async Task<IActionResult> OnPost()
        {
            if (ReCaptcha == null)
            {
                ModelState.AddModelError("ReCaptcha", "There was an error validating reCAPTCHA. Please try again!");
                return Page();
            }

            var recaptcha = await _recaptcha.Validate(ReCaptcha);

            if (recaptcha.score < 0.8m)
            {
                WriteLogInfoLine($"reCAPTCHA failed score {recaptcha.score}");
                ModelState.AddModelError("ReCaptcha", "There was an error validating reCAPTCHA. Please try again!");
                return Page();
            }

            if (!_passwordValidator.StrongPassword(User.Password))
            {
                ModelState.AddModelError("Player.Password", "Please supply a stronger password");
                return Page();
            }

            if (!ModelState.IsValid)
                return Page();

            var response = await _api.CreateUserAsync(User);
            
            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError("ReCaptcha", "Failed to create account, reason: " + await response.Content.ReadAsStringAsync());
                WriteLogInfoLine($"Account creation {User.Name} FAIL");
                return Page();
            }
            
            var qstring = HttpUtility.ParseQueryString(response.Headers.Location.Query);
            HttpContext.Session.SetInt32("newUser", int.Parse(qstring.Get("id")));
            HttpContext.Session.SetString("username", User.Name);
            HttpContext.Session.SetString("password", User.Password);

            WriteLogInfoLine($"Account creation {User.Name} SUCCES");

            TempData["Title"] = "Registration succesfull";
            TempData["Message"] = "To complete youre registration please follow the instructions in the activation email. " +
                                  "If you havn't received the activation email please check your spamfolder or request a new link with the button available on the homepage";

            return RedirectToPage("Activate");
        }
        
        private void WriteLogInfoLine(string msg = null)
        {
            _logger.LogInformation($"{HttpContext.Request.Method}   {HttpContext.Request.Path}  {HttpContext.Connection.RemoteIpAddress}        {msg}");
        }
    }
}