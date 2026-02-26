using System.Runtime.CompilerServices;
using System.Security.AccessControl;
using System.Text;
using System.Web;
using WebServer.Server;
using WebServer.Server.HTTP_Request;
using WebServer.Server.Responses;
using WebServer.Server.Views;

namespace WebServer.demo
{
    public class StartUp
    {
        public static async Task Main()
        {
            await DownloadWebAsTextFile(Form.FileName,
                new string[] { "https://judge.softuni.org/", "https://softuni.org/" });
            var server = new HttpServer(routes =>
            {
                routes
                .MapGet("/", new TextResponse("Hello from the server!"))
                .MapGet("/HTML", new HtmlResponse("<h1>HTML response</h1>"))
                .MapGet("/Redirect", new RedirectResponse("https://softuni.org/"))
                .MapGet("/login", new HtmlResponse(Form.HTML))
                .MapPost("/login", new TextResponse("", AddFormDataAction))
                .MapGet("/Content", new HtmlResponse(Form.DownloadForm))
                .MapPost("/Content", new TextFileResponse(Form.FileName))
                .MapGet("/Cookies", new HtmlResponse("", AddCookiesAction));
            });
            await server.Start();

        }
        private static void AddFormDataAction(
            Request request, Response response)
        {
            response.Body = "";

            foreach (var (key, value) in request.FromData)
            {
                response.Body += $"{key} - {value}";
                response.Body += Environment.NewLine;
            }
        }
        private static async Task<string> DownloadWebSiteContent(string url)
        {
            var httpClient = new HttpClient();
            using (httpClient)
            {
                var response = await httpClient.GetAsync(url);
                var html = await response.Content.ReadAsStringAsync();
                return html.Substring(0, 2000);
            }
        }
        private static async Task DownloadWebAsTextFile(string fileName, string[] urls)
        {
            var downloads = new List<Task<string>>();

            foreach (var url in urls)
            {
                downloads.Add(DownloadWebSiteContent(url));
            }
            var responses = await Task.WhenAll(downloads);

            var responsesString = string.Join(
                Environment.NewLine + new String('-', 100),
                responses);

            await File.WriteAllTextAsync(fileName, responsesString);
        }

        private static void AddCookiesAction(Request request, Response response)
        {
            var requestHasCookies = request.Cookies.Any();
            var bodyText = "";
            if (requestHasCookies)
            {
                var cookieText = new StringBuilder();
                cookieText.AppendLine("<h1>Cookies</h1>");

                cookieText.Append("<table border='1'><th>Name</th><th>Value</th></th>");
                foreach (var cookie in request.Cookies)
                {
                    cookieText.Append("<tr>");
                    cookieText.Append($"<td>{HttpUtility.HtmlEncode(cookie.Name)}</td>");
                    cookieText.Append($"<td>{HttpUtility.HtmlEncode(cookie.Value)}</td>");
                    cookieText.Append("</tr>");
                }
                cookieText.Append("</table>");
                bodyText = cookieText.ToString();
            }

            else
            {
                bodyText = "<h1>Cookies set!</h1>";
            }
            if (!requestHasCookies)
            {
                response.Cookies.Add("My-Cookie", "My-Value");
                response.Cookies.Add("My-Second-Cookie", "My-Second-Value");
            }
        }

    }
}