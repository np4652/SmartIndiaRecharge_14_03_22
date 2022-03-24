using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
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
    public class ProcSaveVocab : IProcedure
    {
        private readonly IDAL _dal;
        public ProcSaveVocab(IDAL dal)
        {
            _dal = dal;
        }
        public object Call(object obj)
        {
            var req = (VocabMaster)obj;
            SqlParameter[] param = {
                new SqlParameter("@ID",req._ID),
                new SqlParameter("@Name",req._Name??""),
                new SqlParameter("@EntryBy",req._EntryBy),
                new SqlParameter("@IND",req._IND)
            };
            var resp = new ResponseStatus();
            resp.Statuscode = ErrorCodes.Minus1;
            resp.Msg = ErrorCodes.AnError;
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    resp.Statuscode = Convert.ToInt32(dt.Rows[0][0]);
                    resp.Msg = dt.Rows[0][1].ToString();
                }
            }
            catch (Exception er)
            {
                resp.Statuscode = ErrorCodes.Minus1;
                resp.Msg = ErrorCodes.AnError;
            }
            return resp;
        }

        public object Call()
        {
            throw new NotImplementedException();
        }

        public string GetName()
        {
            return "proc_SaveVocab";
        }
    }
}
