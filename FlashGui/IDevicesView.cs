using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlashGui
{
    interface IDevicesView
    {
        void disconnected(UsbNetConnect.Device device);

        void connected(UsbNetConnect.Device device, bool sshReady);

        void setModel(DevicesModel devicesModel);
    }
}
