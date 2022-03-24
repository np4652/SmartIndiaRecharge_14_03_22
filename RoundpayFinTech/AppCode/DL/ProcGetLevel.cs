using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetLevel : IProcedureAsync
    {
        private readonly IDAL _dal;
		public ProcGetLevel(IDAL dal) => _dal = dal;

        public async Task<object> Call(object obj)
        {
			var req = (int)obj;
			SqlParameter[] param = {				
				new SqlParameter("@UserID",req)
				
			};
			var res = new List<Level>();
			try
			{
				DataTable dt =  await _dal.GetByProcedureAsync(GetName(),param).ConfigureAwait(true);
				if (dt.Rows.Count > 0)
				{
					foreach (DataRow row in dt.Rows)
					{
						res.Add(new Level
						{
							LevelNo = row["Lvl"] is DBNull ? 0 : Convert.ToInt32(row["Lvl"]),
							LevelCount = row["LevelCount"] is DBNull ? 0 : Convert.ToInt32(row["LevelCount"]),
							TotalCountAT = row["TotalCountAT"] is DBNull ? 0 : Convert.ToInt32(row["TotalCountAT"]),
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

		public Task<object> Call() => throw new NotImplementedException();
        public string GetName() => "Proc_GetLevel";        
    }
}
