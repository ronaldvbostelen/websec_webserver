using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ReversiServer.Assets;
using ReversiServer.Assets.Interface;

namespace ReversiServer.Pages
{
    [Authorize (Roles = "User")]
    public class DisableQRModel : PageModel
    {
        private readonly IReversiApi _api;

        [BindProperty]
        public bool DisableSucces { get; set; }

        public DisableQRModel(IReversiApi api)
        {
            _api = api;
        }

        public void OnGet()
        {
        }

        public async Task<ActionResult> OnPost()
        {
            var response = await _api.DisableTwoFaAsync(int.Parse(User.FindFirstValue("UserId")));

            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMsg"] = "Something went wrong while deactivating your 2FA. Please try again.";
            }
            else
            {
                DisableSucces = true;
            }

            return Page();
        }
    }
}