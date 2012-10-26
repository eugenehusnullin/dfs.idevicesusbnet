using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UsbNetConnect
{
    public class RegistryUtils
    {
        public static String getiTunesMobileDeviceDLLPath()
        {
            String iTunesMobileDeviceDLLPath;

            iTunesMobileDeviceDLLPath = (String)Registry.GetValue(
                    "HKEY_LOCAL_MACHINE\\SOFTWARE\\Wow6432Node\\Apple Inc.\\Apple Mobile Device Support\\Shared",
                    "iTunesMobileDeviceDLL", null);

            if (String.IsNullOrEmpty(iTunesMobileDeviceDLLPath))
            {
                iTunesMobileDeviceDLLPath = (String)Registry.GetValue(
                    "HKEY_LOCAL_MACHINE\\SOFTWARE\\Apple Inc.\\Apple Mobile Device Support\\Shared",
                    "iTunesMobileDeviceDLL", null);
            }

            if (String.IsNullOrEmpty(iTunesMobileDeviceDLLPath))
            {
                throw new Exception("Don't found registry value iTunesMobileDevice.dll.");
            }

            return iTunesMobileDeviceDLLPath;
        }

        public static String getInstallDirPath()
        {
            String installDir;

            installDir = (String)Registry.GetValue(
                    "HKEY_LOCAL_MACHINE\\SOFTWARE\\Wow6432Node\\Apple Inc.\\Apple Application Support",
                    "InstallDir", null);

            if (String.IsNullOrEmpty(installDir))
            {
                installDir = (String)Registry.GetValue(
                    "HKEY_LOCAL_MACHINE\\SOFTWARE\\Apple Inc.\\Apple Application Support",
                    "InstallDir", null);
            }

            if (String.IsNullOrEmpty(installDir))
            {
                throw new Exception("Don't found registry value InstallDir for Apple Application Support.");
            }

            return installDir;
        }
    }
}
