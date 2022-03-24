

using Microsoft.AspNetCore.Http;

namespace RoundpayFinTech.AppCode.Model.Employee
{
    public class Meetingdetails
    {
        public int LoginID { get; set; }
        public int StatusCode { get; set; }
        public int LoginTypeID { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
        public string OutletName { get; set; }
        public string Area { get; set; }
        public string Pincode { get; set; }
        public int PurposeId { get; set; }
        public string Purpose { get; set; }
        public decimal Consumption { get; set; }
        public bool Isusingotherbrands { get; set; }
        public decimal Otherbrandconsumption { get; set; }
        public int ReasonId { get; set; }
        public string Reason { get; set; }
        public string Remark { get; set; }
        public int AttandanceId { get; set; }
        public string MobileNo { get; set; }
        public string Latitute { get; set; }
        public string Longitute { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; }
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
        public IFormFile ShopImage { get; set; }
        public string ShopImagePath { get; set; }
    }

    public class DailyClosingModel
    {
        public int LoginID { get; set; }
        public int StatusCode { get; set; }
        public int LoginTypeID { get; set; }
        public decimal Travel { get; set; }
        public decimal Expense { get; set; }
    }
}
