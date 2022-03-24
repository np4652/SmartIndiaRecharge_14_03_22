using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.Model.Shopping;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Text;

namespace RoundpayFinTech.AppCode.DL.Shopping
{
    public class ProcGetFEImg : IProcedure
    {
        private readonly IDAL _dal;

        public ProcGetFEImg(IDAL dal) => _dal = dal;

        public object Call(object obj)
        {
            var res = new FEImgList
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError,
                ImgList = new List<FEImage>()
            };
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT", req.LoginTypeID),
                new SqlParameter("@LoginID", req.LoginID),
                new SqlParameter("@ID", req.CommonInt),
                new SqlParameter("@CatId", req.CommonInt2),
                new SqlParameter("@IsActive", req.CommonBool)
            };
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    res.Statuscode = ErrorCodes.One;
                    res.Msg = ErrorCodes.SUCCESS;
                    foreach (DataRow dr in dt.Rows)
                    {
                        var listItem = new FEImage
                        {
                            ID = Convert.ToInt32(dr["_ID"], CultureInfo.InvariantCulture),
                            CategoryID = Convert.ToInt32(dr["_CategoryID"], CultureInfo.InvariantCulture),
                            ImgType = (EFEImgType)Convert.ToByte(dr["_ImgType"], CultureInfo.InvariantCulture),
                            ImgTypeID = Convert.ToByte(dr["_ImgType"], CultureInfo.InvariantCulture),
                            FileName = Convert.ToString(dr["_FileName"], CultureInfo.InvariantCulture),
                            IsActive = Convert.ToBoolean(dr["_IsActive"], CultureInfo.InvariantCulture)
                        };
                        StringBuilder sb = new StringBuilder(DOCType.ECommFEImagePath);
                        sb.Append(listItem.FileName);
                        //sb.Append(((byte)listItem.ImgType).ToString());

                        //sb.Append("_");
                        //sb.Append(listItem.ID.ToString());
                        //sb.Append(".*");
                        string path = sb.ToString();
                        if (File.Exists(path))
                        {
                            listItem.FilePath = path;
                        }
                        res.ImgList.Add(listItem);
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
            return res;
        }

        public object Call() => throw new NotImplementedException();
        public string GetName() => "Proc_GetFEImages";
    }

    public class ProcUpdateFEImg : IProcedure
    {
        private readonly IDAL _dal;

        public ProcUpdateFEImg(IDAL dal) => _dal = dal;

        public object Call(object obj)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT", req.LoginTypeID),
                new SqlParameter("@LoginID", req.LoginID),
                new SqlParameter("@ID", req.CommonInt),
                new SqlParameter("@CatID", req.CommonInt2),
                new SqlParameter("@FileName", req.CommonStr),
                new SqlParameter("@ImgType", (byte)req.CommonInt3),
                new SqlParameter("@IsActive", req.CommonBool)
            };
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    res.Statuscode = Convert.ToInt32(dt.Rows[0][0], CultureInfo.InvariantCulture);
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
        public string GetName() => "Proc_UpdateFEImage";
    }

    public class ProcUpdateFEImgStatus : IProcedure
    {
        private readonly IDAL _dal;

        public ProcUpdateFEImgStatus(IDAL dal) => _dal = dal;

        public object Call(object obj)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT", req.LoginTypeID),
                new SqlParameter("@LoginID", req.LoginID),
                new SqlParameter("@ID", req.CommonInt),
                new SqlParameter("@IsActive", req.CommonBool),
                new SqlParameter("@IsDelete", req.CommonBool1)
            };
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    res.Statuscode = Convert.ToInt32(dt.Rows[0][0], CultureInfo.InvariantCulture);
                    res.Msg = dt.Rows[0]["Msg"] is DBNull ? "" : dt.Rows[0]["Msg"].ToString();
                }
                if(res.Statuscode == ErrorCodes.One && req.CommonBool1 == true)
                {
                    StringBuilder sb = new StringBuilder(DOCType.ECommFEImagePath);
                    sb.Append(dt.Rows[0]["_FileName"].ToString());
                    string path = sb.ToString();
                    if (File.Exists(path))
                    {
                        File.Delete(path);
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
            return res;
        }

        public object Call() => throw new NotImplementedException();
        public string GetName() => "Proc_UpdateFEImageStatus";
    }
}
