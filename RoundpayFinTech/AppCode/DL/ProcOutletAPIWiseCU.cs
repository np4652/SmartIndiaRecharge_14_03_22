using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcOutletAPIWiseCU : IProcedure
    {
        private readonly IDAL _dal;
        public ProcOutletAPIWiseCU(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (OutletAPIStatusUpdateReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LoginID",req.UserID),
                new SqlParameter("@OutletID",req.OutletID),
                new SqlParameter("@APIID",req.APIID),
                new SqlParameter("@APIOutletID",req.APIOutletID),
                new SqlParameter("@VerifyStatus",req.APIOutletStatus),
                new SqlParameter("@DocVerifyStatus",req.KYCStatus),
                new SqlParameter("@BBPSID",req.BBPSID??string.Empty),
                new SqlParameter("@BBPSStatus",req.BBPSStatus),
                new SqlParameter("@AEPSID",req.AEPSID??string.Empty),
                new SqlParameter("@AEPSStatus",req.AEPSStatus),
                new SqlParameter("@PANRequestID",req.PSARequestID),
                new SqlParameter("@PANID",req.PSAID??string.Empty),
                new SqlParameter("@PANStatus",req.PSAStatus),
                new SqlParameter("@DMTID",req.DMTID??string.Empty),
                new SqlParameter("@DMTStatus",req.DMTStatus),
                new SqlParameter("@IsVerifyStatusUpdate",req.IsVerifyStatusUpdate),
                new SqlParameter("@IsDocVerifyStatusUpdate",req.IsDocVerifyStatusUpdate),
                new SqlParameter("@IsBBPSUpdate",req.IsBBPSUpdate),
                new SqlParameter("@IsBBPSUpdateStatus",req.IsBBPSUpdateStatus),
                new SqlParameter("@IsAEPSUpdate",req.IsAEPSUpdate),
                new SqlParameter("@IsAEPSUpdateStatus",req.IsAEPSUpdateStatus),
                new SqlParameter("@IsPANRequestIDUpdate",req.IsPANRequestIDUpdate),
                new SqlParameter("@IsPANUpdate",req.IsPANUpdate),
                new SqlParameter("@IsPANUpdateStatus",req.IsPANUpdateStatus),
                new SqlParameter("@IsDMTUpdate",req.IsDMTUpdate),
                new SqlParameter("@IsDMTUpdateStatus",req.IsDMTUpdateStatus),
                new SqlParameter("@RailID",req.RailID??string.Empty),
                new SqlParameter("@RailStatus",req.RailStatus),
                new SqlParameter("@IsRailUpdate",req.IsRailIDUpdate),
                new SqlParameter("@IsRailStatusUpdate",req.IsRailUpdateStatus),
                new SqlParameter("@APICode",req._APICode??string.Empty)
            };
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = 1,
                    UserId = req.UserID
                });
            }
            return true;
        }
        public object Call() => throw new NotImplementedException();
        public string GetName() => "proc_OutletAPIWiseCU";
    }
}
