using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace UsbNetConnect
{
    public delegate void ConnectDeviceEventHandler(object sender, EventArgs e);
    public delegate void DisconnectDeviceEventHandler(object sender, EventArgs e);

    public class DeviceListener
    {
        public event ConnectDeviceEventHandler connected;
        public event DisconnectDeviceEventHandler disconnected;

        private const int ADNCI_MSG_CONNECTED = 1;
        private const int ADNCI_MSG_DISCONNECTED = 2;
        private const int ADNCI_MSG_UNKNOWN = 3;

        private DeviceCalls deviceCalls;
        private List<Device> connectedDevices;
        private DeviceCalls.am_device_notification subscription;
        private bool isrunning = false;
        private static object syncRoot = new Object();

        public void run()
        {
            lock (syncRoot)
            {
                if (isrunning)
                {
                    return;
                }
                else
                {
                    deviceCalls = DeviceCalls.getInstance();
                    connectedDevices = new List<Device>();
                    subscribe();
                    isrunning = true;
                }
            }
        }

        public void stop()
        {
            lock (syncRoot)
            {
                if (!isrunning)
                {
                    return;
                }
                else
                {
                    unsubscribe();
                    isrunning = false;
                }
            }
        }

        private void subscribe()
        {
            DeviceCalls.am_device_notification_callback callback = new DeviceCalls.am_device_notification_callback(subscribeCallback);
            uint unused0 = 0;
            uint unused1 = 0;
            uint cookie = 0;
            subscription = new DeviceCalls.am_device_notification();

            uint result = deviceCalls.amDeviceNotificationSubscribe(callback, unused0, unused1, cookie, ref subscription);
        }

        private void unsubscribe()
        {
            deviceCalls.amDeviceNotificationUnsubscribe(subscription);
        }

        private void subscribeCallback(ref DeviceCalls.am_device_notification_callback_info callbackInfo, uint cookie)
        {
            int interfaceType = deviceCalls.amDeviceGetInterfaceType(callbackInfo.devPtr);
            if (interfaceType == 1)
            {
                switch (callbackInfo.msg)
                {
                    case ADNCI_MSG_CONNECTED:
                        connectedEvent(callbackInfo.devPtr);
                        break;

                    case ADNCI_MSG_DISCONNECTED:
                        disconnectedevent(callbackInfo.devPtr);
                        break;

                    case ADNCI_MSG_UNKNOWN:
                        break;

                    default:
                        break;
                }
            }
        }

        private void connectedEvent(IntPtr devPtr)
        {
            Device device = new Device(devPtr);
            device.setDeviceIdentifier(getDeviceIdentifier(devPtr));

            if (!connectedDevices.Contains(device))
            {
                connectedDevices.Add(device);

                if (connected != null)
                {
                    connected(device, EventArgs.Empty);
                }
            }
        }

        private String getDeviceIdentifier(IntPtr devPtr)
        {
            IntPtr devIdPtr = deviceCalls.amDeviceCopyDeviceIdentifier(devPtr);
            return CFString2String(devIdPtr);
        }

        private String getDeviceName(IntPtr devPtr)
        {
            deviceCalls.amDeviceConnect(devPtr);
            IntPtr domain = String2CFString("DeviceName");
            IntPtr result = deviceCalls.amDeviceCopyValue(devPtr, IntPtr.Zero, domain);
            deviceCalls.amDeviceDisconnect(devPtr);

            if (result != IntPtr.Zero)
            {
                return CFString2String(result);
            }
            return String.Empty;
        }

        private String CFString2String(IntPtr intPtr)
        {
            byte size = Marshal.ReadByte(intPtr, 8);
            byte[] devIdBytes = new byte[size];
            Marshal.Copy(IntPtr.Add(intPtr, 9), devIdBytes, 0, size);
            return Encoding.ASCII.GetString(devIdBytes);
        }

        private IntPtr String2CFString(String value)
        {
            byte[] b = Encoding.ASCII.GetBytes(value);
            return deviceCalls.cfStringCreateWithBytes(IntPtr.Zero, b, b.Length, 0x0600, true);
        }

        private void disconnectedevent(IntPtr devPtr)
        {
            Device device = new Device(devPtr);
            if (connectedDevices.Contains(device))
            {
                connectedDevices.Remove(device);

                if (disconnected != null)
                {
                    disconnected(device, EventArgs.Empty);
                }
            }
        }
    }
}