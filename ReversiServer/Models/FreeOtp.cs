using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReversiServer.Models
{
    public class FreeOtp
    {
//        otpauth://totp/test123?secret=L7VMQD3ED3VQSC3LWQJX4EHDEUEOCFWT6FWQ4YJCC4XRDJ7H6O6NHIA373PBOU72VKXW5CZAW3JKBAFU&algorithm=SHA256&digits=6&period=30
        public string Algorithm { get; } = "SHA256";
        public string Account{ get; set; }
        public string Issuer { get; } = "Reversi";
        public string Secret { get; set; }
        public int Digits { get; } = 6;
        public int Period { get; } = 30;
        public string Type { get; } = "totp";
    }
}
