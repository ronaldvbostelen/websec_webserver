using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Client_tech_resversi_api.Models;
using Client_tech_resversi_api.Models.Non_DB_models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ReversiServer.Assets;
using ReversiServer.Assets.Interface;
using ReversiServer.Models;

namespace ReversiServer.Pages
{
    [Authorize(Roles = "User")]
    public class EnableQRModel : PageModel
    {
        private readonly IReversiApi _api;

        [BindProperty]
        public string Code { get; set; }

        [BindProperty]
        public bool ActivationSucces { get; set; }

        [BindProperty]
        public FreeOtp FreeOtp { get; set; }
        
        public EnableQRModel(IReversiApi api)
        {
            _api = api;
        }

        public async Task<ActionResult> OnGet(string username)
        {
            var userIdClaim = User.Claims.FirstOrDefault(x => x.Type == "UserId");

            if (userIdClaim == null)
            {
                RedirectToPage("Logout");
            }
            
            var userID = int.Parse(userIdClaim.Value);

            var response = await _api.EnableTwoFaAsync(userID);

            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMsg"] = "Something went wrong while setting up 2FA, please try again later";
            }
            else
            {
                FreeOtp = new FreeOtp
                {
                    Account = username,
                    Secret = await response.Content.ReadAsStringAsync()
                };
            }

            return Page();
        }

        public async Task<ActionResult> OnPost()
        {
            var userIdClaim = User.Claims.FirstOrDefault(x => x.Type == "UserId");

            if (userIdClaim == null)
            {
                RedirectToPage("Logout");
            }

            var userID = int.Parse(userIdClaim.Value);

            var response = await _api.ConfirmTwoFaAsync(new TwoFaUser { Code = Code, UserId = userID });

            if (response.IsSuccessStatusCode)
            {
                ActivationSucces = true;
            }
            else
            {
                TempData["ErrorMsg"] = "Something went wrong while setting up 2FA, please try again later (or press QR-code to try again)";
            }

            return Page();
        }
    }
}