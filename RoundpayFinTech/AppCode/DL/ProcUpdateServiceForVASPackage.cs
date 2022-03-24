using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
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
    public class ProcUpdateServiceForVASPackage : IProcedure
    {
        private readonly IDAL _dal;
        public ProcUpdateServiceForVASPackage(IDAL dal) {
            _dal = dal;
        }
        public object Call(object obj)
        {
            CommonReq req = (CommonReq)obj;
            ResponseStatus res = new ResponseStatus
            {
                Statuscode=ErrorCodes.Minus1,
                Msg=ErrorCodes.AnError
            };
            //@LT ,@LoginID , @PackageID , @ServiceID , @IsActive ,@IP ,@Browser 
            SqlParameter[] param = {
                new SqlParameter("@LT",req.LoginTypeID),
                new SqlParameter("@LoginID",req.LoginTypeID),
                new SqlParameter("@PackageID",req.CommonInt),
                new SqlParameter("@ServiceID",req.CommonInt2),
                new SqlParameter("@IsActive",req.CommonBool),
                new SqlParameter("@IP",req.CommonStr??""),
                new SqlParameter("@Browser",req.CommonStr2??""),
                new SqlParameter("@IsChargable",req.CommonBool1),
                new SqlParameter("@Charge",req.CommonInt3)
            };            
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(),param);
                if (dt.Rows.Count>0) {
                    res.Statuscode = Convert.ToInt16(dt.Rows[0][0]);
                    res.Msg = dt.Rows[0]["Msg"] is DBNull ? "" : dt.Rows[0]["Msg"].ToString();
                }
            }
            catch (Exception ex)
            {
            }
            return res;
        }

        public object Call()
        {
            throw new NotImplementedException();
        }

        public string GetName()
        {
            return "proc_UpdateServiceForVASPackage";
        }
    }
}
