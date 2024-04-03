namespace DynamoPMCLI
{
    internal class Program
    {
        
        internal static void Main(string[] args)
        {
            RequestWrapper requestWrapper;
            

            if (args.Count() < 2)
            {
                Console.WriteLine("Bad format");
                return;
            }
            else
            {
                requestWrapper = new RequestWrapper();

                Utils.SetFlags(args.Skip(2));
                Utils.SetConfigs(args);

                var command = args[0];
                switch (command)
                {
                    case nameof(Constants.L1_CLICommands.get):
                        Constants.Log("Get API selected");
                        requestWrapper.HandleGetRequests(args);
                        break;
                    case nameof(Constants.L1_CLICommands.push):
                        Constants.Log("Push API selected");
                        requestWrapper.HandleUpdateRequests(args);
                        break;
                    case nameof(Constants.L1_CLICommands.auth):
                        Constants.Log("Auth stuff");
                        requestWrapper.HandleAuthRequests(args);
                        break;
                    default:
                        Console.WriteLine("Bad format");
                        return;
                                    
                }
            }
        }
    }
}