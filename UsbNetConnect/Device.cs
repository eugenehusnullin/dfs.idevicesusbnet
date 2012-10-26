using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UsbNetConnect
{
    public class Device
    {
        private IntPtr devPtr = IntPtr.Zero;
        private String deviceIdentifier;

        public Device(IntPtr devPtr)
        {
            this.devPtr = devPtr;
        }

        public void setDevPtr(IntPtr devPtr)
        {
            this.devPtr = devPtr;
        }

        public IntPtr getDevPtr()
        {
            return devPtr;
        }

        public void setDeviceIdentifier(String deviceIdentifier)
        {
            this.deviceIdentifier = deviceIdentifier;
        }

        public String getDeviceIdentifier()
        {
            return deviceIdentifier;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return base.Equals(obj);
            }

            Device d = (Device)obj;
            return devPtr.Equals(d.devPtr);
        }

    }
}
