using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcUpdateSender : IProcedure
    {
        private readonly IDAL _dal;
        public ProcUpdateSender(IDAL dal) => _dal = dal;

        public object Call(object obj)
        {
            var req = (SenderRequest)obj;
            var res = new SenderInfo
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            
            SqlParameter[] param = {
                new SqlParameter("@Name", req.Name??string.Empty),
                new SqlParameter("@MobileNo",req.MobileNo??string.Empty),
                new SqlParameter("@Pincode", req.Pincode??string.Empty),
                new SqlParameter("@Address", req.Address??string.Empty),
                new SqlParameter("@City", req.City??string.Empty),
                new SqlParameter("@StateID", req.StateID),
                new SqlParameter("@AadhaarNo", req.AadharNo??string.Empty),
                new SqlParameter("@Dob", req.Dob??string.Empty),
                new SqlParameter("@UserID",req.UserID),
                new SqlParameter("@SelfRefID",req.SelfRefID),
                new SqlParameter("@ReffID", req.ReffID??string.Empty),
                new SqlParameter("@RequestNo", req.RequestNo??string.Empty),
                new SqlParameter("@OTP", req.OTP??string.Empty)
            };
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    res.Statuscode = Convert.ToInt16(dt.Rows[0][0]);
                    res.Msg = dt.Rows[0]["Msg"] is DBNull ? "" : dt.Rows[0]["Msg"].ToString();
                    if (res.Statuscode==ErrorCodes.One)
                    {
                        res.MobileNo = dt.Rows[0]["_MobileNo"] is DBNull ? string.Empty : dt.Rows[0]["_MobileNo"].ToString();
                        res.Name = dt.Rows[0]["_Name"] is DBNull ? string.Empty : dt.Rows[0]["_Name"].ToString();
                        res.Address = dt.Rows[0]["_Address"] is DBNull ? string.Empty : dt.Rows[0]["_Address"].ToString();
                        res.City = dt.Rows[0]["_City"] is DBNull ? string.Empty : dt.Rows[0]["_City"].ToString();
                        res.Area = dt.Rows[0]["_Area"] is DBNull ? string.Empty : dt.Rows[0]["_Area"].ToString();
                        res.Districtname = dt.Rows[0]["_Districtname"] is DBNull ? string.Empty : dt.Rows[0]["_Districtname"].ToString();
                        res.Pincode = dt.Rows[0]["_Pincode"] is DBNull ? string.Empty : dt.Rows[0]["_Pincode"].ToString();
                        res.Statename = dt.Rows[0]["_Statename"] is DBNull ? string.Empty : dt.Rows[0]["_Statename"].ToString();
                        res.UserID = Convert.ToInt32(dt.Rows[0]["_UserID"]);
                        res.ReffID = dt.Rows[0]["_ReffID"] is DBNull ? string.Empty : dt.Rows[0]["_ReffID"].ToString();
                        res.RequestNo = dt.Rows[0]["_RequestNo"] is DBNull ? string.Empty : dt.Rows[0]["_RequestNo"].ToString();
                        res.WID = Convert.ToInt32(dt.Rows[0]["_WID"]);
                        res.SelfRefID = Convert.ToInt32(dt.Rows[0]["_SelfRefID"]);
                        if (dt.Columns.Contains("_Dob")) {
                            res.Dob = dt.Rows[0]["_Dob"] is DBNull ? string.Empty : dt.Rows[0]["_Dob"].ToString();
                        }
                        res.OTP = dt.Rows[0]["_OTP"] is DBNull ? string.Empty : dt.Rows[0]["_OTP"].ToString();
                        res.MahagramStateCode = dt.Rows[0]["_MahagramStateCode"] is DBNull ? string.Empty : dt.Rows[0]["_MahagramStateCode"].ToString();
                        res.VID = dt.Rows[0]["_VID"] is DBNull ? string.Empty : dt.Rows[0]["_VID"].ToString();
                        res.UIDToken = dt.Rows[0]["_UIDToken"] is DBNull ? string.Empty : dt.Rows[0]["_UIDToken"].ToString();
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
                    LoginTypeID = 1,
                    UserId = req. UserID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return res;
        }
        public object Call() => throw new NotImplementedException();
        public string GetName() => "proc_UpdateSender";
    }
}
