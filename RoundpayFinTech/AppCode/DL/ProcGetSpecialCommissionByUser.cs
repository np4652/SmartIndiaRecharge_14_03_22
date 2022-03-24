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
    public class ProcGetSpecialCommissionByUser : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetSpecialCommissionByUser(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LoginID",req.LoginID),
                new SqlParameter("@OID",req.CommonInt)
            };
            var res = new List<SlabSpecialCircleWise>();
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow item in dt.Rows)
                    {
                        res.Add(new SlabSpecialCircleWise { 
                            Circle= item["_Circle"] is DBNull ? string.Empty : item["_Circle"].ToString(),
                            Denomination= item["_Amount"] is DBNull ? 0 : Convert.ToInt32(item["_Amount"]),
                            DenomMin= item["_Min"] is DBNull ? 0 : Convert.ToInt32(item["_Min"]),
                            DenomMax= item["_Max"] is DBNull ? 0 : Convert.ToInt32(item["_Max"]),
                            CommAmount = item["_Comm"] is DBNull ? 0M : Convert.ToDecimal(item["_Comm"]),
                            CommType = item["_CommType"] is DBNull ? false : Convert.ToBoolean(item["_CommType"]),
                            AmtType = item["_AmtType"] is DBNull ? false : Convert.ToBoolean(item["_AmtType"]),
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetName(),
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = req.LoginTypeID,
                    UserId = req.LoginID
                });
            }
            return res;
        }

        public object Call()
        {
            throw new NotImplementedException();
        }

        public string GetName() => "proc_GetSpecialCommissionByUser";
    }
}
