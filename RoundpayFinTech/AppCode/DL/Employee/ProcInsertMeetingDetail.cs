using System;
using System.Data;
using System.Data.SqlClient;
using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.Employee;
using RoundpayFinTech.AppCode.Model.ProcModel;

namespace RoundpayFinTech.AppCode.DL.Employee
{
    public class ProcInsertMeetingDetail : IProcedure
    {
        private readonly IDAL _dal;
        public ProcInsertMeetingDetail(IDAL dal) => _dal = dal;

        public object Call(object obj)
        {
            Meetingdetails _req = (Meetingdetails)obj;
            SqlParameter[] param = {
                new SqlParameter("@LoginID", _req.LoginID),
                new SqlParameter("@LT", _req.LoginTypeID),
                new SqlParameter("@Name", _req.Name),
                new SqlParameter("@OutletName", _req.OutletName),
                new SqlParameter("@Area", _req.Area),
                new SqlParameter("@PinCode", _req.Pincode),
                new SqlParameter("@PurposeId", _req.PurposeId),
                new SqlParameter("@Purpose", _req.Purpose),
                new SqlParameter("@Consumption", _req.Consumption),
                new SqlParameter("@Isusingotherbrands", _req.Isusingotherbrands),
                new SqlParameter("@Otherbrandcosumtion", _req.Otherbrandconsumption),
                new SqlParameter("@ReasonId", _req.ReasonId),
                new SqlParameter("@Reason", _req.Reason??""),
                new SqlParameter("@Remark", _req.Remark??""),
                new SqlParameter("@mobileno", _req.MobileNo),
                new SqlParameter("@latitute", _req.Latitute),
                new SqlParameter("@longitute", _req.Longitute),
                new SqlParameter("@RechargeConsumption", _req.RechargeConsumption),
                new SqlParameter("@BillPaymentConsumption", _req.BillPaymentConsumption),
                new SqlParameter("@MoneyTransferConsumption", _req.MoneyTransferConsumption),
                new SqlParameter("@AEPSConsumption", _req.AEPSConsumption),
                new SqlParameter("@MiniATMConsumption", _req.MiniATMConsumption),
                new SqlParameter("@InsuranceConsumption", _req.InsuranceConsumption),
                new SqlParameter("@HotelConsumption", _req.HotelConsumption),
                new SqlParameter("@PanConsumption", _req.PanConsumption),
                new SqlParameter("@VehicleConsumption", _req.VehicleConsumption),
                new SqlParameter("@BrandName", _req.BrandName)
            };
            ResponseStatus _resp = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    _resp.Statuscode = Convert.ToInt32(dt.Rows[0][0]);
                    _resp.Msg = dt.Rows[0]["Msg"].ToString();
                }
            }
            catch (Exception ex)
            {
                ErrorLog errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = 3,
                    UserId = 1
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return _resp;
        }
        public object Call() => throw new NotImplementedException();
        public string GetName() => "proc_InsertMeetingDetail";
    }
}
