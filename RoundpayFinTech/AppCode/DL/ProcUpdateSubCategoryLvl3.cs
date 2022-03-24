using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcUpdateSubCategoryLvl3 : IProcedure
	{
		private readonly IDAL _dal;

		public ProcUpdateSubCategoryLvl3(IDAL dal) => _dal = dal;

		public object Call(object obj)
		{
			var res = new ResponseStatus
			{
				Statuscode = ErrorCodes.Minus1,
				Msg = ErrorCodes.TempError
			};
			CommonReq req = (CommonReq)obj;
			SqlParameter[] param = {
				new SqlParameter("@LT", req.LoginTypeID),
				new SqlParameter("@LoginID", req.LoginID),
				new SqlParameter("@ID", req.CommonInt),
				new SqlParameter("@SubCategoryID", req.CommonInt2),
				new SqlParameter("@Subcategory", req.CommonStr),
				new SqlParameter("@IsActive", req.CommonBool),
				new SqlParameter("@GroupID", req.CommonInt3),
			};
			try
			{
				DataTable dt = _dal.GetByProcedure(GetName(), param);
				if (dt.Rows.Count > 0)
				{
					res.Statuscode = Convert.ToInt32(dt.Rows[0][0], CultureInfo.InvariantCulture);
					res.Msg = dt.Rows[0]["Msg"] is DBNull ? "" : dt.Rows[0]["Msg"].ToString();
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
		public string GetName() => "Proc_UpdateSubCategoryLvl3";
	}
}
