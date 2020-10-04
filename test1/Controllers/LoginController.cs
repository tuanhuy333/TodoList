﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using test1.Models;

namespace test1.Controllers
{
    public class LoginController : Controller
    {
        public todoEntities db = new todoEntities();
        // GET: Login
        public ActionResult Index()
        {
          
            return View();
        }
        [HttpPost]
        public JsonResult ValidateUser(string userid, string password)
        {
            var data = from c in db.accounts where c.user_name == userid && c.user_password == password select c;
            if (data.Count() > 0)
            {
                // lưu vào Session
                Session["username"] = userid;
                return Json(new { Success = true }, JsonRequestBehavior.AllowGet);
            }
               
               
            else
                return Json(new { Success = false }, JsonRequestBehavior.AllowGet);
        }
    }
}