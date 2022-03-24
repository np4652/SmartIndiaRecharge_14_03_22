using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetWhatsappBotDicList : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetWhatsappBotDicList(IDAL dal) => _dal = dal;

        public object Call(object obj)
        {
            CommonReq req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@FormatType", req.CommonStr??string.Empty)
            };
            var response = new List<WhatsappBotDic>();
            try
            {
                var dt = _dal.Get(GetName(), param);
                if (dt != null && dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        response.Add(new WhatsappBotDic
                        {
                            KeyId = dr["_Id"] is DBNull ? 0 : Convert.ToInt32(dr["_Id"]),
                            Key = dr["_Key"] is DBNull ? string.Empty : Convert.ToString(dr["_Key"]),
                            FormatType = dr["_FormatType"] is DBNull ? string.Empty : Convert.ToString(dr["_FormatType"]),
                            ReplyText1 = dr["_ReplyText1"] is DBNull ? string.Empty : Convert.ToString(dr["_ReplyText1"]),
                            ReplyText2 = dr["_ReplyText2"] is DBNull ? string.Empty : Convert.ToString(dr["_ReplyText2"]),
                            ReplyText3 = dr["_ReplyText3"] is DBNull ? string.Empty : Convert.ToString(dr["_ReplyText3"]),
                            ReplyType = dr["_ReplyType"] is DBNull ? string.Empty : Convert.ToString(dr["_ReplyType"]),
                            IsActive = dr["_IsActive"] is DBNull ? false : Convert.ToBoolean(dr["_IsActive"]),
                        });
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
            return response;
        }


        public object Call() => throw new NotImplementedException();
        public string GetName() => "select * from MASTER_WhatsappBotDic(nolock) where _FormatType=@FormatType or ISNULL(@FormatType,'')=''";
    }
}
