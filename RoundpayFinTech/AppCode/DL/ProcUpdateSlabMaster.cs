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
    public class ProcUpdateSlabMaster : IProcedure
    {
        private readonly IDAL _dal;
        public ProcUpdateSlabMaster(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (SlabMasterReq)obj;
            var res = new ResponseStatus {
                Statuscode=ErrorCodes.Minus1,
                Msg=ErrorCodes.TempError
            };
            SqlParameter[] param = {
                new SqlParameter("@LT",req.LoginTypeID),
                new SqlParameter("@LoginID",req.LoginID),
                new SqlParameter("@SlabID",req.slabMaster.ID),
                new SqlParameter("@Slab",req.slabMaster.Slab??""),
                new SqlParameter("@Remark",req.slabMaster.Remark),
                new SqlParameter("@IsRealSlab",req.slabMaster.IsRealSlab),
                new SqlParameter("@IsAdminDefined",req.slabMaster.IsAdminDefined),
                new SqlParameter("@IP",req.CommonStr??""),
                new SqlParameter("@Browser",req.CommonStr2??""),
                new SqlParameter("@IsB2B",req.slabMaster.IsB2B),
                new SqlParameter("@DMRModelID",req.slabMaster.DMRModelID),
                new SqlParameter("@IsMultiLevel",req.slabMaster.IsMultiLevel),
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

        public string GetName() => "proc_UpdateSlabMaster";
    }


    public class ProcUpdateUserRealCommissionFlag : IProcedure
    {
        private readonly IDAL _dal;
        public ProcUpdateUserRealCommissionFlag(IDAL dal) => _dal = dal;
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
                new SqlParameter("@IsRealApi",req.CommonBool)
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

        public string GetName() => "update tbl_Users set _IsRealApi=@IsRealApi where _RoleID in(2,3) and _ID=@UserID";
    }
}
