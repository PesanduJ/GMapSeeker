using System;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AngleSharp.Dom;
using AngleSharp.Html.Parser;

namespace Gmapseeker
{
    public class EmailExtractor
    {
        private static readonly HttpClient httpClient;
        private static readonly HtmlParser htmlParser;
        private static readonly Regex emailRegex;

        static EmailExtractor()
        {
            HttpClientHandler httpClientHandler = new HttpClientHandler()
            {
                MaxConnectionsPerServer = 20 // Adjust the value based on your requirements
            };
            httpClient = new HttpClient(httpClientHandler);
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/96.0.4664.93 Safari/537.36");

            htmlParser = new HtmlParser();
            emailRegex = new Regex(@"\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Z|a-z]{2,}\b");
        }

        public async Task<string> ExtractEmailFromWebsiteAsync(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return "";
            }

            try
            {
                HttpResponseMessage response = await httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    string htmlContent = await response.Content.ReadAsStringAsync();
                    var document = await htmlParser.ParseDocumentAsync(htmlContent);

                    var extractionTasks = document.QuerySelectorAll("a[href], input[type='email']")
                        .Select(element => Task.FromResult(emailRegex.Match(element.GetAttribute("href") ?? element.GetAttribute("value")).Value))
                        .ToList();

                    while (extractionTasks.Count > 0)
                    {
                        Task<string> completedTask = await Task.WhenAny(extractionTasks);
                        extractionTasks.Remove(completedTask);

                        string emailAddress = await completedTask;
                        if (!string.IsNullOrEmpty(emailAddress))
                        {
                            return emailAddress;
                        }
                    }
                }
                else
                {
                    return "";
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"An error occurred while sending the HTTP request: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
            }

            return "";
        }
    }
}
