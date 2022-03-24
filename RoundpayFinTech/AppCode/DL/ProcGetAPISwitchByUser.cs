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
    public class ProcGetAPISwitchByUser : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetAPISwitchByUser(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT", req.LoginTypeID),
                new SqlParameter("@LoginID", req.LoginID),
                new SqlParameter("@UserID", req.CommonInt),
                new SqlParameter("@OpTypeID", req.CommonInt2)
            };
            var _res = new List<APIOpCode>();
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    var aPIOpCode = new APIOpCode
                    {
                        OID = Convert.ToInt32(dt.Rows[i]["OID"]),
                        Operator = dt.Rows[i]["Operator"].ToString(),
                        OpType = dt.Rows[i]["OpType"].ToString(),
                        APIID = Convert.ToInt32(dt.Rows[i]["_APIID"] is DBNull ? 0 : dt.Rows[i]["_APIID"])
                    };
                    var ANameIDs = new List<APIDetail>();
                    if (dt.Rows[i]["APIs"] is DBNull == false)
                    {

                        if (dt.Rows[i]["APIs"].ToString().Contains((char)160))
                        {
                            string[] APINameIDsArr = dt.Rows[i]["APIs"].ToString().Split((char)160);
                            for (int ia = 0; ia < APINameIDsArr.Length; ia++)
                            {
                                if (APINameIDsArr[ia].Contains("_"))
                                {
                                    ANameIDs.Add(new APIDetail
                                    {
                                        ID = Convert.ToInt32(APINameIDsArr[ia].Split('_')[0]),
                                        Name = APINameIDsArr[ia].Split('_')[1]
                                    });
                                }
                            }
                        }
                        else if (dt.Rows[i]["APIs"].ToString().Contains("_"))
                        {
                            ANameIDs.Add(new APIDetail
                            {
                                ID = Convert.ToInt32(dt.Rows[i]["APIs"].ToString().Split('_')[0]),
                                Name = dt.Rows[i]["APIs"].ToString().Split('_')[1]
                            });
                        }
                    }
                    aPIOpCode.APINameIDs = ANameIDs;
                    _res.Add(aPIOpCode);
                }
            }
            catch (Exception)
            {
            }
            return _res;
        }
        public object Call() => throw new NotImplementedException();
        public string GetName() => "proc_GetAPISwitchByUser";
    }
}
