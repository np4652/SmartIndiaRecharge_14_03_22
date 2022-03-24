using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;


namespace RoundpayFinTech.AppCode.DL
{
    public class procGetAccountOpeningRedirectionData: IProcedure
    {
        private readonly IDAL _dal;
        public procGetAccountOpeningRedirectionData(IDAL dal) => _dal = dal;

        public object Call(object obj)
        {
            CommonReq _req = (CommonReq)obj;
            SqlParameter[] param = {
                 new SqlParameter("@LoginID", _req.LoginID),
                 new SqlParameter("@LT", _req.LoginTypeID),
                 new SqlParameter("@OID", _req.CommonInt)
        };
            List<AccOpenSetting> _res = new List<AccOpenSetting>();
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        AccOpenSetting aos = new AccOpenSetting
                        {
                            ID = dr["_ID"] is DBNull ? 0 : Convert.ToInt32(dr["_ID"]),
                            OID = dr["_OID"] is DBNull ? _req.CommonInt : Convert.ToInt32(dr["_OID"]),
                            RedirectURL = Convert.ToString(dr["_RedirectURL"]),
                            Content = Convert.ToString(dr["_Content"])

                        };
                        _res.Add(aos);
                    }
                }
                else
                {

                    AccOpenSetting aos = new AccOpenSetting
                    {
                        ID=0,
                        OID = _req.CommonInt,
                        RedirectURL ="",
                        Content = ""

                    };
                    _res.Add(aos);

                }
            }
            catch (Exception ex)
            {
                ErrorLog errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = _req.LoginTypeID,
                    UserId = _req.LoginID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return _res;
        }

        public object Call()
        {
            throw new NotImplementedException();
        }

        public string GetName()
        {
            return "proc_GetAccountOpeningRedirectionData";
        }
    }
}
