using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.Employee;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.StaticModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL.Employee
{
    public class ProcGetMeetingDetailRpt : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetMeetingDetailRpt(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var _req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LoginID", _req.LoginID),
                new SqlParameter("@LT", _req.LoginTypeID),
                new SqlParameter("@Top", _req.CommonInt),
                new SqlParameter("@DtFrom", _req.CommonStr ?? DateTime.Now.ToString("dd MMM yyyy")),
                new SqlParameter("@DtTill", _req.CommonStr2 ?? DateTime.Now.ToString("dd MMM yyyy")),
                new SqlParameter("@Criteria", _req.CommonInt2),
                new SqlParameter("@CValue", _req.CommonStr3)
            };
            var res = new List<Meetingdetails>();
            try
            {
                var dt = _dal.GetByProcedureAdapter(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        res.Add(new Meetingdetails
                        {
                            Name = Convert.ToString(dr["_Name"]),
                            OutletName = Convert.ToString(dr["_OutletName"]),
                            Area = Convert.ToString(dr["_Area"]),
                            MobileNo = Convert.ToString(dr["_MobileNo"]),
                            Pincode = Convert.ToString(dr["_PinCode"]),
                            PurposeId = Convert.ToInt32(dr["_PurpuseId"]),
                            Purpose = Convert.ToString(dr["_Purpuse"]),
                            Consumption = dr["_consumption"] is DBNull ? 0 : Convert.ToDecimal(dr["_consumption"]),
                            Isusingotherbrands = dr["_IsUsedOtherBrand"] is DBNull ? false : Convert.ToBoolean(dr["_IsUsedOtherBrand"]),
                            Otherbrandconsumption = dr["_OtherBConsumption"] is DBNull ? 0 : Convert.ToDecimal(dr["_OtherBConsumption"]),
                            ReasonId = Convert.ToInt32(dr["_ReasonID"]),
                            Reason = Convert.ToString(dr["_Reason"]),
                            Remark = Convert.ToString(dr["_Remark"]),
                            Latitute = Convert.ToString(dr["_Latitute"]),
                            Longitute = Convert.ToString(dr["_Longitute"]),
                            EmployeeId = Convert.ToInt32(dr["_UserID"]),
                            EmployeeName = Convert.ToString(dr["_EmployeeName"]),
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
                            //ShopImagePath = DOCType.Employee + FolderType.ShopImage + "/" + Convert.ToString(dr["_MobileNo"]) + ".png"
                            ShopImagePath = "/Image/Employee/ShopImage/" + Convert.ToString(dr["_MobileNo"]) + ".png"
                        });
                    }
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
            return res;
        }
        public object Call() => throw new NotImplementedException();
        public string GetName() => "Proc_GetMeetingDetail";
    }
}