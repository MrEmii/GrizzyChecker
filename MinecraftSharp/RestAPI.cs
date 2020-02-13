using System;
using Newtonsoft.Json;
using xNet;

namespace MinecraftSharp
{
    public class RestAPI
    {
        public static bool checkCapes(string username, string proxy)
        {
            try
            {
                HttpRequest httpRequest = new HttpRequest();
                httpRequest.Proxy = ProxyClient.Parse(ProxyType.Http, proxy);
                CookieDictionary cookies = new CookieDictionary(false);
                httpRequest.Cookies = cookies;
                httpRequest.IgnoreProtocolErrors = true;
                HttpResponse res = httpRequest.Get("http://s.optifine.net/capes/" + username + ".png");

                return res.IsOK;
            }
            catch (HttpException)
            {
                throw;
            }
        }

        public static bool checkChallenges(string bearer, string proxy)
        {
            try
            {
                HttpRequest httpRequest = new HttpRequest();

                httpRequest.Proxy = ProxyClient.Parse(ProxyType.Http, proxy);
                CookieDictionary cookies = new CookieDictionary(false);
                httpRequest.Cookies = cookies;
                httpRequest.IgnoreProtocolErrors = true;
                httpRequest.AllowAutoRedirect = true;
                httpRequest.KeepAlive = true;
                httpRequest.AddHeader(HttpHeader.UserAgent,
                    "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/80.0.3987.87 Safari/537.36");
                httpRequest.AddHeader(HttpHeader.Referer, "https://api.mojang.com/");
                httpRequest.AddHeader("Origin", "https://my.minecraft.net");
                httpRequest.AddHeader("authorization", "Bearer " + bearer);

                var res = httpRequest.Get("https://api.mojang.com/user/security/challenges").ToString();

                dynamic results = JsonConvert.DeserializeObject<dynamic>("{\"res\": " + res + "}", Converter.Settings);


                return results.res.Count > 0;
            }
            catch (HttpException)
            {
                throw;
            }
        }

        public static bool hasMineconCape(string uuid, string accesstoken, string proxy)
        {
            try
            {
                HttpRequest httpRequest = new HttpRequest();

                httpRequest.Proxy = ProxyClient.Parse(ProxyType.Http, proxy);
                CookieDictionary cookies = new CookieDictionary(false);
                httpRequest.Cookies = cookies;
                httpRequest.IgnoreProtocolErrors = true;
                httpRequest.AllowAutoRedirect = true;
                httpRequest.KeepAlive = true;
                httpRequest.AddHeader(HttpHeader.UserAgent,
                    "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/80.0.3987.87 Safari/537.36");
                httpRequest.AddHeader(HttpHeader.Referer, "https://api.mojang.com/");
                httpRequest.AddHeader("Origin", "https://my.minecraft.net");
                httpRequest.AddHeader("authorization", "Bearer " + accesstoken);

                var res = httpRequest.Get("https://api.mojang.com/user/profile/" + uuid + "/cape").ToString();

                dynamic results = JsonConvert.DeserializeObject<dynamic>("{\"res\": " + res + "}", Converter.Settings);


                return results.res.Count > 0;
            }
            catch (HttpException)
            {
                throw;
            }
        }
    }
}