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

namespace FlashGui
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class DevicesView : Window, IDevicesView
    {
        private DevicesModel devicesModel;

        public DevicesView()
        {
            InitializeComponent();
        }

        void IDevicesView.connected(UsbNetConnect.Device device, bool sshReady)
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                DeviceItem di = new DeviceItem();
                di.setDevice(device);
                di.setSshReady(sshReady);
                spDevices.Children.Add(di);
            }));
        }

        void IDevicesView.disconnected(UsbNetConnect.Device device)
        {
            this.Dispatcher.Invoke((Action)(() =>
                {
                    DeviceItem di = null;
                    foreach (UIElement element in spDevices.Children)
                    {
                        di = (DeviceItem)element;
                        if (di.getDevice().Equals(device))
                        {
                            break;
                        }
                    }

                    if (di != null)
                    {
                        spDevices.Children.Remove(di);
                    }
                }));
        }

        void IDevicesView.setModel(DevicesModel devicesModel)
        {
            this.devicesModel = devicesModel;
        }

        private void ListView_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
