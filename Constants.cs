using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DynamoPMCLI
{
    internal static class Constants
    {
        private static readonly string ConfigFile = "dpm.config";
        internal static readonly string DefaultDPMSource = "https://dev.dynamopackages.com";
        public static bool IsVerbose { get; set; }
        public static bool SkipPrompts { get; set; }
        public static bool IsAuthenticated { get; set; }
        public static bool IsAuto { get; set; }

        public static string User =string.Empty;
        public static string Pwd = string.Empty;
        public static string ClientId = string.Empty;
        public static string TrustToken = string.Empty;
        public static string MetadataFilePath = string.Empty;
        public static string PackageFilePath = string.Empty;


        private static string dpmSourceLink = string.Empty;
        public static string DPMSourceLink {
            get {
                if (string.IsNullOrEmpty(dpmSourceLink))
                {
                    return DefaultDPMSource;
                }
                return dpmSourceLink; 
            }
            set 
            {
                if (!string.IsNullOrEmpty(value))
                {
                    dpmSourceLink = Utils.RemoveTrailingSlashes(value);
                }
            } 
        }
        public static string AuthToken { get; set; } = string.Empty;
        public static string CustomConfigPath { get; set; } = null;
        public static string ConfigPath 
        {
            get
            {
                var currentPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                var configPath = Path.Combine(currentPath, Constants.ConfigFile);
                return Constants.CustomConfigPath ?? configPath;
            }
        }
        public static string DPMClientID { get; set; } = string.Empty;
        public static string DPMEnvironment { get; set; } = string.Empty;
        public static Configuration Config { get; set; }

        public static JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        public enum L1_CLICommands
        {
            get,
            push,
            auth
        }
        public enum L2_CLICommands
        {
            package,
            packages,
            package_name,
            user,
            login,
            logout,
            token
        }
        public enum L3_CLICommands
        {
            latest_packages,
        }
        public enum Flags_CLICommands
        {
            v,
            verbose,
            t,
            token,
            c,
            config,
            a,
            auto,
            u,
            user,
            p,
            pwd,
            cid,
            clientid,
            tt,
            trusttoken,
            f,
            file,
            m,
            meta,
            server,
        }
        public enum ConfigKeys
        {
            source,
            token,
            client_id,
            environment
        }
        public static void Log(string message)
        {
            if (IsVerbose)
            {
                Console.WriteLine(message);
            }
        }
        public static void Print(string message)
        {
            Console.WriteLine(message);
        }
        public static string PrintJson(string json)
        {
            try
            {
                using var jDoc = JsonDocument.Parse(json);
                return JsonSerializer.Serialize(jDoc, jsonSerializerOptions);
            }
            catch (JsonException)
            {
                return json;
            }
        }
        public static void PrintResponse(string json)
        {
            if(string.IsNullOrEmpty(json))
            {
                return;
            }
            Console.WriteLine("");
            Console.WriteLine("---------------------Response");
            Console.WriteLine(PrintJson(json));
        }

    }
}
