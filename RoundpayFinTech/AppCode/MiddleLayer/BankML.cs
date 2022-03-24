using Fintech.AppCode.Configuration;
using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.Model.Reports;
using Fintech.AppCode.StaticModel;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using RoundpayFinTech.AppCode.DL;
using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Validators;

namespace RoundpayFinTech.AppCode.MiddleLayer
{
    public class BankML : IBankML
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly IHostingEnvironment _env;
        private readonly ISession _session;
        private readonly IDAL _dal;
        private readonly IConnectionConfiguration _c;
        private readonly LoginResponse _lr;
        private readonly IUserML userML;
        private readonly IRequestInfo _rinfo;
        private readonly LoginResponse _lrEmp;

        public BankML(IHttpContextAccessor accessor, IHostingEnvironment env, bool InSession = true)
        {
            _accessor = accessor;
            _env = env;
            _c = new ConnectionConfiguration(_accessor, _env);
            _dal = new DAL(_c.GetConnectionString());
            _rinfo = new RequestInfo(_accessor, _env);
            if (InSession)
            {
                _session = _accessor.HttpContext.Session;
                _lr = _session.GetObjectFromJson<LoginResponse>(SessionKeys.LoginResponse);
                userML = new UserML(_lr);
                _lrEmp = _session.GetObjectFromJson<LoginResponse>(SessionKeys.LoginResponseEmp);
            }
            bool IsProd = _env.IsProduction();


        }
        public List<BankMaster> BankMaster(int ID)
        {
            var bankmaster = new List<BankMaster>();
            if (_lr.LoginTypeID == LoginType.ApplicationUser && !userML.IsEndUser())
            {
                var commonReq = new CommonReq
                {
                    LoginID = _lr.UserID,
                    LoginTypeID = _lr.LoginTypeID,
                    CommonInt = ID
                };
                IProcedure _proc = new ProcGetBank(_dal);
                bankmaster = ((List<BankMaster>)_proc.Call(commonReq));
            }
            return bankmaster;
        }

        public List<BankMaster> BankMasters()
        {
            var bankMasters = new List<BankMaster>();
            if (_lr.LoginTypeID == LoginType.ApplicationUser)
            {
                var commonReq = new CommonReq
                {
                    LoginID = _lr.UserID,
                    LoginTypeID = _lr.LoginTypeID,
                    CommonInt = 0
                };
                IProcedure _proc = new ProcGetBankMaster(_dal);
                bankMasters = (List<BankMaster>)_proc.Call(commonReq);
            }
            return bankMasters;
        }
        public List<BankMaster> DMRBanks()
        {
            var bankMasters = new List<BankMaster>();
            CommonReq commonReq = new CommonReq();
            commonReq.LoginID = _lr != null ? _lr.UserID : _lrEmp.UserID;
            commonReq.LoginTypeID = _lr != null ? _lr.LoginTypeID : _lrEmp.LoginTypeID;
            commonReq.CommonInt = 0;
            IProcedure _proc = new ProcGetBankMaster(_dal);
            bankMasters = (List<BankMaster>)_proc.Call(commonReq);
            if (bankMasters != null && bankMasters.Count > 0)
            {
                bankMasters = bankMasters.Where(x => x.IsIMPS || x.IsNEFT).ToList();
            }
            return bankMasters;
        }
        public List<BankMaster> AEPSBankMasters()
        {
            IProcedure _proc = new ProcGetBankMaster(_dal);
            return (List<BankMaster>)_proc.Call();
        }

        public BankMaster BankMasters(int ID)
        {
            var bankMaster = new BankMaster();
            if (ID > 0)
            {
                var commonReq = new CommonReq
                {
                    LoginID = 1,
                    LoginTypeID = 1,
                    CommonInt = ID
                };
                IProcedure _proc = new ProcGetBankMaster(_dal);
                bankMaster = (BankMaster)_proc.Call(commonReq);
            }
            return bankMaster;
        }
        public IEnumerable<BankMaster> BankMastersApp(CommonReq commonReq)
        {
            var bankMasters = new List<BankMaster>();
            if (commonReq.LoginTypeID == LoginType.ApplicationUser)
            {
                IProcedure _proc = new ProcGetBankMaster(_dal);
                bankMasters = (List<BankMaster>)_proc.Call(commonReq);
                if (bankMasters != null && bankMasters.Count > 0)
                {
                    bankMasters = bankMasters.Where(x => x.IsNEFT || x.IsIMPS).ToList();
                }
            }
            return bankMasters;
        }
        public List<Bank> Banks(int UserID)
        {
            IProcedure _proc = new ProcGetSavedBank(_dal);
            return (List<Bank>)_proc.Call(new CommonReq
            {
                LoginID = _lr.UserID,
                LoginTypeID = _lr.LoginTypeID,
                CommonInt = UserID
            });
        }

        public IEnumerable<Bank> BanksForApp(CommonReq commonReq)
        {
            var bMList = new List<Bank>();
            //var commonReq = new CommonReq
            //{
            //    LoginID = _lr.UserID,
            //    LoginTypeID = _lr.LoginTypeID,
            //    CommonInt = UserID
            //};
            IProcedure _proc = new ProcGetSavedBank(_dal);
            bMList = (List<Bank>)_proc.Call(commonReq);
            return bMList;
        }
        public IResponseStatus Delete(int ID)
        {
            IResponseStatus _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (_lr.LoginTypeID == LoginType.ApplicationUser && !userML.IsEndUser())
            {
                CommonReq commonReq = new CommonReq
                {
                    LoginID = _lr.UserID,
                    LoginTypeID = _lr.LoginTypeID,
                    CommonInt = ID,
                    CommonInt2 = Deletable.TblBank
                };
                IProcedure _proc = new procDelete(_dal);
                _res = (IResponseStatus)_proc.Call(commonReq);
            }
            return _res;
        }

        public Bank GetBank(int ID)
        {
            if (_lr.LoginTypeID == LoginType.ApplicationUser && !userML.IsEndUser())
            {
                var commonReq = new CommonReq
                {
                    LoginID = _lr.UserID,
                    LoginTypeID = _lr.LoginTypeID,
                    CommonInt = ID
                };
                IProcedure _proc = new ProcGetBankByID(_dal);
                return (Bank)_proc.Call(commonReq);
            }
            return new Bank();
        }
        public IResponseStatus SaveBank(Bankreq _req)
        {
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = "Access denied!"
            };
            if (_lr.LoginTypeID == LoginType.ApplicationUser && !userML.IsEndUser())
            {
                if (Validate.O.IsNumeric(_req.BranchName ?? "") || (_req.BranchName ?? "").Length > 100)
                {
                    _res.Msg = ErrorCodes.InvalidParam + " BranchName";
                    return _res;
                }
                if (Validate.O.IsNumeric(_req.AccountHolder ?? "") || (_req.AccountHolder ?? "").Length > 100)
                {
                    _res.Msg = ErrorCodes.InvalidParam + " AccountHolder";
                    return _res;
                }
                if ((_req.IFSCCode ?? "").Length > 11)
                {
                    _res.Msg = ErrorCodes.InvalidParam + " IFSCCode";
                    return _res;
                }
                IProcedure _proc = new ProcSaveBank(_dal);
                _res = (ResponseStatus)_proc.Call(new Bank
                {
                    ID = _req.ID,
                    BankID = _req.BankID,
                    BranchName = _req.BranchName,
                    AccountHolder = _req.AccountHolder,
                    AccountNo = _req.AccountNo,
                    IFSCCode = _req.IFSCCode,
                    Charge = _req.Charge,
                    EntryBy = _lr.UserID,
                    LT = _lr.LoginTypeID,
                    ISQRENABLE = _req.ISQRENABLE,
                    IsbankLogoAvailable = _req.IsbankLogoAvailable,
                    NeftID = _req.NeftID,
                    NeftStatus = _req.NeftStatus,
                    ThirdPartyTransferID = _req.ThirdPartyTransferID,
                    ThirdPartyTransferStatus = _req.ThirdPartyTransferStatus,
                    CashDepositID = _req.CashDepositID,
                    CashDepositStatus = _req.CashDepositStatus,
                    GCCID = _req.GCCID,
                    GCCStatus = _req.GCCStatus,
                    ChequeID = _req.ChequeID,
                    ChequeStatus = _req.ChequeStatus,
                    ScanPayID = _req.ScanPayID,
                    ScanPayStatus = _req.ScanPayStatus,
                    UPIID = _req.UPIID,
                    UPIStatus = _req.UPIStatus,
                    ExchangeID = _req.ExchangeID,
                    ExchangeStatus = _req.ExchangeStatus,
                    RImageUrl = _req.RImageUrl,
                    UPINUmber = _req.UPINUmber,
                    IsVirtual = _req.IsVirtual,
                    Remark = _req.Remark,
                    CDMID = _req.CDMID,
                    CDM = _req.CDM,
                    CDMType = _req.CDMType,
                    CDMCharges = _req.CDMCharges
                });
            }
            return _res;
        }
        public IResponseStatus SavePaymentModeSetting(PaymentModeMaster settings)
        {
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = "Access denied!"
            };
            if (_lr.LoginTypeID == LoginType.ApplicationUser && !userML.IsEndUser())
            {
                var Bankreq = new PaymentModeMaster
                {
                    LoginID = _lr.UserID,
                    LT = _lr.LoginTypeID,
                    ModeID = settings.ModeID,
                    IsTransactionIdAuto = settings.IsTransactionIdAuto,
                    IsAccountHolderRequired = settings.IsAccountHolderRequired,
                    IsChequeNoRequired = settings.IsChequeNoRequired,
                    IsCardNumberRequired = settings.IsCardNumberRequired,
                    IsMobileNoRequired = settings.IsMobileNoRequired,
                    IsBranchRequired = settings.IsBranchRequired,
                    IsUPIID = settings.IsUPIID,
                    Status = settings.Status,
                };
                IProcedure _proc = new ProcSavePaymentModeSetting(_dal);
                _res = (ResponseStatus)_proc.Call(Bankreq);
            }
            return _res;
        }
        public IEnumerable<Bank> WhiteLabelBanksForApp(CommonReq commonReq)
        {
            var bMList = new List<Bank>();
            IProcedure _proc = new ProcGetWhiteLabelBank(_dal);
            bMList = (List<Bank>)_proc.Call(commonReq);
            return bMList;
        }

        public List<Bank> BankList()
        {
            var bMList = new List<Bank>();
            int UserID;
            if (_lr.UserID == 1)
            {
                UserID = _lr.UserID;
            }
            else
            {
                UserID = _lr.ReferalID;
            }
            var commonReq = new CommonReq
            {
                LoginID = _lr.UserID,
                LoginTypeID = _lr.LoginTypeID,
                CommonInt = UserID
            };
            IProcedure _proc = new ProcGetSavedBank(_dal);
            bMList = (List<Bank>)_proc.Call(commonReq);
            return bMList;
        }
        public IEnumerable<PaymentModeMaster> GetAllPaymentMode()
        {
            var MList = new List<PaymentModeMaster>();
            IProcedure _proc = new ProcGetAllPaymentMode(_dal);
            MList = (List<PaymentModeMaster>)_proc.Call();
            return MList;
        }

        public IEnumerable<PaymentModeMaster> GetPaymentMode(CommonReq commonReq)
        {
            var bMList = new List<PaymentModeMaster>();
            IProcedure _proc = new ProcGetPaymentModeList(_dal);
            bMList = (List<PaymentModeMaster>)_proc.Call(commonReq);
            return bMList;
        }

        public IEnumerable<Bank> BanksAndPaymentModes(CommonReq commonReq)
        {
            var bMList = new List<Bank>();
            IProcedure _proc = new ProcGetSavedBank(_dal);
            bMList = (List<Bank>)_proc.Call(commonReq);
            IProcedure _procPMode = new ProcGetPaymentModeList(_dal);
            var PaymentModes = (List<PaymentModeMaster>)_procPMode.Call(commonReq);
            var _bankwithMode = new List<Bank>();
            foreach (var i in bMList)
            {
                var _finalList = PaymentModes.Where(x => x.BankID == i.BankID).ToList();
                _bankwithMode.Add(new Bank
                {
                    ID = i.ID,
                    BankName = i.BankName,
                    BranchName = i.BranchName,
                    AccountHolder = i.AccountHolder,
                    AccountNo = i.AccountNo,
                    IFSCCode = i.IFSCCode,
                    Charge = i.Charge,
                    Logo = i.Logo,
                    CID = i.CID,
                    ISQRENABLE = i.ISQRENABLE,
                    RImageUrl = i.RImageUrl,
                    QRPath = i.QRPath,
                    UPINUmber = i.UPINUmber,
                    Mode = _finalList
                });
            }
            return _bankwithMode;
        }
        #region Bank Admin Master
        public IEnumerable<BankMaster> GetBankMasterAdmin(CommonReq _req)
        {
            var res = new List<BankMaster>();
            if (_lr.RoleID > 0)
            {
                IProcedure proc = new ProcGetBankMasterAdmin(_dal);
                res = (List<BankMaster>)proc.Call(_req);
            }
            return res;
        }

        public IResponseStatus UpdatebankSetting(int BankID, int StatusColumn)
        {
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            if ((_lr.LoginTypeID == LoginType.ApplicationUser))
            {
                var _req = new BankMaster
                {
                    LoginID = _lr.UserID,
                    LTID = _lr.LoginTypeID,
                    ID = BankID,
                    StatusColumn = StatusColumn,
                };
                IProcedure _proc = new ProcUpdateBankSetting(_dal);
                _res = (ResponseStatus)_proc.Call(_req);
            }
            return _res;
        }

        public IResponseStatus UpdateBank(BankMaster BMaster)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (_lr.LoginTypeID == LoginType.ApplicationUser && _lr.RoleID == Role.Admin)
            {

                BankMaster operatorRequest = new BankMaster
                {

                    LTID = _lr.LoginTypeID,
                    LoginID = _lr.UserID,
                    ID = BMaster.ID,
                    AccountLimit = BMaster.AccountLimit,
                    IFSC = BMaster.IFSC,
                    Code = BMaster.Code,
                    IIN = BMaster.IIN,

                };
                IProcedure proc = new ProcUpdatebankMaster(_dal);
                return (IResponseStatus)proc.Call(operatorRequest);
            }
            return res;
        }
        #endregion
        public List<BankMaster> bindAEPSBanks(string bankName)
        {
            CommonReq req = new CommonReq
            {
                LoginTypeID = LoginType.ApplicationUser,
                LoginID = 1,
                CommonStr = bankName
            };
            IProcedure _proc = new ProcbindAEPSBanks(_dal);
            return (List<BankMaster>)_proc.Call(req);
        }
        public List<BankMaster> GetCrediCardBanks()
        {
            var _proc = new ProcGetBankMaster(_dal);
            return (List<BankMaster>)_proc.CallCC();
        }

        #region Holiday Region
        public IEnumerable<BankHoliday> GetHoliday()
        {
            IProcedure _proc = new ProcGetHoliday(_dal);
            return (List<BankHoliday>)_proc.Call(new CommonReq
            {
                LoginTypeID = _lr.LoginTypeID,
                LoginID = _lr.UserID,
            });
        }
        public IResponseStatus SaveHolidayMaster(int ID, string Date, string Remark, bool IsDeleted)
        {
            IProcedure _proc = new ProcSaveHoliday(_dal);
            return (ResponseStatus)_proc.Call(new BankHoliday
            {
                LTID = _lr.LoginTypeID,
                LoginID = _lr.UserID,
                ID = ID,
                Date = Date,
                Remark = Remark,
                IsDeleted = IsDeleted
            });
        }

        public IEnumerable<BankHoliday> GetUpcomingHolidays()
        {
            IProcedure _proc = new ProcGetUpcomingHoliday(_dal);
            return (List<BankHoliday>)_proc.Call(new CommonReq
            {
                LoginTypeID = _lr.LoginTypeID,
                LoginID = _lr.UserID
            });
        }
        #endregion

        public IResponseStatus UpdateBankIIN(CommonReq req)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (_lr.LoginTypeID == LoginType.ApplicationUser && _lr.RoleID == Role.Admin)
            {
                req.LoginID = _lr.UserID;
                IProcedure proc = new ProcUpdateIIN(_dal);
                return (IResponseStatus)proc.Call(req);
            }
            return res;
        }


        public async Task<IResponseStatus> UploadUTRListAsync(string accountNo, int BankID, List<UtrStatementUpload> records)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if ((_lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser))
            {
                var _req = new UtrStatementUploadReq
                {
                    LoginTypeID = _lr.LoginTypeID,
                    LoginID = _lr.UserID,
                    Record = records,
                    CommonStr = _rinfo.GetRemoteIP(),
                    CommonStr2 = _rinfo.GetBrowser(),
                    CommonStr3 = accountNo,
                    CommonInt = BankID
                };
                IProcedureAsync _proc = new ProcInsertURTList(_dal);
                res = (ResponseStatus)await _proc.Call(_req).ConfigureAwait(false);
            }
            return res;
        }



        public bool UpdateInsertUtrFilter(UtrStatementSetting data)
        {
            try
            {
                var jsonFile = DOCType.UTRStatementJsonFilePath;
                if (!File.Exists(jsonFile))
                {
                    using (System.IO.File.Create(jsonFile));
                    
                }

                    var json = System.IO.File.ReadAllText(jsonFile);
                    //var jObjectList = JsonConvert.DeserializeObject<List<UtrStatementSetting>>(json);
                    var jObjectList = json != "" ? JsonConvert.DeserializeObject<List<UtrStatementSetting>>(json) : new List<UtrStatementSetting>();
                    if (jObjectList.Any(x => x.bankID == data.bankID && x.transactionType == data.transactionType))
                    {
                        foreach (var obj in jObjectList.Where(x => x.bankID == data.bankID && x.transactionType == data.transactionType))
                        {
                            obj.endWith = data.endWith;
                            obj.startWith = data.startWith;
                            obj.identifier = data.identifier;
                        }
                    }
                    else
                    {
                        jObjectList.Add(new UtrStatementSetting
                        {
                            bank = data.bank,
                            bankID = data.bankID,
                            startWith = data.startWith,
                            endWith = data.endWith,
                            identifier = data.identifier,
                            transactionType = data.transactionType



                        });

                        
                    }

                string output = JsonConvert.SerializeObject(jObjectList, Formatting.Indented);
                File.WriteAllText(jsonFile, output);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<IResponseStatus> UtrStatementReconcile(string FiledID)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if ((_lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser))
            {
                var _req = new UtrStatementUploadReq
                {
                    LoginTypeID = _lr.LoginTypeID,
                    LoginID = _lr.UserID,
                    CommonStr3 = FiledID
                };
                IProcedureAsync _proc = new ProcUtrStatementReconcile(_dal);
                res = (ResponseStatus)await _proc.Call(_req).ConfigureAwait(false);
            }
            return res;


        }



        public async Task<IResponseStatus> UtrStatementDelete(string FiledID)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if ((_lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser))
            {
                var _req = new UtrStatementUploadReq
                {
                    LoginTypeID = _lr.LoginTypeID,
                    LoginID = _lr.UserID,
                    CommonStr3 = FiledID
                };
                IProcedureAsync _proc = new ProcUtrStatementDelete(_dal);
                res = (ResponseStatus)await _proc.Call(_req).ConfigureAwait(false);
            }
            return res;
        }
        public IResponseStatus BankShowMl(int ID)
        {
            IProcedure proc = new ProcUpdateBankShow(_dal);
            return (ResponseStatus)proc.Call(ID);
        }
        public IResponseStatus AddParty(int BankID, int UserID)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            CommonReq _req = new CommonReq
            {
                CommonInt = UserID,
                CommonInt2 = BankID
            };
            IProcedure _proc = new Proc_InsertPartyDetail(_dal);
            res = (ResponseStatus)_proc.Call(_req);
            return res;
        }
    }
}
