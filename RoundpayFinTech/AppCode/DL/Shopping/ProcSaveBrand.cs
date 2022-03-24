using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.Model.Shopping;
using System;
using System.Data;
using System.Data.SqlClient;



namespace RoundpayFinTech.AppCode.DL
{
    public class ProcSaveBrand : IProcedure
	{
		private readonly IDAL _dal;

		public ProcSaveBrand(IDAL dal) => _dal = dal;

		public object Call(object obj)
		{
			var res = new ResponseStatus
			{
				Statuscode = ErrorCodes.Minus1,
				Msg = ErrorCodes.TempError
			};
			var req = (Brand)obj;
			SqlParameter[] param = {
				new SqlParameter("@LoginID", req.LoginID),
				new SqlParameter("@CategoryID", req.CategoryID),
				new SqlParameter("@BrandId", req.BrandId),
				new SqlParameter("@BrandName", req.BrandName),
				new SqlParameter("@IsActive", req.IsActive)
			};
			try
			{
				DataTable dt = _dal.GetByProcedure(GetName(), param);
				if (dt.Rows.Count > 0)
				{
					res.Statuscode = Convert.ToInt32(dt.Rows[0][0]);
					res.Msg = Convert.ToString(dt.Rows[0]["Msg"]);
				}
			}
			catch (Exception ex)
			{
				var errorLog = new ErrorLog
				{
					ClassName = GetType().Name,
					FuncName = "Call",
					Error = ex.Message,
					LoginTypeID = req.LT,
					UserId = req.LoginID
				};
				var _ = new ProcPageErrorLog(_dal).Call(errorLog);
			}
			return res;
		}

		public object Call() => throw new NotImplementedException();
		public string GetName() => "Proc_SaveBrand";
	}
}
