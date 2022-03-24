using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text.RegularExpressions;

namespace RoundpayFinTech.AppCode.DL
{
	public class ProcNews : IProcedure
	{
		private readonly IDAL _dal;

        public ProcNews(IDAL dal) => _dal = dal;
        public object Call(object obj)
		{
			var _req = (CommonReq)obj;
			var res = new List<News>();
			var model = new UserNews();
			SqlParameter[] param = {
				new SqlParameter("@LT", _req.LoginTypeID),
				new SqlParameter("@ID", _req.CommonInt),
				new SqlParameter("@NewsDetail", _req.str),
				new SqlParameter("@WID", _req.CommonInt2)
			};
			try
			{
				var dt = _dal.GetByProcedure(GetName(), param);
				if (dt.Rows.Count > 0 && Convert.ToInt32(dt.Rows[0][0]) > 0)
				{
					for (int i = 0; i < dt.Rows.Count; i++)
					{
						var item = new News
						{
							ID = Convert.ToInt32(dt.Rows[i]["Id"].ToString()),
							CreateDate = dt.Rows[i]["CreateDate"].ToString(),
							NewsDetail = Regex.Replace(dt.Rows[i]["NewsDetail"].ToString(), "<.*?>", String.Empty),
							Title =  dt.Rows[i]["Title"].ToString(),
						};
						res.Add(item);						
					}
				}
				model.ListNews = res;
			}
			catch (Exception er)
			{ }
			return model;
		}
        public object Call() => throw new NotImplementedException();
        public string GetName() => "proc_GetNews";
    }
}
