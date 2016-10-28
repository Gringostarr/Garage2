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
using System.Drawing;

namespace Garage20.Controllers
{
    public class VehiclesController : Controller
    {

        public Utilities.Variables vars;

        public VehiclesController()
        {
            this.vars = new Utilities.Variables();
            this.db = new VehicleContext();
        }

        static VehiclesController()
        {
            Utilities.Variables systemVars = new Utilities.Variables();
            parkingPrice = systemVars.ParkingPrice;
            GarageCapacity = systemVars.GarageCapacity;
            loadFactor = new float[GarageCapacity];
        }


        private const float epsilon = 0.01f;
        public static double parkingPrice;
        public static int GarageCapacity;
        private readonly VehicleContext db;
        private static float[] loadFactor;
        private static bool initialized = false;

        // GET: Vehicles
        public ActionResult Index(string orderBy, string filter, string searchString, string colorString, string noWheelsString, string vehicleTypes, string RestrictedView, string view)
        {
            //Patrik test
            var VehicleTypeLst = new List<string>();

            var VehicleQry = from d in db.Vehicles
                             select d.VehicleCategory.Category;
            if (String.IsNullOrEmpty(view)) { view = "true"; };
            ViewBag.RestrictedView = view.ToLower(); // RestrictedView;
            
            // VehicleTypeLst.AddRange(VehicleQry);
            if (!initialized)
            {
                initialized = true;
                InitializeTable();
            }
            ViewBag.vehicleTypes = new SelectList(VehicleQry.Distinct());
            var vehiclesSearch = from v in db.Vehicles
                                 select v;
            ViewBag.SearchString = searchString;
            ViewBag.ColorString = colorString;
            ViewBag.NumberOfWheels = noWheelsString;
            ViewBag.VehicleType = vehicleTypes;
            if (!String.IsNullOrEmpty(searchString))
            {
                vehiclesSearch = vehiclesSearch.Where(s => s.Regnr.StartsWith(searchString));
            }
            if (!String.IsNullOrEmpty(colorString))
            {
                vehiclesSearch = vehiclesSearch.Where(s => s.Color.StartsWith(colorString));
            }
            if (!String.IsNullOrEmpty(noWheelsString))
            {
                int noWheels = int.Parse(noWheelsString);
                vehiclesSearch = vehiclesSearch.Where(s => s.NumberOfWheels.Equals(noWheels));
            }
            if (!string.IsNullOrEmpty(vehicleTypes))
            {
                if (vehicleTypes != "All") vehiclesSearch = vehiclesSearch.Where(s => s.VehicleCategory.Category.ToString() == vehicleTypes);
            }
            //End test

            ViewBag.AllVehicles = true;
            if (! initialized)
            {
                initialized = true;
                InitializeTable();

            }
           if (!string.IsNullOrEmpty(orderBy))
            {
                switch (orderBy)
                {
                    case "regnr":
                        vehiclesSearch = vehiclesSearch.OrderBy(v => v.Regnr);
                        break;

                    case "color":
                        vehiclesSearch = vehiclesSearch.OrderBy(v => v.Color);
                        break;

                    case "wheels":
                        vehiclesSearch = vehiclesSearch.OrderBy(v => v.NumberOfWheels);
                        break;

                    case "checkin":
                        vehiclesSearch = vehiclesSearch.OrderBy(v => v.Checkin);
                        break;

                    case "placing":
                        vehiclesSearch = vehiclesSearch.OrderBy(v => v.Placing);
                        break;

                }
            }
            return View(vehiclesSearch);
        }

        private void InitializeTable()
        {
            foreach (var vehicle in this.db.Vehicles)
            {
                FindParkingSpace(vehicle);
            }
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

        private void FreeParkingSpace(Vehicle vehicle)
        {
            int i = vehicle.Placing;
            int vehicleId = vehicle.VehicleCategoryId;
            var category = db.VehicleCategories.Where(c => c.Id == vehicleId).First();
            float size = category.Size;
            if (size <= 1.0f)
            {
                float temp = Math.Abs(loadFactor[i] - size);
                if (temp < epsilon)
                    temp = 0.0f;
                loadFactor[i] = temp;
            }
            else
            {
                while (size >= 1.0f)
                {
                    loadFactor[i++] = 0.0f;
                    size -= 1.0f;
                }
                loadFactor[i] -= size;
                if (Math.Abs(loadFactor[i]) < epsilon)
                    loadFactor[i] = 0.0f;
            }
        }

        private int FindParkingSpace(Vehicle vehicle)
        {
            int vehicleId = vehicle.VehicleCategoryId;
            var category = db.VehicleCategories.Where(c => c.Id == vehicleId).First();
            float size = category.Size;
            if (size <= 1.0f)
            {
                for (int i = 0; i < GarageCapacity; i++)
                {
                    float temp = loadFactor[i] + size;
                    if (Math.Abs(temp - 1.0f) < epsilon)
                        temp = 1.0f;
                    if (temp <= 1.0f)
                    {
                        loadFactor[i] = temp;
                        return i;
                    }
                }
            }
            else
            {
                for (int i = 0; i < GarageCapacity; i++)
                {
                    int j = i;
                    while (loadFactor[j] == 0.0f && size >= 1.0f)
                    {
                        size -= 1.0f;
                        j++;
                    }
                    if (loadFactor[j] + size <= 1.0f)
                    {
                        for (int k = i; k < j; k++)
                            loadFactor[k] = 1.0f;
                        if (size > 0.0f)
                            loadFactor[j] += size;
                        return i;
                    }
                }
            }
            return -1; 
        }

        public ActionResult Vehicles()
        {
            return View(db.Vehicles.ToList());
        }

        // GET: Vehicles/Create
        public ActionResult Create(int? id)
        {
            ViewBag.MemberId = new SelectList(db.Members, "Id", "Name", id);
            ViewBag.VehicleCategoryId = new SelectList(db.VehicleCategories, "Id", "Category");
            ViewBag.PossibleToAdd = db.Vehicles.Count() < vars.GarageCapacity;
            return View();
        }
        // GET: Vehicles/Search
        public ActionResult Search(string searchString, string colorString, string noWheelsString, string vehicleTypes)
        {
            var VehicleTypeLst = new List<string>();

            var VehicleQry = from d in db.Vehicles
                           orderby d.VehicleType
                           select d.VehicleType;

           // VehicleTypeLst.AddRange(VehicleQry);
            ViewBag.vehicleTypes = new SelectList(VehicleQry.Distinct());

            var vehicles = from v in db.Vehicles
                           select v;

            if (!String.IsNullOrEmpty(searchString))
            {
                vehicles = vehicles.Where(s => s.Regnr.StartsWith(searchString));
            }
            if (!String.IsNullOrEmpty(colorString))
            {
                vehicles = vehicles.Where(s => s.Color.StartsWith(colorString));
            }
            if (!String.IsNullOrEmpty(noWheelsString))
            {
                int noWheels = int.Parse(noWheelsString);
                vehicles = vehicles.Where(s => s.NumberOfWheels.Equals(noWheels));
            }
            if (!string.IsNullOrEmpty(vehicleTypes))
            {
                if (vehicleTypes != "All") { vehicles = vehicles.Where(s => s.VehicleType.ToString() == vehicleTypes); }
            }

            return View(vehicles);
            //return View(db.Vehicles.ToList());
        }
        // POST: Vehicles/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,VehicleCategoryId,MemberId,Regnr,Color,NumberOfWheels,VehicleType,Checkin,Checkout,Placing")] Vehicle vehicle)
        {
            var vehicles = from v in db.Vehicles
                           select v;



            string name = vehicle.Color as string;
            Color color = Color.FromName(name);

            if (!color.IsKnownColor)
            {
                ViewBag.PossibleToAdd = db.Vehicles.Count() < vars.GarageCapacity;
                ModelState.AddModelError("VehicleColor", "Color is not recognized!");
                ViewBag.MemberId = new SelectList(db.Members, "Id", "Name", vehicle.MemberId);
                ViewBag.VehicleCategoryId = new SelectList(db.VehicleCategories, "Id", "Category", vehicle.VehicleCategoryId);
                return View(vehicle);
            }


            if (vehicles.Any(o => o.Regnr == vehicle.Regnr))
            {
                ViewBag.PossibleToAdd = db.Vehicles.Count() < vars.GarageCapacity;
                ModelState.AddModelError("RegNr", "Registration number exists");
                ViewBag.MemberId = new SelectList(db.Members, "Id", "Name", vehicle.MemberId);
                ViewBag.VehicleCategoryId = new SelectList(db.VehicleCategories, "Id", "Category", vehicle.VehicleCategoryId);
                return View(vehicle);
            }
            if (ModelState.IsValid)
            {
                //var vehicles = db.Vehicles.ToList();

                vehicle.Checkin = DateTime.Now;
                vehicle.Checkout = (DateTime)SqlDateTime.MinValue;
                var category = db.VehicleCategories.Where(c => c.Id == vehicle.VehicleCategoryId).First();
                int position = this.FindParkingSpace(vehicle);
                if (position == -1)
                {
                    ViewBag.PossibleToAdd = db.Vehicles.Count() < vars.GarageCapacity;
                    ModelState.AddModelError("Create", "Garage is full");
                    return View(vehicle);
                }
                else
                {
                    vehicle.Placing = position;
                    db.Vehicles.Add(vehicle);
                    db.SaveChanges();
                }
                return RedirectToAction("Index");
            }
            ViewBag.MemberId = new SelectList(db.Members, "Id", "Name", vehicle.MemberId);
            ViewBag.VehicleCategoryId = new SelectList(db.VehicleCategories, "Id", "Category", vehicle.VehicleCategoryId);
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
            ViewBag.MemberId = new SelectList(db.Members, "Id", "Name", vehicle.MemberId);
            ViewBag.VehicleCategoryId = new SelectList(db.VehicleCategories, "Id", "Category", vehicle.VehicleCategoryId);
            return View(vehicle);
        }

        // POST: Vehicles/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,VehicleCategoryId,MemberId,Regnr,Color,NumberOfWheels,VehicleType,Checkin,Checkout,Placing")] Vehicle vehicle)
        {
            if (ModelState.IsValid)
            {
               vehicle.Checkout = (DateTime)SqlDateTime.MinValue;
                db.Entry(vehicle).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.MemberId = new SelectList(db.Members, "Id", "Name", vehicle.MemberId);
            ViewBag.VehicleCategoryId = new SelectList(db.VehicleCategories, "Id", "Category", vehicle.VehicleCategoryId);
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
            this.FreeParkingSpace(vehicle);
            vehicle.Checkout = DateTime.Now;
            TimeSpan parkingtime = vehicle.Checkout - vehicle.Checkin;
            ViewBag.Parkingtime = Math.Round(parkingtime.TotalMinutes, 0);
            ViewBag.Parkingcost = Math.Round((parkingtime.TotalHours * vars.ParkingPrice), 2);
            ViewBag.ParkingVAT = Math.Round((parkingtime.TotalHours * vars.ParkingPrice) /5, 2);

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

            if (db.Vehicles.Count() > 0)
            {
                double totalTime = 0;

                foreach (var vc in db.VehicleCategories)
                {
                    ViewBag.Count += vc.Category + ":" + db.Vehicles.Where(v => v.VehicleCategoryId == vc.Id).Count() + " | ";
                }



                //ViewBag.CarCount = db.Vehicles.Where(v => v.VehicleType == VehicleType.Car).Count();
                //ViewBag.BusCount = db.Vehicles.Where(v => v.VehicleType == VehicleType.Bus).Count();
                //ViewBag.AirplaneCount = db.Vehicles.Where(v => v.VehicleType == VehicleType.Airplane).Count();
                //ViewBag.BoatCount = db.Vehicles.Where(v => v.VehicleType == VehicleType.Boat).Count();
                //ViewBag.MotorcycleCount = db.Vehicles.Where(v => v.VehicleType == VehicleType.Motorcycle).Count();
                ViewBag.WheelCount = db.Vehicles.Sum(x => x.NumberOfWheels);
                ViewBag.VehicleCount = db.Vehicles.Count();

                foreach (var vehicle in db.Vehicles)
                {
                    TimeSpan parkingtime = DateTime.Now - vehicle.Checkin;
                    totalTime += parkingtime.TotalMinutes;
                }

                ViewBag.Parkingtime = Math.Round(totalTime);

                ViewBag.ParkingtimeHour = Utilities.Utility.MinutesToHour(totalTime);

                ViewBag.Parkingcost = Math.Round((totalTime * vars.ParkingPrice / 60), 2);
            }
            


            return View();
        }
    }
}
