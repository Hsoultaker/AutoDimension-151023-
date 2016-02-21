using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoDimension.ServiceReference;
using AutoDimension.Entity;
using System.Windows.Forms;
using System.Net;
using System.IO;

namespace AutoDimension
{
   public class CDogTools
    {
       private static CDogTools mDogTools = null;
       public ulong USerial;
       public int UserAuthority;
       private uint Serial;
       private uint mini;
       private string pwd;
       private string pwd2;
       private CDogTools()
       {
           Serial = 0;
           mini = 3;
           pwd2 = "123456789";
           pwd = "876543212345678";
       }
       public static CDogTools GetInstance()
       {
           if (null == mDogTools)
           {
               mDogTools = new CDogTools();
               mDogTools.GetSerial();
           }
           return mDogTools;
       }
       private bool initLock()
       {
           ulong temp = 0;
           int functioncode = 7;
           int ret = class_cdllt.InitiateLock(Serial);//初始化加密锁

           if (ret == 0) return false;

           if (!WriteLock(1, 1))
           {
               ret = class_cdllt.SetLock(functioncode, ref temp, 0, pwd, "abcdefgh", Serial, mini);
               if (ret == 1)
               {
                   ret = class_cdllt.SetLock(functioncode, ref temp, 0, pwd2, "12345678", Serial, mini);
                   if (ret == 0)
                   {
                       return false;
                   }
               }
           }
           return true;
       }
       public ulong GetSerial()
       {
           int  ret=class_cdllt.InitiateLock(Serial);
           USerial = 0;
         
           if (ret != 0)
           {
               ret = class_cdllt.SetLock(8, ref USerial, 0, "", "", Serial, mini);//获取序列号
               if (ret != 0)
               { 
                   return USerial; 
               }
           }
           return 0;
       }
       public int GetUserAuthority()
       {
           UserAuthority = 0;
           try
           {
               string IP1 = CNetTools.GetInstance().getLocalIP();
               string Mac = CNetTools.GetInstance().getLocalMac();
               string IP2 = CNetTools.GetInstance().GetExtranetIP();
               CEncryptionTools encryptionTools = new CEncryptionTools(USerial);
               string UserKey = string.Format("{0};{1};{2}", IP1, Mac, IP2);
               Mac = encryptionTools.Encode(IP1);
               Mac = encryptionTools.Decode(Mac);
               if (GetSerial() > 0 && Validate())
               {
                   CServers.GetServers().GetUserName();
                   string Authority = CServers.GetServers().GetUserAuthority(USerial, encryptionTools.Encode(UserKey));
                   if (Authority != "-1")
                   {
                       int.TryParse(encryptionTools.Decode(Authority), out UserAuthority);
                   }
                   else
                       UserAuthority = -1;

                   string name = CServers.GetServers().GetUserName();

               }
           }
           catch(Exception ex){
               //MessageBox.Show(ex.ToString());
           }
           return UserAuthority;
       }
       public bool Validate()
       {
          
           int lock32, shield;
           lock32 = 0;
           Random rand = new Random();
           int y = rand.Next(-2094967295, 2094967295);

           int ret = class_cdllt.Lock32_Function(y, ref lock32, Serial);
           if (ret != 0)
           {

               shield = class_cdllt.ShieldPc(y, (int)GetLongData(101), (int)GetLongData(102), (int)GetLongData(103), (int)GetLongData(104));

               if (lock32 == shield)
               {
                   return true;
               }
           }
           return false;

       }
       public bool Validate(MrModuleType power,out bool IsOut)
       {
           try
           {
               ulong ID1 = USerial;
               ulong ID2 = GetSerial();
               IsOut = true;
               CServers.GetServers().GetUserName();
               if (ID2 == 0)
               {
                   MessageBox.Show("未检测到加密狗！请确认是否已经插好！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                   return false;
               }
               else if (ID1 != ID2)
               {
                   MessageBox.Show("在使用中不可以更换加密狗！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                   return false;
               }
               else if (Validate())
               {

                   if (UserAuthority > 0)
                   {
                       if ((UserAuthority & ((int)power)) == ((int)power))
                       {
                           IsOut = false;
                           return true;
                       }
                       else
                       {
                           MessageBox.Show("该模块需要另外付费购买才能运行！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                           IsOut = false;
                           return false;
                       }
                   }
               }
               MessageBox.Show("未检测到加密狗！请确认是否已经插好！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
               return false;
           }
           catch
           {
               MessageBox.Show("服务器异常，请确认网络或者加密狗", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
               IsOut = true;
               return false;
           }



       }
       public string GetFLANGEStr()
       {
           string FLANGEStr= "";
           for(int I=101;I<107;I++)
           {
               FLANGEStr += GetStringData(I);
           }
           return FLANGEStr;
           
       }
       public string GetWEBStr()
       {
           string WEBStr = "";
           for (int I = 111; I < 117; I++)
           {
               WEBStr += GetStringData(I);
           }
           return WEBStr;

       }
       public string GetStringData(int addr)
       {
           if (USerial != 0)
           {
               int ret;
               byte[] ch1 = new byte[10];
               unsafe
               {
                   fixed (byte* Ptime = ch1)
                   {
                       ret = class_cdllt.ReadLock(addr, Ptime, pwd, Serial, mini);
                   }
               }
               ASCIIEncoding AE2 = new ASCIIEncoding();
               char[] CharArray = AE2.GetChars(ch1);
               if (ret != 0)
               {

                   string val = string.Format("{0}{1}{2}{3}", CharArray[0], CharArray[1], CharArray[2], CharArray[3]);
                   return val;
               }
           }
           return "";
       }
       public long GetLongData(int addr)
       {
           if (USerial != 0)
           {
               int ret;
               long data;
               unsafe
               {
                   ret = class_cdllt.ReadLock(addr, &data, pwd, Serial, mini);
                   if (ret != 0)
                       return data;

               }
           }
           return 0;
       }

       private bool WriteLock(int data, int addr)
       {
           unsafe
           {
               int ret = class_cdllt.WriteLock(addr, &data, pwd, Serial, mini);
               if (ret == 1)
                   return true;
               else
                   return false;
           }
       }
       private bool WriteLock(string str, int addr)
       {

           uint len = (uint)(str.Length);
           byte[] writeStr = Encoding.Default.GetBytes(str);
           unsafe
           {
               fixed (byte* ps = writeStr)
               {
                   int ret = class_cdllt.WriteLock(addr, ps, pwd, Serial, mini);
                   if (ret == 1)
                       return true;
                   else
                       return false;
               }
           }
       }
       //获取内网IP
       private string GetInternalIP()
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
       //获取外网IP
       private string GetExternalIP()
       {
           string direction = "";
           WebRequest request = WebRequest.Create("http://city.ip138.com/ip2city.asp");
           using (WebResponse response = request.GetResponse())
           {
               using (StreamReader stream = new StreamReader(response.GetResponseStream()))
               {
                   direction = stream.ReadToEnd();
               }
               int first = direction.IndexOf("Address:") + 9;
               int last = direction.LastIndexOf("</body>");
               direction = direction.Substring(first, last - first);
           }
           return direction;
       }
      
    }
}
