using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;


namespace RoundpayFinTech.AppCode.DL
{ 
    public class procUpdateAccountOpeningSetting : IProcedure
{

    private readonly IDAL _dal;
    public procUpdateAccountOpeningSetting(IDAL dal) => _dal = dal;

        public object Call(object obj)
        {
            var req = (AccOpenSetting)obj;
            SqlParameter[] param =
                {
             
                new SqlParameter("@LoginId", req.LoginID),
                new SqlParameter("@LT", req.LT),
                 new SqlParameter("@OID", req.OID),
                  new SqlParameter("@URl", req.RedirectURL),
                   new SqlParameter("@Content ", req.Content),

            };
            var _res = new ResponseStatus()
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    _res.Statuscode = Convert.ToInt32(dt.Rows[0][0]);
                    _res.Msg = dt.Columns.Contains("Msg") ? dt.Rows[0]["Msg"].ToString() : "";
                }
            }
            catch (Exception ex) { }
            return _res;
        }

        public object Call()
    {
        throw new NotImplementedException();
    }

    public string GetName()
    {
        return "proc_UpdateAccountOpeningSetting";
    }
}
}
