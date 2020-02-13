using System;

namespace MinecraftSharp
{
    public class MinecraftAccount
    {
        public State minecraft_state { get; set; }

        public string account { get; set; }

        public bool hasOptifine {get; set;}

        public bool hasMinecon {get; set;}

        public int minecons_cape {get; set;}

        public MinecraftAccount(State state, string acc)
        {
            minecraft_state = state;
            account = acc;
        }

    }

    public enum State : ushort
    {
        nfa = 0,
        sfa = 1,
        fa = 2,
        waiting = 3,
        failed = 404,
        forbidden = 403,
        NULL = 405
    }
}