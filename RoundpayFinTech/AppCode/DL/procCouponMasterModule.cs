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
using System.Reflection;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class procCouponMasterModule:IProcedure
    {
     private readonly IDAL _dal;
    public procCouponMasterModule(IDAL dal) => _dal = dal;
    public string GetName() => "proc_GetCouponMaster";
        public object Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LoginID",req.LoginID),
                new SqlParameter("@LT",req.LoginTypeID),
                new SqlParameter("@ID",req.CommonInt),

            };
            var res = new List<CoupanMaster>();
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    if (req.CommonInt == -1)
                    {
                        foreach (DataRow row in dt.Rows)
                        {
                            var CoupanMaster = new CoupanMaster
                            {
                                ID = row["_ID"] is DBNull ? 0 : Convert.ToInt32(row["_ID"]),
                                 VoucherType = row["_VoucherType"] is DBNull ? "" : row["_VoucherType"].ToString(),
                                 OID= row["_OID"] is DBNull ? 0 : Convert.ToInt32(row["_OID"]),
                                 OpName= row["_Name"] is DBNull ? "" : row["_Name"].ToString(),
                                Remark = row["_Remark"] is DBNull ? "" : row["_Remark"].ToString(),
                                
                                LastModifyDate= Convert.ToDateTime(row["_ModifyDate"]).ToString("dd-MMM-yyyy hh:mm:ss tt"),
                            
                                Max= row["_Max"] is DBNull ? 0 : Convert.ToInt32(row["_Max"]),
                                Min = row["_Min"] is DBNull ? 0: Convert.ToInt32(row["_Min"]),
                                IsActive= row["_IsActive"] is DBNull ? true : Convert.ToBoolean(row["_IsActive"]),
                                
                               

                            };
                            res.Add(CoupanMaster);
                        }
                    }
                    else
                    {
                        return new CoupanMaster
                        {
                            ID = dt.Rows[0]["_ID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_ID"]),
                            VoucherType = dt.Rows[0]["_VoucherType"] is DBNull ? "" : dt.Rows[0]["_VoucherType"].ToString(),
                            OID = dt.Rows[0]["_OID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_OID"]),
                            OpName = dt.Rows[0]["_Name"] is DBNull ? "" : dt.Rows[0]["_Name"].ToString(),
                            Remark = dt.Rows[0]["_Remark"] is DBNull ? "" : dt.Rows[0]["_Remark"].ToString(),
                           
                            Max = dt.Rows[0]["_Max"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_Max"]),
                            Min = dt.Rows[0]["_Min"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_Min"]),
                            IsActive = dt.Rows[0]["_IsActive"] is DBNull ? true : Convert.ToBoolean(dt.Rows[0]["_IsActive"]),
                           

                        };
                    }

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
            if (req.CommonInt == -1)
                return res;
            else
                return new CoupanMaster() { Min=0,Max=0} ;
        }

        public object Call() => throw new NotImplementedException();

}
    public class ProcUpdateCouponMaster : IProcedure
    {
        private readonly IDAL _dal;
        public ProcUpdateCouponMaster(IDAL dal) => _dal = dal;
        public string GetName() => "proc_UpdateCouponMaster";
        public object Call(object obj)
        {
            var req = (CoupanReq)obj;
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            SqlParameter[] param = {
                new SqlParameter("@LT",req.LoginTypeID),
                new SqlParameter("@LoginID",req.LoginID),
                new SqlParameter("@OID",req.coupanMaster.OID),
                new SqlParameter("@VoucherType",req.coupanMaster.VoucherType),
                new SqlParameter("@Remark",req.coupanMaster.Remark),
                 new SqlParameter("@ID",req.coupanMaster.ID),
                 new SqlParameter("@MinAmount",req.coupanMaster.Min),
                 new SqlParameter("@MaxAmount",req.coupanMaster.Max),
                 new SqlParameter("@IsActive",req.coupanMaster.IsActive),



            }; 
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    res.Statuscode = Convert.ToInt32(dt.Rows[0][0]);
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

        

    }
    public class ProcGetCouponVoucherList:IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetCouponVoucherList(IDAL dal) => _dal = dal;

        public object Call(object obj)
        {
            CoupanVoucher req = (CoupanVoucher)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT",req.LoginTypeID),
                new SqlParameter("@LoginID",req.LoginID),
                new SqlParameter("@VoucherID",req.ID)
            };
            List<CoupanVoucher> res = new List<CoupanVoucher> { };

            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    if (req.CommonInt == -1)
                    {
                        foreach (DataRow row in dt.Rows)
                        {
                            var data = new CoupanVoucher
                            {
                                ID = row["_ID"] is DBNull ? 0 : Convert.ToInt32(row["_ID"]),
                                VoucherID = row["_VoucherID"] is DBNull ? 0 : Convert.ToInt32(row["_VoucherID"]),
                                CouponCode = row["_CouponCode"] is DBNull ? "" : Convert.ToString(row["_CouponCode"]),
                                Amount = row["_Amount"] is DBNull ? 0 : Convert.ToInt32(row["_Amount"]),
                                ApiName= row["_Name"] is DBNull ? "" : Convert.ToString(row["_Name"]),
                                APIID= row["_APIID"] is DBNull ? "0" : Convert.ToString(row["_APIID"]),
                                IsSale= row["_IsSale"] is DBNull ? false : Convert.ToBoolean(row["_IsSale"]),
                                EntryDate=row["_EntryDate"] is DBNull ? "" : Convert.ToDateTime(row["_EntryDate"]).ToString("dd-MMM-yyyy hh:mm:ss tt"),
                                ModifyDate = row["_ModifyDate"] is DBNull ? "" : Convert.ToDateTime(row["_ModifyDate"]).ToString("dd-MMM-yyyy hh:mm:ss tt")

                            };
                            res.Add(data);
                        }
                    }
                    else
                    {
                        
                    }

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
            if (req.CommonInt == -1)
                return res;
            else
                return new CoupanVoucher();



        }

        public object Call() => throw new NotImplementedException();

        public string GetName() => "proc_GetCouponVoucherList";



    }
    public class ProcUpdateCouponVoucher : IProcedure
    {
        private readonly IDAL _dal;
        public ProcUpdateCouponVoucher(IDAL dal) => _dal = dal;
        public string GetName() => "proc_UpdateCouponVoucher";
        public object Call(object obj)
        {
            var req = (CoupanVoucher)obj;
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            SqlParameter[] param = {
                new SqlParameter("@LT",req.LoginTypeID),
                new SqlParameter("@LoginID",req.LoginID),
                new SqlParameter("@ID",req.ID),
                new SqlParameter("@APIID",req.APIID),
                new SqlParameter("@CouponCode",req.CouponCode),
                new SqlParameter("@Amount",req.Amount),
                new SqlParameter("@VoucherID",req.VoucherID),
                 new SqlParameter("@IsSale",req.IsSale),


            };
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    res.Statuscode = Convert.ToInt32(dt.Rows[0][0]);
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



    }

    public class ProcGetDenominationVoucher : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetDenominationVoucher(IDAL dal) => _dal = dal;

        public object Call(object obj)
        {
            CoupanVoucher req = (CoupanVoucher)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT",req.LoginTypeID),
                new SqlParameter("@LoginID",req.LoginID)
                
            };
            List<DenominationVoucher> res = new List<DenominationVoucher> { };

            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    
                        foreach (DataRow row in dt.Rows)
                        {
                            var data = new DenominationVoucher
                            {
                                DenominationID = row["_ID"] is DBNull ? 0 : Convert.ToInt32(row["_ID"]),
                                DenminationAmount = row["_Amount"] is DBNull ? 0 : Convert.ToInt32(row["_Amount"]),
                                
                               

                            };
                            res.Add(data);
                        }
                    
                   

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
            if (req.CommonInt == -1)
                return res;
            else
                return res;



        }

        public object Call() => throw new NotImplementedException();

        public string GetName() => "proc_GetDenominationVoucher";



    }
    public class ProcUpdateCouponSetting : IProcedure
    {
        private readonly IDAL _dal;
        public ProcUpdateCouponSetting(IDAL dal) => _dal = dal;
        public string GetName() => "proc_UpdateCouponSetting";
        public object Call(object obj)
        {
            var req = (DenominationVoucher)obj;
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
               
            try
            {
                //foreach (var itm in req.DenominationIDs.Split(','))
                //{

                    SqlParameter[] param = {
                new SqlParameter("@LT",req.LoginTypeID),
                new SqlParameter("@LoginID",req.LoginID),
                new SqlParameter("@VoucherID",req.VoucherID),
                new SqlParameter("@DenomID",req.DenominationID),
                new SqlParameter("@IsActive",req.IsActive)


            };
                    var dt = _dal.GetByProcedure(GetName(), param);
                    if (dt.Rows.Count > 0)
                    {
                        res.Statuscode = Convert.ToInt32(dt.Rows[0][0]);
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



    }

    public class ProcUpdateCouponStatus : IProcedure
    {
        private readonly IDAL _dal;
        public ProcUpdateCouponStatus(IDAL dal) => _dal = dal;

        public object Call(object obj)
        {
            var _req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LoginID", _req.LoginID),
                new SqlParameter("@LTID", _req.LoginTypeID),
                new SqlParameter("@ID",_req.CommonInt),


        };
            var _resp = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    _resp.Statuscode = Convert.ToInt16(dt.Rows[0][0]);
                    _resp.Msg = dt.Rows[0]["Msg"].ToString();
                }
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = _req.LoginTypeID,
                    UserId = _req.LoginID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return _resp;
        }
        public object Call()
        {
            throw new NotImplementedException();
        }
        public string GetName()
        {
            return "Proc_Update_CouponStatus";
        }

    }


    public class procGetCouponSetting : IProcedure
    {
        private readonly IDAL _dal;
        public procGetCouponSetting(IDAL dal) => _dal = dal;
        public string GetName() => "proc_GetCouponSetting";
        public object Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LoginID",req.LoginID),
                new SqlParameter("@LT",req.LoginTypeID),
                new SqlParameter("@ID",req.CommonInt),

            };
            var res = new List<DenominationVoucher>();
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {

                    foreach (DataRow row in dt.Rows)
                    {
                        var Coupansetting = new DenominationVoucher
                        {
                            VoucherID = row["_ID"] is DBNull ? 0 : Convert.ToInt32(row["_VoucherID"]),
                            DenominationID = row["_DenomID"] is DBNull ? 0 : Convert.ToInt32(row["_DenomID"]),
                            IsActive = row["_IsActive"] is DBNull ? false : Convert.ToBoolean(row["_IsActive"]),




                        };
                        res.Add(Coupansetting);
                    }
                }
                else
                {
                    return new DenominationVoucher
                    {

                        VoucherID = dt.Rows[0]["_ID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_VoucherID"]),
                        DenominationID = dt.Rows[0]["_DenomID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_DenomID"]),
                        IsActive = dt.Rows[0]["_IsActive"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_IsActive"]),


                    };
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
            if (req.CommonInt > 0)
                return res;
            else
                return new DenominationVoucher();
        }

        public object Call() => throw new NotImplementedException();

    }



    public class ProcInsertBulkCoupon : IProcedureAsync
    {
        private IDAL _dal;

        public ProcInsertBulkCoupon(IDAL dal) => _dal = dal;

        public async Task<object> Call(object obj)
        {
            var _req = (CoupanVoucherEXlReq)obj;
            DataTable dataTable = _req.Couponvocher.ToDataTable();
            SqlParameter[] param = {
                new SqlParameter("@dataTable", dataTable),
                new SqlParameter("@APIID", _req.APIID),
                 new SqlParameter("@VocherID", _req.VocherID),
                 new SqlParameter("@LoginID", _req.LoginID),
            };
            var _resp = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            try
            {
                var dt = await _dal.GetByProcedureAsync(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    _resp.Statuscode = Convert.ToInt32(dt.Rows[0][0]);
                    _resp.Msg = dt.Rows[0]["Msg"].ToString();
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
            return _resp;
        }

        public Task<object> Call()
        {
            throw new NotImplementedException();
        }

        public string GetName() => "Proc_InsertCouponVocher";

       
    }

    public class ProcDelCouponVoucher : IProcedure
    {
        private readonly IDAL _dal;
        public ProcDelCouponVoucher(IDAL dal) => _dal = dal;
        public string GetName() => "proc_DeleteCouponVoucher";
        public object Call(object obj)
        {
            var req = (CoupanVoucher)obj;
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            SqlParameter[] param = {
                new SqlParameter("@LT",req.LoginTypeID),
                new SqlParameter("@LoginID",req.LoginID),
                new SqlParameter("@ID",req.ID),

            };
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    res.Statuscode = Convert.ToInt32(dt.Rows[0][0]);
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



    }


}
