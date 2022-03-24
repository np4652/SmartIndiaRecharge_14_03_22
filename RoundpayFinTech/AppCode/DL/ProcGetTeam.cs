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
    public class ProcGetTeam : IProcedureAsync
    {
        private readonly IDAL _dal;
        public ProcGetTeam(IDAL dal)
        {
            _dal = dal;
        }

        public async Task<object> Call(object obj)
        {
			var req = (int)obj;
			SqlParameter[] param = {				
				new SqlParameter("@UserID",req)
				
			};
			var res = new List<Team>();
			try
			{
				DataTable dt =  await _dal.GetAsync(GetName(),param).ConfigureAwait(true);
				if (dt.Rows.Count > 0)
				{
					foreach (DataRow row in dt.Rows)
					{
						res.Add(new Team
						{
							Name = row["_Name"] is DBNull ? "" : row["_Name"].ToString(),
							MobileNo = row["_MobileNo"] is DBNull ? "" : row["_MobileNo"].ToString(),
							OutletName = row["_OutletName"] is DBNull ? "" : row["_OutletName"].ToString(),
						});
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
					UserId = 1
				};
				var _ = new ProcPageErrorLog(_dal).Call(errorLog);
			}
			return res;
		}

        public Task<object> Call()
        {
            throw new NotImplementedException();
        }

        public string GetName() => @"select * from tbl_Users where _IntroducedBy=@UserID";        
    }
}
