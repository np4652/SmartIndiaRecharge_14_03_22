using Fintech.AppCode.DB;
using Fintech.AppCode.HelperClass;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcShowEmpPass : IProcedure
    {
        private readonly IDAL _dal;
        public ProcShowEmpPass(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {           
            SqlParameter[] parameter =
             {
                new SqlParameter("@UserId", (int)obj),
            };
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            try
            {
                var dt = _dal.Get(GetName(), parameter);
                if(dt.Rows.Count>0)
                {
                    _res.Statuscode = ErrorCodes.One;
                    _res.Msg = HashEncryption.O.Decrypt(Convert.ToString(dt.Rows[0]["_Password"]));
                }            
            }
            catch(Exception ex)
            {
                _res.Msg = ex.Message;
            }
            return _res;
        }

        public object Call()
        {
            throw new NotImplementedException();
        }

        public string GetName() => "Select _Password from tbl_Employee where _id<>1 and _id=@Userid";
       
    }
}
