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
    public class ProcSaveServiceField : IProcedure
    {
        private readonly IDAL _dal;
        public ProcSaveServiceField(IDAL dal)
        {
            _dal = dal;
        }

        public object Call(object obj)
        {
            var req = (FieldMasterModel)obj;
            SqlParameter[] param = {
                new SqlParameter("@ID",req._ID),
                new SqlParameter("@Name",req._Name??""),
                new SqlParameter("@FieldType",req._FieldType),
                new SqlParameter("@OID",req._OID),
                new SqlParameter("@EntryBy",req._EntryBy),
                new SqlParameter("@IND",req._IND),
                new SqlParameter("@VocabID",req._VocabID),
                new SqlParameter("@InputType",req._InputType),
                new SqlParameter("@Placeholder",req._Placeholder),
                new SqlParameter("@Label",req._Label),
                new SqlParameter("@IsRequired",req._IsRequired),
                new SqlParameter("@MaxLength",req._MaxLength),
                new SqlParameter("@MinLength",req._MinLength),
                new SqlParameter("@AutoComplete",req._AutoComplete),
                new SqlParameter("@IsDisabled",req._IsDisabled),
                new SqlParameter("@IsReadOnly",req._IsReadOnly)
            };
            var resp = new ResponseStatus();
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
            return "proc_Save_Service_Field";
        }
    }
}
