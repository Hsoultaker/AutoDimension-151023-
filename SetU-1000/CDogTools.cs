using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace UserManagement
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
           pwd = "123456789";
           pwd2 = "876543212345678";
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
       public bool initLock()
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
      
       public bool Validate()
       {
           int lock32, shield;
           lock32 = 0;
           Random rand = new Random();
           int y = rand.Next(-2094967295, 2094967295);

           int ret = class_cdllt.Lock32_Function(y, ref lock32, Serial);
           if (ret != 0)
           {
              // MessageBox.Show(GetLongData(101) + ":" + GetLongData(102) + ":" + GetLongData(103) + ":" + GetLongData(104));
               shield = class_cdllt.ShieldPc(y, (int)GetLongData(101), (int)GetLongData(102), (int)GetLongData(103), (int)GetLongData(104));

               if (lock32 == shield)
               {
                   return true;
               }
           }
           return false;

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

       public bool WriteLock(int addr, int data)
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
       public bool WriteLock( int addr,string str)
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
    }
}
