using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
	public class ProcUpdateNews : IProcedure
	{
		private readonly IDAL _dal;

		public ProcUpdateNews(IDAL dal)
		{
			_dal = dal;
		}
		public object Call(object obj)
		{
			var res = new ResponseStatus
			{
				Statuscode = ErrorCodes.Minus1,
				Msg = ErrorCodes.TempError
			};
			CommonReq _req = (CommonReq)obj;
			UserNews model = new UserNews();
			SqlParameter[] param = {
				new SqlParameter("@LT", _req.LoginTypeID),
				new SqlParameter("@ID", _req.CommonInt),
				new SqlParameter("@NewsDetail", _req.str),
				new SqlParameter("@Title", _req.CommonStr),
				new SqlParameter("@RoleId", _req.CommonStr2),
				new SqlParameter("@WID", _req.CommonInt2),
			};

			try
			{
				DataTable dt = _dal.GetByProcedure(GetName(), param);
				if (dt.Rows.Count > 0 && Convert.ToInt32(dt.Rows[0][0]) > 0)
				{
					res.Statuscode = Convert.ToInt32(dt.Rows[0][0].ToString());
					res.Msg = dt.Rows[0]["Msg"] is DBNull ? "" : dt.Rows[0]["Msg"].ToString();

				}
				return (IResponseStatus)(res);
			}
			catch (Exception er)
			{ }
			return (IResponseStatus)(res);
		}

		public object Call()
		{
			throw new NotImplementedException();
		}

		public string GetName()
		{
			return "proc_UpdateNews";
		}
	}
}
