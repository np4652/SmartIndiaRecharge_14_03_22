using Fintech.AppCode.Configuration;
using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using RoundpayFinTech.AppCode.DL;
using RoundpayFinTech.AppCode.DL.Coin;
using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.Coin;
using RoundpayFinTech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.ThirdParty.TronCoin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.MiddleLayer
{
    public class CoinML : ICoinML
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly IHostingEnvironment _env;
        private readonly IConnectionConfiguration _c;
        private readonly ISession _session;
        private readonly IDAL _dal;
        private readonly LoginResponse _lr;
        private readonly IRequestInfo _info;
        public CoinML(IHttpContextAccessor accessor, IHostingEnvironment env)
        {
            _accessor = accessor;
            _env = env;
            _c = new ConnectionConfiguration(_accessor, _env);
            _session = _accessor != null ? _accessor.HttpContext.Session : null;
            _dal = new DAL(_c.GetConnectionString());
            _lr = _session.GetObjectFromJson<LoginResponse>(SessionKeys.LoginResponse);
            _info = new RequestInfo(_accessor, _env);
        }
        public CoinAddressDetail GenerateQR(string SPKey, int UserID, int OID = 0)
        {
            var res = new CoinAddressDetail
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };

            IProcedure proc = new ProcCheckCoinGenerationAbility(_dal);
            res = (CoinAddressDetail)proc.Call(new CommonReq
            {
                LoginID = UserID,
                CommonStr = SPKey,
                CommonInt= OID
            });

            if (res.Statuscode == ErrorCodes.One)
            {
                SPKey = res.SPKey;
                if ((SPKey ?? string.Empty) == CoinType.Tron)
                {
                    if (string.IsNullOrEmpty(res.CoinToken))
                    {
                        var tronCoinML = new TronCoinML(_dal);
                        var addrDetail = tronCoinML.GenerateAddress();
                        res.Statuscode = addrDetail.Statuscode;
                        res.Msg = addrDetail.Msg;

                        if (addrDetail.Statuscode == ErrorCodes.One)
                        {
                            res.CoinToken = addrDetail.CoinToken;
                            res.CoinAddress = addrDetail.CoinAddress;
                            res.CoinHexAddress = addrDetail.CoinHexAddress;
                            proc = new ProcUpdateCoinTokenDetail(_dal);
                            var updateCoinStatus = (ResponseStatus)proc.Call(new CommonReq
                            {
                                LoginID = UserID,
                                CommonInt = res.OID,
                                CommonStr = res.CoinToken,
                                CommonStr2 = res.CoinAddress,
                                CommonStr3 = res.CoinHexAddress
                            });
                            res.Statuscode = updateCoinStatus.Statuscode;
                            res.Msg = updateCoinStatus.Msg;
                        }
                    }
                }
            }
            return res;
        }

        public CoinAddressDetail CheckBalance(string SPKey, int UserID, int OID = 0)
        {
            var res = new CoinAddressDetail
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };

            IProcedure proc = new ProcCheckCoinGenerationAbility(_dal);
            res = (CoinAddressDetail)proc.Call(new CommonReq
            {
                LoginID = UserID,
                CommonStr = SPKey,
                CommonInt= OID
            });

            if (res.Statuscode == ErrorCodes.One)
            {
                SPKey = res.SPKey;
                if ((SPKey ?? string.Empty) == CoinType.Tron)
                {
                    if (!string.IsNullOrEmpty(res.CoinToken))
                    {
                        var tronCoinML = new TronCoinML(_dal);
                        var addrDetail = tronCoinML.BalanceStatus(new CoinBalanceRequest
                        {
                            CoinAddress = res.CoinAddress
                        });
                        res.Statuscode = addrDetail.Statuscode;
                        res.Msg = addrDetail.Msg;
                        if (addrDetail.Statuscode == ErrorCodes.One)
                        {
                            List<MoneyConversionRate> getMoney = null;
                            decimal? TRXAmount = null;
                            if (addrDetail.SmartContractBalance > 0)
                            {

                                proc = new ProcGetMoneyConversionRate(_dal);
                                getMoney = (List<MoneyConversionRate>)proc.Call(new CommonReq
                                {
                                    CommonInt = res.OID
                                });

                                if (getMoney.Count > 0)
                                {

                                    decimal? HCTAmount = null;
                                    var rate = GetRate(getMoney, MoneySymbol.SUN, MoneySymbol.HCT);
                                    if (rate.IsFound)
                                    {
                                        HCTAmount = addrDetail.SmartContractBalance * rate.Rate;
                                        rate = GetRate(getMoney, MoneySymbol.HCT, MoneySymbol.TRX);
                                        if (rate.IsFound)
                                        {
                                            TRXAmount = HCTAmount * rate.Rate;
                                        }
                                    }
                                }


                                if (res.IsFrozen == false)
                                {
                                   
                                    if (TRXAmount != null)
                                    {
                                        if (TRXAmount >= 1)
                                        {
                                            if (!res.IsFrozen)
                                            {
                                                //Frozen
                                                var frozenRes = tronCoinML.Freeze(new CoinEasyTransferReq
                                                {
                                                    Address = res.CoinReceiverAddress,
                                                    Amount = 1,
                                                    FreezeFor = "BANDWIDTH",
                                                    SenderAddress = res.CoinAddress,
                                                    Token = res.CoinToken
                                                });
                                                if (frozenRes.Statuscode == ErrorCodes.One)
                                                {
                                                    proc = new ProcUpdateFreezeStatus(_dal);
                                                    res.IsFrozen = (bool)proc.Call(new CommonReq
                                                    {
                                                        LoginID = UserID,
                                                        CommonInt = frozenRes.FreezeDuration
                                                    });
                                                }
                                            }
                                        }
                                    }
                                }
                                if (res.IsFrozen)
                                {
                                    decimal amountInINR = 0M;
                                    var rateConversion = GetRate(getMoney, MoneySymbol.TRX, MoneySymbol.USD);
                                    if (rateConversion.IsFound)
                                    {
                                        var uSDRate = TRXAmount * rateConversion.Rate;
                                        rateConversion = GetRate(getMoney, MoneySymbol.USD, MoneySymbol.INR);
                                        if (rateConversion.IsFound)
                                        {
                                            amountInINR = (uSDRate * rateConversion.Rate) ?? 0;
                                        }
                                    }
                                    proc = new ProcCoinCreditService(_dal);
                                    var serviceProcResp = (CoinCreditServiceProcResponse)proc.Call(new CoinCreditServiceProcRequest
                                    {
                                        UserID = UserID,
                                        OID = res.OID,
                                        AmountType = nameof(MoneySymbol.SUN),
                                        AmountInCoin = addrDetail.SmartContractBalance,
                                        AmountInINR = amountInINR,
                                        SenderAddrss = res.CoinAddress,
                                        ReceiverAddress = res.CoinReceiverAddress
                                    });
                                    res.Statuscode = serviceProcResp.Statuscode;
                                    res.Msg = serviceProcResp.Msg;
                                    if (res.Statuscode == ErrorCodes.One)
                                    {
                                        var transferRes = tronCoinML.TransferCoin(new CoinEasyTransferReq
                                        {
                                            Address = res.CoinReceiverAddress,
                                            Amount = addrDetail.SmartContractBalance,
                                            SenderAddress = res.CoinAddress,
                                            Token = res.CoinToken
                                        });
                                       
                                        proc = new ProcCoinCreditServiceUpdateStatus(_dal);
                                        var procServiceUpdateRes= (ResponseStatus)proc.Call(new CoinCreditServiceUpdateProcReq
                                        {
                                            TID = serviceProcResp.TID,
                                            Status = transferRes.Statuscode == ErrorCodes.One ? RechargeRespType.SUCCESS : RechargeRespType.FAILED,
                                            VendorID= transferRes.VendorID,
                                            LiveID=transferRes.LiveID,
                                            IP= _info.GetRemoteIP(),
                                            Browser=_info.GetBrowser()
                                        });
                                        res.Statuscode = procServiceUpdateRes.Statuscode;
                                        res.Msg = procServiceUpdateRes.Msg;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return res;
        }
        public List<MoneyConversionRate> GetRates(int OID) {
            IProcedure proc = new ProcGetMoneyConversionRate(_dal);
            return (List<MoneyConversionRate>)proc.Call(new CommonReq
            {
                CommonInt = OID
            });
        }
        private RateWithStatus GetRate(List<MoneyConversionRate> moneyConversionRates, int from, int to)
        {
            if (moneyConversionRates != null)
            {
                return moneyConversionRates.Where(w => w.FromSymbolID == from && w.ToSymbolID == to).Select(x => new RateWithStatus { Rate = x.Rate, IsFound = true }).ToList()[0];
            }
            return new RateWithStatus();
        }
    }
}
