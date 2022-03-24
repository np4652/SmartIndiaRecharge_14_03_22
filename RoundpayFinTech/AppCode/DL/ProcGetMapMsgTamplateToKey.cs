using Fintech.AppCode.DB;
using Fintech.AppCode.HelperClass;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.DepartmentModel;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetMapMsgTamplateToKey : IProcedureAsync
    {
        private readonly IDAL _dal;
        public ProcGetMapMsgTamplateToKey(IDAL dal) => _dal = dal;

        public async Task<object> Call(object obj)
        {
            string DevKey = (string)obj;
            SqlParameter[] param = {
                new SqlParameter("@DevKey", HashEncryption.O.DevEncrypt(DevKey??""))
            };
            var __res = new TemplateFormatKeyMapping();
            try
            {
                var _Formats = new List<MasterMessage>();
                var _Keywords = new List<MessageTemplateKeyword>();
                DataSet ds = await _dal.GetByProcedureAdapterDSAsync(GetName(), param);
                {
                    if (ds.Tables.Count > 0)
                    {
                        DataTable dtMessage = ds.Tables[0];
                        DataTable dtKeywords = ds.Tables[1];
                        if(dtMessage!=null && dtMessage.Rows.Count > 0)
                        {
                            foreach(DataRow dr in dtMessage.Rows)
                            {
                                var _f = new MasterMessage()
                                {
                                    ID = Convert.ToInt32(dr["_ID"]),
                                    FormatType=Convert.ToString(dr["_FormatType"])
                                };
                                _Formats.Add(_f);
                            }
                        }
                        if (dtKeywords != null && dtKeywords.Rows.Count > 0)
                        {
                            foreach (DataRow dr in dtKeywords.Rows)
                            {
                                var _Key = new MessageTemplateKeyword()
                                {
                                    ID = Convert.ToInt32(dr["_ID"]),
                                    Name = Convert.ToString(dr["_Name"]),
                                    Keyword = Convert.ToString(dr["_Keyword"]),
                                    FormatID = Convert.ToInt32(dr["_FormatID"]),
                                    IsActive = dr["_IsActive"] is DBNull ? false : Convert.ToBoolean(dr["_IsActive"])
                                };
                                _Keywords.Add(_Key);
                            }
                        }
                        __res.Messgaes = _Formats;
                        __res.Keyswords = _Keywords;
                    }
                }
            }
            catch (Exception ex)
            {
                
            }
            return __res;
        }

        public Task<object> Call() => throw new NotImplementedException();
        public string GetName()=>"GetMapMsgTamplateToKey";
    }
}
