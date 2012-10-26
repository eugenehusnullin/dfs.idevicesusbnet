using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace UsbNetConnect
{
    public class DeviceCalls : IDisposable
    {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate IntPtr CFStringCreateWithBytes(IntPtr alloc, byte[] bytes, Int32 numBytes, Int32 encoding, bool isExternalRepresentation);
        //CFStringRef CFStringCreateWithBytes(CFAllocatorRef alloc, const UInt8 *bytes, CFIndex numBytes, CFStringEncoding encoding, Boolean isExternalRepresentation);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate IntPtr __CFStringMakeConstantString(byte[] bytes);
        //CFStringRef  __CFStringMakeConstantString(const char *cStr);



        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate uint AMDeviceNotificationSubscribe(am_device_notification_callback callback, uint unused0, uint unused1, uint cookie, ref am_device_notification subscription);
        //__DLLIMPORT mach_error_t AMDeviceNotificationSubscribe(am_device_notification_callback callback, 
        //                        unsigned int unused0, unsigned int unused1, 
        //                        unsigned int cookie, 
        //                        struct am_device_notification **subscription);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate uint AMDeviceNotificationUnsubscribe(am_device_notification subscription);
        //__DLLIMPORT mach_error_t AMDeviceNotificationUnsubscribe(am_device_notification* subscription);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void am_device_notification_callback(ref am_device_notification_callback_info callbackInfo, uint cookie);
        //__DLLIMPORT mach_error_t AMDeviceNotificationUnsubscribe(am_device_notification* subscription);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate IntPtr AMDeviceCopyDeviceIdentifier(IntPtr devPtr);
        //__DLLIMPORT CFStringRef AMDeviceCopyDeviceIdentifier(struct am_device *device);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate IntPtr AMDeviceCopyValue(IntPtr devPtr, IntPtr domain, IntPtr result);
        //__DLLIMPORT CFStringRef AMDeviceCopyValue(struct am_device *device, CFStringRef domain, CFStringRef cfstring);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int AMDeviceGetConnectionID(IntPtr devPtr);
        //__DLLIMPORT unsigned int AMDeviceGetConnectionID(struct am_device *device);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int USBMuxConnectByPort(int connectionId, int iPhone_port_network_byte_order, ref int outHandle);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate uint AMDeviceConnect(IntPtr devPtr);
        //__DLLIMPORT mach_error_t AMDeviceConnect(struct am_device *device);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate uint AMDeviceDisconnect(IntPtr devPtr);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int AMDeviceGetInterfaceType(IntPtr devPtr);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int AMDeviceIsPaired(IntPtr devPtr);
        //__DLLIMPORT mach_error_t AMDeviceIsPaired(struct am_device *device);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int AMDevicePair(IntPtr devPtr);
	    //__DLLIMPORT mach_error_t AMDevicePair(struct am_device *device);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int AMDeviceValidatePairing(IntPtr devPtr);
        //__DLLIMPORT mach_error_t AMDeviceValidatePairing(struct am_device *device);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int AMDeviceStartSession(IntPtr devPtr);
        //AMDeviceStartSession

        [StructLayout(LayoutKind.Sequential)]
        public struct am_device
        {
            public IntPtr unknown0;
            public int deviceId;
            public int productId;
            public IntPtr serial;
            public int unknown1;
            public int unknown2;
            public int lockdown_conn;
            public IntPtr unknown3;
            public int unknown4;
            public IntPtr unknown5;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct am_device_notification_callback_info
        {
            public IntPtr devPtr;
            public int msg;
            //public IntPtr subscriptionPtr;
        };

        [StructLayout(LayoutKind.Sequential)]
        public struct am_device_notification
        {
            public uint unknown0;
            public uint unknown1;
            public uint unknown2;
            public am_device_notification_callback callback;
            public uint cookie;
        }

        private IntPtr dllPtr = IntPtr.Zero;
        private IntPtr dllCoreFoundationPtr = IntPtr.Zero;

        public AMDeviceNotificationSubscribe amDeviceNotificationSubscribe;
        public AMDeviceNotificationUnsubscribe amDeviceNotificationUnsubscribe;
        public AMDeviceCopyDeviceIdentifier amDeviceCopyDeviceIdentifier;
        public AMDeviceCopyValue amDeviceCopyValue;
        public AMDeviceGetConnectionID amDeviceGetConnectionID;
        public USBMuxConnectByPort usbMuxConnectByPort;
        public AMDeviceConnect amDeviceConnect;
        public AMDeviceDisconnect amDeviceDisconnect;
        public AMDeviceGetInterfaceType amDeviceGetInterfaceType;
        public AMDeviceIsPaired amDeviceIsPaired;
        public AMDevicePair amDevicePair;
        public AMDeviceValidatePairing amDeviceValidatePairing;
        public AMDeviceStartSession amDeviceStartSession;

        public CFStringCreateWithBytes cfStringCreateWithBytes;
        public __CFStringMakeConstantString __cfStringMakeConstantString;

        private static DeviceCalls instance = null;
        private static object syncRoot = new Object();

        public static DeviceCalls getInstance()
        {
            if (instance == null)
            {
                lock (syncRoot)
                {
                    if (instance == null)
                    {
                        String iTunesMobileDeviceDllPath = RegistryUtils.getiTunesMobileDeviceDLLPath();
                        String installDirPath = RegistryUtils.getInstallDirPath();

                        if (String.IsNullOrEmpty(iTunesMobileDeviceDllPath) || String.IsNullOrEmpty(installDirPath))
                        {
                            throw new Exception("Path to itunes dll not found, reinstall itunes");
                        }

                        add2PathEnv(installDirPath);

                        DeviceCalls dc = new DeviceCalls();
                        dc.initDll(iTunesMobileDeviceDllPath);
                        dc.initCoreFoundationDll(installDirPath + "\\CoreFoundation.dll");

                        instance = dc;
                    }
                }
            }
            return instance;
        }

        private static void add2PathEnv(String installDirPath)
        {
            string path = Environment.GetEnvironmentVariable("path");
            path += ";" + installDirPath;
            Environment.SetEnvironmentVariable("path", path);
        }

        private void initDll(String iTunesMobileDeviceDLLPath)
        {
            dllPtr = NativeMethods.LoadLibrary(iTunesMobileDeviceDLLPath);

            if (dllPtr == IntPtr.Zero)
            {
                throw new Exception("Cannot load dll: " + iTunesMobileDeviceDLLPath);
            }

            // AMDeviceNotificationSubscribe
            IntPtr funcPtr = NativeMethods.GetProcAddress(dllPtr, "AMDeviceNotificationSubscribe");
            if (funcPtr == IntPtr.Zero)
            {
                throw new Exception("Not found AMDeviceNotificationSubscribe");
            }
            amDeviceNotificationSubscribe = (AMDeviceNotificationSubscribe)Marshal
                .GetDelegateForFunctionPointer(funcPtr, typeof(AMDeviceNotificationSubscribe));

            // AMDeviceNotificationUnsubscribe
            funcPtr = NativeMethods.GetProcAddress(dllPtr, "AMDeviceNotificationUnsubscribe");
            if (funcPtr == IntPtr.Zero)
            {
                throw new Exception("Not found AMDeviceNotificationUnsubscribe");
            }
            amDeviceNotificationUnsubscribe = (AMDeviceNotificationUnsubscribe)Marshal
                .GetDelegateForFunctionPointer(funcPtr, typeof(AMDeviceNotificationUnsubscribe));

            // AMDeviceCopyDeviceIdentifier
            funcPtr = NativeMethods.GetProcAddress(dllPtr, "AMDeviceCopyDeviceIdentifier");
            if (funcPtr == IntPtr.Zero)
            {
                throw new Exception("Not found AMDeviceCopyDeviceIdentifier");
            }
            amDeviceCopyDeviceIdentifier = (AMDeviceCopyDeviceIdentifier)Marshal
                .GetDelegateForFunctionPointer(funcPtr, typeof(AMDeviceCopyDeviceIdentifier));
            //Console.WriteLine("AMDeviceCopyDeviceIdentifier address = {0:X}", funcPtr.ToInt32());

            // AMDeviceCopyValue
            funcPtr = NativeMethods.GetProcAddress(dllPtr, "AMDeviceCopyValue");
            if (funcPtr == IntPtr.Zero)
            {
                throw new Exception("Not found AMDeviceCopyValue");
            }
            amDeviceCopyValue = (AMDeviceCopyValue)Marshal
                .GetDelegateForFunctionPointer(funcPtr, typeof(AMDeviceCopyValue));

            // AMDeviceGetConnectionID
            funcPtr = NativeMethods.GetProcAddress(dllPtr, "AMDeviceGetConnectionID");
            if (funcPtr == IntPtr.Zero)
            {
                throw new Exception("Not found AMDeviceGetConnectionID");
            }            
            amDeviceGetConnectionID = (AMDeviceGetConnectionID)Marshal
                .GetDelegateForFunctionPointer(funcPtr, typeof(AMDeviceGetConnectionID));

            // USBMuxConnectByPort
            funcPtr = NativeMethods.GetProcAddress(dllPtr, "USBMuxConnectByPort");
            if (funcPtr == IntPtr.Zero)
            {
                throw new Exception("Not found USBMuxConnectByPort");
            }
            usbMuxConnectByPort = (USBMuxConnectByPort)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(USBMuxConnectByPort));

            // AMDeviceConnect
            funcPtr = NativeMethods.GetProcAddress(dllPtr, "AMDeviceConnect");
            if (funcPtr == IntPtr.Zero)
            {
                throw new Exception("Not found AMDeviceConnect");
            }
            amDeviceConnect = (AMDeviceConnect)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(AMDeviceConnect));

            // AMDeviceDisconnect
            funcPtr = NativeMethods.GetProcAddress(dllPtr, "AMDeviceDisconnect");
            if (funcPtr == IntPtr.Zero)
            {
                throw new Exception("Not found AMDeviceDisconnect");
            }
            amDeviceDisconnect = (AMDeviceDisconnect)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(AMDeviceDisconnect));

            //AMDeviceGetInterfaceType
            funcPtr = NativeMethods.GetProcAddress(dllPtr, "AMDeviceGetInterfaceType");
            if (funcPtr == IntPtr.Zero)
            {
                throw new Exception("Not found AMDeviceGetInterfaceType");
            }
            amDeviceGetInterfaceType = (AMDeviceGetInterfaceType)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(AMDeviceGetInterfaceType));

            //AMDeviceIsPaired
            funcPtr = NativeMethods.GetProcAddress(dllPtr, "AMDeviceIsPaired");
            if (funcPtr == IntPtr.Zero)
            {
                throw new Exception("Not found AMDeviceIsPaired");
            }
            amDeviceIsPaired = (AMDeviceIsPaired)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(AMDeviceIsPaired));

            //AMDevicePair
            funcPtr = NativeMethods.GetProcAddress(dllPtr, "AMDevicePair");
            if (funcPtr == IntPtr.Zero)
            {
                throw new Exception("Not found AMDevicePair");
            }
            amDevicePair = (AMDevicePair)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(AMDevicePair));

            //AMDeviceValidatePairing
            funcPtr = NativeMethods.GetProcAddress(dllPtr, "AMDeviceValidatePairing");
            if (funcPtr == IntPtr.Zero)
            {
                throw new Exception("Not found AMDeviceValidatePairing");
            }
            amDeviceValidatePairing = (AMDeviceValidatePairing)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(AMDeviceValidatePairing));

            //AMDeviceStartSession
            funcPtr = NativeMethods.GetProcAddress(dllPtr, "AMDeviceStartSession");
            if (funcPtr == IntPtr.Zero)
            {
                throw new Exception("Not found AMDeviceStartSession");
            }
            amDeviceStartSession = (AMDeviceStartSession)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(AMDeviceStartSession));
        }

        private void initCoreFoundationDll(String coreFoundationDllPath)
        {
            dllCoreFoundationPtr = NativeMethods.LoadLibrary(coreFoundationDllPath);

            if (dllCoreFoundationPtr == IntPtr.Zero)
            {
                throw new Exception("Cannot load dll: " + coreFoundationDllPath);
            }

            // CFStringCreateWithBytes
            IntPtr funcPtr = NativeMethods.GetProcAddress(dllCoreFoundationPtr, "CFStringCreateWithBytes");
            if (funcPtr == IntPtr.Zero)
            {
                throw new Exception("Not found CFStringCreateWithBytes");
            }
            cfStringCreateWithBytes = (CFStringCreateWithBytes)Marshal
                .GetDelegateForFunctionPointer(funcPtr, typeof(CFStringCreateWithBytes));

            //__CFStringMakeConstantString
            funcPtr = NativeMethods.GetProcAddress(dllCoreFoundationPtr, "__CFStringMakeConstantString");
            if (funcPtr == IntPtr.Zero)
            {
                throw new Exception("Not found __CFStringMakeConstantString");
            }
            __cfStringMakeConstantString = (__CFStringMakeConstantString)Marshal
                .GetDelegateForFunctionPointer(funcPtr, typeof(__CFStringMakeConstantString));
        }

        private void freeLibrary()
        {
            if (dllPtr != IntPtr.Zero)
            {
                NativeMethods.FreeLibrary(dllPtr);
            }

            if (dllCoreFoundationPtr != IntPtr.Zero)
            {
                NativeMethods.FreeLibrary(dllCoreFoundationPtr);
            }
        }

        void IDisposable.Dispose()
        {
            freeLibrary();
        }
    }
}
