using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Dynamic;
using System.Linq;
using System.Web;
using test1.Models;

namespace test1.ViewModel
{
    public class CommentViewModel
    {
       public string userName { get; set; }
       public string content { get; set; }
       /* public CommentViewModel(string userName, string content)
        {
            this.userName = userName;
            this.content = content;
        }*/
      
    }
}