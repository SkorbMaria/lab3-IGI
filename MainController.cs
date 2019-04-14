using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using lab1_ef;

namespace lab2_igi.Controllers
{
    public class MainController : Controller
    {         
        IMemoryCache _memoryCache;
        static BibliotekaContext db = new BibliotekaContext();

        public MainController(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        [ResponseCache(CacheProfileName = "NoCaching")]
        public IActionResult Index()
        {
            Initializer.Initialize(db);
            return View();
        }

        // using mem cache
        [Route("gr")]
        public ActionResult GetRooms()
        {
            string path = Request.Path.Value.ToLower();
            Room roomMemoryCached = (Room)_memoryCache.Get(path);

            // new room in session
            if (HttpContext.Session.Get("RoomSession") != null)
            {
                string[] roomSess = HttpContext.Session.GetString("RoomSession").Split(";");
                Room roomSession = new Room()
                {
                    RoomId = Convert.ToInt32(roomSess[0]),
                    RoomNo = roomSess[1],
                    Capacity = Convert.ToInt32(roomSess[2]),
                    Cost = Convert.ToDouble(roomSess[3])
                };
                ViewData["roomSession"] = roomSession;
            }
            
            ViewData["roomFromMemory"] = roomMemoryCached;
            return View(db.Rooms.ToList());
        }

        [Route("gst")]
        public ActionResult GetServiceTypes()
        {
            string path = Request.Path.Value.ToLower();
            ServiceType stMemoryCached = (ServiceType)_memoryCache.Get(path);

            ViewData["stFromMemory"] = stMemoryCached;

            if (Request.Cookies["STypeCookie"] != null)
            {
                ServiceType stFromCookie = JsonConvert.DeserializeObject<ServiceType>(Request.Cookies["STypeCookie"].ToString());
                ViewData["stFromCookie"] = stFromCookie;
            }
            
            return View(db.ServiceTypes.ToList());
        }

        [Route("gemp")]
        public ActionResult GetEmployees()
        {
            string path = Request.Path.Value.ToLower();

            ViewData["empFromMemory"] = (Employee)_memoryCache.Get(path);

            return View(db.Employees.ToList());
        }

        [HttpGet]
        public ActionResult AddRoom()
        {
            var room = new Room();
            return View(room);
        }

        // using session
        [HttpPost]
        public ActionResult AddRoom(Room room)
        {
            db.Rooms.Add(room);
            db.SaveChanges();

            room.CostDate = DateTime.Now;            
            string roomToSession = room.RoomId + ";" + room.RoomNo + ";" + room.Capacity + ";" + room.Cost;
            HttpContext.Session.SetString("RoomSession", roomToSession);

            return Redirect("~/gr");
        }


        [HttpGet]
        public ActionResult AddServiceType()
        {
            var room = new ServiceType();
            return View(room);
        }

        // using cookies
        [HttpPost]
        public ActionResult AddServiceType(ServiceType serviceType)
        {
            db.ServiceTypes.Add(serviceType);
            db.SaveChanges();

            if (Request.Cookies["STypeCookie"] == null)
            {
                CookieOptions cookie = new CookieOptions
                {
                    Expires = DateTime.Now.AddMinutes(1)
                };

                string value = JsonConvert.SerializeObject(serviceType);
                Response.Cookies.Append("STypeCookie", value, cookie);
            }

            return Redirect("~/gst");
        }
    }
}