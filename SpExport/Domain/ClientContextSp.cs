using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SpExport.Domain
{
    public class ClientContextSp : ClientContext
    {
        public ClientContextSp(string url)
            : base(url)
        {

        }
        public override void ExecuteQuery()
        {
   
                base.ExecuteQuery();

          

        }
        protected override void OnExecutingWebRequest(WebRequestEventArgs args)
        {

            base.OnExecutingWebRequest(args);
        }
    }
}
