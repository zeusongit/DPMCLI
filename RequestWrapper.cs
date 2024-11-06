using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace DynamoPMCLI
{
    internal class RequestWrapper
    {
        private HttpCaller http;
        private AuthManager authManager;
        internal RequestWrapper()
        {
            this.http = new HttpCaller();
            this.authManager = new AuthManager();
        }
        internal void HandleGetRequests(string[] args)
        {
            Constants.Log("Getting data");
            string url = string.Empty;
            var authRequired = false;
            var isDownload = false;
            var arg = args[1];
            
            if (Enum.TryParse(arg, out Constants.L2_CLICommands flag))
            {
                url = Constants.DPMSourceLink;
                var value = Utils.GetFlagValue(args, flag.ToString());
                switch (flag)
                {
                    case Constants.L2_CLICommands.package:
                        url += "/" + nameof(Constants.L2_CLICommands.package) + "/" + value;
                        break;
                    case Constants.L2_CLICommands.package_name:
                        url += "/" + nameof(Constants.L2_CLICommands.package) + "/dynamo/" + value;
                        break;
                    case Constants.L2_CLICommands.user:
                        url += "/" + nameof(Constants.L2_CLICommands.user) + "/" + nameof(Constants.L3_CLICommands.latest_packages);
                        authRequired = true;
                        break;
                    case Constants.L2_CLICommands.packages:
                        url += "/" + nameof(Constants.L2_CLICommands.packages);
                        isDownload = true;
                        break;
                }
            }

            if (!string.IsNullOrEmpty(url))
            {
                var response = http.MakeRequest(new RequestInfo(url, authRequired, isDownload));
                Constants.PrintResponse(response);
            }
        }
        
        internal void HandleAuthRequests(string[] args)
        {
            Constants.Log("Handling auth requests");
            if (authManager == null) return;
            var arg = args[1];

            if (Enum.TryParse(arg, out Constants.L2_CLICommands flag))
            {
                switch (flag)
                {
                    case Constants.L2_CLICommands.login:
                        if(authManager.IsLoggedIn())
                        {
                            Constants.Print("You are already logged in! Refreshing token.");
                            TryLogout();
                        }
                        TryLogin();
                        break;
                    case Constants.L2_CLICommands.logout:
                        TryLogout();
                        break;
                    case Constants.L2_CLICommands.token:
                        var t = string.IsNullOrEmpty(Constants.AuthToken) ? "You need to login first or re-login to refresh session!" : Constants.AuthToken;
                        Constants.Print(t);
                        break;
                }
            }
        }

        internal void HandleUpdateRequests(string[] args)
        {
            string url = string.Empty;
            string path = string.Empty;
            string metapath = string.Empty;
            var arg = args[1];

            if (Enum.TryParse(arg, out Constants.L2_CLICommands flag))
            {
                url = Constants.DPMSourceLink;
                switch (flag)
                {
                    case Constants.L2_CLICommands.package:
                        path = Constants.PackageFilePath;
                        metapath = Constants.MetadataFilePath;
                        if (Constants.IsAuto)
                        {
                            authManager.InitiateAutoSignIn();
                        }
                        url += "/" + nameof(Constants.L2_CLICommands.package);
                        break;
                }
            }

            try
            {
                if (!string.IsNullOrEmpty(url))
                {
                    var req = new RequestInfo(url, HttpMethod.Post, true);
                    if (!string.IsNullOrEmpty(path) && !string.IsNullOrEmpty(metapath))
                    {
                        req.PackagePath = path;
                        req.PackageMetadataPath = metapath;
                        if (ShouldPublishVersion(req))
                        {
                            req.Method = HttpMethod.Put;
                        }
                        else 
                        {
                            Constants.Print("Could not find any existing package, publishing a new package..");
                        }
                    }
                    else
                    {
                        Constants.Print("Metadata or Package file paths missing.");
                    }
                    var response = http.MakeRequest(req);
                    Constants.PrintResponse(response);
                }
            }
            catch (Exception ex)
            {
                Constants.Print("Failed to update Package. "+ ex.Message);
            }
        }

        private bool ShouldPublishVersion(RequestInfo req)
        {
            try
            {
                var meta = http.GetPackageMetadataWithHash(req);
                if (meta != null && meta["name"] != null)
                {
                    Constants.Print("Checking if package with name " + meta["name"].ToString() + " already exist..");
                    var url = Constants.DPMSourceLink;
                    url += "/" + nameof(Constants.L2_CLICommands.package) + "/dynamo/" + meta["name"].ToString();
                    var response = http.MakeRequest(new RequestInfo(url));
                    if (!string.IsNullOrEmpty(response))
                    {
                        var resp = JObject.Parse(response);
                        if (resp != null && resp["success"] != null && resp["content"] != null && resp["content"]["name"] != null)
                        {
                            var pkgExist = resp["success"].ToString() == "True" && resp["content"]["name"].ToString() == meta["name"].ToString();
                            if (pkgExist)
                            {
                                Constants.Print("Package found, publishing new version (" + meta["version"].ToString() + ")");
                                return true;
                            }
                        }
                    }
                    else
                    {
                        throw new Exception("Unable to validate if the package exist.");
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }

            return false;
        }

        #region local helper methods
        private bool TryLogin()
        {
            if (authManager.Login())
            {
                Constants.Print("Logged in successfully!");
                Constants.AuthToken = authManager.GetToken();
                if (string.IsNullOrEmpty(Constants.AuthToken))
                {
                    Constants.Print("Failed to get token");
                    return false;
                }
                if (Utils.ConfirmPrompt("Do you want to save the token to config? (y/n):"))
                {
                    UpdateConfigToken();
                }

                return true;
            }
            else
            {
                Constants.Print("Failed to log in");
                return false;
            }
        }
        private bool TryLogout()
        {
            if (authManager.Logout())
            {
                Constants.Print("Logged out successfully!");
                return true;
            }
            else
            {
                Constants.Print("Failed to log out");
                return false;
            }
        }
        private void UpdateConfigToken()
        {
            Constants.Config.Token = Constants.AuthToken;
            Utils.UpdateConfigs();
        }
        #endregion local helper methods
    }
}