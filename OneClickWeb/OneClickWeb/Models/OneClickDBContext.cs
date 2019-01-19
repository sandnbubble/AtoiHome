using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;

namespace OneClickWeb.Models
{
    public class OneClickDBContext : DbContext
    {
        public DbSet<UploadImage> UploadImages { get; set; }
        public DbSet<UserProfile> UserProfiles { get; set; }
    }
}