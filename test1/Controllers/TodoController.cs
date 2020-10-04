using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using test1.Models;

namespace test1.Controllers
{
    public class TodoController : Controller
    {
        todoEntities _context = new todoEntities();


        // GET: Todo
        public ActionResult ShowTable()
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
                    var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
                    var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
                    var searchValue = Request.Form.GetValues("search[value]").FirstOrDefault();


                    //Paging Size (10,20,50,100)    
                    int pageSize = length != null ? Convert.ToInt32(length) : 0;
                    int skip = start != null ? Convert.ToInt32(start) : 0;
                    int recordsTotal = 0;



                // Getting all Customer data  
                /*
                 var customerData = (from tempcustomer in _context.accounts
                                     select tempcustomer);

            */
                var customerData = (from t in _context.todoitems
                                    join a in _context.accounts on t.user_id equals a.user_id
                                    where a.user_name ==  username
                                    select new
                                    {
                                        t.title,
                                        t.start_date,
                                        t.end_date,
                                        t.status,
                                        t.partner,
                                        
                                        t.todo_id,
                                        a.user_name
                                       
                                    });


                //Sorting    
                    if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                    {
                        //customerData = customerData.OrderBy(sortColumn + " " + sortColumnDir);
                    }
                    //Search    
                    if (!string.IsNullOrEmpty(searchValue))
                    {
                        customerData = customerData.Where(m => m.user_name == searchValue);
                    }
                  

                    //total number of rows count     
                    recordsTotal = customerData.Count();

                    //Paging     
                    var data = customerData.OrderBy(a => a.start_date).Skip(skip).Take(pageSize).ToList();

                //Returning Json Data  


                return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data, JsonRequestBehavior.AllowGet });

            }

            catch (Exception)
            {
                throw;
            }
           // return View();
        }

        public ActionResult Edit(int id)
        {
            return View();

        }
    }
}