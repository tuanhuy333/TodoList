//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace test1.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class todoitem
    {
        public int todo_id { get; set; }
        public int user_id { get; set; }
        public string title { get; set; }
        public System.DateTime start_date { get; set; }
        public System.DateTime end_date { get; set; }
        public int status { get; set; }
        public string partner { get; set; }
        public string file_attach { get; set; }
        public string phamvi { get; set; }
    }
}
