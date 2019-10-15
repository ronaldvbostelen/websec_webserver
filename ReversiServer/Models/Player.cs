using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ReversiServer.Models
{
    public class Player
    {
        public Guid PlayerId { get; set; }
        
        [Required]
        [StringLength(255, MinimumLength = 8, ErrorMessage = "Username must be with a minimum length of '8'.")]
        [RegularExpression("[a-zA-Z0-9]*$", ErrorMessage = "No white space or specialchars in username")]
        public string Username { get; set; }

        [StringLength(255)]
        [MinLength(4)]
        public string Screenname { get; set; }

        [Required]
        [StringLength(255, MinimumLength = 8, ErrorMessage = "Password does not meet the requirements.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public string Salt { get; set; }

        [Required]
        [StringLength(255)]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        public bool Verified { get; set; }

        public int Role { get; set; }

        public char Status { get; set; }

        public bool Deleted { get; set; }

        public int loginAttempt { get; set; }
    }
}