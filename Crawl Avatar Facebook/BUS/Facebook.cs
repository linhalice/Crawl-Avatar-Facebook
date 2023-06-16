using Leaf.xNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Crawl_Avatar_Facebook.BUS
{
    internal class Facebook
    {

        public static string[] useragents = File.ReadAllLines("Useragent.txt");
        public static string[] proxies = File.ReadAllLines("proxy.txt");

        public static Random random = new Random();

        public static object lockRandom = new object();

        public static bool AvatarDownload(string uid)
        {
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    HttpRequest httpRequest = new HttpRequest();
                    httpRequest.KeepAlive = true;
                    httpRequest.Cookies = new CookieStorage();

                    httpRequest.AddHeader(HttpHeader.Accept, "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3");
                    httpRequest.AddHeader(HttpHeader.AcceptLanguage, "en-US,en;q=0.5");
                    httpRequest.AddHeader("origin", "https://www.facebook.com");
                    httpRequest.AddHeader("sec-fetch-dest", "empty");
                    httpRequest.AddHeader("sec-fetch-mode", "cors");
                    httpRequest.AddHeader("sec-fetch-site", "same-origin");
                    lock (lockRandom)
                    {
                        httpRequest.UserAgent = useragents[random.Next(useragents.Length)];
                        httpRequest.Proxy = Leaf.xNet.HttpProxyC­lient.Parse(proxies[random.Next(proxies.Length)]);
                    }

                    string filePath = $"Avatar\\{uid}.jpg";
                    httpRequest.Get($"https://graph.facebook.com/{uid}/picture?height=1000&access_token=6628568379%7Cc1e620fa708a1d5696fb991c1bde5662").ToFile(filePath);
                    return true;
                }
                catch
                {
                   
                }
            }
            return false;
        }



    }
}
