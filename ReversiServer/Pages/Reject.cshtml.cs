using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ReversiServer.Pages
{
    public class RejectModel : PageModel
    {
        public async Task OnGet()
        {
            await HttpContext.SignOutAsync();
        }
    }
}