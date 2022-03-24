using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcAadharOTPReference : IProcedure
    {
        private readonly IDAL _dal;
        public ProcAadharOTPReference(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param = { 
                new SqlParameter("@AadharNo",req.CommonStr??string.Empty),
                new SqlParameter("@APIID",req.CommonInt),
                new SqlParameter("@ReferenceNo",req.CommonStr2??string.Empty),
                new SqlParameter("@DirectorName",req.CommonStr3??string.Empty),
                new SqlParameter("@Hash",req.CommonStr4??string.Empty)
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
                    res.Statuscode = ErrorCodes.One;
                    res.Msg = ErrorCodes.SUCCESS;
                    if (res.Statuscode == ErrorCodes.One)
                    {
                        res.CommonInt = dt.Rows[0]["_ReferenceID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_ReferenceID"]);
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
                    UserId = req.CommonInt
                });
            }
            return res;
        }

        public object Call()
        {
            throw new NotImplementedException();
        }

        public ResponseStatus GetAadharRefrence(int ReferenceID) {
            var res = new ResponseStatus();
            try
            {
                StringBuilder query = new StringBuilder("select _RefrenceID,_AadharNo,_DirectorName,_Hash from tbl_AadharOTPReference where _ID=@ID");
                query.Replace("@ID", ReferenceID.ToString());
                var dt = _dal.Get(query.ToString());
                if (dt.Rows.Count > 0) {
                    res.CommonStr = dt.Rows[0]["_RefrenceID"] is DBNull ? string.Empty : dt.Rows[0]["_RefrenceID"].ToString();
                    res.CommonStr2 = dt.Rows[0]["_AadharNo"] is DBNull ? string.Empty : dt.Rows[0]["_AadharNo"].ToString();
                    res.CommonStr3 = dt.Rows[0]["_DirectorName"] is DBNull ? string.Empty : dt.Rows[0]["_DirectorName"].ToString();
                    res.CommonStr4 = dt.Rows[0]["_Hash"] is DBNull ? string.Empty : dt.Rows[0]["_Hash"].ToString();
                }
            }
            catch (Exception)
            {
            }
            return res;
        }
        public string GetName() => "proc_AadharOTPReference";
    }
}
