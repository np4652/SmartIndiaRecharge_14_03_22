using Fintech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.Model.SDK;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Interfaces
{
    public interface IDeviceML
    {
        SDKVendorDetail APIOutletDetail(int InterfaceType, int OutletID);
        List<DeviceMaster> DeviceMasters();
        bool DeviceSave(DeviceMaster obj);
        Task<BalanceEquiryResp> CheckBalance(string _pidData, string aadhar, string bank, int _InterfaceType,int OID, string IMEI, string Lattitude, string Longitude);
        Task<BalanceEquiryResp> CheckBalance(PidData pidData, string aadhar, string bank, int _InterfaceType, int UserID, int OutletID,int RMode,int OID, string IMEI, string Lattitude, string Longitude, string _PIDData);
        Task<WithdrawlResponse> Withdrawl(string _pidData, string aadhar, string bank, int _InterfaceType, int Amount, int OID,string IMEI,string Lattitude,string Longitude);
        Task<WithdrawlResponse> Withdrawl(PidData pidData, string aadhar, string bank, int _InterfaceType, int Amount, int UserID, int OutletID, int RMode, int OID, string IMEI, string Lattitude, string Longitude, string _PIDData);
        Task<WithdrawlResponse> Aadharpay(PidData pidData, string aadhar, string bank, int _InterfaceType, int Amount, int UserID, int OutletID, int RMode, int OID, string IMEI, string Lattitude, string Longitude, string _PIDData);
        Task<WithdrawlResponse> Aadharpay(string _pidData, string aadhar, string bank, int _InterfaceType, int Amount, int OID, string IMEI, string Lattitude, string Longitude);
        MiniStatementResponse MiniStatement(PidData pidData, string aadhar, string Bank, string BankIIN, int _InterfaceType, int UserID, int OutletID, int RMode, int OID, string SPKey, string IMEI, string Lattitude, string Longitude, string _PIDData);
        Task<DepositResponse> DepositGenerateOTP(DepositRequest depositRequest);
        Task<DepositResponse> DepositVerifyOTP(DepositRequest depositRequest);
        Task<DepositResponse> DepositAccount(DepositRequest depositRequest);
        List<MasterVendorModel> VendorMaster(MasterVendorModel req);
        IResponseStatus VendorMasterCU(MasterVendorModel req);
        IResponseStatus VendorMasterToggle(MasterVendorModel req);
        List<MasterDeviceModel> DeviceModelMaster(MasterDeviceModel req);
        IResponseStatus DeviceModelMasterCU(MasterDeviceModel req);
        IResponseStatus DeviceModelMasterToggle(MasterDeviceModel req);
        IEnumerable<MasterVendorModel> VendorDDl();
        VendorBindOperators VendorOperatorAllocation(int id);
        IResponseStatus VendorOperatorUpdate(VendorBindOperators req);
        List<MPosDeviceInventoryModel> MPosDevice(MPosDeviceInventoryModel req);
        IResponseStatus MPosDeviceCU(MPosDeviceInventoryModel req);
        IEnumerable<MasterDeviceModel> DeviceModelDDl(int vendorId);
        IResponseStatus MPosDeviceAssignMap(MPosDeviceInventoryModel req, bool IsMap = false);
        IResponseStatus MPosToggle(MPosDeviceInventoryModel req);
        IResponseStatus MPosDeviceUnAssignUnMap(MPosDeviceInventoryModel req);
    }
}
