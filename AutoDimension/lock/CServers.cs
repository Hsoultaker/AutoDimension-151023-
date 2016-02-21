using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoDimension.ServiceReference;

namespace AutoDimension
{
    public class CServers
    {
        static private DimServiceClient dsc;
        public static DimServiceClient GetServers()
        {
            if (null == dsc)
            {
                dsc = new DimServiceClient();
                
            }
            return dsc;
        }
    }
}
