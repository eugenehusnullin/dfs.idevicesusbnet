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
using System.Windows.Navigation;
using System.Windows.Shapes;
using UsbNetConnect;

namespace FlashGui
{
    /// <summary>
    /// Логика взаимодействия для DeviceItem.xaml
    /// </summary>
    public partial class DeviceItem : UserControl
    {
        private Device device;
        private bool sshReady;

        public DeviceItem()
        {
            InitializeComponent();
        }

        public void setDevice(Device device)
        {
            this.device = device;

            lblDeviceClass.Content = device.getDeviceClass();
            lblDeviceName.Content = device.getDeviceName();
            lblProductVersion.Content = device.getProductVersion();
        }

        public Device getDevice()
        {
            return device;
        }

        internal void setSshReady(bool sshReady)
        {
            this.sshReady = sshReady;

            btnFlash.IsEnabled = sshReady;

            if (sshReady)
            {
                lblInfo.Content = "You can flash this device.";
            }
            else
            {
                lblInfo.Content = "You can't flash this device.";
            }
        }
    }
}
