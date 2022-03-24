using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Model.ProcModel
{
    public class HotelDestination
    {
        public int ID { get; set; }
        public string CityName { get; set; }
        public string CountryCode { get; set; }
        public string CountryName { get; set; }
        public int DestinationID { get; set; }
        public string StateProvince { get; set; }
        public Int16 DestType { get; set; }
        public DateTime EntryDate { get; set; }
        public DateTime ModifyDate { get; set; }
        public bool IsTop { get; set; }
    }
}
