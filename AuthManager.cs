using System.Diagnostics;
using System.IO.Compression;
using System.Reflection;
using Autodesk.IDSDK;

namespace DynamoPMCLI
{
    internal class AuthManager
    {
        private string ToolDirName = "AutoSignInTool";
        private string IDSDKDirName = "IDSDKInstaller";
        private string AutoSignInToolURL = "https://art-bobcat.autodesk.com/artifactory/team-identity-desktop-nuget/IDSDK/release/1.12.1.0-4e12ff/idservices-autosignin_win_release_intel64_v140.1.12.1.nupkg";
        private string IDSDKInstallerURL = "https://art-bobcat.autodesk.com/artifactory/team-identity-desktop-nuget/IDSDK/release/1.12.1.0-4e12ff/idservices-installer_win_release_intel64_v140.1.12.1.nupkg";
        private string ToolPath = string.Empty;
        private string IDSDKInstallerPath = string.Empty;
        public AuthManager() { 
            ToolPath = Path.Combine([Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), ToolDirName, "bin", "Autodesk", "Autodesk IdSDK", "x64", "Release", "AutoSignIn.exe"]);
            IDSDKInstallerPath = Path.Combine([Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), IDSDKDirName, "bin", "AdskIdentityManager-Installer.exe"]);
        }
        internal void InitiateAutoSignIn()
        {
            var currDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var toolDirPath = Path.Combine(currDir, ToolDirName);

            if (!File.Exists(ToolPath))
            {
                SetupAutoSignInTool(toolDirPath);
            }
            else 
            {
                Console.WriteLine("AutoSignIn tool found.");
            }
            CheckIDSDKInstallation();
            var tkn = GetAutoSignInToolToken();
            if (!string.IsNullOrEmpty(tkn))
            {
                Constants.AuthToken = tkn.Trim();
                Constants.IsAuthenticated = true;
            }
        }
        private void SetupAutoSignInTool(string toolDirPath)
        {
            var toolUrl = AutoSignInToolURL;

            Console.WriteLine("Setting up AutoSignIn tool...");

            DownloadAutoSignInTool(toolUrl, toolDirPath);
            ExtractAutoSignInTool(toolDirPath+".zip", toolDirPath);
        }
        private void DownloadAutoSignInTool(string url, string dest)
        {
            if (!File.Exists(dest + ".zip"))
            {
                Console.WriteLine("Downloading AutoSignIn tool");
                var curlCmd = "curl \"" + url + "\" --output \"" + dest + ".zip\"";
                ExecuteCommand(curlCmd, out _);
                return;
            }
            Console.WriteLine("AutoSignIn tool already downloaded, skipping download...");
        }
        private void SetupIDSDKInstall()
        {
            try
            {
                var installerUrl = IDSDKInstallerURL;
                var currDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                var dest = Path.Combine(currDir, IDSDKDirName);
                if (!File.Exists(dest + ".zip"))
                {
                    Console.WriteLine("Downloading IDSDK");
                    var curlCmd = "curl \"" + installerUrl + "\" --output \"" + dest + ".zip\"";
                    ExecuteCommand(curlCmd, out _);
                }
                else 
                {
                    Console.WriteLine("IDSDKInstaller already downloaded, skipping download...");
                }
                if (Directory.Exists(dest))
                {
                    var dir = new DirectoryInfo(dest);
                    dir.Delete(true);
                }
                Constants.Print("Extracting IDSDK..");
                Directory.CreateDirectory(dest);
                ZipFile.ExtractToDirectory(dest + ".zip", dest);
                if (!File.Exists(IDSDKInstallerPath))
                {
                    Console.WriteLine("IDSDK installer not ready");
                    return;
                }
                Constants.Print("Installing IDSDK..");
                var installCmd = IDSDKInstallerPath + " --mode unattended";
                ExecuteCommand(installCmd, out _);
            }
            catch (Exception)
            {
                Console.WriteLine("IDSDK install failed");
            }
        }
        internal string GetAutoSignInToolToken()
        {
            if (string.IsNullOrEmpty(Constants.User) || string.IsNullOrEmpty(Constants.Pwd) || string.IsNullOrEmpty(Constants.ClientId))
            {
                Console.WriteLine("Cannot process request, please provide login credentials, client-id and optionally, the trust-token to make the request in auto mode.");
                return "";
            }
            Console.WriteLine("Fetching auth token...");
            var tkn = string.Empty;
            if (IsLoggedInAutoSignInCommand())
            {
                tkn = GetTokenAutoSignInCommand();
            }
            else 
            {
                if (string.IsNullOrEmpty(tkn))
                {
                    Console.WriteLine("Token not found. Logging in...");
                    var loginCmd = "\"" + ToolPath + "\"" + " --login --user="+ Constants.User + " --pwd="+ Constants.Pwd + " --trust_token=" + Constants.TrustToken + " --client_id=" + Constants.ClientId;
                    ExecuteCommand(loginCmd, out _);

                    if (IsLoggedInAutoSignInCommand())
                    {
                        tkn = GetTokenAutoSignInCommand();
                    }
                }
            }
            if (!string.IsNullOrEmpty(tkn))
            {
                Console.WriteLine("Auth token generated successfully");
            }
            else
            {
                Console.WriteLine("Error fetching token");
            }
            return tkn;
        }
        private void ExtractAutoSignInTool(string filePath, string dest)
        {
            if (File.Exists(filePath))
            {
                if (Directory.Exists(dest))
                {
                    var dir = new DirectoryInfo(dest);
                    dir.Delete(true);
                }
                Directory.CreateDirectory(dest);
                ZipFile.ExtractToDirectory(filePath, dest);
                if (!File.Exists(ToolPath))
                {
                    Console.WriteLine("AutoSignIn tool not ready");
                }
            }
        }

        private string GetTokenAutoSignInCommand()
        {
            var getTokenCmd = "\"" + ToolPath + "\"" + " --get_token";
            var tkn = ExecuteCommand(getTokenCmd, out _);
            if (!string.IsNullOrEmpty(tkn))
            {
                return tkn;
            }
            return string.Empty;
        }
        private bool IsLoggedInAutoSignInCommand()
        {
            var isLoggedInCmd = "\"" + ToolPath + "\"" + " --is_logged_in";
            var isloggedIn = ExecuteCommand(isLoggedInCmd, out int code);
            return code == 0;
        }
        private bool CheckIDSDKInstallation()
        {
            try
            {
                Constants.Print("Checking if IDSDK is installed..");
                var isLoggedInCmd = "\"" + ToolPath + "\"" + " --is_logged_in";
                var isloggedIn = ExecuteCommand(isLoggedInCmd, out int code);
                return code == 0;
            }
            catch(Exception) 
            {
                Constants.Print("Trying to install IDSDK..");
                SetupIDSDKInstall();
            }
            return false;
        }

        static string ExecuteCommand(string command, out int code)
        {
            string output = string.Empty;
            code = 1;
            try
            {
                int exitCode = 0;
                ProcessStartInfo processInfo;
                Process process;
                string error = string.Empty;

                processInfo = new ProcessStartInfo("cmd.exe", "/c " + command);
                processInfo.CreateNoWindow = true;
                processInfo.UseShellExecute = false;
                // *** Redirect the output ***
                processInfo.RedirectStandardError = true;
                processInfo.RedirectStandardOutput = true;
                process = Process.Start(processInfo);

                var task = Task.Run(() =>
                {
                    process.WaitForExit();
                });
                if (!task.Wait(TimeSpan.FromSeconds(30)))
                {
                    throw new Exception("Auth action timed out.");
                }

                // *** Read the streams ***
                output = process.StandardOutput.ReadToEnd();
                error = process.StandardError.ReadToEnd();

                exitCode = process.ExitCode;

                if (!string.IsNullOrEmpty(error))
                {
                    code = exitCode;
                    //ignore log4cplus warning
                    if (!error.ToLower().Contains("log4cplus"))
                    {
                        Console.WriteLine(error);
                        Console.WriteLine(exitCode.ToString());
                    }
                }
                process.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
            return output;
        }
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
                    idsdk_status_code statusCode = Client.Login();
                    if (Client.IsSuccess(statusCode))
                    {
                        Constants.IsAuthenticated = true;
                        return true;
                    }
                }
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
            return string.Empty;
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
