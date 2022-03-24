using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetBookingStatus : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetBookingStatus(IDAL dal) => _dal = dal;
        public string GetName() => "proc_GetDTHBookingStatus";
        public object Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@ID",req.CommonInt),
            };
            var res = new DTHSubscriptionReport();
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {

                    res.ID = dt.Rows[0]["_ID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_ID"]);
                    res.BookingStatus = dt.Rows[0]["_BookingStatus"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_BookingStatus"]);
                    res.TechnicianName = dt.Rows[0]["_TechnicianName"] is DBNull ? "" : dt.Rows[0]["_TechnicianName"].ToString();
                    res.TechnicianMobile = dt.Rows[0]["_TechnicianMobile"] is DBNull ? "" : dt.Rows[0]["_TechnicianMobile"].ToString();
                    res.STBID = dt.Rows[0]["_STBID"] is DBNull ? "" : dt.Rows[0]["_STBID"].ToString();
                    res.CustomerID = dt.Rows[0]["_CustomerID"] is DBNull ? "" : dt.Rows[0]["_CustomerID"].ToString();
                    res.VCNO = dt.Rows[0]["_VCNO"] is DBNull ? "" : dt.Rows[0]["_VCNO"].ToString();
                    res.InstallationTime = dt.Rows[0]["_InstallTime"] is DBNull ? "" : dt.Rows[0]["_InstallTime"].ToString();
                    res.InstalltionCharges = dt.Rows[0]["_InstallCharges"] is DBNull ? "" : dt.Rows[0]["_InstallCharges"].ToString();
                    res.Remark = dt.Rows[0]["_Remark"] is DBNull ? "" : dt.Rows[0]["_Remark"].ToString();
                    res.ApprovalTime = dt.Rows[0]["_Approvaltime"] is DBNull ? "" : dt.Rows[0]["_Approvaltime"].ToString();
                    res.Area = dt.Rows[0]["_Area"] is DBNull ? "" : dt.Rows[0]["_Area"].ToString();
                    res.City = dt.Rows[0]["_City"] is DBNull ? "" : dt.Rows[0]["_City"].ToString();
                    res.State = dt.Rows[0]["_State"] is DBNull ? "" : dt.Rows[0]["_State"].ToString();
                    res.Pincode = dt.Rows[0]["_Pincode"] is DBNull ? "" : dt.Rows[0]["_Pincode"].ToString();
                    res.Gender = dt.Rows[0]["_Gender"] is DBNull ? "" : dt.Rows[0]["_Gender"].ToString();
                    res.Address = dt.Rows[0]["_Address"] is DBNull ? "" : dt.Rows[0]["_Address"].ToString();
                }
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = 1,
                    UserId = 1
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
                return res;
           
        }

        public object Call() => throw new NotImplementedException();
    }
}
