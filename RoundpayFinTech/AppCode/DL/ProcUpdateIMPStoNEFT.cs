using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcUpdateIMPStoNEFT : IProcedure
    {
        private readonly IDAL _dal;
        public ProcUpdateIMPStoNEFT(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (CommonReq)obj;
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            SqlParameter[] param = {
                new SqlParameter("@UserID",req.LoginID),
                new SqlParameter("@IsnNEFTRouting",req.CommonBool)
            };
            try
            {
                _dal.Execute(GetName(), param);
                res.Statuscode = ErrorCodes.One;
                res.Msg = "Status Change Successfully!";
            }
            catch (Exception ex)
            {
                res.Statuscode = ErrorCodes.Minus1;
                res.Msg = "Technical Issue!";
            }
            return res;
        }

        public object Call() => throw new NotImplementedException();

        public string GetName() => "update tbl_Users set _IsnNEFTRouting=@IsnNEFTRouting where _RoleID in(2,3) and _ID=@UserID";
    }
}
