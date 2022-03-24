using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetCircleswitchingMulti : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetCircleswitchingMulti(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (CommonReq)obj;

            SqlParameter[] param = {
                new SqlParameter("@CircleID",req.CommonInt),
                new SqlParameter("@LoginID",req.LoginID)
            };
            var res = new List<CircleAPISwitchDetail>();
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow item in dt.Rows)
                    {
                        res.Add(new CircleAPISwitchDetail
                        {
                            APIID=item["_APIID"] is DBNull?0:Convert.ToInt32(item["_APIID"]),
                            OID=item["_OID"] is DBNull?0:Convert.ToInt32(item["_OID"]),
                            APIName=item["_Name"] is DBNull?string.Empty:Convert.ToString(item["_Name"]),
                            Operator = item["_Operator"] is DBNull?string.Empty:Convert.ToString(item["_Operator"]),
                            IsSwitched = item["_IsSwitched"] is DBNull?false:Convert.ToBoolean(item["_IsSwitched"]),
                            MaxCount = item["_MaxCount"] is DBNull?0:Convert.ToInt32(item["_MaxCount"])
                        });
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
                    UserId = 1
                });
            }
            return res;
        }

        public object Call()
        {
            throw new NotImplementedException();
        }

        public string GetName() => "proc_GetCircleswitchingMulti";
    }
}
