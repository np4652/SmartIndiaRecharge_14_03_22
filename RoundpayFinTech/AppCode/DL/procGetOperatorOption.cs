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
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class procGetOperatorOption : IProcedure
    {
        private readonly IDAL _dal;
        public procGetOperatorOption(IDAL dal)
        {
            _dal = dal;
        }
        public object Call(object obj)
        {
            CommonReq _req = (CommonReq)obj;
            SqlParameter[] param = {
                 new SqlParameter("@LoginID", _req.LoginID),
                 new SqlParameter("@LT", _req.LoginTypeID),
                 new SqlParameter("@OID", _req.CommonInt)
        };
            List<OperatorOptional> _res = new List<OperatorOptional>();
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                foreach(DataRow dr in dt.Rows)
                {
                    OperatorOptional oo = new OperatorOptional
                    {
                        OID = Convert.ToInt32(dr["OpID"]),
                        OptionalType= Convert.ToInt32(dr["_OptionalType"]),
                        DisplayName= dr["_DisplayName"].ToString(),
                        Remark = dr["_Remark"].ToString(),
                        IsList = Convert.ToBoolean(dr["_IsList"]),
                        IsMultiSelection = Convert.ToBoolean(dr["_IsMultiSelection"])
                    };
                    _res.Add(oo);
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
            return "proc_GetOperatorOption";
        }
    }
}
