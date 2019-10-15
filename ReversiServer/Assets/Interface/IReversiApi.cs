using System.Net.Http;
using System.Threading.Tasks;
using Client_tech_resversi_api.Models.Non_DB_models;
using Client_tech_resversi_api.Models.Principal;

namespace ReversiServer.Assets.Interface
{
    public interface IReversiApi
    {
        Task<HttpResponseMessage> CreateUserAsync(RegisterUser user);
        Task<HttpResponseMessage> LoginAsync(LoginUser user);
        Task<HttpResponseMessage> ValidateActivationCodeAsync(int userId, string code);
        Task<HttpResponseMessage> NewActivationCodeAsync(int userId);
        Task<HttpResponseMessage> RequestResetCodeAsync(string emailOrUsername);
        Task<HttpResponseMessage> ResetPasswordAsync(ResetUserPassword user);
        Task<HttpResponseMessage> FetchUserProfileAsync(int userId);
        Task<HttpResponseMessage> UpdateUserAsync(int userId, UserProfile user);
        Task<HttpResponseMessage> ChangePasswordAsync(int userId, ChangeUserPassword changeUserPassword);
        Task<HttpResponseMessage> GetAllUsersAsync();
        Task<HttpResponseMessage> GetUserAsync(int userId);
        Task<HttpResponseMessage> ChangeEmailAsync(int userId, string emailaddress);
        Task<HttpResponseMessage> ChangePasswordAsync(int userId, string password);
        Task<HttpResponseMessage> BanUserAsync(int userId);
        Task<HttpResponseMessage> UnBanUserAsync(int userId);
        Task<HttpResponseMessage> AddUserRoleAsync(UserRole userRole);
        Task<HttpResponseMessage> RevokeUserRoleAsync(int roleId);
        Task<HttpResponseMessage> AddUserClaimAsync(UserClaim userClaim);
        Task<HttpResponseMessage> RevokeUserClaimAsync(int claimId);
        Task<HttpResponseMessage> EnableTwoFaAsync(int userId);
        Task<HttpResponseMessage> DisableTwoFaAsync(int userId);
        Task<HttpResponseMessage> ConfirmTwoFaAsync(TwoFaUser twoFaUser);
        Task<HttpResponseMessage> LoginTwoFaAsync(TwoFaLoginUser twoFaLoginUser);
    }
}