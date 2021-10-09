using System;
using System.Collections.Generic;
using System.Text;
using System.Management;
using System.Net.NetworkInformation;
using System.Linq;

namespace VRCEventUtil.Models.Util
{
    public static class DeviceIdentificationHelper
    {
        /// <summary>
        /// BIOSのシリアルナンバーを取得します．
        /// </summary>
        /// <returns></returns>
        public static string GetBIOSSerialNo()
        {
            var scope = new ManagementScope("root\\cimv2");
            scope.Connect();

            var q = new ObjectQuery("select SerialNumber from Win32_BIOS");

            var searcher = new ManagementObjectSearcher(scope, q);
            var co = searcher.Get();

            var lst = co.Cast<ManagementObject>().Select(o => o.GetPropertyValue("SerialNumber").ToString());

            return string.Join("-", lst.ToArray());
        }

        /// <summary>
        /// ドライブのボリュームシリアル番号を取得します．
        /// </summary>
        /// <returns></returns>
        public static string GetVolumeNo(char volumeLetter = 'C')
        {
            ManagementObject mo = new ManagementObject($"Win32_LogicalDisk=\"{volumeLetter}:\"");
            return (string)mo.Properties["VolumeSerialNumber"].Value;
        }

        /// <summary>
        /// MACアドレスを取得します．
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<string> GetMacAddress()
        {
            var interfaces = NetworkInterface.GetAllNetworkInterfaces();
            var lst = interfaces.Select(nif => nif.GetPhysicalAddress().ToString());
            return lst;
        }
    }
}
