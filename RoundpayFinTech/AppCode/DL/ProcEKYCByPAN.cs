using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcEKYCByPAN : IProcedure
    {
        private readonly IDAL _dal;
        public ProcEKYCByPAN(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (EKYCByPANModelProcReq)obj;
            SqlParameter[] param = { 
                new SqlParameter("@UserID",req.UserID),
                new SqlParameter("@PANNo",req.PANNumber??string.Empty),
                new SqlParameter("@FullName",req.FullName??string.Empty),
                new SqlParameter("@FirstName",req.FirstName??string.Empty),
                new SqlParameter("@LastName",req.LastName??string.Empty),
                new SqlParameter("@Title",req.Title??string.Empty),
                new SqlParameter("@IsAadharAttached",req.IsAadharSeeded),
                new SqlParameter("@DirectorName",req.DirectorName??string.Empty),
                new SqlParameter("@APIID",req.APIID),
                new SqlParameter("@IsExternal",req.IsExternal),
                new SqlParameter("@ChildUserID",req.ChildUserID),
                new SqlParameter("@APIStatus",req.APIStatus)
            };
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    res.Statuscode = dt.Rows[0][0] is DBNull ? res.Statuscode : Convert.ToInt16(dt.Rows[0][0]);
                    res.Msg = dt.Rows[0]["Msg"] is DBNull ? string.Empty : dt.Rows[0]["Msg"].ToString();
                    if (res.Statuscode == ErrorCodes.One)
                    {
                        res.CommonInt = dt.Rows[0]["_EKYCID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_EKYCID"]);
                    }
                }
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = 1,
                    UserId = req.UserID
                });
            }
            return res;
        }

        public object Call()
        {
            throw new NotImplementedException();
        }

        public string GetName() => "proc_EKYCByPAN";
    }
}
