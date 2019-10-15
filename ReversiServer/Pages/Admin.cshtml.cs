using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Client_tech_resversi_api.Assets.Interfaces;
using Client_tech_resversi_api.Models.Non_DB_models;
using Client_tech_resversi_api.Models.Principal;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore.Internal;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ReversiServer.Assets;
using ReversiServer.Assets.Interface;

namespace ReversiServer.Pages
{
    [Authorize(Roles = "Helpdesk, Admin, SuperAdmin")]
    public class AdminModel : PageModel
    {
        private readonly IReversiApi _api;

        [BindProperty]
        public List<UserOverviewLight> NonVerifiedUsers { get; set; }

        [BindProperty]
        public List<UserOverviewLight> VerifiedUsers { get; set; }

        [BindProperty]
        public List<UserOverviewLight> HelpdeskUsers { get; set; }

        [BindProperty]
        public List<UserOverviewLight> Admins { get; set; }

        [BindProperty]
        public List<UserOverviewLight> SuperAdmins { get; set; }

        [BindProperty]
        public UserOverview SelectedUser { get; set; }

        [EmailAddress]
        [BindProperty]
        public string NewEmail { get; set; }

        [BindProperty]
        public string NewPassword { get; set; }

        [BindProperty]
        public UserClaim NewClaim { get; set; } = new UserClaim();

        [BindProperty]
        public string SelectedRole { get; set; }

        public SelectList RolesList { get; set; } = new SelectList(
            new[]
            {
                new SelectListItem(RoleTypes.User, RoleTypes.User),
                new SelectListItem(RoleTypes.Admin, RoleTypes.Admin),
                new SelectListItem(RoleTypes.Helpdesk, RoleTypes.Helpdesk),
                new SelectListItem(RoleTypes.SuperAdmin, RoleTypes.SuperAdmin)
            }, "Value", "Text"
        );

        [BindProperty]
        public string SelectedUserRole { get; set; }

        [BindProperty]
        public string SelectedUserClaim { get; set; }

        public SelectList SelectedUserRolesList { get; set; }
        public SelectList SelectedUserClaimsList { get; set; }

        public AdminModel(IReversiApi api)
        {
            _api = api;
            
            NonVerifiedUsers = new List<UserOverviewLight>();
            VerifiedUsers = new List<UserOverviewLight>();
            HelpdeskUsers = new List<UserOverviewLight>();
            Admins = new List<UserOverviewLight>();
            SuperAdmins = new List<UserOverviewLight>();
        }

        public async Task<ActionResult> OnGet(int? userId)
        {
            await CreateUserLists();
            
            if (userId != null)
            {
                var response = await _api.GetUserAsync((int) userId);

                if (!response.IsSuccessStatusCode)
                {
                    TempData["ErrorMsg"] = "Something went wrong, please reload the page.";
                    return Page();
                }
                SelectedUser = JsonConvert.DeserializeObject<UserOverview>(await response.Content.ReadAsStringAsync());
                var selectList = new List<SelectListItem>();
                foreach (var role in SelectedUser.UserRoles)
                {
                    selectList.Add(new SelectListItem(role.Role,role.Id.ToString()));
                }
                SelectedUserRolesList = new SelectList(selectList,"Value","Text");
                var claimsSelectList = new List<SelectListItem>();
                foreach (var userClaim in SelectedUser.UserClaims)
                {
                    claimsSelectList.Add(new SelectListItem(userClaim.Claim,userClaim.Id.ToString()));
                }
                SelectedUserClaimsList = new SelectList(claimsSelectList, "Value","Text");
            }
            
            return Page();

        }

        public async Task<ActionResult> OnPost()
        {
            return await OnGet(SelectedUser.UserId);
        }

        public async Task<ActionResult> OnPostChangeEmail()
        {
            var respose = await _api.ChangeEmailAsync(SelectedUser.UserId, NewEmail);

            if (!respose.IsSuccessStatusCode)
            {
                ModelState.AddModelError("NewEmail","Unable to update email, please try again.");
            }
            else
            {
                NewEmail = string.Empty;
                TempData["SuccessMsg"] = "Account has been updated";
            }

            return await OnGet(SelectedUser.UserId);
        }

        public async Task<ActionResult> OnPostChangePassword()
        {
            var response = await _api.ChangePasswordAsync(SelectedUser.UserId, NewPassword);

            if (response.IsSuccessStatusCode)
            {
                NewPassword = string.Empty;
                TempData["SuccessMsg"] = "Account has been updated";
            }
            else
            {
                ModelState.AddModelError("NewPassword", "Unable to change password, please try again.");
            }

            return await OnGet(SelectedUser.UserId);
        }

        public async Task<ActionResult> OnPostBan()
        {
            var response = await _api.BanUserAsync(SelectedUser.UserId);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMsg"] = "Account has been banned";
            }
            else
            {
                TempData["ErrorMsg"] = "Unable to ban user, please try again.";
            }

            return await OnGet(SelectedUser.UserId);
        }

        public async Task<ActionResult> OnPostUnBan()
        {
            var response = await _api.UnBanUserAsync(SelectedUser.UserId);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMsg"] = "Account has been unbanned";
            }
            else
            {
                TempData["ErrorMsg"] = "Unable to un-ban user, please try again.";
            }

            return await OnGet(SelectedUser.UserId);
        }

        public async Task<ActionResult> OnPostSetRole()
        {
            var response = await _api.AddUserRoleAsync(new UserRole {Role = SelectedRole, UserId = SelectedUser.UserId});

            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMsg"] = "Unable to create new role, please try again.";
            }
            else
            {
                TempData["SuccessMsg"] = "Account has been updated";
            }

            return await OnGet(SelectedUser.UserId);
        }

        public async Task<ActionResult> OnPostRemoveRole()
        {
            var response = await _api.RevokeUserRoleAsync(int.Parse(SelectedUserRole));

            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMsg"] = "Unable to revoke role, please try again.";
            }
            else
            {
                TempData["SuccessMsg"] = "Account has been updated";
            }

            return await OnGet(SelectedUser.UserId);
        }

        public async Task<ActionResult> OnPostSetClaim()
        {
            var response = await _api.AddUserClaimAsync(NewClaim);

            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMsg"] = "Unable to set new role, please try again.";
            }
            else
            {
                TempData["SuccessMsg"] = "Account has been updated";
            }

            return await OnGet(SelectedUser.UserId);
        }

        public async Task<ActionResult> OnPostRemoveClaim()
        {
            var response = await _api.RevokeUserClaimAsync(int.Parse(SelectedUserClaim));

            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMsg"] = "Unable to set new role, please try again.";
            }
            else
            {
                TempData["SuccessMsg"] = "Account has been updated";
            }

            return await OnGet(SelectedUser.UserId);
        }

        private async Task CreateUserLists()
        {
            var response = await _api.GetAllUsersAsync();

            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMsg"] = "Something went wrong, please reload the page.";
                return;
            }

            var jArray = JArray.Parse(await response.Content.ReadAsStringAsync());

            foreach (var token in jArray)
            {
                var person = token.ToObject<UserOverview>();

                if (person.UserRoles.Any(x => x.Role == RoleTypes.SuperAdmin))
                {
                    SuperAdmins.Add(person);
                }
                else if (person.UserRoles.Any(x => x.Role == RoleTypes.Admin))
                {
                    Admins.Add(person);
                }
                else if (person.UserRoles.Any(x => x.Role == RoleTypes.Helpdesk))
                {
                    HelpdeskUsers.Add(person);
                }
                else if (person.Verified)
                {
                    VerifiedUsers.Add(person);
                }
                else
                {
                    NonVerifiedUsers.Add(person);
                }
            }
        }
    }
}