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
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetPESDetails : IProcedureAsync
    {
        private readonly IDAL _dal;
        public ProcGetPESDetails(IDAL dal) => _dal = dal;

        public async Task<object> Call(object obj)
        {
            CommonReq req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT", req.LoginTypeID),
                new SqlParameter("@LoginID", req.LoginID),
                new SqlParameter("@TID", req.CommonInt)
            };
            var list = new List<PESReportViewModel>();
            
            try
            {
                DataTable dt = await _dal.GetByProcedureAsync(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        list.Add(new PESReportViewModel
                        {
                            _ID = dt.Rows[i]["_ID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[i]["_ID"]),
                            _OID = dt.Rows[i]["_OID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[i]["_OID"]),
                            Opname = dt.Rows[i]["Opname"] is DBNull ? "" :dt.Rows[i]["Opname"].ToString(),
                            _FieldID = dt.Rows[i]["_FieldID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[i]["_FieldID"]),
                            FieldName = dt.Rows[i]["FieldName"] is DBNull ? "" : dt.Rows[i]["FieldName"].ToString(),
                            _FieldValue = dt.Rows[i]["_FieldValue"] is DBNull ? "" : dt.Rows[i]["_FieldValue"].ToString(),
                            _FieldLabel = dt.Rows[i]["_FieldLabel"] is DBNull ? "" : dt.Rows[i]["_FieldLabel"].ToString(),
                            _FieldType = dt.Rows[i]["_FieldType"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[i]["_FieldType"]),
                            _InputType = dt.Rows[i]["_InputType"] is DBNull ? "" : dt.Rows[i]["_InputType"].ToString(),
                            _Remark = dt.Rows[i]["_Remark"] is DBNull ? "" : dt.Rows[i]["_Remark"].ToString(),
                            _EntryDate = dt.Rows[i]["_EntryDate"] is DBNull ? "" : dt.Rows[i]["_EntryDate"].ToString(),
                            _EntryBy = dt.Rows[i]["_EntryBy"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[i]["_EntryBy"]),
                            _TID = dt.Rows[i]["_TID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[i]["_TID"]),
                            _IsRequired = dt.Rows[i]["_IsRequired"] is DBNull ? false : Convert.ToBoolean(dt.Rows[i]["_IsRequired"]),
                            _Customername = dt.Rows[i]["_Customername"] is DBNull ? "" : dt.Rows[i]["_Customername"].ToString(),
                            _CustomerMobno = dt.Rows[i]["_ID"] is DBNull ? "" : dt.Rows[i]["_CustomerMobno"].ToString(),
                            _Amount = dt.Rows[i]["_Amount"] is DBNull ? 0 : Convert.ToDecimal(dt.Rows[i]["_Amount"])
                        });
                    }
                }
            }
            catch (Exception ex)
            { }
            return list;
        }

        public Task<object> Call()
        {
            throw new NotImplementedException();
        }

        public string GetName() => "sp_GetPESDetails";
    }
}
