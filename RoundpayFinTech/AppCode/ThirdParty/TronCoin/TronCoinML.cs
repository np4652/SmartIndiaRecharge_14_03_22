using Fintech.AppCode.DB;
using Fintech.AppCode.StaticModel;
using Fintech.AppCode.WebRequest;
using Newtonsoft.Json;
using RoundpayFinTech.AppCode.DL.Coin;
using RoundpayFinTech.AppCode.Model.Coin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.ThirdParty.TronCoin
{
    public class TronCoinML
    {
        private const string BaseURI = "http://tronnode.joborientedtraining.com:3000/";
        private const string EndPointCreateAccount = BaseURI + "createAccount";
        private const string EndPointGetBalance = BaseURI + "getBalance?tronAddress=";
        private const string EndPointSendTR20Token = BaseURI + "sendtrc20";//Currently used for send tron
        private const string EndPointSendTransaction = BaseURI + "sendTransaction";
        private const string EndPointFreeze = BaseURI + "freeze";
        private const string CoinRateINRURI = "https://api.coingecko.com/api/v3/simple/price?ids=tron-bsc&vs_currencies=inr&include_market_cap=false&include_24hr_vol=false&include_24hr_change=false&include_last_updated_at=false";
        private const string ContractAddress = "TLPCLksQPk5EbtKRfuTr2Dcf9sXwpmAzGu";
        private readonly IDAL _dal;

        public TronCoinML(IDAL dal) => _dal = dal;

        public CoinAddressDetail GenerateAddress()
        {
            var res = new CoinAddressDetail
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.NODATA
            };
            var req = EndPointCreateAccount;
            var resp = string.Empty;
            try
            {
                resp = AppWebRequest.O.CallUsingHttpWebRequest_GET(EndPointCreateAccount);
                if (!string.IsNullOrEmpty(resp))
                {
                    var appResp = JsonConvert.DeserializeObject<TronGenerateAddressResp>(resp);
                    if (appResp != null)
                    {
                        if (appResp.address != null)
                        {
                            if (!string.IsNullOrEmpty(appResp.address.base58))
                            {
                                res.CoinAddress = appResp.address.base58;
                                res.CoinToken = appResp.privateKey;
                                res.CoinHexAddress = appResp.address.hex;
                                res.Statuscode = ErrorCodes.One;
                                res.Msg = ErrorCodes.SUCCESS;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                resp = "Exception" + resp + ex.Message;
            }
            new ProcCoinRequestResponse(_dal).SaveAPIReqResp(GetType() + ".GenerateAddress", req, resp);
            return res;
        }

        public CoinBalanceResponse BalanceStatus(CoinBalanceRequest coinBalanceRequest)
        {
            var res = new CoinBalanceResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.NODATA
            };
            var req = EndPointGetBalance + coinBalanceRequest.CoinAddress;
            var resp = string.Empty;
            try
            {
                resp = AppWebRequest.O.CallUsingHttpWebRequest_GET(req);
                if (!string.IsNullOrEmpty(req))
                {
                    var appResp = JsonConvert.DeserializeObject<TronGetBalanceResp>(resp);
                    if (appResp != null)
                    {
                        if (appResp.data != null)
                        {
                            if (appResp.data.Count > 0)
                            {
                                if (appResp.data[0].balance > 0)
                                {
                                    res.Statuscode = ErrorCodes.One;
                                    res.Msg = ErrorCodes.SUCCESS;
                                    res.Balance = appResp.data[0].balance;
                                    if (appResp.data[0].trc20 != null)
                                    {
                                        if (appResp.data[0].trc20.Count > 0)
                                        {
                                            res.SmartContractBalance = Convert.ToInt64(appResp.data[0].trc20[0].TLPCLksQPk5EbtKRfuTr2Dcf9sXwpmAzGu);
                                        }
                                    }

                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                resp = "Exception" + resp + ex.Message;
            }
            new ProcCoinRequestResponse(_dal).SaveAPIReqResp(GetType() + ".BalanceStatus", req, resp);
            return res;
        }
        public CoinTransferResponse Freeze(CoinEasyTransferReq coinEasyTransferReq) {
            var res = new CoinTransferResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.NODATA
            };
            var req = EndPointFreeze;
            var resp = string.Empty;
            try
            {
                var reqObj = new TronFreezeRequest
                {
                    privateKey = coinEasyTransferReq.Token,
                    receiverAddress = coinEasyTransferReq.Address,
                    senderAddress = coinEasyTransferReq.SenderAddress,
                    freezeFor = coinEasyTransferReq.FreezeFor,
                    amountInTRX = coinEasyTransferReq.Amount
                };
                req = req + "?" + JsonConvert.SerializeObject(reqObj);
                resp = AppWebRequest.O.PostJsonDataUsingHWR(EndPointFreeze, reqObj);
                if (!string.IsNullOrEmpty(req))
                {
                    var appResp = JsonConvert.DeserializeObject<EasyTransferResponse>(resp);
                    if (appResp != null)
                    {
                        if (appResp != null)
                        {
                            if (appResp.result)
                            {
                                res.Statuscode = ErrorCodes.One;
                                res.Msg = ErrorCodes.SUCCESS;
                                res.Status = RechargeRespType.SUCCESS;
                                res.VendorID = appResp.txid;
                                res.LiveID = appResp.txid;
                                res.FreezeDuration = appResp.transaction.raw_data.contract[0].parameter.value.frozen_duration;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                resp = "Exception" + resp + ex.Message;
            }
            new ProcCoinRequestResponse(_dal).SaveAPIReqResp(GetType() + ".Freeze", req, resp);
            return res;
        }
        public CoinTransferResponse TransferCoin(CoinEasyTransferReq coinEasyTransferReq)
        {
            var res = new CoinTransferResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.NODATA
            };
            var req = EndPointSendTR20Token;
            var resp = string.Empty;
            try
            {
                var reqObj = new EasyTransferRawRequest
                {
                    privateKey = coinEasyTransferReq.Token,
                    receiverAddress = coinEasyTransferReq.Address,
                    senderAddress = coinEasyTransferReq.SenderAddress,
                    contractAddress = ContractAddress,
                    amountInSun = coinEasyTransferReq.Amount
                };
                req = req + "?" + JsonConvert.SerializeObject(reqObj);
                resp = AppWebRequest.O.PostJsonDataUsingHWR(EndPointSendTR20Token, reqObj);
                if (!string.IsNullOrEmpty(req))
                {
                    var appResp = JsonConvert.DeserializeObject<EasyTransferResponse>(resp);
                    if (appResp != null)
                    {
                        if (appResp != null)
                        {
                            if (appResp.result)
                            {
                                res.Statuscode = ErrorCodes.One;
                                res.Msg = ErrorCodes.SUCCESS;
                                res.Status = RechargeRespType.SUCCESS;
                                res.VendorID = appResp.txid;
                                res.LiveID = appResp.txid;                                
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                resp = "Exception" + resp + ex.Message;
            }
            new ProcCoinRequestResponse(_dal).SaveAPIReqResp(GetType() + ".TransferCoin", req, resp);
            return res;
        }
        public decimal SUN2inrRate()
        {

            var req = CoinRateINRURI;
            var resp = string.Empty;
            try
            {
                resp = AppWebRequest.O.CallUsingHttpWebRequest_GET(req);
                if (!string.IsNullOrEmpty(req))
                {
                    var appResp = JsonConvert.DeserializeObject<TronINRResponse>(req);
                    if (appResp != null)
                    {
                        if (appResp.TronBSC != null)
                        {
                            return appResp.TronBSC.inr;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                resp = "Exception" + resp + ex.Message;
            }
            new ProcCoinRequestResponse(_dal).SaveAPIReqResp(GetType() + ".SUN2inrRate", req, resp);
            return 0;
        }
    }
}
