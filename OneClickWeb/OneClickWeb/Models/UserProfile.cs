using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OneClickWeb.Models
{
    [Table("UserProfile")]
    public class UserProfile
    {
        public int ID { get; set; }
        [Key]
        //[Display(Name = "Email address")]
        //[Required(ErrorMessage = "The email address is required")]
        //[EmailAddress(ErrorMessage = "Invalid Email Address")]
        public String Email { get; set; }
        public String Password { get; set; }
        public String TelNo { get; set; }
        public string SessionID { get; set; }
    }
}