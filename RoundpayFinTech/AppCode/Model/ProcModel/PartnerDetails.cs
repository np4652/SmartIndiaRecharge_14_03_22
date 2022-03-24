using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Model.ProcModel
{
    public class PartnerCreate
    {
		public int ID { get; set; }

		public string APIUserName { get; set; }
		public int UserID { get; set; }
		public string Name { get; set; }
		public string FatherName { get; set; }
		public DateTime DOB { get; set; }
		public string OutletName { get; set; }
		public string MobileNo { get; set; }
		public string EmailID { get; set; }
		public string PAN { get; set; }
		public string AADHAR { get; set; }
		public string CompanyPAN { get; set; }
		public string GSTIN { get; set; }
		public string AuthPersonName { get; set; }
		public string AuthPersonAADHAR { get; set; }
		public string CurrentAccountNo { get; set; }
		public string Address { get; set; }

		public string City { get; set; }

		public string State { get; set; }
		public string Block { get; set; }
		public string District { get; set; }
		public string Pincode { get; set; }
		public string Banner { get; set; }

		public string Logo { get; set; }
		public bool IsActive { get; set; }

		public int Status { get; set; }
	}


	public class PartnerListResp
	{
		public int Statuscode { get; set; }
		public string Msg { get; set; }

		public int RoleID { get; set; }
		public List<PartnerCreate> list { get; set; }
	}

	public class PartnerDetailsResp
	{
		public int Statuscode { get; set; }
		public string Msg { get; set; }

		public int RoleID { get; set; }

		
		public PartnerCreate data { get; set; }
		public class RegisterAsChildPartnerResp
		{
			public int UserId { get; set; }
			public int Statuscode { get; set; }
			public string Msg { get; set; }
			public int RoleID { get; set; }
			public string PSAId { get; set; }
			public string FatherName { get; set; }
		}
	}
}
