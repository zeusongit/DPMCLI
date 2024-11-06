using System.Diagnostics;
using System.Net.Http.Headers;
using System.Reflection;
using System.Security.Cryptography;
using Newtonsoft.Json.Linq;

namespace DynamoPMCLI
{
    internal class HttpCaller
    {
        HttpClient client;

        internal HttpCaller(Dictionary<string, string>? header = null)
        {
            this.client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("User-Agent", "DPMCLI");
            if (header != null && header.Count != 0)
            {
                foreach(var item in header)
                {
                    client.DefaultRequestHeaders.Add(item.Key, item.Value);
                }
            }
        }
        internal string MakeRequest(RequestInfo req)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            try
            {
                var type = req.Method;
                var res = string.Empty;
                var url = req.Url;
                if (req.AuthRequired && !AuthorizeRequest())
                {
                    return string.Empty;
                }
                Constants.Print("--Initiating package request to service..");
                Constants.Log("Making request to: " + url);
                if (type == HttpMethod.Get)
                {
                    if (req.IsDownload)
                    {
                        res = DownloadGetRequest(url);
                    }
                    else
                    {
                        var response = ProcessAsyncGetRequest(req);
                        res = response.Result;
                    }
                }
                else if(type == HttpMethod.Post)
                {
                    var response = ProcessAsyncUpdateRequest(req);
                    res = response.Result;
                }
                else if (type == HttpMethod.Put)
                {
                    var response = ProcessAsyncUpdateRequest(req);
                    res = response.Result;
                }
                return res;
            }
            catch (Exception ex)
            {
                Constants.Print("Failed to make request.");
                Constants.Print(ex.Message);
                Constants.Log(ex.StackTrace);
                return string.Empty;
            }
            finally
            {
                sw.Stop();
                Constants.Print("Request completed in :" + sw.ElapsedMilliseconds + "ms");
            }
        }

        internal string DownloadGetRequest(string url)
        {
            try
            {
                string fileName = "dpmcli-download-" + DateTime.Now.ToString("yyyyMMddHHmmssffff") + ".json";
                var contentBytes = client.GetByteArrayAsync(url).Result;
                MemoryStream stream = new MemoryStream(contentBytes);
                var currentPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                FileStream file = new FileStream(currentPath + @"\" + fileName, FileMode.Create, FileAccess.Write);
                stream.WriteTo(file);
                file.Close();
                stream.Close();
                return "Download Complete!";
            }
            catch (Exception)
            {
                Constants.Print("Failed to download packages.");
                return string.Empty;
            }
        }

        private async Task<string> ProcessAsyncGetRequest(RequestInfo req)
        {
            var res = await client.GetAsync(req.Url);
            return await res.Content.ReadAsStringAsync();
        }
        private async Task<string?> ProcessAsyncUpdateRequest(RequestInfo req)
        {

            if (!string.IsNullOrEmpty(req.PackagePath))
            {
                var hashed_pkg_meta = GetPackageMetadataWithHash(req)?.ToString();
                if (string.IsNullOrEmpty(hashed_pkg_meta)) return null;
                using (MultipartFormDataContent content = new MultipartFormDataContent())
                {
                    using (var fileStream = File.OpenRead(req.PackagePath))
                    using (var fileContent = new StreamContent(fileStream))
                    {                        
                        fileContent.Headers.ContentType = new MediaTypeHeaderValue("text/plain");
                        content.Add(fileContent, "pkg", Path.GetFileName(req.PackagePath));
                        content.Add(new StringContent(hashed_pkg_meta), "pkg_header");

                        if (req.Method == HttpMethod.Post)
                        {
                            using (var response = await client.PostAsync(req.Url, content))
                            {
                                //response.EnsureSuccessStatusCode();
                                return await response.Content.ReadAsStringAsync();
                            }
                        }
                        else if (req.Method == HttpMethod.Put)
                        {
                            using (var response = await client.PutAsync(req.Url, content))
                            {
                                //response.EnsureSuccessStatusCode();
                                return await response.Content.ReadAsStringAsync();
                            }
                        }
                        return null;
                    }
                }
            }
            else {
                return null;
            }

        }
        internal JObject GetPackageMetadataWithHash(RequestInfo req)
        {
            var hash = string.Empty;
            using (var fileStream = File.OpenRead(req.PackagePath))
            {
                hash = GetSHA256Hash(fileStream);
                Constants.Log("SHA256: " + hash);
            }

            using (var metaStream = File.OpenText(req.PackageMetadataPath))
            {
                var pkg_meta = metaStream.ReadToEnd();
                var jObject = JObject.Parse(pkg_meta);
                jObject["file_hash"] = @"" + hash + @"";
                Constants.Log(Environment.NewLine + "Metadata: " + jObject.ToString());
                return jObject;
            }
        }
        private string GetSHA256Hash(FileStream file)
        {
            using (SHA256 mySHA256 = SHA256.Create())
            {
                file.Position = 0;
                // Compute the hash of the fileStream.
                byte[] hashValue = mySHA256.ComputeHash(file);

                return Convert.ToBase64String(hashValue);
            }
        }
        private bool AuthorizeRequest()
        {
            if (Constants.IsAuthenticated)
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Constants.AuthToken);
                return true;
            }
            else 
            {
                Constants.Print("You are not authenticated. Please login first.");
                return false;
            }
        }
    }
}
