using System;
using System.Collections.Generic;
using System.Linq;

namespace MinecraftSharp
{
    public class Data
    {
        public static List<string> AccountsList = new List<string>();
        public static List<string> ProxyList = new List<string>();

        public static List<MinecraftAccount> success_list = new List<MinecraftAccount>();
        public static List<MinecraftAccount> error_list = new List<MinecraftAccount>();


        public static int CurrentAccount = 0;

        public static string GetCurrentAccount()
        {
            return AccountsList[CurrentAccount];
        }

        public static string GetRandomProxy()
        {
            var rnd = new Random();
            var index = rnd.Next(0, ProxyList.Count);
            return ProxyList[index];
        }
    }
}