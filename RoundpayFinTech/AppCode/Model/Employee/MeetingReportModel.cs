using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Model.Employee
{
    public class MeetingReportModel
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public decimal TotalTravel { get; set; }
        public decimal TotalExpense { get; set; }
        public int MeetingCount { get; set; }
        public bool IsClosed { get; set; }
        public DateTime EntryDate { get; set; }
    }

    public class MeetingAddOnReportModel
    {
        public int Id { get; set; }
        public int AttendanceId { get; set; }
        public string Name { get; set; }
        public string OutletName { get; set; }
        public string Mobile { get; set; }
        public string Area { get; set; }
        public int Pincode { get; set; }
        public int PurposeId { get; set; }
        public string Purpose { get; set; }
        public decimal Consumption { get; set; }
        public bool IsUsedOtherBrand { get; set; }
        public decimal OtherBconsumption { get; set; }
        public int ReasonId { get; set; }
        public string Reason { get; set; }
        public string Remark { get; set; }
        public decimal RechargeConsumption { get; set; }
        public decimal BillPaymentConsumption { get; set; }
        public decimal MoneyTransferConsumption { get; set; }
        public decimal AEPSConsumption { get; set; }
        public decimal MiniATMConsumption { get; set; }
        public decimal InsuranceConsumption { get; set; }
        public decimal HotelConsumption { get; set; }
        public decimal PanConsumption { get; set; }
        public decimal VehicleConsumption { get; set; }
        public string BrandName { get; set; }
    }

    public class MapPointsModel
    {
        public string Description { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
    }
}
