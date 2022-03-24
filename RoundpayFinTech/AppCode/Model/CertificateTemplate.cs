using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Configuration;
using System;
using System.Text;


namespace RoundpayFinTech.AppCode.Model
{
    public class CertificateTemplate
    {
        public string EsseintialServices(CertificateModel res)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(@"<html lang='en'><head><meta charset='utf-8'></head><body style='font-family:Book Antiqua'><div class='form-outter-page' style='margin-bottom: 30px;'><div class='a4-page-logo text-center'><img src='{logo}' style='display: block; margin: auto; margin: 1em auto 2em;width: 150px;'></div><div class='main-body' style='padding: 30px;'><table class='table cus-table'><thead><tr><th colspan='2' style='padding:.75rem;border-top: 0px solid #dee2e6;vertical-align: bottom;border-bottom: 0px solid #dee2e6;'><label style='width:100%'><img src='{ICICIlogo}' style='width: 140px;float: right;'/></label></th></tr></thead><tbody><tr><td style='padding: .75rem;vertical-align: top; border-top: 0px solid #dee2e6;'><h4 style='text-align: center;text-decoration: underline;font-size: 22px;font-weight: 600; text-transform: capitalize;'>To whomsoever it is concerned</h4></td></tr><tr><td style='padding: .75rem;vertical-align: top; border-top: 0px solid #dee2e6;'><p style='font-size: 18px; line-height: 20px;'><b style='padding: 0 5px 0 0;'>Subject:</b> Continuance of essential financial services through Business Correspondents and their Agents.<br/></p><p style='font-size: 18px; line-height: 20px;'>Pursuant to the order passed by GOI and Ministry of Finance  to tackle novel  Corona  Virus (COVID-19), banking has been declared as essential services and banks has been advised to ensure continuity of services through all its customer touch points including Business Correspondent.</p><p style='font-size: 18px; line-height: 20px;'>Under present circumstances Business Correspondent through their BC agent network plays a vital role in providing basic banking services (Cash Deposit and Cash Withdrawal) to citizens across geographies in the country. Advisory has been issued to BC’s on precaution to be taken during this period.</p><p style='font-size: 18px; line-height: 20px;'>This letter has been issued to BC’s to allow them hassle free travel between their residenceand corporate office/branches  during  the  lockdown  period  so  that  uninterrupted  banking  services  can  be  provided  to customers.  BC’s should only be facilitating banking services and no other services during this period. BC’scan issue similarletterto Business Correspondent Agents.</p><p style='font-size: 18px; line-height: 20px;'>Attached: Advisory issued by Government of India.</p><p style='font-size: 18px; line-height: 20px;' style='margin-bottom: 0;'>BC Name:<b style='padding: 0 0px 0 5px !important;'><label style='padding: 0; margin: 0;'>{BCName} [{BCID}]</label></b> </p><p style='font-size: 18px; line-height: 20px;'>Services Provided by BC: <b style='padding: 0 0px 0 5px !important;'>Domestic Money Remittance, AEPS basedCashWithdrawal</b></p><h5 style='text-transform: capitalize;font-size: 20px; font-weight: 600;margin-top: 5em;margin-bottom: 5em;'>Authorised Signatory</h5><div style='position: relative;'><img src='{sign}' style='float: left; width: 120px; position: absolute; bottom: -49px;left: 38px;' /></div><h6 style='font-size: 16px; margin-bottom: 3px;font-weight: 500;text-transform:capitalize; padding-top: 65px;'>{OwnerName}</h6><span>{OwnerDesignation}</span></td></tr></tbody></table></div><div style='width:100%;position:fixed;bottom:1.5em;margin: 2em 10px 0;text-align:center'><div style='width:100%;'><h3 style='width:100%;text-align: center;font-size: 22px;color: #6d0000;font-weight: 700;'>{companyName}</h3><p style='width:100%;font-size: 18px;text-align: center;margin-bottom: 0px; line-height: 20px;'><b style='padding: 0 5px 0 0;'>Corporate Office:</b> {Address} </br><b style='padding: 0 5px 0 0;'>Ph: </b>{PhoneNo},<b style='padding: 0 5px 0 0;'> E-mail: </b>{EmailId}, <b>Website: </b>{website}</p></div></div></div></body></html>");
            sb.Replace("{logo}", DOCType.Logo.Replace("{0}", res.WID.ToString()));
            sb.Replace("{companyName}", res.Name);
            sb.Replace("{OwnerName}", res.OwnerName);
            sb.Replace("{OwnerDesignation}", res.OwnerDesignation);
            sb.Replace("{Address}", res.Address);
            sb.Replace("{mobileNo}", res.mobileNo);
            sb.Replace("{PhoneNo}", res.PhoneNo);
            sb.Replace("{EmailId}", res.EmailId);
            sb.Replace("{website}", res.website);
            sb.Replace("{ICICIlogo}", DOCType.CertificateImglogoICICI);
            sb.Replace("{sign}", DOCType.CertificateImgSign.Replace("{0}", res.WID.ToString()));
            sb.Replace("{BCName}", res.OutletName);
            sb.Replace("{BCID}", res.OutletID.ToString());
            return sb.ToString();
        }

        public string MerchantCertificate(CertificateModel res)
        {
            StringBuilder sb = new StringBuilder();

            if (ApplicationSetting.IsRPOnly)
            {
                sb.Append(@"<!DOCTYPE html><html xmlns='http://www.w3.org/1999/xhtml'><head><title></title><link href='//maxcdn.bootstrapcdn.com/bootstrap/4.0.0/css/bootstrap.min.css' rel='stylesheet' id='bootstrap-css'><style type='text/css'></style> <script src='//cdnjs.cloudflare.com/ajax/libs/jquery/3.2.1/jquery.min.js'></script> <script src='//maxcdn.bootstrapcdn.com/bootstrap/4.0.0/js/bootstrap.min.js'></script> <style>.main-bg{position:relative;margin:0px auto;margin-top:0px;width:960px}</style></head><body><div class='container'><div class='row'><div class='col'><div class='main-bg'><div class='certificate-bg' style=' background:url({BackGroundCer}); -webkit-print-color-adjust: exact;'><div class='align-content-around'><table style='text-align: center; height: 646px; width: 877px'><tbody><tr><td colspan='3'><div class='logo' style='margin: 5px; position: absolute;top: 10px; left: 38%;'> <img src='{logo}' style='width: 240px' /></div><h2 style='font-size: 69px;margin-top: 84px;color: #0d0b0c;margin-top: 1.49em;margin-bottom: 0px;margin-left: 73px;'>𝕮𝖊𝖗𝖙𝖎𝖋𝖎𝖈𝖆𝖙𝖊</h2><h6 style='font-size: 19px; margin-top:0px;margin-left: 73px;font-family:arial;'>OF BUSINESS CORRESPONDENT AGENT</h6><p style='font-size: 18px;margin-bottom: 10px;text-align: left;line-height: 32px;margin-top: 55px;margin-left: 50px;font-family: arial;'> This is to certify that <b style='font-weight:500'>{BCName}</b> BC ID <b>115580</b> with Pan Number <b>{Pannumber}</b> his/her place of operation<b> {UserAddress}</b> has been appointed as Business Correspondent Agent (BCA)  through <b>{companyName}</b> who is Business Correspondent (BC) of the Bank. The BCA has been duly trained during Training Session held on <b>{joiningDate}</b> and has been authorized to act as BCA of the Bank with effect from <b>{joiningDate}</b> until further notice.</p></td></tr><tr><td style='text-align: left; width: 50%; padding-bottom: 65px; padding-left: 4em'><p style='margin-top:16px;margin-bottom:0;'> <img src='{logo2}' style='width:170px;margin-left:73px;' /></p><p style='margin-top:0'> <img src='{tapits}' style='width: 139px;margin-left: 90px;' /></p></td><td style='text-align: left; width: 50%; padding-bottom: 65px; padding-left: 2em'><div style='text-align: right;padding-right: 5.6em;'> <img src='{sign}' style='width: 77px;'></div><p style='text-align: right;padding-right: 92px;font-size: 16px;line-height: 20px;font-weight: 300;padding-top: 0;margin-top: 0;font-family:arial;'> For <b>{companyName}</b> <br> <b>{OwnerName}</b>,<br> <b>{OwnerDesignation}</b></p></td></tr></tbody></table></div></div></div></div></div></div></body></html>");
            }
            else
            {

                sb.Append(@"<html><body style='padding:1.5em;text-align:center'><div class='bg-img' style='background:#ebebeb;padding:1em; z-index: -2; position: relative;'><img src='{side}' style='position: absolute;width: 50%; right: 1px; bottom: 0; z-index: -1;' /><div class='main text-center'style='background:#fff;text-align:center;z-index:10'><table style='text-align:center'><tbody><tr><td colspan='3'><h2 style='font-size:60px;margin-top:60px;color:#0672a0;margin-bottom:35px;'>𝕸𝖊𝖗𝖈𝖍𝖆𝖓𝖙 𝕮𝖊𝖗𝖙𝖎𝖋𝖎𝖈𝖆𝖙𝖊</h2><p style='font-size:25px;margin-bottom:20px;'>This is to certify that <b>{BCName}</b> having his place operation at {Address}  for {companyName}. has been appointed as bussiness correspondent Agent (BCA) of ICICI Bank Ltd. The BCA is duty trained during Traning session & has been  authorised to act as a BCA of the bank with effect from {joiningDate} until further notice.</p></td></tr><tr><td style='text-align:left;width: 26%;padding-bottom:65px;padding-left:2em'><p>Merchant code<br/>{OutletId}</p><img src='{tapits}' style='width:92px'/></td><td style='text-align:center;padding-bottom:65px'><img src='{logo2}' style='width:170px'/></td><td style='text-align:right;padding-bottom:65px;padding-right:2em'><img src='{sign}' style='width:92px'/><p>For {companyName}<br/>{OwnerName}<br/>{OwnerDesignation}</p></td>
            </tr></tbody></table></div></div></body></html>");

            }

            sb.Replace("{companyName}", res.Name);
            sb.Replace("{OwnerName}", res.OwnerName);
            sb.Replace("{OwnerDesignation}", res.OwnerDesignation);
            sb.Replace("{Address}", res.Address);
            sb.Replace("{mobileNo}", res.mobileNo);
            sb.Replace("{PhoneNo}", res.PhoneNo);
            sb.Replace("{Pannumber}", res.Pan);
            sb.Replace("{EmailId}", res.EmailId);
            sb.Replace("{website}", res.website);
            sb.Replace("{logo}", DOCType.Logo.Replace("{0}", res.WID.ToString()));
            sb.Replace("{logo2}", DOCType.CertificateFooterImage.Replace("{0}", res.WID.ToString()));
            sb.Replace("{sign}", DOCType.CertificateImgSign.Replace("{0}", res.WID.ToString()));
            sb.Replace("{tapits}", DOCType.CertificateTapitSign.Replace("{0}", res.WID.ToString()));
            sb.Replace("{BackGroundCer}", (DOCType.CertificateBackGroundSign.Replace('\\', '/')).Replace("{0}", res.WID.ToString()));
            sb.Replace("{side}", DOCType.CertificateImgSide);
            sb.Replace("{joiningDate}", res.joiningDate);
            sb.Replace("{BCName}", res.OutletName);
            sb.Replace("{OutletId}", res.OutletID.ToString());
            sb.Replace("{UserAddress}", res.UserAddress);
            return sb.ToString();
        }


        public string IrctcCertificate(IrctcCertificateModel res)
        {
            StringBuilder sb = new StringBuilder();

            var date = Convert.ToDateTime(res.ExpDate);
            
            sb.Append(@"<html><body style='color: black; font-family:Verdana, Geneva, sans-serif; text-align: center;'> 
<div style='border: 5px solid tan; background:url({bg_irctc}) no-repeat ; background-size:cover; padding:10px; height:1400px;'> 
<table width='90%' height='100%' border='0' align='center' cellpadding='0' cellspacing='0'> 
<tr> <td height='390' colspan='2' align='center' valign='bottom'>
<p style='color: #9b5800; font-size: 38px; line-height:55px; font-weight:600' > AUTHORISED RAIL - ETICKETING CENTER OF</p></td></tr>
<tr> <td height='130' colspan='2' align='center'><img src='{logo1}' style='width:350px;object-fit: contain;' /></td></tr>
<tr> <td height='50'><p style=' font-size:20px; line-height:32px;'>IRCTC certificate no.{IrctcID}</p></td>
<td align='right'><p style=' font-size:20px; line-height:32px;'>Date :{DATE_1}</p></td></tr>
<tr> <td height='50'><p style=' font-size:20px; line-height:32px;'>Scheme Under: ISC</p></td><td>&nbsp;</td></tr>
<tr> <td height='200' colspan='2'><p align='justify' style=' font-size:20px; line-height:32px;'> This is to certified that <strong> M/s. {OutletName}</strong> owned by <strong>{Name}</strong> Located at <strong>{Address}</strong>, <strong> Contact : {Mobile}</strong >, <strong> Email : {EmailID}</strong> having <strong> PAN No. : {PAN}</strong> is a Retail service Provider of<strong> M/s. {Company}</strong>(Principal service provider of IRCTC) and is hereby authorized to book Reserved Railway E - tickets as per Rail E-ticketing Terms and conditions using <strong> ICS id : {IrctcID}</strong> for the general public which will be valid for a period of one year from <strong>date : {DATE_2}.</strong></p></td></tr>
<tr> <td height='200' colspan='2' align='right'><img src='{SignLogo}'  style='height:150px;object-fit: contain;' /></td></tr>
<tr> <td colspan='2'><p align='justify' style='font-size:15px; line-height:25px;' > Statutory Warning: In case any of the agent is found VIOLATING IRCTC terms and condition both the <strong>{IrctcID}</strong> and <strong><a href='{URl}' target='_blank'>{URl}</a></strong> shall be subjected to Permanent deactivation and Punitive or any other action as per Railways Act 143 and relevant guidelines issued by IRCTC in this regards.</p></td></tr></table> </div></body></html>");





            sb.Replace("{URl}", res.URl);
            sb.Replace("{IrctcID}", res.IrctcID);
            sb.Replace("{DATE_1}", date.AddDays(-365).ToString("dd MMM yyyy"));
            sb.Replace("{OutletName}", res.OutletName);
            sb.Replace("{Name}", res.OwnerName);
            sb.Replace("{Address}", res.Address);
            sb.Replace("{Mobile}", res.mobileNo);
            sb.Replace("{EmailID}", res.EmailId);
            sb.Replace("{PAN}", res.Pan);
            sb.Replace("{Company}", res.CompanyName);
            sb.Replace("{DATE_2}", date.ToString("dd MMM yyyy"));
            sb.Replace("{logo1}", DOCType.Logo.Replace("{0}", res.WID.ToString()));
            sb.Replace("{SignLogo}", DOCType.CertificateImgSign.Replace("{0}", res.WID.ToString()));
            sb.Replace("{bg_irctc}",DOCType.IrctcCertificateBackGroundSign.Replace('\\', '/').Replace("{0}", res.WID.ToString()));
            

            return sb.ToString();




































            //    sb.Replace("{ companyName}", res.Name);
            //sb.Replace("{OwnerName}", res.OwnerName);
            //sb.Replace("{OwnerDesignation}", res.OwnerDesignation);
            //sb.Replace("{Address}", res.Address);
            //sb.Replace("{mobileNo}", res.mobileNo);
            //sb.Replace("{PhoneNo}", res.PhoneNo);
            //sb.Replace("{Pannumber}", res.Pan);
            //sb.Replace("{EmailId}", res.EmailId);
            //sb.Replace("{website}", res.website);
            //sb.Replace("{logo}", DOCType.Logo.Replace("{0}", res.WID.ToString()));
            //sb.Replace("{logo2}", DOCType.CertificateFooterImage.Replace("{0}", res.WID.ToString()));
            //sb.Replace("{sign}", DOCType.CertificateImgSign.Replace("{0}", res.WID.ToString()));
            //sb.Replace("{tapits}", DOCType.CertificateTapitSign.Replace("{0}", res.WID.ToString()));
            //sb.Replace("{BackGroundCer}", (DOCType.CertificateBackGroundSign.Replace('\\', '/')).Replace("{0}", res.WID.ToString()));
            //sb.Replace("{side}", DOCType.CertificateImgSide);
            //sb.Replace("{joiningDate}", res.joiningDate);
            //sb.Replace("{BCName}", res.OutletName);
            //sb.Replace("{OutletId}", res.OutletID.ToString());
            return sb.ToString();
        }
    }
}