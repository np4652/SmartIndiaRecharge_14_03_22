using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
	public class ProcAddNews : IProcedure
	{
		private readonly IDAL _dal;

		public ProcAddNews(IDAL dal)
		{
			_dal = dal;
		}
		public object Call(object obj)
		{
			IResponseStatus res = new ResponseStatus
			{
				Statuscode = ErrorCodes.Minus1,
				Msg = ErrorCodes.TempError
			};
			CommonReq _req = (CommonReq)obj;						
			SqlParameter[] param = {
				new SqlParameter("@LT", _req.LoginTypeID),
				new SqlParameter("@RoleId", _req.CommonStr2),
				new SqlParameter("@NewsDetail", _req.str),
				new SqlParameter("@Title", _req.CommonStr),
				new SqlParameter("@WID", _req.CommonInt),
			};

			try
			{
				DataTable dt = _dal.GetByProcedure(GetName(), param);
				if (dt.Rows.Count > 0 && Convert.ToInt32(dt.Rows[0][0]) > 0)
				{
					res.Statuscode = Convert.ToInt32(dt.Rows[0][0].ToString());
					res.Msg = dt.Rows[0]["Msg"] is DBNull ? "" : dt.Rows[0]["Msg"].ToString();

				}
			}
			catch (Exception er)
			{ }
			return res;
		}

		public object Call()
		{
			throw new NotImplementedException();
		}

		public string GetName()
		{
			return "proc_AddNews";
		}
	}
}
