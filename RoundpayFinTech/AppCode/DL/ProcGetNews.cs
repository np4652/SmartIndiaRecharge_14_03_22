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
    public class ProcGetNews : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetNews(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var _req = (CommonReq)obj;
            var model = new List<News>();
            SqlParameter[] param = {
                new SqlParameter("@LT", _req.LoginID),
                new SqlParameter("@ID", _req.CommonInt),
                new SqlParameter("@NewsDetail", _req.str??""),
                new SqlParameter("@GetNewsRole", _req.CommonInt3),
                new SqlParameter("@WID", _req.CommonInt4)
            };
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    if (dt.Columns.Contains("CreateDate"))
                    {
                        foreach (DataRow item in dt.Rows)
                        {
                            model.Add(new News
                            {
                                ID = Convert.ToInt32(item["Id"].ToString()),
                                CreateDate = item["CreateDate"].ToString(),
                                NewsDetail = item["NewsDetail"].ToString(),
                                Title = item["Title"].ToString()
                            });
                        }
                    }
                }
            }
            catch (Exception er)
            { }
            return model;
        }
        public object Call() => throw new NotImplementedException();
        public string GetName() => "proc_GetNews";
    }

    public class ProcGetNewsForLogin : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetNewsForLogin(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var _req = (CommonReq)obj;
            var model = new List<News>();
            SqlParameter[] param =
            {
                new SqlParameter("@LT", _req.LoginTypeID),
                new SqlParameter("@LoginID", _req.LoginID),
                new SqlParameter("@IsLoginNews", _req.IsListType),
                new SqlParameter("@WID", _req.CommonInt)
            };
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    if (dt.Columns.Contains("CreateDate"))
                    {
                        foreach (DataRow item in dt.Rows)
                        {
                            model.Add(new News
                            {
                                ID = Convert.ToInt32(item["Id"].ToString()),
                                CreateDate = item["CreateDate"].ToString(),
                                NewsDetail = item["NewsDetail"].ToString(),
                                Title = item["Title"].ToString()
                            });
                        }
                    }
                }
            }
            catch (Exception er)
            { }
            return model;
        }
        public object Call() => throw new NotImplementedException();
        public string GetName() => "proc_GetNewsForLogin";
    }
}
