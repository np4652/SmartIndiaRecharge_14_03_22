using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcOperatorCU : IProcedure
    {
        private readonly IDAL _dal;
        public ProcOperatorCU(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (OperatorRequest)obj;
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            SqlParameter[] param = {
                new SqlParameter("@LoginID",req.LoginID),
                new SqlParameter("@LT",req.LoginTypeID),
                new SqlParameter("@ID",req.Detail.OID),
                new SqlParameter("@Name",req.Detail.Name??""),
                new SqlParameter("@OPID ",req.Detail.OPID??""),
                new SqlParameter("@Type",req.Detail.OpType),
                new SqlParameter("@CommSettingType",req.Detail.CommSettingType),
                new SqlParameter("@Length",req.Detail.Length),
                new SqlParameter("@Min",req.Detail.Min),
                new SqlParameter("@Max",req.Detail.Max),
                new SqlParameter("@HSNCode",req.Detail.HSNCode??""),
                new SqlParameter("@StartWith",req.Detail.StartWith??""),
                new SqlParameter("@BusinessModel",req.Detail.BusinessModel??""),
                new SqlParameter("@IsBBPS",req.Detail.IsBBPS),
                new SqlParameter("@CircleValType",req.Detail.CircleValidationType),
                new SqlParameter("@IsBilling",req.Detail.IsBilling),
                new SqlParameter("@InSlab",req.Detail.InSlab),
                new SqlParameter("@IP",req.CommonStr),
                new SqlParameter("@Browser",req.CommonStr2??""),
                new SqlParameter("@IsOPID",req.Detail.IsOPID),
                new SqlParameter("@AccountName",req.Detail.AccountName??""),
                new SqlParameter("@AccountRemak",req.Detail.AccountRemak??""),
                new SqlParameter("@IsPartial",req.Detail.IsPartial),
                new SqlParameter("@IsTakeCustomerNum",req.Detail.IsTakeCustomerNum),
                new SqlParameter("@TollFree",req.Detail.TollFree??""),
                new SqlParameter("@LengthMax",req.Detail.LengthMax),
                new SqlParameter("@AllowedChannel",req.Detail.AllowedChannel),
                new SqlParameter("@IsAccountNumeric",req.Detail.IsAccountNumeric),
                new SqlParameter("@ExactNessID",req.Detail.ExactNessID)
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
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = req.LoginTypeID,
                    UserId = req.LoginID
                });
            }
            return res;
        }
        public object Call() => throw new NotImplementedException();
        public string GetName() => "proc_Operator_CU";

        public ResponseStatus changeValidationType(int OID, int CircleValidationType)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            SqlParameter[] param = {
                new SqlParameter("@OID",OID),
                new SqlParameter("@CVType",CircleValidationType)
            };
            var dt = _dal.Get("update tbl_Operator set _CircleValidationType=@CVType where _ID=@OID;select 1,'Update successfully'Msg", param);
            if (dt.Rows.Count > 0)
            {
                res.Statuscode = Convert.ToInt32(dt.Rows[0][0]);
                res.Msg = Convert.ToString(dt.Rows[0]["Msg"]);
            }
            return res;
        }
    }
}
