using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace UsbNetConnect
{
    public class Ws2Functions : IDisposable
    {
        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate int Ws2_32_Send(int socket, byte[] buffer, int length, int flags);
        // int send(_In_  SOCKET s, _In_  const char *buf, _In_  int len, _In_  int flags);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate int Ws2_32_Recv(int socket, byte[] buffer, int length, int flags);
        // int recv(_In_   SOCKET s, _Out_  char *buf, _In_   int len, _In_   int flags);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate int Ws2_32_CloseSocket(int socket);
        // int closesocket(_In_  SOCKET s);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate int Ws2_32_WSAGetLastError();
        //int WSAGetLastError(void);

        public Ws2_32_Send send;
        public Ws2_32_Recv recv;
        public Ws2_32_CloseSocket closeSocket;
        public Ws2_32_WSAGetLastError wsaGetLastError;

        private IntPtr dllPtr = IntPtr.Zero;

        private static Ws2Functions instance = null;
        private static object syncRoot = new Object();

        public static Ws2Functions getInstance()
        {
            if (instance == null)
            {
                lock (syncRoot)
                {
                    if (instance == null)
                    {
                        Ws2Functions ws2 = new Ws2Functions();
                        ws2.initDll();
                        instance = ws2;
                    }
                }
            }
            return instance;
        }

        public void initDll()
        {
            String system32path = Environment.SystemDirectory;
            dllPtr = NativeMethods.LoadLibrary(system32path + @"\ws2_32.dll");
            if (dllPtr == IntPtr.Zero)
            {
                throw new Exception("Not found Ws2_32.dll");
            }

            //Ws2_32_Send
            IntPtr funcPtr = NativeMethods.GetProcAddress(dllPtr, "send");
            if (funcPtr == IntPtr.Zero)
            {
                throw new Exception("Not found send");
            }
            send = (Ws2_32_Send)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(Ws2_32_Send));

            //Ws2_32_Recv
            funcPtr = NativeMethods.GetProcAddress(dllPtr, "recv");
            if (funcPtr == IntPtr.Zero)
            {
                throw new Exception("Not found recv");
            }
            recv = (Ws2_32_Recv)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(Ws2_32_Recv));

            //Ws2_32_CloseSocket
            funcPtr = NativeMethods.GetProcAddress(dllPtr, "closesocket");
            if (funcPtr == IntPtr.Zero)
            {
                throw new Exception("Not found closesocket");
            }
            closeSocket = (Ws2_32_CloseSocket)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(Ws2_32_CloseSocket));

            //Ws2_32_WSAGetLastError
            funcPtr = NativeMethods.GetProcAddress(dllPtr, "WSAGetLastError");
            if (funcPtr == IntPtr.Zero)
            {
                throw new Exception("Not found WSAGetLastError");
            }
            wsaGetLastError = (Ws2_32_WSAGetLastError)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(Ws2_32_WSAGetLastError));
        }

        public void freeLibrary()
        {
            if (dllPtr != IntPtr.Zero)
            {
                NativeMethods.FreeLibrary(dllPtr);
            }
        }

        void IDisposable.Dispose()
        {
            freeLibrary();
        }
    }
}
