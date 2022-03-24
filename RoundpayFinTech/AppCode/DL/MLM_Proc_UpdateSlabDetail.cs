using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Fintech.AppCode.Model;

namespace RoundpayFinTech.AppCode.DL
{
    public class MLM_Proc_UpdateSlabDetail : IProcedure
    {
        private readonly IDAL _dal;
        public MLM_Proc_UpdateSlabDetail(IDAL dal) => _dal = dal;

        public object Call(object obj)
        {
            MLMSlabRequest req = (MLMSlabRequest)obj;
            var res = new AlertReplacementModel
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            SqlParameter[] param =
            {
                new SqlParameter("@LT",req.LoginTypeID),
                new SqlParameter("@LoginID",req.LoginID),
                new SqlParameter("@SlabID",req.Commission.SlabID),
                new SqlParameter("@OID",req.Commission.OID),
                new SqlParameter("@Comm",req.Commission.Comm),
                new SqlParameter("@CommType",req.Commission.CommType),
                new SqlParameter("@AmtType",req.Commission.AmtType),                
                new SqlParameter("@IP",req.CommonStr??""),
                new SqlParameter("@Browser",req.CommonStr2??""),
                new SqlParameter("@SlabLevelID",req.Commission.LevelID),
            };

            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);

                if (dt.Rows.Count > 0)
                {
                    res.Statuscode = Convert.ToInt16(dt.Rows[0][0]);
                    res.Msg = dt.Rows[0]["Msg"] is DBNull ? "" : dt.Rows[0]["Msg"].ToString();
                }
            }
            catch (Exception ex)
            {
                ErrorLog errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = req.LoginTypeID,
                    UserId = req.LoginID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return res;
        }

        public object Call() => throw new NotImplementedException();
        public string GetName() => "MLM_Proc_UpdateSlabDetail";
    }
}
