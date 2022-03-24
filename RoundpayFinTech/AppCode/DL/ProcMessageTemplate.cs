using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcMessageTemplate : IProcedure
    {
        private readonly IDAL _dal;
        public ProcMessageTemplate(IDAL dal)
        {
            _dal = dal;
        }
        public object Call(object obj)
        {
            int FormatID = (int)obj;
            SqlParameter[] param = {
                new SqlParameter("@FormatID",  FormatID)
            };
            List<MessageTemplateKeyword> MsgTemp = new List<MessageTemplateKeyword>();
            DataTable dt = _dal.Get(GetName(), param);
            if (dt.Rows.Count > 0)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    MessageTemplateKeyword Temp = new MessageTemplateKeyword()
                    {
                        ID = Convert.ToInt32(dr["_ID"]),
                        Name = dr["_Name"].ToString(),
                        Keyword = dr["_Keyword"].ToString()
                    };
                    MsgTemp.Add(Temp);
                }
            }
            return MsgTemp;
        }
        public object Call()
        {
            throw new NotImplementedException();
        }
        public string GetName() => "select distinct k.* from MESSAGE_TEMPLATE_KEYWORDS k,tbl_MessageReplacementKeyMap map where k._ID=map._KeyWordID and (map._FormatID=@FormatID or @FormatID = 0) and map._IsActive=1";
    }
}
