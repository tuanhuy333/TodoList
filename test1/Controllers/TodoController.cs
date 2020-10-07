using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Objects;
using System.Data.SqlClient;
using System.Linq;

using System.Web.Mvc;
using test1.Models;
using test1.ViewModel;

namespace test1.Controllers
{
    public class TodoController : Controller
    {
        todoEntities _context = new todoEntities();




        [HttpPost]
        public ActionResult updateStatus(int todo_id)
        {
            todoitem todo = (from t in _context.todoitems
                            where t.todo_id == todo_id
                            select t).FirstOrDefault();
            todo.status = 1;
            _context.SaveChanges();
            return RedirectToAction("showTable","Todo");
        }
        // GET: Todo
        public ActionResult ShowTable()
        {
            if (Session["username"] != null)
            {

                return View();
            }
            return RedirectToAction("index", "login", new { area = "" }); // có thể truyền string "ch đăng nhập" để thông báo
        }

        public void checkDateAndUpdate()
        {
           
            var today = DateTime.Now;

            var q = _context.todoitems.Where(t => DbFunctions.TruncateTime(t.end_date) < today.Date)
                            .OrderBy(t => t.end_date).ToList();
            
            foreach (var item in q)
            {
                item.status = -1; // đã trễ
                _context.SaveChanges();
            }
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
                checkDateAndUpdate();

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




                // tất cả todo của nhân viên
                var data1 = (from t in _context.todoitems
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

                             });
                // todo của nhân viên khac có phạm vi là private và là staff
                var data2 = from t in _context.todoitems
                            join a in _context.accounts on t.user_id equals a.user_id
                            where a.user_type == 1 + ""  // la nhan vien
                                && a.user_name != username
                                && t.phamvi == "private"

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

                // không làm chung
                var data3 = from t in _context.todoitems
                            join a in _context.accounts on t.user_id equals a.user_id
                            where a.user_type == 1 + ""  // la nhan vien
                                && a.user_name != username
                                && !t.partner.Contains(username)// ko có 'huy'
                                && t.phamvi == "private"


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
                var customerData = data1.Except(data2).Except(data3); // tập hợp ko có trong 2

                
                //Sorting    
                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                    {
                        //customerData = customerData.OrderBy(sortColumn + " " + sortColumnDir);
                    }
                    //Search    
                    if (!string.IsNullOrEmpty(searchValue))
                    {
                        customerData = customerData.Where(m => m.user_name.Contains( searchValue)
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
        /*
        public ActionResult Edit(int? id)
        {
            try
            {
                var mModel = (from t in _context.todoitems
                           join a in _context.accounts on t.user_id equals a.user_id
                           where t.todo_id == id
                               
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

                           });
                TodoViewModel model = new TodoViewModel();
                foreach(var item in mModel.ToList())
                {
                    model.tenCV = item.title;
                    model.nguoitao = item.user_name;
                    model.ngayBD = item.start_date;
                    model.ngayKT = item.end_date;
                    model.nguoilamchung = item.partner;
                    model.phamvi = item.phamvi;
                    model.trangthai = item.status+""; 
                    
                }

                return View(model);
               
            }
            catch (Exception)
            {
                throw;
            }

        }
        */
        public ActionResult Edit(int id)
        {
            if (Session["username"] != null)
            {
                var data = (from t in _context.todoitems
                            join a in _context.accounts on t.user_id equals a.user_id
                            where t.todo_id == id
                            select new TodoViewModel
                            {
                                tenCV = t.title,
                                ngayBD = t.start_date.ToString(),
                                ngayKT = t.end_date.ToString(),
                                trangthai = t.status.ToString(),
                                nguoilamchung = t.partner,
                                todo_id = t.todo_id,
                                username = a.user_name,
                            }).FirstOrDefault();
                var comment = _context.comments.Where(m => m.todo_id == data.todo_id).FirstOrDefault();
                if (comment != null)
                {
                    data.binhluan = comment.content_comment.ToString();

                }

                return View(data);
            }
            else
            {
                return RedirectToAction("index", "login", new { area = "" });
            }


        }
        [HttpPost]
        public ActionResult Edit(TodoViewModel todoViewModel, FormCollection fc)
        {
            int id = Convert.ToInt32(fc["todo_id"]);
            string status = fc["status"];
            try
            {
                string strSQL = "UPDATE todoitem  SET status=@status WHERE todo_id=@todo_id ";
                List<SqlParameter> parameterList = new List<SqlParameter>();
                parameterList.Add(new SqlParameter("@status", status));
                parameterList.Add(new SqlParameter("@todo_id", id));
                SqlParameter[] Param = parameterList.ToArray();
                int noOfRowInserted = _context.Database.ExecuteSqlCommand(strSQL, Param);
                return RedirectToAction("ShowTable");
            }
            catch (Exception e)
            {
                return RedirectToAction("Todo/add");
            }

        }
        public ActionResult add()
        {
            if (Session["username"] != null)
            {
                string u_name = (String)Session["username"];
                account acc = _context.accounts.Where(m => m.user_name == u_name).FirstOrDefault();
                var data = new TodoViewModel();
                data.getAllAccount = _context.accounts.Where(m=>m.user_id != acc.user_id).ToList();
                data.userid = acc.user_id;

                return View(data);


            }
            else
            {
                return RedirectToAction("index", "login", new { area = "" });
            }


        }
        [HttpPost]
        public ActionResult add(TodoViewModel model, FormCollection fc)
        {
            todoitem todoitem = new todoitem();
            todoitem.title = model.tenCV;
            todoitem.start_date = Convert.ToDateTime(fc["ngayBD"]);
            todoitem.end_date = Convert.ToDateTime(fc["ngayKT"]);
            todoitem.partner = fc["partner"];
            todoitem.phamvi = fc["range"];
            todoitem.user_id = Convert.ToInt32(fc["userid"]);
            todoitem.status = 0;
            try
            {
                string strSQL = "INSERT INTO todoitem ";
                strSQL += " (user_id,title,start_date,end_date,status,partner,phamvi)";
                strSQL += " VALUES";
                strSQL += " (@user_id,@title,@start_date,@end_date";
                strSQL += ",@status,@partner,@phamvi)";
                List<SqlParameter> parameterList = new List<SqlParameter>();
                parameterList.Add(new SqlParameter("@user_id", todoitem.user_id));
                parameterList.Add(new SqlParameter("@title", todoitem.title));
                parameterList.Add(new SqlParameter("@start_date", todoitem.start_date));
                parameterList.Add(new SqlParameter("@end_date", todoitem.end_date));
                parameterList.Add(new SqlParameter("@status", todoitem.status));
                parameterList.Add(new SqlParameter("@partner", todoitem.partner));
                parameterList.Add(new SqlParameter("@phamvi", todoitem.phamvi));
                SqlParameter[] Param = parameterList.ToArray();

                int noOfRowInserted = _context.Database.ExecuteSqlCommand(strSQL, Param);
                return RedirectToAction("ShowTable");

            }
            catch (Exception e)
            {
                return RedirectToAction("Todo/add");
            }
        }

    }
}