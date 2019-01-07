using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;

namespace OneClickWeb.Models
{
    public class UploadImage
    {
        public int ID { get; set; }
        public string UserID { get; set; }
        public DateTime UploadDate { get; set; }
        public string ImagePath { get; set; }
    }

    public class UploadImageDBContext : DbContext
    {
        public DbSet<UploadImage> UploadImages { get; set; }
    }
}