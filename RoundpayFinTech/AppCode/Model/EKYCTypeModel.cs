using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Model
{
    public class EKYCTypeModel
    {
        public int ID { get; set; }
        public string EKYCStep { get; set; }
    }
    public class EKYCGetDetail
    {
        public int EKYCID { get; set; }
        public int CompanyTypeID { get; set; }
        public int StepCompleted { get; set; }
        public bool IsGSTINEKYCDone { get; set; }
        public bool IsAadharEKYCDone { get; set; }
        public bool IsPANEKYCDone { get; set; }
        public bool IsBanckAccountEKYCDone { get; set; }
        public bool IsGSTSkipped { get; set; }
        public bool IsEKYCDone { get; set; }
        public bool IsGSTIN { get; set; }
        public string GSTAuthorizedSignatory { get; set; }
        public SelectList Directors { get; set; }
        public string GSTIN { get; set; }
        public string PAN { get; set; }
        public string AadharNo { get; set; }
        public string SelectedDirector { get; set; }
        public string PanOfDirector { get; set; }
        public string AccountNumber { get; set; }
        public string AccountHolder { get; set; }
        public string IFSC { get; set; }
        public int EditStepID { get; set; }
        public string DOB { get; set; }
        public string Name { get; set; }
        public string OutletName { get; set; }
        public List<EKYCTypeModel> EKYCType { get; set; }

        public SelectList BankList { get; set; }
        public SelectList CompanyTypeSelect { get; set; }

        public bool ShouldSerailizeGSTAuthorizedSignatory() => false;
    }
    public class EKYCSwitchAPIResp
    {
        public int Statuscode { get; set; }
        public string Msg { get; set; }
        public int APIID { get; set; }
        public string APICode { get; set; }
        public string MobileNo { get; set; }
        public int OID { get; set; }
    }
    public class EKYCRequestModel
    {
        public int UserID { get; set; }
        public int ChildUserID { get; set; }
        public int CompanyTypeID { get; set; }
        public string VerificationAccount { get; set; }
        public string IFSC { get; set; }
        public string OTP { get; set; }
        public int ReferenceID { get; set; }
        public bool IsSkip { get; set; }
        public bool IsExternal { get; set; }
        public int RequestMode { get; set; }
        public string DirectorName { get; set; }
        public int BankID { get; set; }
        public int EditStepID { get; set; }
    }
    public class EKYCGSTINResponseModel
    {
        public int Statuscode { get; set; }
        public string Msg { get; set; }
        public int EKYCID { get; set; }
        public bool IsVerified { get; set; }
        public string PANNo { get; set; }
        
        public List<string> AuthorisedSignatory { get; set; }
    }
    public class EKYCByGSTINModel
    {
        public int Statuscode { get; set; }
        public string Msg { get; set; }
        public string LegalName { get; set; }
        public string TradeName { get; set; }
        public string AuthorisedSignatory { get; set; }
        public string EmailID { get; set; }
        public string MobileNo { get; set; }
        public string GSTIN { get; set; }
        public string Address { get; set; }
        public string StateJurisdiction { get; set; }
        public string CentralJurisdiction { get; set; }
        public string RegisterDate { get; set; }
        public string AgreegateturnOver { get; set; }
        public string PANNumber { get; set; }
        public bool APIStatus { get; set; }
        public int CompanyTypeID { get; set; }
    }
    public class EKYCByAadharModelOTP
    {
        public int Statuscode { get; set; }
        public string Msg { get; set; }
        public int ReferenceID { get; set; }
        public bool IsOTPSent { get; set; }
        public bool IsAadharValid { get; set; }
        public bool IsNumberLinked { get; set; }
        public string InitiateID { get; set; }
        public bool IsCallSDK { get; set; }
    }
    public class EKYCAadharModel
    {
        public string Profile { get; set; }
        public string FullName { get; set; }
        public string AadhaarNo { get; set; }
        public string DOB { get; set; }
        public string Gender { get; set; }
        public string Country { get; set; }
        public string District { get; set; }
        public string State { get; set; }
        public string PostOffice { get; set; }
        public string Location { get; set; }
        public string VTC { get; set; }
        public string SubDistrict { get; set; }
        public string Street { get; set; }
        public string House { get; set; }
        public string Landmark { get; set; }
        public string Pincode { get; set; }
        public bool HasImage { get; set; }
        public string ParentName { get; set; }
        public bool IsMobileVerified { get; set; }
        public string ShareCode { get; set; }
        public int Statuscode { get; set; }
        public string Msg { get; set; }
    }
    public class EKYCByGSTINProcReq : EKYCByGSTINModel
    {
        public int UserID { get; set; }
        public int APIID { get; set; }
        public bool IsSkip { get; set; }
        public bool IsExternal { get; set; }
        public int ChildUserID { get; set; }
    }
    public class EKYCByAadharModel
    {
        public string Profile { get; set; }
        public string FullName { get; set; }
        public string AadhaarNo { get; set; }
        public string DOB { get; set; }
        public string Gender { get; set; }
        public string Country { get; set; }
        public string District { get; set; }
        public string State { get; set; }
        public string PostOffice { get; set; }
        public string Location { get; set; }
        public string VTC { get; set; }
        public string SubDistrict { get; set; }
        public string Street { get; set; }
        public string House { get; set; }
        public string Landmark { get; set; }
        public string Pincode { get; set; }
        public bool HasImage { get; set; }
        public string ParentName { get; set; }
        public bool IsMobileVerified { get; set; }
        public string ShareCode { get; set; }
        public string DirectorName { get; set; }
        public int Statuscode { get; set; }
        public string Msg { get; set; }
    }
    public class EKYCByAadharProcReq : EKYCByAadharModel
    {
        public int InitiateID { get; set; }
        public string VendorID { get; set; }
        public int UserID { get; set; }
        public int EKYCID { get; set; }
        public int APIID { get; set; }
        public bool IsExternal { get; set; }
        public int ChildUserID { get; set; }
        public bool APIStatus { get; set; }
    }

    public class EKYCByPANModel
    {
        public int Statuscode { get; set; }
        public string Msg { get; set; }
        public string DirectorName { get; set; }
        public bool IsAadharSeeded { get; set; }
        public bool IsPANValid { get; set; }
        public string FirstName { get; set; }
        public string PANNumber { get; set; }
        public string FullName { get; set; }
        public string LastName { get; set; }
        public string Title { get; set; }
    }
    public class EKYCByPANModelProcReq : EKYCByPANModel
    {
        public int UserID { get; set; }
        public int EKYCID { get; set; }
        public int APIID { get; set; }
        public bool IsExternal { get; set; }
        public int ChildUserID { get; set; }
        public bool APIStatus { get; set; }
    }
    public class EKYCStepEditReq
    {
        public int UserID { get; set; }
        public int EKYCID { get; set; }
        public int EditStepID { get; set; }
        public bool IsExternal { get; set; }
        public int ChildUserID { get; set; }
        public bool APIStatus { get; set; }
    }
    public class EKYCByBankAccountModel
    {
        public int Statuscode { get; set; }
        public string Msg { get; set; }
        public string AccountNumber { get; set; }
        public string IFSC { get; set; }
        public string AccountHolder { get; set; }
        public string LiveID { get; set; }
    }
    public class EKYCByBankAccountModelProcReq : EKYCByBankAccountModel
    {
        public int UserID { get; set; }
        public int EKYCID { get; set; }
        public int APIID { get; set; }
        public int BankID { get; set; }
        public string Bank { get; set; }
        public bool IsExternal { get; set; }
        public int ChildUserID { get; set; }
        public bool APIStatus { get; set; }
    }
    public class InitiateEKYCResponse
    {
        public int Statuscode { get; set; }
        public string Msg { get; set; }
        public int InitiateID { get; set; }
        public string VendorID { get; set; }
        public string SecurityKey { get; set; }
    }
}
