using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetRailwayKycPendings : IProcedureAsync
    {
        private readonly IDAL _dal;
        public ProcGetRailwayKycPendings(IDAL dal) => _dal = dal;
        public async Task<object> Call(object obj)
        {
            UserRequest _req = (UserRequest)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT", _req.LTID),
                new SqlParameter("@LoginID", _req.LoginID)
            };
            var resp = new List<GetEditUser>();
            try
            {
                var dt = await _dal.GetByProcedureAsync(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        var userReport = new GetEditUser
                        {
                            OutletID = row["OutletID"] is DBNull ? 0 : Convert.ToInt32(row["OutletID"]),
                            PartnerName = Convert.ToString(row["PartnerName"]),
                            Name = Convert.ToString(row["_Name"]),
                            OutletName = Convert.ToString(row["_Company"]),
                            DOB = Convert.ToString(row["_DOB"]),
                            PAN = Convert.ToString(row["_PAN"]),
                            AADHAR = Convert.ToString(row["_AADHAR"]),
                            DisplayName = row["_OperatorName"] is DBNull ? row["_OpTypename"].ToString() : row["_OperatorName"].ToString(),
                            DisplayID = row["_IRCTCID"] is DBNull ? "" : row["_IRCTCID"].ToString()
                        };
                        resp.Add(userReport);
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
                    LoginTypeID = _req.LTID,
                    UserId = _req.LoginID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return resp;
        }

        public Task<object> Call() => throw new NotImplementedException();
        public string GetName() => "proc_GetRailwayKycPendings";
    }
}
