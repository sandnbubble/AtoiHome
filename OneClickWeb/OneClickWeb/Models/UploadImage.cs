using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations.Schema;

namespace OneClickWeb.Models
{
    [Table("UploadImage")]
    public class UploadImage
    {
        public int ID { get; set; }
        public string UserID { get; set; }
        public DateTime UploadDate { get; set; }
        public string ImagePath { get; set; }
    }
}