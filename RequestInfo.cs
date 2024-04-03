using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamoPMCLI
{
    internal class RequestInfo
    {
        internal string Url { get; set; }
        internal string PackagePath { get; set; }
        internal string PackageMetadataPath { get; set; }
        internal HttpMethod Method { get; set; }
        internal bool AuthRequired { get; set; }
        internal bool IsDownload { get; set; }

        public RequestInfo(string url, bool authRequired = false, bool isDownload = false)
        {
            this.Url = url;
            this.Method = HttpMethod.Get;
            this.AuthRequired = authRequired;
            this.IsDownload = isDownload;
        }
        public RequestInfo(string url, HttpMethod method, bool authRequired = false, bool isDownload = false)
        {
            this.Url = url;
            this.Method = method;
            this.AuthRequired = authRequired;
            this.IsDownload = isDownload;
        }
    }
}
