using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
	public class ProcGetAsignNews : IProcedure
	{

		private readonly IDAL _dal;

		public ProcGetAsignNews(IDAL dal)
		{
			_dal = dal;
		}
		public object Call(object obj)
		{
			CommonReq _req = (CommonReq)obj;
			UserNews model = new UserNews();
			var resp = new List<News>();
			SqlParameter[] param = {
				new SqlParameter("@LT", _req.LoginTypeID),
				new SqlParameter("@ID", _req.CommonInt),
				new SqlParameter("@NewsDetail", _req.str),				
				new SqlParameter("@GetNewsRole", _req.CommonInt3),
			};

			try
			{
				DataTable dt = _dal.GetByProcedure(GetName(), param);
				if (dt.Rows.Count > 0 && Convert.ToInt32(dt.Rows[0][0]) > 0)
				{
					for (int i = 0; i < dt.Rows.Count; i++)
					{
						News item = new News
						{
							ID = Convert.ToInt32(dt.Rows[i]["NewsId"].ToString()),							
							RoleId = Convert.ToInt32(dt.Rows[i]["RoleId"].ToString()),
							
						};
						resp.Add(item);
					}
					model.ListNews = resp;
				}
			}
			catch (Exception er)
			{ }
			return model;
		}

		public object Call()
		{
			throw new NotImplementedException();
		}

		public string GetName()
		{
			return "proc_GetNews";
		}

	}
}
