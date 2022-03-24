using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.Model.Reports;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Interfaces
{
    public interface IBankML
    {
        IEnumerable<BankHoliday> GetUpcomingHolidays();
        Bank GetBank(int ID);
        List<BankMaster> BankMasters();
        List<BankMaster> DMRBanks();
        List<BankMaster> AEPSBankMasters();
        BankMaster BankMasters(int ID);
        List<Bank> Banks(int UserID);
        IResponseStatus SaveBank(Bankreq bank);
        IEnumerable<Bank> BanksForApp(CommonReq commonReq);
        IEnumerable<BankMaster> BankMastersApp(CommonReq commonReq);
        IEnumerable<Bank> WhiteLabelBanksForApp(CommonReq commonReq);
        List<Bank> BankList();
        IEnumerable<PaymentModeMaster> GetAllPaymentMode();
        IEnumerable<PaymentModeMaster> GetPaymentMode(CommonReq commonReq);
        IEnumerable<Bank> BanksAndPaymentModes(CommonReq cq);
        IResponseStatus SavePaymentModeSetting(PaymentModeMaster settings);
        IEnumerable<BankMaster> GetBankMasterAdmin(CommonReq _req);
        IResponseStatus UpdatebankSetting(int BankID, int StatusColumn);
        IResponseStatus UpdateBank(BankMaster BMaster);
        List<BankMaster> bindAEPSBanks(string bankName);
        List<BankMaster> GetCrediCardBanks();
        IResponseStatus SaveHolidayMaster(int ID, string Date, string Remark, bool IsDeleted);
        IEnumerable<BankHoliday> GetHoliday();
        IResponseStatus UpdateBankIIN(CommonReq req);
        Task<IResponseStatus> UploadUTRListAsync(string accountNo, int BankID, List<UtrStatementUpload> records);
        bool UpdateInsertUtrFilter(UtrStatementSetting data);
        List<BankMaster> BankMaster(int ID);
        Task<IResponseStatus> UtrStatementReconcile(string FiledID);
        Task<IResponseStatus> UtrStatementDelete(string FiledID);
        IResponseStatus BankShowMl(int ID);
        IResponseStatus AddParty(int BankID, int UserID);
    }
}
