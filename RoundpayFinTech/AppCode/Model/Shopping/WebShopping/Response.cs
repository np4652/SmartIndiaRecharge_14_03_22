using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Model.Shopping.WebShopping
{
    public class Response
    {
        public int Id { get; set; }
        public bool Status { get; set; }
        public string Message { get; set; }
        public object Data { get; set; }

    }
    public class Response<T>
    {
        public int Id { get; set; }
        public bool Status { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }

    }
}
