using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using Microsoft.VisualBasic;

namespace DynamoPMCLI
{
    internal class Utils
    {
        internal static void SetFlags(IEnumerable<string> args)
        {
            if (args.Count() < 1) return;
            var flags = args.Where(x => x.StartsWith("-") || x.StartsWith("--"));
            Console.WriteLine("Setting flags...");
            foreach (var item in flags)
            {
                string itm = string.Empty;
                if(item.Contains("--"))
                {
                    itm = item.Replace("--", "");
                }
                else
                {
                    itm = item.Replace("-", "");
                }
                if (Enum.TryParse(itm, out Constants.Flags_CLICommands flag))
                {
                    var value = GetFlagValue(args, item);
                    switch (flag)
                    {
                        case Constants.Flags_CLICommands.v:
                        case Constants.Flags_CLICommands.verbose:
                            Constants.Print("Set to verbose");
                            Constants.IsVerbose = true;
                            break;
                        case Constants.Flags_CLICommands.server:
                            if (!string.IsNullOrEmpty(value))
                            {
                                Constants.Print("Server set to: " + value);
                                Constants.DPMSourceLink = value;
                            }
                            break;
                        case Constants.Flags_CLICommands.t:
                        case Constants.Flags_CLICommands.token:
                            if (!string.IsNullOrEmpty(value))
                            {
                                Constants.IsAuthenticated = true;
                                Constants.AuthToken = value;
                            }
                            break;
                        case Constants.Flags_CLICommands.c:
                        case Constants.Flags_CLICommands.config:
                            if (!string.IsNullOrEmpty(value))
                            {
                                Constants.CustomConfigPath = value;
                                Constants.Log("Using custom config file: " + value);
                            }
                            break;
                        case Constants.Flags_CLICommands.auto:
                            if (!string.IsNullOrEmpty(value))
                            {
                                Constants.IsAuto = true;
                                Constants.Log("Set to use AutoSignIn Tool");
                            }
                            break;
                        case Constants.Flags_CLICommands.cid:
                        case Constants.Flags_CLICommands.clientid:
                            if (!string.IsNullOrEmpty(value))
                            {
                                Constants.ClientId = value;
                            }
                            break;
                        case Constants.Flags_CLICommands.u:
                        case Constants.Flags_CLICommands.user:
                            if (!string.IsNullOrEmpty(value))
                            {
                                Constants.User = value;
                            }
                            break;
                        case Constants.Flags_CLICommands.p:
                        case Constants.Flags_CLICommands.pwd:
                            if (!string.IsNullOrEmpty(value))
                            {
                                Constants.Pwd = value;
                            }
                            break;
                        case Constants.Flags_CLICommands.tt:
                        case Constants.Flags_CLICommands.trusttoken:
                            if (!string.IsNullOrEmpty(value))
                            {
                                Constants.TrustToken = value;
                            }
                            break;
                        case Constants.Flags_CLICommands.f:
                        case Constants.Flags_CLICommands.file:
                            if (!string.IsNullOrEmpty(value))
                            {
                                Constants.PackageFilePath = value;
                                Constants.Log("Package at: " + value);
                            }
                            break;
                        case Constants.Flags_CLICommands.m:
                        case Constants.Flags_CLICommands.meta:
                            if (!string.IsNullOrEmpty(value))
                            {
                                Constants.MetadataFilePath = value;
                                Constants.Log("Metadata at: " + value);
                            }
                            break;
                    }
                }
            }
        }
        public static string RemoveTrailingSlashes(string str)
        {
            str = str.Trim();
            while (true)
            {
                if (str.EndsWith("/"))
                {
                    str = str.Remove(str.Length - 1);
                }
                else
                {
                    break;
                }
            }
            return str;
        }
        public static string GetFlagValue(IEnumerable<string> args, string flag)
        {
            var argList = args.ToList();
            var index = argList.IndexOf(flag.ToString());
            var value = index == -1 || index == argList.Count - 1 ? string.Empty : argList.ElementAt(index + 1);
            return value;

        }
        internal static void SetConfigs(string[] args)
        {
            try
            {
                Constants.Log("Loading configs.");
                if (Constants.ConfigPath != null)
                {
                    if (File.Exists(Constants.ConfigPath))
                    {
                        LoadConfigs();
                        if (Constants.Config != null)
                        {
                            if (!string.IsNullOrEmpty(Constants.Config.Source))
                            {
                                Constants.DPMSourceLink = Constants.Config.Source;
                                Constants.Log("Package Manager Source set to: " + Constants.DPMSourceLink);
                            }
                            if (!string.IsNullOrEmpty(Constants.Config.Token))
                            {
                                Constants.AuthToken = Constants.Config.Token;
                                Constants.IsAuthenticated = true;
                                Constants.Log("Authenticated");
                            }
                            if (!string.IsNullOrEmpty(Constants.Config.ClientID))
                            {
                                Constants.DPMClientID = Constants.Config.ClientID;
                            }
                            if (!string.IsNullOrEmpty(Constants.Config.Environment))
                            {
                                Constants.DPMEnvironment = Constants.Config.Environment;
                                Constants.Log("Environment set to: " + Constants.Config.Environment);
                            }
                        }
                        else
                        {
                            Constants.Print("Config file was empty");
                        }
                    }
                    else 
                    {
                        Constants.Print("Config file not found");
                    }
                }
            }
            catch (Exception e)
            {
                Constants.Print("Failed while loading configs");
                Constants.Print("Package Manager Source set as: " + Constants.DPMSourceLink);
                Constants.Log(e.Message);
            }
        }

        internal static void LoadConfigs()
        {
            var json = File.ReadAllText(Constants.ConfigPath);
            Constants.Config = JsonSerializer.Deserialize<Configuration>(json);
        }
        internal static void UpdateConfigs()
        {
            var config = JsonSerializer.Serialize(Constants.Config, Constants.jsonSerializerOptions);
            File.WriteAllText(Constants.ConfigPath, config);
            Constants.Print("Config updated at: " + Constants.ConfigPath);
        }

        internal static bool ConfirmPrompt(string message, string[] options = null)
        {
            if(Constants.SkipPrompts) return true;
            if(string.IsNullOrEmpty(message)) return false;

            Constants.Print(message);
            var response = Console.ReadLine();
            Constants.Log("Prompt Response: " + response);
            string[] validResponses = options ?? ["y","yes"];
            if (!string.IsNullOrEmpty(response) && validResponses.Contains(response.ToLower()))
            {
                return true;
            }
            return false;
        }
    }
}
