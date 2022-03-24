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
using System.IO;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetOperator : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetOperator(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@OID",req.CommonInt),
                new SqlParameter("@Type",req.CommonInt2),
                new SqlParameter("@LoginID",req.LoginID)
            };
            var res = new List<OperatorDetail>();
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    if (req.CommonInt == 0 || req.CommonInt2 > 0)
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
                                InSlab = row["_InSlab"] is DBNull ? false : Convert.ToBoolean(row["_InSlab"]),
                                IsTakeCustomerNum = row["_IsTakeCustomerNum"] is DBNull ? false : Convert.ToBoolean(row["_IsTakeCustomerNum"]),
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
                                BillerID = row["_BillerID"] is DBNull ? "" : row["_BillerID"].ToString(),
                                Charge = row["_Charge"] is DBNull ? 0 : Convert.ToDecimal(row["_Charge"]),
                                ChargeAmtType = row["_ChargeAmtType"] is DBNull ? false : Convert.ToBoolean(row["_ChargeAmtType"]),
                                StateID = row["_StateID"] is DBNull ? 0 : Convert.ToInt32(row["_StateID"]),
                                Ind = row["_Ind"] is DBNull ? 0 : Convert.ToInt32(row["_Ind"]),
                                AccountNoKey = row["_AccountNoKey"] is DBNull ? string.Empty : row["_AccountNoKey"].ToString(),
                                RegExAccount = row["_RegExAccount"] is DBNull ? string.Empty : row["_RegExAccount"].ToString(),
                                CustomerNoKey = row["_CustomerNoKey"] is DBNull ? string.Empty : row["_CustomerNoKey"].ToString(),
                                IsAmountValidation = row["_IsAmountValidation"] is DBNull ? false : Convert.ToBoolean(row["_IsAmountValidation"]),
                                AllowedChannel = row["_AllowedChannel"] is DBNull ? 3 : Convert.ToInt16(row["_AllowedChannel"]),
                                PlanOID = row["_PlanOID"] is DBNull ? 0 : Convert.ToInt16(row["_PlanOID"]),
                                RofferOID = row["_RofferOID"] is DBNull ? 0 : Convert.ToInt16(row["_RofferOID"]),
                                DTHCustInfoOID = row["_DTHCustInfoOID"] is DBNull ? 0 : Convert.ToInt16(row["_DTHCustInfoOID"]),
                                DTHHREFOID = row["_DTHHREFOID"] is DBNull ? 0 : Convert.ToInt16(row["_DTHHREFOID"]),
                                ExactNessID = row["_ExactNess"] is DBNull ? 0 : Convert.ToInt16(row["_ExactNess"]),
                                ExactNess = row["_ExactNessName"] is DBNull ? "" : row["_ExactNessName"].ToString(),
                                ServiceID= row["_ServiceTypeID"] is DBNull ? 0 : Convert.ToInt32(row["_ServiceTypeID"])

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
                    else
                    {
                        return new OperatorDetail
                        {
                            OID = dt.Rows[0]["_ID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_ID"]),
                            Name = dt.Rows[0]["_Name"] is DBNull ? "" : dt.Rows[0]["_Name"].ToString(),
                            OPID = dt.Rows[0]["_OPID"] is DBNull ? "" : dt.Rows[0]["_OPID"].ToString(),
                            OpType = dt.Rows[0]["_Type"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_Type"]),
                            Length = dt.Rows[0]["_Length"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_Length"]),
                            LengthMax = dt.Rows[0]["_LengthMax"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_LengthMax"]),
                            Min = dt.Rows[0]["_Min"] is DBNull ? 0 : Convert.ToDecimal(dt.Rows[0]["_Min"]),
                            Max = dt.Rows[0]["_Max"] is DBNull ? 0 : Convert.ToDecimal(dt.Rows[0]["_Max"]),
                            HSNCode = dt.Rows[0]["_HSNCode"] is DBNull ? "" : dt.Rows[0]["_HSNCode"].ToString(),
                            StartWith = dt.Rows[0]["_StartWith"] is DBNull ? "" : dt.Rows[0]["_StartWith"].ToString(),
                            BusinessModel = dt.Rows[0]["_BusinessModel"] is DBNull ? "" : dt.Rows[0]["_BusinessModel"].ToString(),
                            Image = (dt.Rows[0]["_ID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_ID"])) + ".png",
                            IsActive = dt.Rows[0]["_IsActive"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_IsActive"]),
                            IsBBPS = dt.Rows[0]["_IsBBPS"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_IsBBPS"]),
                            IsBilling = dt.Rows[0]["_IsBilling"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_IsBilling"]),
                            InSlab = dt.Rows[0]["_InSlab"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_InSlab"]),
                            IsPartial = dt.Rows[0]["_IsPartial"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_IsPartial"]),
                            IsTakeCustomerNum = dt.Rows[0]["_IsTakeCustomerNum"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_IsTakeCustomerNum"]),
                            CircleValidationType = dt.Rows[0]["_CircleValidationType"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_CircleValidationType"]),
                            OperatorType = dt.Rows[0]["OpType"] is DBNull ? "" : dt.Rows[0]["OpType"].ToString(),
                            ModifyDate = dt.Rows[0]["_ModifyDate"] is DBNull ? "" : dt.Rows[0]["_ModifyDate"].ToString(),
                            AccountName = dt.Rows[0]["_AccountName"] is DBNull ? "" : dt.Rows[0]["_AccountName"].ToString(),
                            AccountRemak = dt.Rows[0]["_AccountRemak"] is DBNull ? "" : dt.Rows[0]["_AccountRemak"].ToString(),
                            IsAccountNumeric = dt.Rows[0]["_IsAccountNumeric"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_IsAccountNumeric"]),
                            CommSettingType = dt.Rows[0]["_CommSettingType"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_CommSettingType"]),
                            TollFree = dt.Rows[0]["_TollFree"] is DBNull ? "" : dt.Rows[0]["_TollFree"].ToString(),
                            BillerID = dt.Rows[0]["_BillerID"] is DBNull ? "" : dt.Rows[0]["_BillerID"].ToString(),
                            Charge = dt.Rows[0]["_Charge"] is DBNull ? 0 : Convert.ToDecimal(dt.Rows[0]["_Charge"]),
                            ChargeAmtType = dt.Rows[0]["_ChargeAmtType"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_ChargeAmtType"]),
                            StateID = dt.Rows[0]["_StateID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_StateID"]),
                            Ind = dt.Rows[0]["_Ind"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_Ind"]),
                            AccountNoKey = dt.Rows[0]["_AccountNoKey"] is DBNull ? string.Empty : dt.Rows[0]["_AccountNoKey"].ToString(),
                            RegExAccount = dt.Rows[0]["_RegExAccount"] is DBNull ? string.Empty : dt.Rows[0]["_RegExAccount"].ToString(),
                            IsAmountValidation = dt.Rows[0]["_IsAmountValidation"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_IsAmountValidation"]),
                            AllowedChannel = dt.Rows[0]["_AllowedChannel"] is DBNull ? 3 : Convert.ToInt16(dt.Rows[0]["_AllowedChannel"]),
                            PlanOID = dt.Rows[0]["_PlanOID"] is DBNull ? 0 : Convert.ToInt16(dt.Rows[0]["_PlanOID"]),
                            RofferOID = dt.Rows[0]["_RofferOID"] is DBNull ? 0 : Convert.ToInt16(dt.Rows[0]["_RofferOID"]),
                            DTHCustInfoOID = dt.Rows[0]["_DTHCustInfoOID"] is DBNull ? 0 : Convert.ToInt16(dt.Rows[0]["_DTHCustInfoOID"]),
                            DTHHREFOID = dt.Rows[0]["_DTHHREFOID"] is DBNull ? 0 : Convert.ToInt16(dt.Rows[0]["_DTHHREFOID"]),
                            ExactNessID = dt.Rows[0]["_ExactNess"] is DBNull ? 0 : Convert.ToInt16(dt.Rows[0]["_ExactNess"]),
                            ExactNess = dt.Rows[0]["_ExactNessName"] is DBNull ? "" : dt.Rows[0]["_ExactNessName"].ToString(),
                            ServiceID = dt.Rows[0]["_ServiceTypeID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_ServiceTypeID"])
                        };
                    }
                }
            }
            catch (Exception ex)
            {
            }
            if (req.CommonInt == 0 || req.CommonInt2 > 0)
                return res;
            else
                return new OperatorDetail();
        }

        public object Call() => throw new NotImplementedException();

        public string GetName() => "proc_GetOperator";
        public IEnumerable<OpTypeMaster> OpTypeMasters(int ServiceID)
        {
            SqlParameter[] param = {
                new SqlParameter("@ServiceID",ServiceID)
            };
            var opTypes = new List<OpTypeMaster>();
            DataTable dt = _dal.Get("select * from MASTER_OPTYPE where (_ServiceTypeID=@ServiceID or @ServiceID=0)  and  _InSlab=1  and  _ServiceTypeID<>-1 order by _Ind,_OpType", param);
            if (dt.Rows.Count > 0)
            {
                try
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        var opType = new OpTypeMaster
                        {
                            ID = Convert.ToInt32(dt.Rows[i]["_ID"]),
                            OpType = dt.Rows[i]["_OpType"].ToString(),
                            Remark = dt.Rows[i]["_Remark"].ToString(),
                            IsB2CVisible = dt.Rows[i]["_IsB2CVisible"] is DBNull ? false : Convert.ToBoolean(dt.Rows[i]["_IsB2CVisible"]),
                            ServiceTypeID = Convert.ToInt32(dt.Rows[i]["_ServiceTypeID"] is DBNull ? 0 : dt.Rows[i]["_ServiceTypeID"])
                        };
                        opTypes.Add(opType);
                    }
                }
                catch (Exception)
                {
                }
            }
            return opTypes;
        }
        public IEnumerable<OpTypeMaster> APIOpTypeMasters(string APICode)
        {
            SqlParameter[] param = {
                new SqlParameter("@APICode",APICode)
            };
            var opTypes = new List<OpTypeMaster>();
            DataTable dt = _dal.Get("select a._OpTypeID, mo._OpType, a._APIOpType from tbl_APIOpType a inner join MASTER_OPTYPE mo on mo._ID = a._OpTypeID where a._APICode=@APICode order by mo._OpType", param);
            if (dt.Rows.Count > 0)
            {
                try
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        var opType = new OpTypeMaster
                        {
                            ID = dt.Rows[i]["_OpTypeID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[i]["_OpTypeID"]),
                            OpType = dt.Rows[i]["_OpType"] is DBNull ? string.Empty : dt.Rows[i]["_OpType"].ToString(),
                            APIOpType = dt.Rows[i]["_APIOpType"] is DBNull ? string.Empty : dt.Rows[i]["_APIOpType"].ToString()
                        };
                        opTypes.Add(opType);
                    }
                }
                catch (Exception)
                {
                }
            }
            return opTypes;
        }
        public IEnumerable<OpTypeMaster> OpTypesInSlab()
        {
            var opTypes = new List<OpTypeMaster>();
            DataTable dt = _dal.Get("select Op.* from MASTER_OPTYPE Op,Master_Service S where Op._ServiceTypeID=S._ID And Op._InSlab=1 And S._IsActive=1 order by _OpType ");
            if (dt.Rows.Count > 0)
            {
                try
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        OpTypeMaster opType = new OpTypeMaster
                        {
                            ID = Convert.ToInt32(dt.Rows[i]["_ID"]),
                            OpType = dt.Rows[i]["_OpType"].ToString(),
                            Remark = dt.Rows[i]["_Remark"].ToString(),
                            ServiceTypeID = Convert.ToInt32(dt.Rows[i]["_ServiceTypeID"] is DBNull ? 0 : dt.Rows[i]["_ServiceTypeID"])
                        };
                        opTypes.Add(opType);
                    }
                }
                catch (Exception)
                {
                }
            }
            return opTypes;
        }
        public IEnumerable<OpTypeMaster> OpTypesInRange()
        {
            var opTypes = new List<OpTypeMaster>();
            DataTable dt = _dal.Get("select distinct Op.* from MASTER_OPTYPE Op left outer join Master_Service S on Op._ServiceTypeID=S._ID left outer join tbl_Operator o on o._Type = Op._ID where Op._InSlab=1 And S._IsActive=1 and o._CommSettingType = 2");
            if (dt.Rows.Count > 0)
            {
                try
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        OpTypeMaster opType = new OpTypeMaster
                        {
                            ID = Convert.ToInt32(dt.Rows[i]["_ID"]),
                            OpType = dt.Rows[i]["_OpType"].ToString(),
                            Remark = dt.Rows[i]["_Remark"].ToString(),
                            IsB2CVisible = dt.Rows[i]["_IsB2CVisible"] is DBNull ? false : Convert.ToBoolean(dt.Rows[i]["_IsB2CVisible"]),
                            ServiceTypeID = Convert.ToInt32(dt.Rows[i]["_ServiceTypeID"] is DBNull ? 0 : dt.Rows[i]["_ServiceTypeID"])
                        };
                        opTypes.Add(opType);
                    }
                }
                catch (Exception)
                {
                }
            }
            return opTypes;
        }
        public string getMaxOpCode(int OpType)
        {
            SqlParameter[] param = {
                new SqlParameter("@Type",OpType)
            };
            string res = string.Empty;
            DataTable dt = _dal.Get("select Max(_OPID) from tbl_Operator where _Type=@Type", param);
            if (dt != null && dt.Rows.Count > 0)
            {
                res = Convert.ToString(dt.Rows[0][0]);
            }
            return res;
        }
        public IEnumerable<OpTypeMaster> OpTypesVendorMaster(int id)
        {
            var opTypes = new List<OpTypeMaster>();
            DataTable dt = _dal.Get("select Op.* from MASTER_OPTYPE Op,Master_Service S where Op._ServiceTypeID=S._ID And Op._InSlab=1 And S._IsActive=1 and Op._ID in(select Item from dbo.fn_SplitString((select _type from tbl_API where _Id = " + id.ToString() + "), char(160))) ");
            if (dt.Rows.Count > 0)
            {
                try
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        OpTypeMaster opType = new OpTypeMaster
                        {
                            ID = Convert.ToInt32(dt.Rows[i]["_ID"]),
                            OpType = dt.Rows[i]["_OpType"].ToString(),
                            Remark = dt.Rows[i]["_Remark"].ToString(),
                            ServiceTypeID = Convert.ToInt32(dt.Rows[i]["_ServiceTypeID"] is DBNull ? 0 : dt.Rows[i]["_ServiceTypeID"])
                        };
                        opTypes.Add(opType);
                    }
                }
                catch (Exception)
                {
                }
            }
            return opTypes;
        }
        public List<OperatorDetail> GetOperatorsByOpTypes(string OpTypes)
        {
            SqlParameter[] param = {
                new SqlParameter("@OpTypes",OpTypes??"1"),
            };
            var res = new List<OperatorDetail>();
            DataTable dt = _dal.Get("select  _ID, _Name, _OPID, _Type, _IsGroupLeader, _Length,_LengthMax, _Min, _Max, _HSNCode, _StartWith, _BusinessModel,_Image, _IsActive, _IsBBPS, _IsBilling, _EntryBy, _EntryDate, _ModifyBy, isnull(dbo.CustomFormat(_ModifyDate),'Not yet') _ModifyDate, _BackupAPIID, _CircleValidationType, _IsPartial,(select mo._OpType from MASTER_OPTYPE mo where mo._ID = _Type) OpType,_AccountName,_AccountRemak,_IsAccountNumeric,_SampleImage, _CommSettingType,_TollFree,_Charge,_ChargeAmtType,_StateID,_AllowedChannel from tbl_Operator where _Type in(select Item from dbo.fn_SplitString(@OpTypes,',')) order by _Type,_Name", param);
            if (dt.Rows.Count > 0)
            {
                try
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
                            AllowedChannel = row["_AllowedChannel"] is DBNull ? 3 : Convert.ToInt16(row["_AllowedChannel"])
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
                catch (Exception)
                {
                }
            }
            return res;
        }
        public List<IndustryTypeModelProc> GetIndustryWiseOpTypes()
        {
            List<IndustryTypeModelProc> res = new List<IndustryTypeModelProc>();
            string query = "select mi._ID,mi._Industry,mi._Ind,mi._Remark ,mo._ID _OpTypeID,mo._OpType from MASTER_INDUSTRY_TYPE mi inner join  MASTER_OPTYPE mo on mo._IndustryType=mi._ID order by mi._Ind ";
            try
            {
                DataTable dt = _dal.Get(query);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow item in dt.Rows)
                    {
                        res.Add(new IndustryTypeModelProc
                        {
                            ID = item["_ID"] is DBNull ? 0 : Convert.ToInt32(item["_ID"]),
                            IndustryType = item["_Industry"] is DBNull ? string.Empty : item["_Industry"].ToString(),
                            Ind = item["_Ind"] is DBNull ? 0 : Convert.ToInt32(item["_Ind"]),
                            Remark = item["_Remark"] is DBNull ? string.Empty : item["_Remark"].ToString(),
                            OpTypeID = item["_OpTypeID"] is DBNull ? 0 : Convert.ToInt32(item["_OpTypeID"]),
                            OpType = item["_OpType"] is DBNull ? string.Empty : item["_OpType"].ToString()
                        });
                    }
                }
            }
            catch (Exception ex)
            {
            }
            return res;
        }
    }

}
