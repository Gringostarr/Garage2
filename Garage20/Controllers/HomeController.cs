using Garage20.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Garage20.Controllers
{
    public class HomeController : Controller
    {
        private VehicleContext db = new VehicleContext();

        public ActionResult Index()
        {

            Utilities.Variables vars = new Utilities.Variables();
            ViewBag.Parkingspots = vars.GarageCapacity - db.Vehicles.Count();
            ViewBag.ParkingPrice = vars.ParkingPrice;
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}