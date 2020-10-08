using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using test1.Models;
using test1.ViewModel;

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
                                userid = a.user_id,
                                range = t.phamvi
                            }).FirstOrDefault();
                var comment = _context.comments.Where(m => m.todo_id == data.todo_id).FirstOrDefault();
                if (comment != null)
                {
                    data.binhluan = comment.content_comment.ToString();

                }
                data.listComment = this.getCommentByTodoId(data.todo_id);
                string u_name = (String)Session["username"];
                account acc = _context.accounts.Where(m => m.user_name == u_name).FirstOrDefault();
                data.getAllAccount = _context.accounts.Where(m => m.user_id != acc.user_id).ToList();

                return View(data);
            }
            else
            {
                return RedirectToAction("index", "login", new { area = "" });
            }


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
        public ActionResult Edit(TodoViewModel todoViewModel, FormCollection fc, HttpPostedFileBase file)
        {
            int id = Convert.ToInt32(fc["todo_id"]);
            string filePath = string.Empty;
            todoitem todoitem = new todoitem();
            todoitem.title = todoViewModel.tenCV;
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
                String strSQL = "";
                SqlParameter[] Param =null;
                if (file == null)
                {
                    strSQL = "UPDATE todoitem SET  ";
                    strSQL += " title=@title ,end_date=@end_date,partner=@partner,phamvi=@range ";
                    strSQL += "WHERE todo_id = @todo_id ";
                    List<SqlParameter> parameterList = new List<SqlParameter>();
                    parameterList.Add(new SqlParameter("@title", todoitem.title));
                    parameterList.Add(new SqlParameter("@end_date", todoitem.end_date));
                    parameterList.Add(new SqlParameter("@partner", todoitem.partner));
                    parameterList.Add(new SqlParameter("@range", todoitem.phamvi));
                    parameterList.Add(new SqlParameter("@todo_id", id));
                    Param = parameterList.ToArray();
                }
                else
                {
                    strSQL = "UPDATE todoitem SET  ";
                    strSQL += " title=@title ,end_date=@end_date,partner=@partner,file_attach=@file_attach,phamvi=@range ";
                    strSQL += "WHERE todo_id = @todo_id ";
                    List<SqlParameter> parameterList = new List<SqlParameter>();
                    parameterList.Add(new SqlParameter("@title", todoitem.title));
                    parameterList.Add(new SqlParameter("@end_date", todoitem.end_date));
                    parameterList.Add(new SqlParameter("@partner", todoitem.partner));
                    parameterList.Add(new SqlParameter("@range", todoitem.phamvi));
                    parameterList.Add(new SqlParameter("@file_attach", filePath));
                    parameterList.Add(new SqlParameter("@todo_id", id));
                    Param = parameterList.ToArray();
                }
                

                int noOfRowInserted = _context.Database.ExecuteSqlCommand(strSQL, Param);
                return RedirectToAction("ShowTableAdmin");

            }
            catch (Exception e)
            {
                return RedirectToAction("ShowTableAdmin");
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
    }
}