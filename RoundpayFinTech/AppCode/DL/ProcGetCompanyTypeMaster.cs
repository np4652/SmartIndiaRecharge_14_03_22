using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetCompanyTypeMaster : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetCompanyTypeMaster(IDAL dal)
        {
            _dal = dal;
        }
        public object Call(object obj)
        {
            var _req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LoginID", _req.LoginID),
                new SqlParameter("@LT", _req.LoginTypeID),
                new SqlParameter("@ID", _req.CommonInt)
            };
            var bankMasters = new List<MasterCompanyType>();
            var bankMaster = new MasterCompanyType();
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    if (_req.CommonInt > 0)
                    {
                        bankMaster = new MasterCompanyType
                        {
                            ID = Convert.ToInt32(dt.Rows[0]["_ID"]),
                            CompanyName = dt.Rows[0]["_CompanyType"].ToString()
                           
                        };
                    }
                    else
                    {
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            var bankM = new MasterCompanyType
                            {
                                ID = Convert.ToInt32(dt.Rows[i]["_ID"]),
                                CompanyName = dt.Rows[i]["_CompanyType"].ToString()
                                
                            };
                            bankMasters.Add(bankM);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = _req.LoginTypeID,
                    UserId = _req.LoginID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            if (_req.CommonInt > 0)
                return bankMaster;
            return bankMasters;
        }

        public object Call()
        {
            throw new NotImplementedException();
        }

        public string GetName() => "proc_GetCompanyTypeMaster";
    }
}
