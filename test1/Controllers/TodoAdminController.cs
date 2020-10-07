using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using test1.Models;

namespace test1.Controllers
{
    public class TodoAdminController : Controller
    {
        todoEntities _context = new todoEntities();

        // GET: TodoAdmin
        public ActionResult ShowTableAdmin()
        {
            return View();
        }

        public ActionResult LoadData()
        {
            // Mặc định tìm kiếm theo rỗng
            String username = "";
            if (Session["username"] != null)
            {
                username = Session["username"].ToString();
            }


            try
            {


                var draw = Request.Form.GetValues("draw").FirstOrDefault();
                var start = Request.Form.GetValues("start").FirstOrDefault();
                var length = Request.Form.GetValues("length").FirstOrDefault();
                
                var searchValue = Request.Form.GetValues("search[value]").FirstOrDefault();


                //Paging Size (10,20,50,100)    
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt32(start) : 0;
                int recordsTotal = 0;




                // tất cả todo của nhân viên
               
                var customerData = from t in _context.todoitems
                                    join a in _context.accounts on t.user_id equals a.user_id
                                   select new
                                   {
                                       t.title,
                                       t.start_date,
                                       t.end_date,
                                       t.status,
                                       t.partner,
                                       t.todo_id,
                                       t.phamvi,

                                       a.user_name

                                   };


                //Search    
                if (!string.IsNullOrEmpty(searchValue))
                {
                    customerData = customerData.Where(m => m.user_name.Contains(searchValue)
                                                || m.title.Contains(searchValue)
                                                || m.end_date.ToString().Contains(searchValue)
                                                || m.phamvi.Contains(searchValue));
                }


                //total number of rows count     
                recordsTotal = customerData.Count();

                //Sort and Paging     
                var data = customerData.OrderByDescending(a => a.status) // sắp xếp theo tình trạng công việc

                                    .Skip(skip).Take(pageSize).ToList(); // phân trang

                //Returning Json Data  


                return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data, JsonRequestBehavior.AllowGet });

            }

            catch (Exception)
            {
                throw;
            }
            // return View();
        }
    }
}