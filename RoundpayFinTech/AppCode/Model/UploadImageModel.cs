using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Model
{
    public class UploadImageModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public IFormFile LogoImage { get; set; }
        public IFormFile BgImage { get; set; }
        public IFormFile BannerImage { get; set; }
        public string LogoPath { get; set; }
        public string BgImgPath { get; set; }
        public string BannerImgPath { get; set; }
    }
}
