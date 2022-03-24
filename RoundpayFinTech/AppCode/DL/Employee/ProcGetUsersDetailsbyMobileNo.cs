using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.Employee;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL.Employee
{
    public class ProcGetUsersDetailsbyMobileNo : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetUsersDetailsbyMobileNo(IDAL dal) => _dal = dal;

        public object Call(object obj)
        {
            var _req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LoginID", _req.LoginID),
                new SqlParameter("@LT", _req.LoginTypeID),
                new SqlParameter("@MobileNo", _req.CommonStr),
            };
            var res = new Meetingdetails();
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    res.StatusCode = Convert.ToInt32(dt.Rows[0]["_StatusCode"]);
                    if (res.StatusCode == 1)
                    {
                        res.Name = Convert.ToString(dt.Rows[0]["_Name"]);
                        res.OutletName = Convert.ToString(dt.Rows[0]["_Outletname"]);
                        res.Area = Convert.ToString(dt.Rows[0]["_Address"]);
                        res.Pincode = Convert.ToString(dt.Rows[0]["_PinCode"]);
                        res.Pincode = Convert.ToString(dt.Rows[0]["_PinCode"]);
                    }
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
            return res;
        }
        public object Call() => throw new NotImplementedException();
        public string GetName() => "proc_GetUsersDetailsbyMobileNo";
    }
}

