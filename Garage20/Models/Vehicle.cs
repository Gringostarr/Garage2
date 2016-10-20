using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Web;

namespace Garage20.Models
{
    public class Vehicle
    {
        public int Id { get; set; }
        private string regNr;
        [RegularExpression(@"[a-zA-Z]{3}(([0-9][0-9][1-9])|([1-9][0-9]0)|(0[1-9]0))", ErrorMessage ="Invalid registration number")]
        public string Regnr {
            get
            {
                return regNr;
            }
            set
            {
                regNr = value.ToUpper();
            }
        }

        [VehicleColor(ErrorMessage = "Unrecognized color")]
        public string Color { get; set; }

        [Range(0, 100)]
        [Display(Name = "Wheels")]
        public int NumberOfWheels { get; set; }

        public VehicleType VehicleType { get; set; }

        public DateTime Checkin { get; set; }

        public DateTime Checkout { get; set; }
    }

    public enum VehicleType
    {
        Car,
        Airplane,
        Boat,
        Bus,
        Motorcycle
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    sealed public class VehicleColor : ValidationAttribute
    {
        public VehicleColor()
        {

        }

        public override bool IsValid(object value)
        {
            string name = value as string;

            Color color = Color.FromName(name);
            return color.IsKnownColor;
        }

        public override string FormatErrorMessage(string name)
        {
            return String.Format(CultureInfo.CurrentCulture, ErrorMessageString, name);
        }
    }
}