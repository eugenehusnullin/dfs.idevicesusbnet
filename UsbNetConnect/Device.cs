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
        private String deviceClass;
        private String deviceName;
        private String productVersion;

        public Device(IntPtr devPtr)
        {
            this.devPtr = devPtr;
        }

        public void setDeviceClass(String deviceClass)
        {
            this.deviceClass = deviceClass;
        }

        public String getDeviceClass()
        {
            return deviceClass;
        }

        public void setDeviceName(String deviceName)
        {
            this.deviceName = deviceName;
        }

        public String getDeviceName()
        {
            return deviceName;
        }

        public void setProductVersion(String productVersion)
        {
            this.productVersion = productVersion;
        }

        public String getProductVersion()
        {
            return productVersion;
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

        public override string ToString()
        {
            return deviceName;
        }

    }
}
