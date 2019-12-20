using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Drawing;

namespace Schedule_Mgr
{
    /// <summary>
    /// Interaction logic for ShowQRWindow.xaml
    /// </summary>
    public partial class ShowQRWindow : Window
    {
        private string key, username;
        public ShowQRWindow(string key, string username)
        {
            InitializeComponent();
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            this.key = key;
            this.username = username;
            ImageSource QRCode = GenerateQRCode(key, username);
            QRImage.Source = QRCode;
            tutorialVideo.Source = (new Uri("C:\\Users\\Lidio\\iCloudDrive\\Desktop\\Software_Development\\GP_Booking\\Schedule_Mgr\\GP_Booking\\Schedule_Mgr\\Assets\\blank.mp4"));

        }


        private ImageSource GenerateQRCode(string key, string username)
        {
            string qrString = "otpauth://totp/GP:" + username + "?secret=" + key + "&issuer=GP";
            QRCoder.QRCodeGenerator qrGenerator = new QRCoder.QRCodeGenerator();
            QRCoder.QRCodeData qrCodeData = qrGenerator.CreateQrCode(qrString, QRCoder.QRCodeGenerator.ECCLevel.Q);
            QRCoder.QRCode qrCode = new QRCoder.QRCode(qrCodeData);
            Bitmap qrCodeImage = qrCode.GetGraphic(20);

            using (MemoryStream memory = new MemoryStream()) 
            {
                qrCodeImage.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage bitmapimage = new BitmapImage();
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();

                return bitmapimage;
            }
        }
    }
}
