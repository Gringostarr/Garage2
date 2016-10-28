using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Garage20.Models
{
    public class VehicleCategory
    {
        public int Id { get; set; }
        [Required]
        public string Category { get; set; }
        public  float Size { get; set; }
    }
}