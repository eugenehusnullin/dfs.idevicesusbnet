using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UsbNetConnect
{
    class UsbMuxConnectByPortException : Exception
    {
        public UsbMuxConnectByPortException(int errorCode)
            : base("ERROR of execution usbMuxConnectByPort, result is " + errorCode)
        {
        }
    }
}
