using RoundpayFinTech.AppCode.Model.Shopping.WebShopping.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Model.Shopping
{
    public class FEImage
    {
        public int ID { get; set; }
        public int CategoryID { get; set; }
        public IEnumerable<Menu> Categories { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public EFEImgType ImgType { get; set; }
        public byte ImgTypeID { get; set; }
        public bool IsActive { get; set; }
    }

    public class FEImgList
    {
        public int Statuscode { get; set; }
        public string Msg { get; set; }
        public List<FEImage> ImgList { get; set; }
    }

    public enum EFEImgType : byte
    {
        Banner = 1,
        SideUpper = 2,
        SideLower = 3,
        CategoryOffer = 4,
        PromotionPopUp = 5,
        RightBanner = 6
    }

    public class BannerList
    {
        public List<FEImage> Banners { get; set; }
        public FEImage SideUpper { get; set; }
        public FEImage SideLower { get; set; }
    }
}
