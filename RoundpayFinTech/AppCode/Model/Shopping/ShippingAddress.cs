using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Model.Shopping
{
    public class SAddress
    {
        public int LoginID { get; set; }
        public int ID { get; set; }
        public int UserId { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public int CityID { get; set; }
        public int StateID { get; set; }
        public string State { get; set; }
        public int PIN { get; set; }
        public string Landmark { get; set; }
        public string MobileNo { get; set; }
        public string CustomerName { get; set; }
        public string Title { get; set; }
        public string Area { get; set; }
        public bool IsDefault { get; set; }
        public bool IsDeleted { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
    }
}
