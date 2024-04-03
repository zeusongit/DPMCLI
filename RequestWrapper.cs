using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                var value = Utils.GetFlagValue(args, flag);
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
            Constants.Log("Updating data");
            string url = string.Empty;
            string path = string.Empty;
            string metapath = string.Empty;
            var arg = args[1];

            if (Enum.TryParse(arg, out Constants.L2_CLICommands flag))
            {
                url = Constants.DPMSourceLink;
                var value = Utils.GetFlagValue(args, flag);
                switch (flag)
                {
                    case Constants.L2_CLICommands.package:
                        url += "/" + nameof(Constants.L2_CLICommands.package);
                        path = args[2];
                        metapath = args[3];
                        break;
                }
            }

            if (!string.IsNullOrEmpty(url))
            {
                var req = new RequestInfo(url, HttpMethod.Post,true);
                if (!string.IsNullOrEmpty(path))
                {
                    req.PackagePath = path;
                    req.PackageMetadataPath = metapath;
                }
                var response = http.MakeRequest(req);
                Constants.PrintResponse(response);
            }
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