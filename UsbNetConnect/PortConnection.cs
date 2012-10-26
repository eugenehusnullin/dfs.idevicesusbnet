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
    class PortConnection
    {
        public const int BUFFER_SIZE = 256;

        private Ws2Functions ws2Functions;
        private DeviceCalls deviceCalls;

        private TcpListener listener;
        private bool isStarted;
        
        private Device device;
        private short port;

        private List<Socket> activeSockets;

        public PortConnection(short port, Device device)
        {
            this.port = port;
            this.device = device;

            deviceCalls = DeviceCalls.getInstance();
            ws2Functions = Ws2Functions.getInstance();

            activeSockets = new List<Socket>();
        }

        public int start()
        {
            isStarted = true;
            startTcpListener();
            new Thread(new ThreadStart(startAcceptSocket)).Start();

            return getPort();
        }

        public void stop()
        {
            isStarted = false;
            listener.Stop();

            foreach (Socket socket in activeSockets)
            {
                if (socket.IsBound)
                {
                    socket.Close();
                }
            }
        }

        public int getPort()
        {
            return ((IPEndPoint)listener.LocalEndpoint).Port;
        }

        private void startTcpListener()
        {
            IPAddress localAddr = IPAddress.Parse("127.0.0.1");
            listener = new TcpListener(localAddr, 0);
            listener.Start();
        }

        private void startAcceptSocket()
        {
            try
            {
                while (isStarted)
                {
                    Socket socket = listener.AcceptSocket();
                    new Thread(() => startConnection(socket)).Start();
                }
            }
            catch (SocketException)
            {
                // A blocking operation was interrupted by a call to WSACancelBlockingCall
            }
        }

        private void startConnection(Socket socket)
        {
            activeSockets.Add(socket);

            Stream stream = new NetworkStream(socket);
            int outHandle = initHandle(port);
            CountdownEvent countdownEvent = new CountdownEvent(2);

            Thread threadToDevice = new Thread(() => toDevice(outHandle, stream, countdownEvent));
            Thread threadFromDevice = new Thread(() => fromDevice(outHandle, stream, countdownEvent));
            
            threadToDevice.Start();
            threadFromDevice.Start();
            countdownEvent.Wait();

            activeSockets.Remove(socket);
            socket.Close();
        }

        private int initHandle(short port)
        {
            short networkByteOrderPort = IPAddress.HostToNetworkOrder(port);
            int connectionID = deviceCalls.amDeviceGetConnectionID(device.getDevPtr());
            int outHandle = -1;

            int errorCode = deviceCalls.usbMuxConnectByPort(connectionID, networkByteOrderPort, ref outHandle);
            if (errorCode != 0)
            {
                throw new UsbMuxConnectByPortException(errorCode);
            }

            return outHandle;
        }

        private void toDevice(int outHandle, Stream stream, CountdownEvent countdownEvent)
        {
            byte[] buffer = new byte[BUFFER_SIZE];
            while (true)
            {
                try
                {
                    int bytesRecv = stream.Read(buffer, 0, BUFFER_SIZE);
                    if (bytesRecv == 0)
                    {
                        break;
                    }
                    ws2Functions.send(outHandle, buffer, bytesRecv, 0);
                }
                catch (System.IO.IOException)
                {
                    break;
                }
            }

            ws2Functions.closeSocket(outHandle);

            countdownEvent.Signal();
        }

        private void fromDevice(int outHandle, Stream stream, CountdownEvent countdownEvent)
        {
            byte[] buffer = new byte[BUFFER_SIZE];
            while (true)
            {
                try
                {
                    int bytesRecv = ws2Functions.recv(outHandle, buffer, BUFFER_SIZE, 0);
                    if (bytesRecv <= 0)
                    {
                        break;
                    }
                    else
                    {
                        stream.Write(buffer, 0, bytesRecv);
                    }
                }
                catch (System.IO.IOException)
                {
                    break;
                }
            }

            countdownEvent.Signal();
        }
    }
}
