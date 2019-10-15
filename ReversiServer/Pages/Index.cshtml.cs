using Client_tech_resversi_api.Models.Non_DB_models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using ReversiServer.Assets;
using ReversiServer.Assets.Interface;
using System.ComponentModel;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace ReversiServer.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly IReversiApi _api;
        private readonly IUserIdentity _userIdentity;
        private readonly IJArrayToClaimsList _jArrayToClaimsList;

        [BindProperty]
        public bool NewUser { get; set; }

        [BindProperty]
        public LoginUser User { get; set; }

        [BindProperty]
        public string LoginMsg { get; set; }

        [BindProperty]
        public bool Success { get; set; }

        [DisplayName("Remember me?")]
        [BindProperty]
        public bool DoYouCookie { get; set; }

        [BindProperty]
        public int LoginAttempts { get; set; }

        [BindProperty]
        public bool TwoFa { get; set; }

        [BindProperty]
        public string TwoFaCode { get; set; }
        
        public IndexModel(ILogger<IndexModel> logger, IReversiApi api, IUserIdentity userIdentity, IJArrayToClaimsList jArrayToClaimsList)
        {
            _jArrayToClaimsList = jArrayToClaimsList;
            _userIdentity = userIdentity;
            _api = api;
            _logger = logger;
        }

        public async Task<ActionResult> OnGet()
        {
            WriteLogInfoLine();

            if (HttpContext.User.IsInRole("User"))
            { 
                return RedirectToPage("Welcome");
            }
            
            NewUser = HttpContext.Session.GetInt32("newUser") != null;
            
            return Page();
        }

        public async Task<ActionResult> OnPost()
        {
            var response = await _api.LoginAsync(User);

            if (!response.IsSuccessStatusCode)
            {
                Success = false;
                WriteLogInfoLine($"Logged in: {User.Name} FAIL");

                switch (response.StatusCode)
                {
                    case HttpStatusCode.Locked:
                        LoginMsg = "Your account has been locked. Please contact our admin at: admin@drysolidkiss.nl";
                        break;
                    case HttpStatusCode.Forbidden:
                        LoginMsg = "You account has not been activated. You might want to request a new activation key";
                        HttpContext.Session.SetInt32("newUser",int.Parse(await response.Content.ReadAsStringAsync()));
                        HttpContext.Session.SetString("username", User.Name);
                        HttpContext.Session.SetString("password", User.Password);
                        NewUser = true;
                        break;
                    case HttpStatusCode.InternalServerError:
                        LoginMsg = "Sorry, someone spilled some coffee on our server. Please try again.";
                        break;
                    case HttpStatusCode.UpgradeRequired:
                        TwoFa = true;
                        HttpContext.Session.SetString("username", User.Name);
                        HttpContext.Session.SetString("password", User.Password);
                        break;
                    default:
                        LoginAttempts++;
                        LoginMsg = LoginAttempts < 6 ? $"Sorry, wrong credentials [x{LoginAttempts}]" : $"Bro, calm down, thats alot of wrong attempts [x{LoginAttempts}]";
                        break;
                }

                return Page();
            }

            await CreateUser(response);
            
            Success = true;
            LoginMsg = "Welcome";
            WriteLogInfoLine($"Logged in: {User.Name} SUCCES");
            return RedirectToPage("Welcome");
        }

        public async Task<ActionResult> OnPostTwoFa()
        {
            var twoFaUser = new TwoFaLoginUser
            {
                Code = TwoFaCode,
                Name = HttpContext.Session.GetString("username"),
                Password = HttpContext.Session.GetString("password")
            };

            var response = await _api.LoginTwoFaAsync(twoFaUser);

            if (!response.IsSuccessStatusCode)
            {
                User = new LoginUser{Name = twoFaUser.Name, Password = twoFaUser.Password};
                LoginMsg = "Something went wrong while validating your 2FA, please try again";
                return await OnPost();
            }

            await CreateUser(response);

            Success = true;
            LoginMsg = "Welcome";
            WriteLogInfoLine($"Logged in (2FA): {User.Name} SUCCES");
            return RedirectToPage("Welcome");
        }

        private async Task<bool> CreateUser(HttpResponseMessage message)
        {
            var claimsList = _jArrayToClaimsList.CreateClaimsList(JArray.Parse(await message.Content.ReadAsStringAsync()));

            await _userIdentity
                .SetAuthenticationScheme("Identity.Application")
                .CreateClaimsIdentity(claimsList)
                .SignInUserAsync(DoYouCookie);

            return HttpContext.User.IsInRole("User");
        }

        private void WriteLogInfoLine(string msg = null)
        {
            _logger.LogInformation($"{HttpContext.Request.Method}   {HttpContext.Request.Path}  {HttpContext.Connection.RemoteIpAddress}        {msg}");
        }
    }
}