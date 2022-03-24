using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
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
    public class ProcGetActiveOperators : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetActiveOperators(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LoginID",req.LoginID),
                new SqlParameter("@OpTypeID",req.CommonInt)
            };
            var res = new List<OperatorDetail>();
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        var operatorDetail = new OperatorDetail
                        {
                            OID = row["_ID"] is DBNull ? 0 : Convert.ToInt32(row["_ID"]),
                            Name = row["_Name"] is DBNull ? "" : row["_Name"].ToString(),
                            OPID = row["_OPID"] is DBNull ? "" : row["_OPID"].ToString(),
                            OpType = row["_Type"] is DBNull ? 0 : Convert.ToInt32(row["_Type"]),
                            Length = row["_Length"] is DBNull ? 0 : Convert.ToInt32(row["_Length"]),
                            LengthMax = row["_LengthMax"] is DBNull ? 0 : Convert.ToInt32(row["_LengthMax"]),
                            Min = row["_Min"] is DBNull ? 0 : Convert.ToDecimal(row["_Min"]),
                            Max = row["_Max"] is DBNull ? 0 : Convert.ToDecimal(row["_Max"]),
                            HSNCode = row["_HSNCode"] is DBNull ? "" : row["_HSNCode"].ToString(),
                            StartWith = row["_StartWith"] is DBNull ? "" : row["_StartWith"].ToString(),
                            BusinessModel = row["_BusinessModel"] is DBNull ? "" : row["_BusinessModel"].ToString(),
                            IsActive = row["_IsActive"] is DBNull ? false : Convert.ToBoolean(row["_IsActive"]),
                            IsBBPS = row["_IsBBPS"] is DBNull ? false : Convert.ToBoolean(row["_IsBBPS"]),
                            IsBilling = row["_IsBilling"] is DBNull ? false : Convert.ToBoolean(row["_IsBilling"]),
                            IsPartial = row["_IsPartial"] is DBNull ? false : Convert.ToBoolean(row["_IsPartial"]),
                            CircleValidationType = row["_CircleValidationType"] is DBNull ? 0 : Convert.ToInt32(row["_CircleValidationType"]),
                            OperatorType = row["OpType"] is DBNull ? "" : row["OpType"].ToString(),
                            ModifyDate = row["_ModifyDate"] is DBNull ? "" : row["_ModifyDate"].ToString(),
                            Image = (row["_ID"] is DBNull ? 0 : Convert.ToInt32(row["_ID"])) + ".png",
                            AccountName = row["_AccountName"] is DBNull ? "" : row["_AccountName"].ToString(),
                            AccountRemak = row["_AccountRemak"] is DBNull ? "" : row["_AccountRemak"].ToString(),
                            IsGroupLeader = row["_IsGroupLeader"] is DBNull ? false : Convert.ToBoolean(row["_IsGroupLeader"]),
                            IsAccountNumeric = row["_IsAccountNumeric"] is DBNull ? false : Convert.ToBoolean(row["_IsAccountNumeric"]),
                            CommSettingType = row["_CommSettingType"] is DBNull ? 0 : Convert.ToInt32(row["_CommSettingType"]),
                            TollFree = row["_TollFree"] is DBNull ? "" : row["_TollFree"].ToString(),
                            Charge = row["_Charge"] is DBNull ? 0 : Convert.ToDecimal(row["_Charge"]),
                            ChargeAmtType = row["_ChargeAmtType"] is DBNull ? false : Convert.ToBoolean(row["_ChargeAmtType"]),
                            StateID = row["_StateID"] is DBNull ? 0 : Convert.ToInt32(row["_StateID"]),
                            AllowedChannel = row["_AllowedChannel"] is DBNull ? 0 : Convert.ToInt16(row["_AllowedChannel"])
                        };
                        string[] ext = { ".png", ".jpg", ".jpeg", ".pdf" };
                        foreach (string s in ext)
                        {
                            string fileName = operatorDetail.OID + s;
                            string file = DOCType.OperatorPdfFile + fileName;
                            if (File.Exists(file))
                            {
                                operatorDetail.PlanDocName = fileName;
                                break;
                            }
                        }
                        res.Add(operatorDetail);
                    }
                }
            }
            catch (Exception ex)
            {
            }
            return res;
        }

        public object Call()
        {
            throw new NotImplementedException();
        }

        public string GetName() => "proc_GetActiveOperators";
    }
}
