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
    public class ProcUpdateDocType : IProcedure
    {
        private readonly IDAL _dal;
        public ProcUpdateDocType(IDAL dal)
        {
            _dal = dal;
        }
        public object Call(object obj)
        {
            DocTypeMaster req = (DocTypeMaster)obj;
            SqlParameter[] param = {
                new SqlParameter("@ID", req.ID),
                new SqlParameter("@IsOptional", req.IsOptional),
                new SqlParameter("@LoginID", req.UserId),
                new SqlParameter("@Remark", req.Remark ?? ""),
                new SqlParameter("@IsOutlet", req.IsOutlet),
                new SqlParameter("@LT", req.LoginTypeID)
            };
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    _res.Statuscode= Convert.ToInt16(dt.Rows[0][0].ToString());
                    _res.Msg = dt.Rows[0][1].ToString();
                }

            }
            catch (Exception)
            {
                //_res.Description = ex.Message;
            }
            return _res;
        }

        public object Call()
        {
            throw new NotImplementedException();
        }

        public string GetName()
        {
            return "proc_UpdateDocType";
        }
    }
}