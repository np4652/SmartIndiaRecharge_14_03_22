using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcDenomCommissionDetail : IProcedure
	{
		private readonly IDAL _dal;

		public ProcDenomCommissionDetail(IDAL dal) => _dal = dal;

		public object Call(object obj)
		{
			var res = new List<DenomCommissionDetail>();
			var req = (CommonReq)obj;
			SqlParameter[] param = {
				new SqlParameter("@LT", req.LoginTypeID),
				new SqlParameter("@LoginID", req.LoginID),
				new SqlParameter("@OID", req.CommonInt),
			};
			try
			{
				DataTable dt = _dal.GetByProcedure(GetName(), param);
				if (dt.Rows.Count > 0)
				{
					foreach (DataRow dr in dt.Rows)
					{
						var data = new DenomCommissionDetail
						{
							DenomID = Convert.ToInt32(dr["_ID"]),
							Amount = Convert.ToString(dr["_Amount"]),
							Commission = Convert.ToDecimal(dr["_Comm"]),
							AmtType = Convert.ToInt32(dr["_AmtType"]),
						};
						res.Add(data);
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
					LoginTypeID = req.LoginTypeID,
					UserId = req.LoginID
				};
				var _ = new ProcPageErrorLog(_dal).Call(errorLog);
			}
			return res;
		}

		public object Call() => throw new NotImplementedException();
		public string GetName() => "Proc_denomCommissionDetail";
	}
}

