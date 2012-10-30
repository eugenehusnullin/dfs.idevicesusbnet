using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UsbNetConnect;

namespace ConsoleExample
{
    class Program
    {
        private DeviceListener deviceListener;
        private List<Device> connectedDevices = new List<Device>();
        
        static void Main(string[] args)
        {            
            Program program = new Program();
            program.start();
            
            Console.WriteLine("Press Enter to exit");
            while (true)
            {
                String s = Console.ReadLine();
                if (s.Equals(String.Empty))
                {
                    Console.WriteLine("Exiting...");
                    break;
                }
                else if (s.Equals("stop"))
                {                    
                    program.stop();
                    Console.WriteLine("DeviceListener stoped.");
                } else if (s.Equals("1")) {
                    program.openConnection(program.getDevices()[0]);
                    Console.WriteLine("1 DeviceConnection started.");
                }
                else if (s.Equals("2"))
                {
                    program.closeConnection(program.getDevices()[0]);
                    Console.WriteLine("2 DeviceConnection stoped.");
                }
                else
                {
                    Console.WriteLine("***");
                }
            }

            program.stop();

            Console.WriteLine("Exit complete.");
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

        public List<Device> getDevices()
        {
            return connectedDevices;
        }

        public void openConnection(Device device)
        {
            int port = deviceListener.connectToPort(device, 22);
            Console.WriteLine("Listening port is " + port);
        }

        public void closeConnection(Device device)
        {
            deviceListener.disconnectFromPort(device, 22);
        }

        private void connected(object sender, EventArgs e)
        {
            Device device = (Device)sender;
            connectedDevices.Add(device);

            Console.WriteLine("Connected device!");
        }

        private void disconnected(object sender, EventArgs e)
        {
            Device device = (Device)sender;
            connectedDevices.Remove(device);

            Console.WriteLine("Disconnected device!");
        }
    }
}
