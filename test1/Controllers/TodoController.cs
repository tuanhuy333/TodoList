using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Objects;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
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
                                filepath = t.file_attach,
                                userid = a.user_id
                            }).FirstOrDefault();
                var comment = _context.comments.Where(m => m.todo_id == data.todo_id).FirstOrDefault();
                if (comment != null)
                {
                    data.binhluan = comment.content_comment.ToString();

                }
                data.listComment = this.getCommentByTodoId(data.todo_id);

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
                return RedirectToAction("Todo/add/");
            }

        }
        public ActionResult add()
        {
            if (Session["username"] != null)
            {
                string u_name = (String)Session["username"];
                account acc = _context.accounts.Where(m => m.user_name == u_name).FirstOrDefault();
                var data = new TodoViewModel();
                data.getAllAccount = _context.accounts.Where(m => m.user_id != acc.user_id).ToList();
                data.userid = acc.user_id;

                return View(data);


            }
            else
            {
                return RedirectToAction("index", "login", new { area = "" });
            }


        }
        [HttpPost]
        public ActionResult add(TodoViewModel model, FormCollection fc, HttpPostedFileBase file)
        {
            string filePath = string.Empty;
            todoitem todoitem = new todoitem();
            todoitem.title = model.tenCV;
            todoitem.start_date = Convert.ToDateTime(fc["ngayBD"]);
            todoitem.end_date = Convert.ToDateTime(fc["ngayKT"]);
            if (fc["partner"] == null)
            {
                todoitem.partner = "";
            }
            else
            {
                todoitem.partner = fc["partner"];
            }
            todoitem.phamvi = fc["range"];
            todoitem.user_id = Convert.ToInt32(fc["userid"]);
            todoitem.status = 0;
            if (file != null)
            {
                byte[] uploadedFile = new byte[file.InputStream.Length];
                file.InputStream.Read(uploadedFile, 0, uploadedFile.Length);
                string folderPath = "~/Content/upload_files/";
                this.WriteBytesToFile(this.Server.MapPath(folderPath), uploadedFile, file.FileName);
                filePath = folderPath + file.FileName;
            }
            try
            {
                string strSQL = "INSERT INTO todoitem ";
                strSQL += " (user_id,title,start_date,end_date,status,partner,phamvi,file_attach)";
                strSQL += " VALUES";
                strSQL += " (@user_id,@title,@start_date,@end_date";
                strSQL += ",@status,@partner,@phamvi,@file)";
                List<SqlParameter> parameterList = new List<SqlParameter>();
                parameterList.Add(new SqlParameter("@user_id", todoitem.user_id));
                parameterList.Add(new SqlParameter("@title", todoitem.title));
                parameterList.Add(new SqlParameter("@start_date", todoitem.start_date));
                parameterList.Add(new SqlParameter("@end_date", todoitem.end_date));
                parameterList.Add(new SqlParameter("@status", todoitem.status));
                parameterList.Add(new SqlParameter("@partner", todoitem.partner));
                parameterList.Add(new SqlParameter("@phamvi", todoitem.phamvi));
                parameterList.Add(new SqlParameter("@file", filePath));

                SqlParameter[] Param = parameterList.ToArray();

                int noOfRowInserted = _context.Database.ExecuteSqlCommand(strSQL, Param);
                return RedirectToAction("ShowTable");

            }
            catch (Exception e)
            {
                return RedirectToAction("add");
            }
        }
        private void WriteBytesToFile(string rootFolderPath, byte[] fileBytes, string filename)
        {
            try
            {
                // Verification.  
                if (!Directory.Exists(rootFolderPath))
                {
                    // Initialization.  
                    string fullFolderPath = rootFolderPath;

                    // Settings.  
                    string folderPath = new Uri(fullFolderPath).LocalPath;

                    // Create.  
                    Directory.CreateDirectory(folderPath);
                }

                // Initialization.                  
                string fullFilePath = rootFolderPath + filename;

                // Create.  
                FileStream fs = System.IO.File.Create(fullFilePath);

                // Close.  
                fs.Flush();
                fs.Dispose();
                fs.Close();

                // Write Stream.  
                BinaryWriter sw = new BinaryWriter(new FileStream(fullFilePath, FileMode.Create, FileAccess.Write));

                // Write to file.  
                sw.Write(fileBytes);

                // Closing.  
                sw.Flush();
                sw.Dispose();
                sw.Close();
            }
            catch (Exception ex)
            {
                // Info.  
                throw ex;
            }
        }
        public ActionResult DownloadFile(int todo_id)
        {
            try
            {
                // Loading dile info.  
                todoitem item = this._context.todoitems.Where(m => m.todo_id == todo_id).FirstOrDefault();

                // Info.  
                return this.GetFile(item.file_attach);
            }
            catch (Exception ex)
            {
                // Info  
                Console.Write(ex);
            }

            // Info.  
            string url = "/Todo/edit/" + todo_id;
            return Redirect(url);
        }
        private FileResult GetFile(string filePath)
        {
            // Initialization.  
            FileResult file = null;

            try
            {
                // Initialization.  
                string contentType = MimeMapping.GetMimeMapping(filePath);

                // Get file.  
                file = this.File(filePath, contentType);
            }
            catch (Exception ex)
            {
                // Info.  
                throw ex;
            }

            // info.  
            return file;
        }
        private List<CommentViewModel> getCommentByTodoId(int todo_id)
        {
            List<CommentViewModel> recs = _context.comments.Where(m => m.todo_id == todo_id).Join(_context.accounts, c => c.user_id, p => p.user_id,
                         (c, p) => new CommentViewModel
                         {
                             userName = p.user_name.ToString(),
                             content = c.content_comment.ToString()
                         }).ToList();

            return recs;
        }
        [HttpPost]
        public JsonResult comment(int user_id, string todo_id, string content)
        {
            if ( todo_id != null && content != "")
            {
                try
                {
                    String a= Session["username"].ToString();
                    account acc = _context.accounts.Where(m => m.user_name == a).FirstOrDefault();
                    string strSQL = "INSERT INTO comment(user_id,todo_id,content_comment) VALUES(@user_id,@todo_id,@content_comment)";
                    List<SqlParameter> parameterList = new List<SqlParameter>();
                    parameterList.Add(new SqlParameter("@user_id", acc.user_id));
                    parameterList.Add(new SqlParameter("@todo_id", todo_id));
                    parameterList.Add(new SqlParameter("@content_comment", content));
                    SqlParameter[] Param = parameterList.ToArray();
                    int noOfRowInserted = _context.Database.ExecuteSqlCommand(strSQL, Param);         
                    return Json(new { Success = true, username = acc.user_name, commentbody = content }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    return Json(new { Success = false }, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                return Json(new { Success = false }, JsonRequestBehavior.AllowGet);

            }
        }

}
}