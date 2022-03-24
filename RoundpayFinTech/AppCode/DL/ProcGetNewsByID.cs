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
    public class ProcGetNewsByID : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetNewsByID(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var _req = (CommonReq)obj;
            var model = new News();
            SqlParameter[] param = {
                new SqlParameter("@ID", _req.CommonInt)
            };
            try
            {
                var dt = _dal.Get(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    model.ID = Convert.ToInt32(dt.Rows[0]["Id"].ToString());
                    model.CreateDate = dt.Rows[0]["CreateDate"].ToString();
                    model.NewsDetail = dt.Rows[0]["NewsDetail"].ToString();
                    model.Title = dt.Rows[0]["Title"].ToString();
                }
            }
            catch (Exception er)
            {
                
            }
            return model;
        }
        public object Call() => throw new NotImplementedException();
        public string GetName() => "select * from tbl_news with(nolock) where id=@ID";
    }
}
