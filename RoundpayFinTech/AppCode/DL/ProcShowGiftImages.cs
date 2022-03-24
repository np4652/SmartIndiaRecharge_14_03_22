using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.DL;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcShowGiftImages : IProcedure
    {
        private readonly IDAL _dal;
        public ProcShowGiftImages(IDAL dal) => _dal = dal;
        public string GetName() => "Proc_ShowGiftImages";
        public object Call(object obj)
        {
            var _req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT",_req.LoginTypeID),
                new SqlParameter("@LoginID",_req.LoginID)
            };
            var resList = new List<TargetModel>();
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        var res = new TargetModel
                        {
                            ServiceName = dr["ServiceName"] is DBNull ? "" : dr["ServiceName"].ToString(),
                            RoleID = dr["RoleID"] is DBNull ? 0 : Convert.ToInt32(dr["RoleID"]),
                            OID = dr["ServiceID"] is DBNull ? 0 : Convert.ToInt32(dr["ServiceID"]),//in table it's _OID
                            SlabID = dr["SlabID"] is DBNull ? 0 : Convert.ToInt32(dr["SlabID"])
                        };

                        string[] ext = { ".png", ".jpg", ".jpeg" };
                        foreach (string s in ext)
                        {
                            string fileName = "Gift_" + res.RoleID + "_" + res.OID + "_" + res.SlabID + s;
                            string file = DOCType.GiftImgPath + fileName;
                            if (File.Exists(file))
                            {
                                res.ImgaePath = "/Image/GiftImage/" + fileName;
                                break;
                            }
                        }

                        resList.Add(res);

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
                    LoginTypeID = _req.LoginTypeID,
                    UserId = _req.LoginID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return resList;
        }

        public object Call() => throw new NotImplementedException();


    }
}
