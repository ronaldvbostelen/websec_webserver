using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using ReversiServer.Assets.Interface;

namespace ReversiServer.Assets
{
    public class ArrayToClaimsList : IJArrayToClaimsList
    {
        public List<Claim> CreateClaimsList(JArray array)
        {
            var list = new List<Claim>();

            foreach (JToken jToken in array)
            {
                list.Add(new Claim(jToken["type"].ToString(), jToken["value"].ToString()));
            }

            return list;
        }
    }
}
