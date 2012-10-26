using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UsbNetConnect
{
    public class DeviceConnection
    {
        private static object syncRoot = new Object();

        private DeviceCalls deviceCalls;        
        private Device device;        

        private Dictionary<short, PortConnection> portConnections;

        public DeviceConnection(Device device)
        {
            this.device = device;
            deviceCalls = DeviceCalls.getInstance();

            portConnections = new Dictionary<short, PortConnection>();            
        }

        public int connectToPort(short port)
        {
            lock (syncRoot)
            {
                if (portConnections.ContainsKey(port))
                {
                    return portConnections[port].getPort();
                }
                else
                {
                    if (portConnections.Count == 0)
                    {
                        deviceConnect();
                    }

                    PortConnection portCon = new PortConnection(port, device);
                    int listenPort = portCon.start();
                    portConnections.Add(port, portCon);

                    return listenPort;
                }
            }
        }

        public void disconnectFromPort(short port)
        {
            lock (syncRoot)
            {
                if (portConnections.ContainsKey(port))
                {
                    portConnections[port].stop();
                    portConnections.Remove(port);
                    if (portConnections.Count == 0)
                    {
                        deviceDisconnect();
                    }
                }
            }
        }

        public void disconnectFromAllPorts()
        {
            lock (syncRoot)
            {
                foreach (KeyValuePair<short, PortConnection> entry in portConnections)
                {
                    disconnectFromPort(entry.Key);
                }
            }
        }

        private void deviceConnect()
        {
            uint uret = deviceCalls.amDeviceConnect(device.getDevPtr());
            if (uret != 0)
            {
                throw new Exception("ERROR of execution amDeviceConnect, result is " + uret);
            }
        }

        private void deviceDisconnect()
        {
            deviceCalls.amDeviceDisconnect(device.getDevPtr());
        }
    }
}
