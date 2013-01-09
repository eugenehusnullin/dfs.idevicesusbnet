using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UsbNetConnect;

namespace FlashGui
{
    class DevicesModel
    {
        private IDevicesView devicesView;
        private DeviceListener deviceListener;
        private List<Device> connectedDevices;

        public DevicesModel()
        {
            connectedDevices = new List<Device>();            
        }

        public void setDevicesView(IDevicesView devicesView)
        {
            this.devicesView = devicesView;
        }

        public IDevicesView getDevicesView()
        {
            return devicesView;
        }

        public void start()
        {
            deviceListener = new DeviceListener();
            deviceListener.connected += new ConnectDeviceEventHandler(connected);
            deviceListener.disconnected += new DisconnectDeviceEventHandler(disconnected);
            deviceListener.startListening();
        }

        public void stop()
        {
            deviceListener.stopListening();
            deviceListener.disconnectFromAllDevices();
        }

        private void connected(object sender, EventArgs e)
        {
            Device device = (Device)sender;
            connectedDevices.Add(device);

            bool sshReady = deviceListener.connectToPort(device, 22) != -1;

            devicesView.connected(device, sshReady);
        }

        private void disconnected(object sender, EventArgs e)
        {
            Device device = (Device)sender;
            connectedDevices.Remove(device);

            devicesView.disconnected(device);
        }
    }
}
