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
    public class procGetUpper : IProcedure
    {
        private readonly IDAL _dal;
        public procGetUpper(IDAL dal)
        {
            _dal = dal;
        }
        public object Call(object obj)
        {
            int LoginID = (int)obj;
            List<ResponseStatus> _res = new List<ResponseStatus>();
            try
            {
                SqlParameter[] param = {
                 new SqlParameter("@LoginID", LoginID)
                };
                DataTable dt = _dal.Get(GetName(), param);
                foreach (DataRow dr in dt.Rows)
                {
                    ResponseStatus status = new ResponseStatus
                    {
                        Msg = dr["Role"].ToString(),
                        CommonStr= dr["Outletname"].ToString(),
                        CommonStr2 = dr["MobileNo"].ToString(),
                        CommonInt =Convert.ToInt32(dr["Userid"])
                    };
                    _res.Add(status);
                }
            }
            catch (Exception ex)
            {
            }
            return _res;
        }
        public object Call()
        {
            throw new NotImplementedException();
        }
        public string GetName()
        {
            return "select _Role Role,Outletname,	MobileNo,Userid from fn_GetTopIntroDetail(@LoginID) f inner join master_role r on r._ID=f.RoleID order by _ind desc";
        }

    }
}
