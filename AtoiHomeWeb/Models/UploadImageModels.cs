using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace AtoiHomeWeb.Models
{
    [Table("UploadImage")]
    public class UploadImageModels
    {
        public int ID { get; set; }
        public string UserID { get; set; }
        public DateTime UploadDate { get; set; }
        public string ImagePath { get; set; }
    }
}