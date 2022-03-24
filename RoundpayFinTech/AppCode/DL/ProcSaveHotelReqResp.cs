using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.ThirdParty.HotelAPI;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
        public class ProcSaveHotelReqResp : IProcedure
        {
            private readonly IDAL _dal;
            public ProcSaveHotelReqResp(IDAL dal) => _dal = dal;
            public object Call(object obj)
            {
                var res = new TekTvlError
                {
                      ErrorCode = ErrorCodes.Minus1,
                     ErrorMessage = ErrorCodes.TempError
                };
                var req = (HotelApiReqRes)obj;
                SqlParameter[] param = {
                  new SqlParameter("@ReqURL",req.ReqUrl),
                new SqlParameter("@Request",req.Request),
                new SqlParameter("@Response",(req.Response)),
                new SqlParameter("@ClassName",req.ClassName),
                new SqlParameter("@Method",req.Method),
                new SqlParameter("@EndUserIP",req.EndUserIP),
                new SqlParameter("@UserID",req.UserID)
            };
                try
                {
                    var dt = _dal.GetByProcedure(GetName(), param);
                    if (dt.Rows.Count > 0)
                    {
                    res.ErrorCode = 1;
                    res.ErrorMessage = "Message";
                }
                }
                catch (Exception ex)
                {
                    var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                    {
                        ClassName = GetType().Name,
                        FuncName = "Call",
                        Error = ex.Message
                        // LoginTypeID = req.LoginTypeID,
                        // UserId = req.LoginTypeID
                    });
                }
                return res;
            }

            public object Call() => throw new NotImplementedException();


            public string GetName() => "proc_SaveHotelReqResp";
        }

 
}
