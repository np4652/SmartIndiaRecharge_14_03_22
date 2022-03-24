using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetVideoLink : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetVideoLink(IDAL dal)
        {
            _dal = dal;
        }
        public object Call(object obj)
        {
            CommonReq req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT",req.LoginTypeID),
                new SqlParameter("@LoginID",req.LoginID),
                new SqlParameter("@ID",req.CommonInt)
            };
            List<Video> res = new List<Video> { };
            DataTable dt = _dal.GetByProcedure(GetName(), param);
            if (dt.Rows.Count > 0)
            {
                
                    foreach (DataRow row in dt.Rows)
                    {
                        var VideoList = new Video
                        {
                            ID = row["_ID"] is DBNull ? 0 : Convert.ToInt32(row["_ID"]),
                            URL = row["_URL"] is DBNull ?string.Empty : row["_URL"].ToString(),
                            Title = row["_Title"] is DBNull ? string.Empty : row["_Title"].ToString(),
                        };
                        if (req.CommonInt > 0)
                        {
                            return VideoList;
                        }
                        else
                        {
                            res.Add(VideoList);
                        }
                  
                }
            }
            if (req.CommonInt > 0)
                return new Video { };
            return res;
        }

        public object Call()
        {
            throw new NotImplementedException();
        }

        public string GetName()
        {
            return "proc_GetVideo";
        }
    }
}

