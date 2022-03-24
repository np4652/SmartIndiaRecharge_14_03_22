using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.App;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Interfaces
{
    public interface IPackageML
    {
        PackageModel GetPackageMaster();
        SlabMaster GetPackageMaster(int PackageID);
        IResponseStatus UpdatePackageMaster(SlabMaster packageMaster);
        IResponseStatus UpdatePackageDetail(int PackageID, int ServiceID, bool IsActive);
        PackageMaster GetPackageCommission(CommonReq req);
        IResponseStatus UpdateLevelPackageCommission(PkgLevelCommissionReq req);
        PackageCommission GetPackageCommissionChannel();
        IResponseStatus UpdateAvailablePackage(PackageAvailableModel req);
        IResponseStatus UpgradePackage(PackageAvailableModel req);
        List<PackageAvailableModel> GetAvailablePackage(PackageAvailableModel req);
        PackageModel AvailablePackage(int UserId);
        PackageModel GetUpgradePackage(int UserId);
        Task<IResponseStatus> UpgradePackage(int PackageID);
        Task<IResponseStatus> UpgradePackage(PackageUpgradeRequest req);
        PackageCommission GetPackageCommissionChannel(CommonReq req);
        List<_AvailablePackage> GetAvailablePackages(int UserID);
        AvailablePackageResponse GetAvailablePackagesForApp(int LoginID, int LoginTypeID);
        IResponseStatus UpgradePackageForApp(UpgradePackageReq AppReq);

        #region VASPackage
        IResponseStatus UpdateVASPackageMaster(SlabMaster packageMaster);
        PackageModel VASGetPackageMaster();
        SlabMaster VASGetPackageMaster(int ID);
        IResponseStatus UpdateVASPackageDetail(int PackageID, int ServiceID, bool IsActive, bool IsChargeable, int Charge);
        PackageModel VASPackageMasterForPlans();
        ResponseStatus VASPlanBuyOrRenew(int p, bool a, int u, int lt);
        #endregion
    }
}
