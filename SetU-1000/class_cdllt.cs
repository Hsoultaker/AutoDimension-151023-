using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace UserManagement
{
    class class_cdllt
    {
        //C prototype:BOOL InitiateLock(unsigned long uSerial);
        [DllImport("cdll8.dll", CharSet = CharSet.Ansi)]
        public static extern int InitiateLock(uint uSerial);

        //C prototype:BOOL Lock32_Function(unsigned long uRandomNum,unsigned long *puRet,unsigned long uSerial);
        [DllImport("cdll8.dll", CharSet = CharSet.Ansi)]
        public static extern int Lock32_Function(int uRandomNum, ref int puRet, uint uSerial);

        //BOOL Counter(char *pszPsd,unsigned long uSerial,unsigned long uMini,unsigned long uSign,unsigned long *uCount);
        [DllImport("cdll8.dll", CharSet = CharSet.Ansi)]
        public static extern int Counter(string pszPsd, uint uSerial, uint uMini,uint uSign, ref ulong uCount);

        //BOOL SetLock(unsigned long uFunction, unsigned long *puParam,unsigned long uParam,char *pszParam,char *pszPsd,unsigned long uSerial,unsigned long uMini);
        [DllImport("cdll8.dll", CharSet = CharSet.Ansi)]
        unsafe public static extern int SetLock(int functioncode, ref ulong puParam, uint uParam, string pass, string password, uint con, uint mini);

        [DllImport("cdll8.dll", CharSet = CharSet.Ansi)]
        public static extern void UnShieldLock();

        //BOOL ReadLock(unsigned long uAddr,void *pBuffer,char *pszPsd,unsigned long uSerial,unsigned long uMini);
        [DllImport("cdll8.dll", CharSet = CharSet.Ansi)]
        unsafe public static extern int ReadLock(int uAddr, void* pBuffer, String pass, uint uSerial, uint uMini);
        [DllImport("cdll8.dll", CharSet = CharSet.Ansi)]
        unsafe public static extern int ReadLockEx(int uAddr, void* pBuffer,uint length, String pass, uint uSerial, uint uMini);

        //BOOL WriteLock(unsigned long uAddr,void *Buffer,char *pszPsd,unsigned long uSerial,unsigned long uMini);
        [DllImport("cdll8.dll", CharSet = CharSet.Ansi)]
        unsafe public static extern int WriteLock(int uAddr, void* pBuffer, string pszPsd, uint uSerial, uint uMini);
        [DllImport("cdll8.dll", CharSet = CharSet.Ansi)]
        unsafe public static extern int WriteLockEx(int uAddr, void* pBuffer,uint length, string pszPsd, uint uSerial, uint uMini);

        //BOOL ReadTime(time_t *ptTime,char *pszPsd,unsigned long uSerial,unsigned long uMini);
        [DllImport("cdll8.dll", CharSet = CharSet.Ansi)]
        public static extern int ReadTime(ref UInt32 time_t, string pszPsd, uint uSerial, uint uMini);

        //BOOL ResetTime(char* pszPsd,unsigned long uSerial,unsigned long uMini);
        [DllImport("cdll8.dll", CharSet = CharSet.Ansi)]
        public static extern int ResetTime(string pszPsd, uint uSerial, uint uMini);

        /*unsigned long ShieldPC(unsigned long uRandomNum,
								 unsigned short int uKey1,
								 unsigned short int uKey2,
								 unsigned short int uKey3,
								 unsigned short int uKey4);
         
        [DllImport("cdll8.dll", CharSet = CharSet.Ansi)]
        public static extern uint ShieldPC(uint Rand, uint key1, uint key2, uint key3, uint key4);*/

        [DllImport("cdll8.dll", CharSet = CharSet.Ansi)]
        public static extern uint LYFGetLastErr();

        /*
         BOOL SetLedTime(In unsigned short Led_on , 
		                unsigned short Led_Time, 
		                unsigned char flag, 
		                const char* password, 
		                unsigned long uSerial,
		                unsigned long uMini);
        */
        [DllImport("cdll8.dll",CharSet = CharSet.Ansi)]
        public static extern int SetLedTime(UInt32 Led_on, UInt32 Led_Time,int flag,string pszPsd, UInt32 uSerial, UInt32 uMini);

        public static int ShieldPc(int x, int key1, int key2, int key3, int key4)
        {
            int ReturnData;
            int y, y1, y2, x1, x2, y11, y22;
            int outdata1, outdata2;

            outdata2 = Convert.ToInt32((x & 0xffff0000) >> 16);
            outdata1 = Convert.ToInt32(x & 0x0000ffff);

            x1 = outdata1;
            x2 = outdata2;

            y1 = (x1 ^ key2);
            y11 = (x2 ^ key1);
            y1 &= 0xffff;
            y11 &= 0xffff;

            y1 = (y1 + y11);
            y1 &= 0xffff;
            if (y1 > 32767)
            {
                y1 = (y1 - 32767);
                y1 &= 0xffff;
            }

            y1 = (y1 ^ 16);
            y1 &= 0xffff;
            if (y1 > 32767)
            {
                y1 = (y1 - 32767);
                y1 &= 0xffff;
            }

            y1 = (y1 % key4);
            y1 &= 0xffff;
            y = (y1 ^ key3);
            y &= 0xffff;
            if (y > 50000)
            {
                y = (y - 50000 - 1);
                y &= 0xffff;
            }

            y11 = (x1 + key1);
            y11 &= 0xffff;
            if (y11 > 32767)
            {
                y11 = (y11 - 32767);
                y11 &= 0xffff;
            }

            y22 = (y11 % key3);
            y22 &= 0xffff;
            y11 = (x2 ^ key4);
            y11 &= 0xffff;
            y2 = (y22 ^ y11);
            y2 &= 0xffff;

            if (y2 > 50000)
            {
                y2 = (y2 - 50000 - 1);
                y2 &= 0xffff;
            }
            y = (y ^ y2);
            y &= 0xffff;


            ReturnData = y2;


            ReturnData = ReturnData << 16;
            ReturnData = ReturnData ^ y;

            return ReturnData;
        }
    }
}

 