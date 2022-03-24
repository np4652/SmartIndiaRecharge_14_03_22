using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcPdfData : IProcedure
    {
        private readonly IDAL _dal;
        public ProcPdfData(IDAL dal) => _dal = dal;

        public object Call(object obj)
        {
            ResponseStatus res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            CommonReq req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT", req.LoginTypeID),
                new SqlParameter("@UserID", req.LoginID)
              };

            try
            {
                DataTable dt = _dal.Get(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    res.CommonStr = Convert.ToString(dt.Rows[0]["joiningDate"]);
                    res.CommonInt = dt.Rows[0]["_KYCStatus"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_KYCStatus"]);
                    res.CommonStr2 = Convert.ToString(dt.Rows[0]["_Pan"]);
                    res.CommonStr3 = Convert.ToString(dt.Rows[0]["_Address"]);
                }
                return (IResponseStatus)(res);
            }
            catch (Exception er)
            { }
            return (IResponseStatus)(res);
        }

        public object Call()
        {
            throw new NotImplementedException();
        }

         public string GetName() => "select dbo.CustomFormat(_EntryDate+2) joiningDate,_KYCStatus,isnull(_Pan,'') _Pan , isnull(_Address , '')  _Address from tbl_OutletsOfUser where _UserID=@UserID";
        
    }


    public class ProcIrctcPdfData : IProcedure
    {
        private readonly IDAL _dal;
        public ProcIrctcPdfData(IDAL dal) => _dal = dal;

        public object Call(object obj)
        {
            
            CommonReq req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT", req.LoginTypeID),
                new SqlParameter("@UserID", req.LoginID)
              };

            var res1 = new IrctcCertificateModel {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            try
            {
                DataTable dt = _dal.Get(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    var date = Convert.ToString(dt.Rows[0]["_IRCTCExpiry"]);
                    res1.Pan = dt.Rows[0]["_Pan"] is DBNull ? null : Convert.ToString(dt.Rows[0]["_Pan"]);
                    res1.OutletName= dt.Rows[0]["_Company"] is DBNull ? null : Convert.ToString(dt.Rows[0]["_Company"]);
                    res1.OwnerName= dt.Rows[0]["_Name"] is DBNull ? null : Convert.ToString(dt.Rows[0]["_Name"]);
                    res1.ExpDate = String.IsNullOrEmpty(date) ? null : Convert.ToDateTime(dt.Rows[0]["_IRCTCExpiry"]).ToString("dd MMM yyyy");
                    res1.IrctcID= dt.Rows[0]["_IRCTCID"] is DBNull ? null : Convert.ToString(dt.Rows[0]["_IRCTCID"]);
                    res1.mobileNo = dt.Rows[0]["_MobileNo"] is DBNull ? null : Convert.ToString(dt.Rows[0]["_MobileNo"]);
                    res1.EmailId = dt.Rows[0]["_EmailID"] is DBNull ? null : Convert.ToString(dt.Rows[0]["_EmailID"]);
                    res1.Address = dt.Rows[0]["_Address"] is DBNull ? null : Convert.ToString(dt.Rows[0]["_Address"]);
                    res1.IrctcStatus=dt.Rows[0]["_IRCTCStaus"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_IRCTCStaus"]);
                    res1.joiningDate = String.IsNullOrEmpty(date) ? null : Convert.ToDateTime(dt.Rows[0]["_IRCTCExpiry"]).AddDays(-365).ToString("dd MMM yyyy");
                    res1.Statuscode = 1;
                }
                else
                {

                    res1.Msg = "No Record Found.";
                }
                return (IrctcCertificateModel)(res1);
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
            return (IrctcCertificateModel)(res1);
        }

        public object Call()
        {
            throw new NotImplementedException();
        }

        public string GetName() => "select _EntryDate, _IRCTCStaus,isnull(_Pan,'') _Pan,_Name,_Company,_MobileNo,_EmailID,isnull(dbo.CustomFormat(_IRCTCExpiry),'' )as _IRCTCExpiry,_IRCTCID,_Address from tbl_OutletsOfUser where _UserID=@UserID and  _IRCTCStaus is not null";

    }


    public class ProcGetOutLetData : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetOutLetData(IDAL dal) => _dal = dal;

        public object Call(object obj)
        {

            CommonReq req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT", req.LoginTypeID),
                new SqlParameter("@UserID", req.LoginID)
              };

            var res1 = new IrctcCertificateModel
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            try
            {
                DataTable dt = _dal.Get(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    
                    res1.Pan = dt.Rows[0]["_Pan"] is DBNull ? null : Convert.ToString(dt.Rows[0]["_Pan"]);
                    res1.OutletName = dt.Rows[0]["_Company"] is DBNull ? null : Convert.ToString(dt.Rows[0]["_Company"]);
                    res1.OwnerName = dt.Rows[0]["_Name"] is DBNull ? null : Convert.ToString(dt.Rows[0]["_Name"]);
                   
                    res1.IrctcID = dt.Rows[0]["_IRCTCID"] is DBNull ? null : Convert.ToString(dt.Rows[0]["_IRCTCID"]);
                    res1.mobileNo = dt.Rows[0]["_MobileNo"] is DBNull ? null : Convert.ToString(dt.Rows[0]["_MobileNo"]);
                    res1.EmailId = dt.Rows[0]["_EmailID"] is DBNull ? null : Convert.ToString(dt.Rows[0]["_EmailID"]);
                    res1.Address = dt.Rows[0]["_Address"] is DBNull ? null : Convert.ToString(dt.Rows[0]["_Address"]);
                    res1.IrctcStatus = dt.Rows[0]["_IRCTCStaus"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_IRCTCStaus"]);
                    
                    res1.Statuscode = 1;
                }
                else
                {

                    res1.Msg = "No Record Found.";
                }
                return (IrctcCertificateModel)(res1);
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
            return (IrctcCertificateModel)(res1);
        }

        public object Call()
        {
            throw new NotImplementedException();
        }

        public string GetName() => "select * from tbl_OutletsOfUser where _UserID=@UserID";

    }



}
