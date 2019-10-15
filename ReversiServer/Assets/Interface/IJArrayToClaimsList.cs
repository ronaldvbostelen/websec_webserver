using System.Collections.Generic;
using System.Security.Claims;
using Newtonsoft.Json.Linq;

namespace ReversiServer.Assets.Interface
{
    public interface IJArrayToClaimsList
    {
        List<Claim> CreateClaimsList(JArray array);
    }
}