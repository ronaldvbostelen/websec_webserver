using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ReversiServer.Assets.Interface;

namespace ReversiServer.Assets
{
    public class StringValidator : IPasswordValidator
    {
        private const string digitRegex = @"[\d+]";
        private const string upperRegex = @"[A-Z+]";
        private const string lowerRegex = @"[a-z+]";
        private const string specialRegex = "[!@#$%^&*()_+-=,.<>?]";

        public bool StrongPassword(string password) => password.Length > 7 &&
                                                       Regex.IsMatch(password, digitRegex) &&
                                                       Regex.IsMatch(password, upperRegex) &&
                                                       Regex.IsMatch(password, lowerRegex) &&
                                                       Regex.IsMatch(password, specialRegex);
    }
}
