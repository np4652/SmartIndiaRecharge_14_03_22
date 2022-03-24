using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetPSAForMachine : IProcedureAsync
    {
        private readonly IDAL _dal;
        public ProcGetPSAForMachine(IDAL dal) => _dal = dal;
        public async Task<object> Call(object obj)
        {
            SqlParameter[] param = {
                new SqlParameter("@MachineName",((string)obj)??string.Empty)
            };
            var res = new List<PSADetailForMachine>();
            try
            {
                DataTable dt = await _dal.GetByProcedureAsync(GetName(), param).ConfigureAwait(false);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow item in dt.Rows)
                    {
                        res.Add(new PSADetailForMachine
                        {
                            OutletAPIID=item["_ID"] is DBNull?0:Convert.ToInt32(item["_ID"]),
                            ContactPerson = item["_Name"] is DBNull ? string.Empty : item["_Name"].ToString(),
                            OutletName = item["_Company"] is DBNull ? string.Empty : item["_Company"].ToString(),
                            MobileNO = item["_MobileNo"] is DBNull ? string.Empty : item["_MobileNo"].ToString(),
                            EmailID = item["_EmailID"] is DBNull ? string.Empty : item["_EmailID"].ToString(),
                            Address = item["_Address"] is DBNull ? string.Empty : item["_Address"].ToString(),
                            Pincode = item["_Pincode"] is DBNull ? string.Empty : item["_Pincode"].ToString(),
                            State = item["_State"] is DBNull ? string.Empty : item["_State"].ToString(),
                            PSAID = item["_PANID"] is DBNull ? string.Empty : item["_PANID"].ToString()
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

        public Task<object> Call()
        {
            throw new NotImplementedException();
        }

        public string GetName() => "proc_GetPSAForMachine";
    }
}
