using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model.Employee;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL.Employee
{
    public class ProcGetMeetingDetailReport : IProcedureAsync
    {
        private readonly IDAL _dal;
        public ProcGetMeetingDetailReport(IDAL dal) => _dal = dal;
        public async Task<object> Call(object obj)
        {
            var _req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LoginID", _req.LoginID),
                new SqlParameter("@LT", _req.LoginTypeID),
                new SqlParameter("@AttendanceID", _req.CommonInt)
            };
            var _alist = new List<MeetingAddOnReportModel>();
            try
            {
                var dt = await _dal.GetByProcedureAsync(GetName(), param).ConfigureAwait(false);
                foreach (DataRow dr in dt.Rows)
                {
                    _alist.Add(new MeetingAddOnReportModel
                    {
                        //Statuscode = ErrorCodes.One,
                        //Msg = "Success",
                        //ServiceID = Convert.ToInt32(dr["_ServiceID"]),
                        //OPTypeID = Convert.ToInt32(dr["_OPTypeID"]),
                        Id = Convert.ToInt32(dr["_ID"]),
                        AttendanceId = Convert.ToInt32(dr["_AttandanceID"]),
                        Name = Convert.ToString(dr["_Name"]),
                        OutletName = Convert.ToString(dr["_OutletName"]),
                        Mobile = Convert.ToString(dr["_MobileNo"]),
                        Area = Convert.ToString(dr["_Area"]),
                        Pincode = Convert.ToInt32(dr["_PinCode"]),
                        PurposeId = Convert.ToInt32(dr["_PurpuseId"]),
                        Purpose = Convert.ToString(dr["_Purpuse"]),
                        Consumption = Convert.ToDecimal(dr["_consumption"]),
                        IsUsedOtherBrand = Convert.ToBoolean(dr["_IsUsedOtherBrand"]),
                        OtherBconsumption = Convert.ToDecimal(dr["_OtherBconsumption"]),
                        ReasonId = Convert.ToInt32(dr["_ReasonID"]),
                        Reason = Convert.ToString(dr["_Reason"]),
                        Remark = Convert.ToString(dr["_Remark"]),
                        RechargeConsumption = dr["_RechargeConsumption"] is DBNull ? 0 : Convert.ToDecimal(dr["_RechargeConsumption"]),
                        BillPaymentConsumption = dr["_BillPaymentConsumption"] is DBNull ? 0 : Convert.ToDecimal(dr["_BillPaymentConsumption"]),
                        MoneyTransferConsumption = dr["_MoneyTransferConsumption"] is DBNull ? 0 : Convert.ToDecimal(dr["_MoneyTransferConsumption"]),
                        AEPSConsumption = dr["_AEPSConsumption"] is DBNull ? 0 : Convert.ToDecimal(dr["_AEPSConsumption"]),
                        MiniATMConsumption = dr["_MiniATMConsumption"] is DBNull ? 0 : Convert.ToDecimal(dr["_MiniATMConsumption"]),
                        InsuranceConsumption = dr["_InsuranceConsumption"] is DBNull ? 0 : Convert.ToDecimal(dr["_InsuranceConsumption"]),
                        HotelConsumption = dr["_HotelFlightConsumption"] is DBNull ? 0 : Convert.ToDecimal(dr["_HotelFlightConsumption"]),
                        PanConsumption = dr["_PanCardConsumption"] is DBNull ? 0 : Convert.ToDecimal(dr["_PanCardConsumption"]),
                        VehicleConsumption = dr["_VehicleInsuranceConsumption"] is DBNull ? 0 : Convert.ToDecimal(dr["_VehicleInsuranceConsumption"]),
                        BrandName = Convert.ToString(dr["_BrandName"]),
                    });
                }
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = _req.LoginTypeID,
                    UserId = _req.LoginID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return _alist;
        }

        public Task<object> Call() => throw new NotImplementedException();

        public string GetName() => "proc_GetMeetingDetailReport";
    }
}
