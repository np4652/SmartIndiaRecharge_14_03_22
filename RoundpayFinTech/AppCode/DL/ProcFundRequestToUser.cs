using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcFundRequestToUser : IProcedure
    {
        private readonly IDAL _dal;
        public ProcFundRequestToUser(IDAL dal)
        {
            _dal = dal;
        }
        public object Call(object obj)
        {
            CommonReq   _req= (CommonReq)obj;
            SqlParameter[] param ={
               new SqlParameter("@LoginID", _req.LoginID),
               new SqlParameter("@LTID", _req.LoginTypeID)
            };
            List<FundRequestToUser> _resp = new List<FundRequestToUser>();
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        FundRequestToUser res = new FundRequestToUser
                        {
                            ParentName = dr["Outletname"].ToString(),
                            ParentID = Convert.ToInt32(dr["UserID"]),
                            ParentRoleID = Convert.ToInt32(dr["RoleID"])
                        };
                        _resp.Add(res);
                    }
                }
            }
            catch (Exception ex)
            {
            }
            return _resp;
        }

        public object Call()
        {
            throw new NotImplementedException();
        }

        public string GetName()=>"proc_FundRequestToUser";
    }
}
