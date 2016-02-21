using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;
using System.Collections;

namespace WcfService
{
    public class Global : System.Web.HttpApplication
    {
       
        protected void Application_Start(object sender, EventArgs e)
        {
            Hashtable hOnline = (Hashtable)System.Web.HttpContext.Current.Application["Online"];
        }

        protected void Session_Start(object sender, EventArgs e)
        {
           string id= Session.SessionID.ToString();
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {

        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {

        }

        protected void Application_Error(object sender, EventArgs e)
        {

        }

        protected void Session_End(object sender, EventArgs e)
        {

            string id = Session.SessionID.ToString();
            if(DimService.UserIDlst.ContainsKey(id))
            {
                DimService.UserIDlst.Remove(id);
            }
        }

        protected void Application_End(object sender, EventArgs e)
        {
           //SSOHelper.GlobalSessionEnd();
        }
    }
}