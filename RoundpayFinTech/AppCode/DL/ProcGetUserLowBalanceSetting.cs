using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetUserLowBalanceSetting : IProcedure
	{
		private readonly IDAL _dal;
		public ProcGetUserLowBalanceSetting(IDAL dal) => _dal = dal;
		
		public object Call(object obj)
		{
			int UserID = (int)obj;
			SqlParameter[] param = new SqlParameter[1];
			param[0] = new SqlParameter("@UserID", UserID);
			var setting = new LowBalanceSetting();
			try
			{
				var dt = _dal.GetByProcedure(GetName(), param);
				if (dt.Rows.Count > 0)
				{
					setting.AlertBalance = dt.Rows[0]["_AlertBalance"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_AlertBalance"]);
					setting.MobileNos = Convert.ToString(dt.Rows[0]["_MobileNos"]);
					setting.Emails = Convert.ToString(dt.Rows[0]["_Emails"]);
					setting.MobileNo = Convert.ToString(dt.Rows[0]["_MobileNo"]);
					setting.EmailID = Convert.ToString(dt.Rows[0]["_EmailID"]);
					setting.WhatsappNo = Convert.ToString(dt.Rows[0]["_WhatsappNo"]);
					setting.TelegramNo = Convert.ToString(dt.Rows[0]["_TelegramNo"]);
					setting.HangoutId = Convert.ToString(dt.Rows[0]["_HangoutId"]);
				}
			}
			catch (Exception ex)
			{
				var errorLog = new ErrorLog
				{
					ClassName = GetType().Name,
					FuncName = "Call",
					Error = ex.Message,
					LoginTypeID = LoginType.ApplicationUser,
					UserId = UserID
				};
				var _ = new ProcPageErrorLog(_dal).Call(errorLog);
			}
			return setting;
		}

		public object Call() => throw new NotImplementedException();

		public string GetName() => "proc_GetUserLowBalanceSetting";
		
	}
}