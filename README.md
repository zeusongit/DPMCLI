# Dynamo Package Manager CLI
 Dynamo Package Manager CLI is a tool that let's the user interact with Dynamo Package Manager without any interface from terminal.

### Config file
 There is a config file named `dpm.config` that can be used to configure the app to point to a specific endpoint. This should be in the same directory as the executable.
 These are the default values for the config file:
```
{
  "Source": "https://www.dynamopackages.com",
  "ClientID": "No0wuv5G1sGm3mcMLrVuvS5qKFqxekiN",
  "Token": "<token>"
}
```

 ## Usage

 Build the solution to get dpm.exe or download the latest release from here: https://github.com/zeusongit/DPMCLI/releases/download/v1.0.0.0/DPMCLI_v1.0.0.0.zip, this is the executable that will be used to run DPM commands and make sure you are in the same directory as the executable.

 The commands have the following syntax:

 ```
 dpm.exe <Action> <Item> <Data - optional> <Options - optional>
 ```

## AUTH commands

With the app you can login to your adsk account to obtain a token for authenticated PM calls.

### Login
It will open a login window in your default web browser, after successful login, you can save the token in config file, so that you do not have to login again (recommended).

```
dpm.exe auth login
```

### Logout
This will logout of your current session. Can be used if the current tgoken is expired, or you want to start a new session/token.
```
dpm.exe auth logout
```

### Token
This will return the current token
```
dpm.exe auth token
```

## GET commands

 These commands are used to get package and user information using PM calls, these include mostly un-authenticated calls which do not require a token, but a few like get user's latest packages require a token.

 ### All packages
 This will download a json file (dpmcli-download-xxx.json) with all packages as returned from the service, in the current directory.
 ```
 dpm.exe get packages
 ```

 ### A package
 This will return package details given it's ID or name.
 ```
 dpm.exe get package <ID>
 dpm.exe get package_name <Name>
 ```

 ### User packages
 This will return the package information for the currently logged in user.
 ```
 dpm.exe get user latest_packages
 ```

 ## POST commands

 These commands can be used to perform the allowed CRUD operations on the service.

 ### Publish package
 This command can be used to publish a package.

```
dpm.exe push package <absolute-path-to-package-zip> <absolute-path-to-metadata-file>
```
For this command you need a zip file, that is the content which will be uploaded as the package.
And another json file, that will consist all the metadata related to your package.

Sample Package Metadata File:
```
{
    "contents": "",
    "description": "boiling cuddly wide flaky long narrow package",
    "engine": "dynamo",
    "engine_version": "0.3.1",
    "name": "Package Name",
    "version": "1.0.0",
    "repository_url": "",
    "site_url": "",
    "license": "MIT",
    "keywords": ["key", "word"],
    "copyright_year": "2024",
    "copyright_holder": "DynamoCLI"
}
``` 

`Note: If publish fails try logging out and re-login to refresh the token.`

### Automation

--auto flag can be used for automations, this will download the AutoSignIn Tool and IDSDK (if needed), and provided the correct credentials will post a package to the desired package manager server.

-f file
-m metadata (pkg.json)

```
dpm.exe push package -u <username> -p <password> -cid "<clientid>" -tt <trust-token> --auto -f <path-to-package-zip-file> -m <path-to-pkg.json-file>
```

