using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using ZXing;
using ZXing.QrCode;

namespace DuAnRapChieuPhim.Models
{
    public class QRCodeGenerator
    {
        public static string GenerateQRCode(string content)
        {
            BarcodeWriter barcodeWriter = new BarcodeWriter();
            barcodeWriter.Format = BarcodeFormat.QR_CODE;
            barcodeWriter.Options = new QrCodeEncodingOptions
            {
                Width = 300,
                Height = 300
            };

            var bitmap = barcodeWriter.Write(content);

            // Convert bitmap to base64 string
            using (MemoryStream ms = new MemoryStream())
            {
                bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                byte[] byteImage = ms.ToArray();
                return Convert.ToBase64String(byteImage);
            }
        }

        public static string SaveQRCodeToFile(long id, string orderCode, string serverPath)
        {
            string imagePath = Path.Combine(serverPath, $"IMG\\QRCode_{id}.png");

            BarcodeWriter barcodeWriter = new BarcodeWriter();
            barcodeWriter.Format = BarcodeFormat.QR_CODE;
            barcodeWriter.Options = new QrCodeEncodingOptions
            {
                Width = 300,
                Height = 300
            };

            var bitmap = barcodeWriter.Write(orderCode);

            // Lưu bitmap xuống tập tin
           bitmap.Save(imagePath, System.Drawing.Imaging.ImageFormat.Png);

            return imagePath;
        }
    }
}