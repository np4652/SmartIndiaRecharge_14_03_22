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
    public class ProcUpdateMemberMaster : IProcedure
    {
        private readonly IDAL _dal;
        public ProcUpdateMemberMaster(IDAL dal) => _dal = dal;

        public object Call(object obj)
        {
            var req = (MembershipMasteReq)obj;
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            SqlParameter[] param = {
                new SqlParameter("@LT",req.LoginTypeID),
                new SqlParameter("@LoginID",req.LoginID),
                new SqlParameter("@IP",req.CommonStr??""),
                new SqlParameter("@Browser",req.CommonStr2??""),
                new SqlParameter("@MemberShipType",req.memMaster.MemberShipType),
                new SqlParameter("@CouponCount",req.memMaster.CouponCount),
                new SqlParameter("@CouponValue",req.memMaster.CouponValue),
                new SqlParameter("@IsCouponAllowed",req.memMaster.IsCouponAllowed),
                new SqlParameter("@Remark",req.memMaster.Remark),
                new SqlParameter("@CouponValidityDays",req.memMaster.CouponValidityDays),
                new SqlParameter("@IsActive",req.memMaster.IsActive),
                new SqlParameter("@Cost",req.memMaster.Cost),
                new SqlParameter("@ID",req.memMaster.ID),
                new SqlParameter("@SlabID",req.memMaster.SlabID),
                new SqlParameter("@MinInterval",req.memMaster.MinInterval),
                new SqlParameter("@ReferralIncome",req.memMaster.ReferralIncome),
                new SqlParameter("@PackageValidity",req.memMaster.PackageValidity)



    };
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    res.Statuscode = Convert.ToInt16(dt.Rows[0][0]);
                    res.Msg = dt.Rows[0]["Msg"] is DBNull ? "" : dt.Rows[0]["Msg"].ToString();
                }
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = req.LoginTypeID,
                    UserId = req.LoginID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return res;
        }

        public object Call() => throw new NotImplementedException();

        public string GetName() => "proc_UpdateMembershipMaster";

    }
}
