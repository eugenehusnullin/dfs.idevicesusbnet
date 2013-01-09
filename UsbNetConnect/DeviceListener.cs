using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
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
        private Dictionary<Device, DeviceConnection> deviceConnections = new Dictionary<Device, DeviceConnection>();

        private DeviceCalls.am_device_notification_callback callback;

        private AutoResetEvent syncUnsubscribe = new AutoResetEvent(false);

        public DeviceListener()
        {
            callback = new DeviceCalls.am_device_notification_callback(subscribeCallback);
        }

        public int connectToPort(Device device, short port)
        {
            lock (syncRoot)
            {
                if (deviceConnections.ContainsKey(device))
                {
                    return deviceConnections[device].connectToPort(port);
                }
                else
                {
                    DeviceConnection dc = new DeviceConnection(device);
                    int openedPort = dc.connectToPort(port);
                    if (openedPort > 0)
                    {
                        deviceConnections.Add(device, dc);
                    }
                    return openedPort;
                }
            }
        }

        public void disconnectFromPort(Device device, short port)
        {
            lock (syncRoot)
            {
                if (deviceConnections.ContainsKey(device))
                {
                    int cntPorts = deviceConnections[device].disconnectFromPort(port);
                    if (cntPorts == 0)
                    {
                        deviceConnections.Remove(device);
                    }
                }
            }
        }

        public void disconnectFromAllPorts(Device device)
        {
            lock (syncRoot)
            {
                if (deviceConnections.ContainsKey(device))
                {
                    deviceConnections[device].disconnectFromAllPorts();
                    deviceConnections.Remove(device);
                }
            }
        }

        public void disconnectFromAllDevices()
        {
            lock (syncRoot)
            {
                foreach (KeyValuePair<Device, DeviceConnection> entry in deviceConnections.ToArray())
                {
                    disconnectFromAllPorts(entry.Key);
                }
            }
        }

        public void startListening()
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

        public void stopListening()
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
            syncUnsubscribe.Reset();
            
            uint unused0 = 0;
            uint unused1 = 0;
            uint cookie = 0;
            subscription = new DeviceCalls.am_device_notification();

            uint result = deviceCalls.amDeviceNotificationSubscribe(callback, unused0, unused1, cookie, ref subscription);
        }

        private void unsubscribe()
        {
            deviceCalls.amDeviceNotificationUnsubscribe(subscription);

            syncUnsubscribe.WaitOne(1000);
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
            else if (interfaceType == -1)
            {
                syncUnsubscribe.Set();
            }
        }

        private void connectedEvent(IntPtr devPtr)
        {
            Device device = new Device(devPtr);
            device.setDeviceIdentifier(getDeviceIdentifier(devPtr));
            device.setDeviceClass(getDeviceValue(devPtr, "DeviceClass"));
            device.setDeviceName(getDeviceValue(devPtr, "DeviceName"));
            device.setProductVersion(getDeviceValue(devPtr, "ProductVersion"));

            if (!connectedDevices.Contains(device))
            {
                connectedDevices.Add(device);

                if (connected != null)
                {
                    connected(device, EventArgs.Empty);
                }
            }
        }

        private void disconnectedevent(IntPtr devPtr)
        {
            Device device = new Device(devPtr);

            if (deviceConnections.ContainsKey(device))
            {
                deviceConnections[device].disconnectFromAllPorts();
                deviceConnections.Remove(device);
            }

            if (connectedDevices.Contains(device))
            {
                connectedDevices.Remove(device);

                if (disconnected != null)
                {
                    disconnected(device, EventArgs.Empty);
                }
            }
        }

        private String getDeviceIdentifier(IntPtr devPtr)
        {
            IntPtr devIdPtr = deviceCalls.amDeviceCopyDeviceIdentifier(devPtr);
            return CFString2String(devIdPtr);
        }

        public String getDeviceValue(IntPtr devPtr, String valueName)
        {
            deviceCalls.amDeviceConnect(devPtr);
            IntPtr domain = String2CFString(valueName);
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
    }
}