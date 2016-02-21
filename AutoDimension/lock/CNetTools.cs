using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Management;
using System.Management.Instrumentation;
using System.Net;
using System.IO;

namespace AutoDimension
{
    public class CNetTools
    {
        
            [DllImport("Iphlpapi.dll")]
            private static extern int SendARP(Int32 dest, Int32 host, ref Int64 mac, ref Int32 length);
            [DllImport("Ws2_32.dll")]
            private static extern Int32 inet_addr(string ip);
            private static CNetTools mCNetTools;
            public static CNetTools GetInstance()
            {
                if (null == mCNetTools)
                {
                    mCNetTools = new CNetTools();
                    
                }
                return mCNetTools;
            }
            //获取本机的IP
            public string getLocalIP()
            {
                IPHostEntry host;
                string localIP = "?";
                host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (IPAddress ip in host.AddressList)
                {
                    if (ip.AddressFamily.ToString() == "InterNetwork")
                    {
                        localIP = ip.ToString();
                        break;
                    }
                }
                return localIP;
            }
            //获取本机的MAC
            public string getLocalMac()
            {
                string mac = null;
                ManagementObjectSearcher query = new ManagementObjectSearcher("SELECT * FROM Win32_NetworkAdapterConfiguration");
                ManagementObjectCollection queryCollection = query.Get();
                foreach (ManagementObject mo in queryCollection)
                {
                    if (mo["IPEnabled"].ToString() == "True")
                        mac = mo["MacAddress"].ToString();
                }
                return (mac);
            }

            //获取远程主机IP
            public string[] getRemoteIP(string RemoteHostName)
            {
                IPHostEntry ipEntry = Dns.GetHostByName(RemoteHostName);
                IPAddress[] IpAddr = ipEntry.AddressList;
                string[] strAddr = new string[IpAddr.Length];
                for (int i = 0; i < IpAddr.Length; i++)
                {
                    strAddr[i] = IpAddr[i].ToString();
                }
                return (strAddr);
            }
            //获取远程主机MAC
            public string getRemoteMac(string localIP, string remoteIP)
            {
                Int32 ldest = inet_addr(remoteIP); //目的ip
                Int32 lhost = inet_addr(localIP); //本地ip

                try
                {
                    Int64 macinfo = new Int64();
                    Int32 len = 6;
                    int res = SendARP(ldest, 0, ref macinfo, ref len);
                    return Convert.ToString(macinfo, 16);
                }
                catch (Exception err)
                {
                    Console.WriteLine("Error:{0}", err.Message);
                }
                return 0.ToString();
            }
            /// <summary>
            /// 得到外网IP地址
            /// </summary>
            /// <returns></returns>
            public string GetExtranetIP()
            {
                Uri uri = new Uri("http://city.ip138.com/ip2city.asp");
                System.Net.HttpWebRequest req = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(uri);
                req.Method = "get";
                using (Stream s = req.GetResponse().GetResponseStream())
                {
                    using (StreamReader reader = new StreamReader(s))
                    {
                        char[] ch = { '[', ']' };
                        string str = reader.ReadToEnd();
                        System.Text.RegularExpressions.Match m = System.Text.RegularExpressions.Regex.Match(str, @"\[(?<IP>[0-9\.]*)\]");
                        return m.Value.Trim(ch);

                    }
                }
            }

        }
    
}
