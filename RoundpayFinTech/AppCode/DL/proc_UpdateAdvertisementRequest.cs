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
    public class procUpdateAdvertisementRequest:IProcedure
    {

        private readonly IDAL _dal;
        public procUpdateAdvertisementRequest(IDAL dal) => _dal = dal;
        public string GetName() => "proc_UpdateAdvertisementRequest";
        public object Call(object obj)
        {
            var req = (AdvertisementRequest)obj;
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
           
            SqlParameter[] param = {
                new SqlParameter("@Id",req.Id),
                new SqlParameter("@UserId",req.UserID),
                new SqlParameter("@PackageId", req.PackageId),
                new SqlParameter("@Status", 1),
                new SqlParameter("@Remark",""),
                 new SqlParameter("@ContentText",req.ContentText),
                 new SqlParameter("@ContentImage",req.ContentImage),
                  new SqlParameter("@Type",req.Type),
                  new SqlParameter("@ReturnUrl",req.ReturnUrl),

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
                    LoginTypeID = req.LT,
                    UserId = req.UserID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return res;
        }

        public object Call() => throw new NotImplementedException();
    }

    public class proc_GetAdvertisementRequest : IProcedure
    {
        private readonly IDAL _dal;
        public proc_GetAdvertisementRequest(IDAL dal) => _dal = dal;

        public object Call(object obj)
        {
            var req = (AdvertisementRequest)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT",req.LT),
                new SqlParameter("@Id",req.Id),
                new SqlParameter("@UserId",req.UserID),
                new SqlParameter("@Status",req.Status),
                new SqlParameter("@ToDate",req.ToDate),
                new SqlParameter("@ToFromDate",req.FromDate),
                 new SqlParameter("@MobileNo",req.MobileNo),

            };
            List<AdvertisementRequest> res = new List<AdvertisementRequest> { };

            try
            {
                var currentdate = DateTime.Now;
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                        foreach (DataRow row in dt.Rows)
                        {
                        
                        
                        var data = new AdvertisementRequest
                            {
                                Id = row["_ID"] is DBNull ? 0 : Convert.ToInt32(row["_ID"]),
                                PackageName = row["_PackageName"] is DBNull ? "" : Convert.ToString(row["_PackageName"]),
                                UserID = row["_UserID"] is DBNull ? 0 : Convert.ToInt32(row["_UserID"]),
                                Status = row["_Status"] is DBNull ? 0 : Convert.ToInt32(row["_Status"]),
                                ContentText = row["_ContentText"] is DBNull ? "" : Convert.ToString(row["_ContentText"]),
                                ContentImage = row["_ContentImage"] is DBNull ? "" : Convert.ToString(row["_ContentImage"]),
                                StartDate= row["_StartDate"] is DBNull ? "" : Convert.ToDateTime(row["_StartDate"]).ToString("dd MMM yyyy"),
                                EndDate = row["_EndDate"] is DBNull ? "" : Convert.ToDateTime(row["_EndDate"]).ToString("dd MMM yyyy"),
                                Type = row["_Type"] is DBNull ? "" : Convert.ToString(row["_Type"]),
                                ReturnUrl= row["_ReturnUrl"] is DBNull ? "" : Convert.ToString(row["_ReturnUrl"]),
                                Name= row["_Name"] is DBNull ? "" : Convert.ToString(row["_Name"]),
                                MobileNo= row["_MobileNo"] is DBNull ? "" : Convert.ToString(row["_MobileNo"])

                        };
                        if (data.Status == 2)
                            data.CurrentStatus = (currentdate >= Convert.ToDateTime(row["_StartDate"]) && currentdate <= Convert.ToDateTime(row["_EndDate"])) ? "Active" : currentdate >= Convert.ToDateTime(row["_EndDate"]) ? "Expired" : "";
                        

                      
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
                    LoginTypeID = req.LT,
                    UserId = req.UserID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
           
                return res;
            



        }

        public object Call() => throw new NotImplementedException();

        public string GetName() => "proc_GetAdvertisementRequest";



    }

    public class proc_GetAdvertisementPackage : IProcedure
    {
        private readonly IDAL _dal;
        public proc_GetAdvertisementPackage(IDAL dal) => _dal = dal;

        public object Call()
        {
            
            List<AdvertisementPackage> res = new List<AdvertisementPackage> { };

            try
            {
                DataTable dt = _dal.Get(GetName());
                if (dt.Rows.Count > 0)
                {
                    
                    foreach (DataRow row in dt.Rows)
                    {
                        var data = new AdvertisementPackage
                        {
                            ID = row["_ID"] is DBNull ? 0 : Convert.ToInt32(row["_ID"]),
                            PackageName = row["_PackageName"] is DBNull ? "" : Convert.ToString(row["_PackageName"]),
                            PackageCost = row["_PackageCost"] is DBNull ? "" : Convert.ToString(row["_PackageCost"]),
                            PackageValidity = row["_PackageValidity"] is DBNull ? 0 : Convert.ToInt32(row["_PackageValidity"]),
                            IsActive = row["_IsActive"] is DBNull ?false : Convert.ToBoolean(row["_IsActive"]),
                            

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
                    LoginTypeID = 0,
                    UserId = 0
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }

            return res;




        }

        public object Call(object o) => throw new NotImplementedException();

        public string GetName() => "select * from tbl_Advertisement_Package";



    }



    public class proc_ApproveAdvertisementRequest : IProcedure
    {
        private readonly IDAL _dal;
        public proc_ApproveAdvertisementRequest(IDAL dal) => _dal = dal;

        public object Call(object obj)

        {
            var req = (AdvertisementRequest)obj;
            SqlParameter[] param = {
                new SqlParameter("@Id",req.Id),
                new SqlParameter("@UserId",req.UserID),
                new SqlParameter("@Status",req.Status)
            };

            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };


            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    // _ID _PackageId  _UserID _Status _Remark _ContentText    _ContentImage _EntryDate  _ModifyDate _EntryBy    _ModifyBy _PackageName
                    {
                        res.Statuscode = Convert.ToInt32(dt.Rows[0][0]);
                        res.Msg = dt.Rows[0]["Msg"] is DBNull ? "" : dt.Rows[0]["Msg"].ToString();
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
                    LoginTypeID = req.LT,
                    UserId = req.UserID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }

            return res;




        }

        public object Call() => throw new NotImplementedException();

        public string GetName() => "proc_ApproveAdvertisementRequest";



    }



    public class proc_GetAdvertisementRequestforB2C : IProcedure
    {
        private readonly IDAL _dal;
        public proc_GetAdvertisementRequestforB2C(IDAL dal) => _dal = dal;

        public object Call()
        {
            
            
            List<AdvertisementRequest> res = new List<AdvertisementRequest> { };

            try
            {
                var currentdate = DateTime.Now;
                DataTable dt = _dal.Get(GetName());
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow row in dt.Rows)
                    {


                        var data = new AdvertisementRequest
                        {
                            //Id = row["_ID"] is DBNull ? 0 : Convert.ToInt32(row["_ID"]),
                            //PackageName = row["_PackageName"] is DBNull ? "" : Convert.ToString(row["_PackageName"]),
                            //UserID = row["_UserID"] is DBNull ? 0 : Convert.ToInt32(row["_UserID"]),
                            //Status = row["_Status"] is DBNull ? 0 : Convert.ToInt32(row["_Status"]),
                            ContentText = row["_ContentText"] is DBNull ? "" : Convert.ToString(row["_ContentText"]),
                            ContentImage = row["_ContentImage"] is DBNull ? "" : Convert.ToString(row["_ContentImage"]),
                            StartDate = row["_StartDate"] is DBNull ? "" : Convert.ToDateTime(row["_StartDate"]).ToString("dd MMM yyyy"),
                            EndDate = row["_EndDate"] is DBNull ? "" : Convert.ToDateTime(row["_EndDate"]).ToString("dd MMM yyyy"),
                            //Type = row["_Type"] is DBNull ? "" : Convert.ToString(row["_Type"]),
                            ReturnUrl = row["_ReturnUrl"] is DBNull ? "" : Convert.ToString(row["_ReturnUrl"]),

                        };
                        if (data.Status == 2)
                            data.CurrentStatus = (currentdate >= Convert.ToDateTime(row["_StartDate"]) && currentdate <= Convert.ToDateTime(row["_EndDate"])) ? "Active" : currentdate >= Convert.ToDateTime(row["_EndDate"]) ? "Expired" : "";



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
                    LoginTypeID = 1,
                    UserId = 1
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }

            return res;




        }

        public object Call( object obj) => throw new NotImplementedException();

        public string GetName() => "select * from tbl_AdvertisementRequest where _Status=2 and Getdate() BETWEEN _StartDate and _EndDate";



    }

}
