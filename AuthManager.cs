using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.IDSDK;
using Greg.AuthProviders;

namespace DynamoPMCLI
{
    internal class AuthManager
    {
        #region IDSDK methods
        internal bool Login()
        {
            if (IsLoggedIn())
            {
                Constants.IsAuthenticated = true;
                return true;
            }
            else
            {
                if (Initialize())
                {
                    Constants.Print("Logging in...");
                    SpinAnimation.Start(100);
                    idsdk_status_code statusCode = Client.Login();
                    if (Client.IsSuccess(statusCode))
                    {
                        Constants.IsAuthenticated = true;
                        SpinAnimation.Stop();
                        return true;
                    }
                }
                SpinAnimation.Stop();
                return false;
            }
        }
        internal bool IsLoggedIn()
        {
            if (Initialize())
            {
                bool ret = Client.IsLoggedIn();
                return ret;
            }
            return false;
        }
        internal bool Logout()
        {
            if (IsLoggedIn())
            {
                idsdk_status_code statusCode = Client.Logout(idsdk_logout_flags.IDSDK_LOGOUT_MODE_SILENT);
                if (Client.IsSuccess(statusCode))
                {
                    Deinitialize();
                    Constants.IsAuthenticated = false;
                    return true;
                }
            }
            return false;
        }
        #endregion

        #region IDSDK Utilities
        private bool SetProductConfigs(string productLineCode, idsdk_server server, string oauthKey)
        {
            idsdk_status_code bRet = Client.SetProductConfig(oauthKey, "", productLineCode, DateTime.Now.Year.ToString(), "1.2.3.4", server);
            return Client.IsSuccess(bRet);
        }

        /// <summary>
        /// Returns the OAuth2 token for the current session, or an empty string if token is not available.
        /// </summary>
        internal string GetToken()
        {
            idsdk_status_code ret = Client.GetToken(out string strToken);
            if (Client.IsSuccess(ret))
            {
                return strToken;
            }
            return String.Empty;
        }

        private bool Initialize()
        {
            if (Client.IsInitialized()) return true;
            idsdk_status_code bRet = Client.Init();

            if (Client.IsSuccess(bRet))
            {
                if (Client.IsInitialized())
                {
                    try
                    {
                        IntPtr hWnd = Process.GetCurrentProcess().MainWindowHandle;
                        if (hWnd != null)
                        {
                            Client.SetHost(hWnd);
                        }

                        bool ret = GetClientIDAndServer(out idsdk_server server, out string client_id);
                        if (ret)
                        {
                            ret = SetProductConfigs("DPMCLI", server, client_id);
                            Client.SetServer(server);
                            return ret;
                        }
                    }
                    catch (Exception)
                    {
                        return false;
                    }
                }
            }
            return false;
        }
        private bool Deinitialize()
        {
            idsdk_status_code bRet = Client.DeInit();

            if (Client.IsSuccess(bRet))
            {
                return true;
            }
            return false;
        }
        private bool GetClientIDAndServer(out idsdk_server server, out string client_id)
        {
            server = idsdk_server.IDSDK_PRODUCTION_SERVER;

            client_id = Constants.DPMClientID;

            string env = Constants.DPMEnvironment;
            if (!string.IsNullOrEmpty(env))
            {
                if (env.Trim().ToLower() == "stg")
                {
                    server = idsdk_server.IDSDK_STAGING_SERVER;
                }
                else if (env.Trim().ToLower() == "dev")
                {
                    server = idsdk_server.IDSDK_DEVELOPMENT_SERVER;
                }
            }
            return !string.IsNullOrEmpty(client_id);
        }
        #endregion
    }
}
