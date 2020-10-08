using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using test1.Models;

namespace test1.ViewModel
{
    public class TodoViewModel
    {

        public string tenCV { get; set; }

        public string ngayBD { get; set; }

        public string ngayKT { get; set; }

        public string trangthai { get; set; }
        public string nguoilamchung { get; set; }
        public string binhluan { get; set; }
        public List<account> getAllAccount { get; set; }
        public int todo_id { get; set; }
        public string username { get; set; }
        public int userid { get; set; }
        public HttpPostedFileBase file { get; set; }
        public string filepath { get; set; }
        public List<CommentViewModel> listComment { get; set; }
        public string range { get; set; }


    }
}