using Microsoft.AspNetCore.Http;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace RoundpayFinTech
{
    public static class CompressImage
    {
        public static bool Compress_Image(Stream bmp1, string tempImgNameWithPath, int height, int width)
        {
            var image = Image.FromStream(bmp1);
            Image newimg = new Bitmap(image, new Size(width, height));
            ImageCodecInfo jgpEncoder = GetEncoderInfo("image/jpeg");
            // for the Quality parameter category.
            System.Drawing.Imaging.Encoder myEncoder = System.Drawing.Imaging.Encoder.Quality;

            EncoderParameters myEncoderParameters = new EncoderParameters(1);
            EncoderParameter myEncoderParameter = new EncoderParameter(myEncoder, 75L);
            myEncoderParameters.Param[0] = myEncoderParameter;
            try
            {
                newimg.Save(tempImgNameWithPath, jgpEncoder, myEncoderParameters);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static bool CompressImageByPercentage(IFormFile file, string tempImgNameWithPath, long quality = 100L)
        {
            var image = Image.FromStream(file.OpenReadStream());
            Image newimg = new Bitmap(image);
            ImageCodecInfo jgpEncoder = GetEncoderInfo("image/jpeg");
            // for the Quality parameter category.
            Encoder myEncoder = Encoder.Quality;

            EncoderParameters myEncoderParameters = new EncoderParameters(1);
            EncoderParameter myEncoderParameter = new EncoderParameter(myEncoder, quality);
            myEncoderParameters.Param[0] = myEncoderParameter;
            try
            {
                if (File.Exists(tempImgNameWithPath))
                {
                    File.Delete(tempImgNameWithPath);
                }
                newimg.Save(tempImgNameWithPath, jgpEncoder, myEncoderParameters);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        public static byte[] CompressImageByPercentage(byte[] fileInBytes, int percent)
        {
            byte[] returnBytes;
            using (Stream stram = new MemoryStream())
            {
                stram.Write(fileInBytes, 0, fileInBytes.Length);
                var image = Image.FromStream(stram);
                int width = image.Width;
                int height = image.Height;
                Image newimg = new Bitmap(image);
                float hResol = image.HorizontalResolution - (image.HorizontalResolution * percent / 100);
                float vResol = image.VerticalResolution - (image.VerticalResolution * percent / 100);
                int widthResized = width-(width * percent / 100);
                int heightResized = height-(height * percent / 100);
                Bitmap resizedImage = new Bitmap(widthResized, heightResized);
                resizedImage.SetResolution(hResol,vResol);
                using (Graphics gfx = Graphics.FromImage(resizedImage))
                {
                    gfx.DrawImage(image, new Rectangle(0, 0, width, height),
                        new Rectangle(0, 0, image.Width, image.Height), GraphicsUnit.Pixel);
                }
                using (MemoryStream ms = new MemoryStream())
                {
                    resizedImage.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                    returnBytes = ms.ToArray();
                }
                
            }
            return returnBytes;
        }

        public static bool UploadPngImage(IFormFile file, string tempImgNameWithPath, string imageFormat = "png")
        {
            if (file == null)
            {
                return false;
            }
            Image newimg = new Bitmap(Image.FromStream(file.OpenReadStream()));
            try
            {
                if (File.Exists(tempImgNameWithPath))
                {
                    File.Delete(tempImgNameWithPath);
                }
                if (imageFormat.ToLower() == ".jpeg" || imageFormat.ToLower() == ".jpg")
                {
                    newimg.Save(tempImgNameWithPath, ImageFormat.Jpeg);
                }
                else
                {
                    newimg.Save(tempImgNameWithPath, ImageFormat.Png);
                }

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private static ImageCodecInfo GetEncoderInfo(string mimeType)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();
            for (int i = 0; i < codecs.Length; i++)
                if (codecs[i].MimeType == mimeType)
                    return codecs[i];
            return null;
        }
    }
}
