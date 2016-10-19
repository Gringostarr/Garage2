using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Garage20.Models;
using System.Data.SqlTypes;

namespace Garage20.Controllers
{
    public class VehiclesController : Controller
    {
        private VehicleContext db = new VehicleContext();
        private double parkingPrice = 60;

        // GET: Vehicles
        public ActionResult Index(string orderBy, string filter)
        {
            var vehicles = db.Vehicles.ToList();
            ViewBag.AllVehicles = true;
            ViewBag.VehicleType = "";
            if (filter != null)
            {
                ViewBag.AllVehicles = false;
                ViewBag.VehicleType = this.PluralOf(filter);
                vehicles = vehicles.Where(v => v.VehicleType.ToString() == filter).ToList();
            }
            else if (orderBy != null)
            {
                switch (orderBy)
                {
                    case "regnr":
                        vehicles = vehicles.OrderBy(v => v.Regnr).ToList();
                        break;

                    case "color":
                        vehicles = vehicles.OrderBy(v => v.Color).ToList();
                        break;

                    case "wheels":
                        vehicles = vehicles.OrderBy(v => v.NumberOfWheels).ToList();
                        break;

                    case "checkin":
                        vehicles = vehicles.OrderBy(v => v.Checkin).ToList();
                        break;

                    case "checkout":
                        vehicles = vehicles.OrderBy(v => v.Checkout).ToList();
                        break;

                }
            }
            return View(vehicles);
        }

        // GET: Vehicles/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Vehicle vehicle = db.Vehicles.Find(id);
            if (vehicle == null)
            {
                return HttpNotFound();
            }
            return View(vehicle);
        }

        public ActionResult Vehicles()
        {
            return View(db.Vehicles.ToList());
        }

        // GET: Vehicles/Create
        public ActionResult Create()
        {
            return View();
        }
        // GET: Vehicles/Search
        public ActionResult Search(string searchString, string colorString, string noWheelsString, string vehicleType)
        {
            var VehicleTypeLst = new List<string>();

            var VehicleQry = from d in db.Vehicles
                           orderby d.VehicleType
                           select d.VehicleType;

           // VehicleTypeLst.AddRange(VehicleQry);
            ViewBag.vehicleType = new SelectList(VehicleQry.Distinct());

            var vehicles = from v in db.Vehicles
                           select v;

            if (!String.IsNullOrEmpty(searchString))
            {
                vehicles = vehicles.Where(s => s.Regnr.StartsWith(searchString));
            }
            if (!String.IsNullOrEmpty(colorString))
            {
                vehicles = vehicles.Where(s => s.Regnr.StartsWith(colorString));
            }
            if (!String.IsNullOrEmpty(noWheelsString))
            {
                int noWheels = int.Parse(noWheelsString);
                vehicles = vehicles.Where(s => s.NumberOfWheels.Equals(noWheels));
            }
            if (!string.IsNullOrEmpty(vehicleType))
            {
                if (vehicleType != "All") { vehicles = vehicles.Where(s => s.VehicleType.ToString() == vehicleType); }
            }

            return View(vehicles);
            //return View(db.Vehicles.ToList());
        }
        // POST: Vehicles/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Regnr,Color,NumberOfWheels,VehicleType,Checkin,Checkout")] Vehicle vehicle)
        {
            if (ModelState.IsValid)
            {
                vehicle.Checkin = DateTime.Now;
                vehicle.Checkout = (DateTime)SqlDateTime.MinValue;
                db.Vehicles.Add(vehicle);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(vehicle);
        }

        // GET: Vehicles/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Vehicle vehicle = db.Vehicles.Find(id);
            if (vehicle == null)
            {
                return HttpNotFound();
            }
            return View(vehicle);
        }

        // POST: Vehicles/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Regnr,Color,NumberOfWheels,VehicleType,Checkin,Checkout")] Vehicle vehicle)
        {
            if (ModelState.IsValid)
            {
                db.Entry(vehicle).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(vehicle);
        }

        // GET: Vehicles/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Vehicle vehicle = db.Vehicles.Find(id);
            if (vehicle == null)
            {
                return HttpNotFound();
            }
            vehicle.Checkout = DateTime.Now;
            TimeSpan parkingtime = vehicle.Checkout - vehicle.Checkin;
            ViewBag.Parkingtime = Math.Round(parkingtime.TotalMinutes, 0);
            ViewBag.Parkingcost = Math.Round((parkingtime.TotalHours * parkingPrice), 2);
            ViewBag.ParkingVAT = Math.Round((parkingtime.TotalHours * parkingPrice) /5, 2);

            return View(vehicle);
        }

        // POST: Vehicles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Vehicle vehicle = db.Vehicles.Find(id);
            db.Vehicles.Remove(vehicle);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private string PluralOf(string vehicleType)
        {
            switch (vehicleType.ToLower())
            {
                case "car": return "Cars";
                case "bus": return "Buses";
                case "motorcycle": return "MotorCycles";
                case "boat": return "Boats";
                case "airplane": return "Airplanes";
                default: return vehicleType + "s";
            }
        }

        public ActionResult Statistics()
        {
            double totalTime = 0;
            ViewBag.CarCount = db.Vehicles.Where(v => v.VehicleType == VehicleType.Car).Count();
            ViewBag.BusCount = db.Vehicles.Where(v => v.VehicleType == VehicleType.Bus).Count();
            ViewBag.AirplaneCount = db.Vehicles.Where(v => v.VehicleType == VehicleType.Airplane).Count();
            ViewBag.BoatCount = db.Vehicles.Where(v => v.VehicleType == VehicleType.Boat).Count();
            ViewBag.MotorcycleCount = db.Vehicles.Where(v => v.VehicleType == VehicleType.Motorcycle).Count();
            ViewBag.WheelCount = db.Vehicles.Sum(x => x.NumberOfWheels);
            ViewBag.VehicleCount = db.Vehicles.Count();

            foreach (var vehicle in db.Vehicles)
            {
                TimeSpan parkingtime = DateTime.Now - vehicle.Checkin;
                totalTime += parkingtime.TotalMinutes;
            }

            ViewBag.Parkingtime = Math.Round(totalTime);

            ViewBag.ParkingtimeHour = Utilities.Utility.MinutesToHour(totalTime);

            ViewBag.Parkingcost = Math.Round((totalTime * parkingPrice/60), 2);



            return View();
        }



    }
}
