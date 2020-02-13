using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using ThreadState = System.Threading.ThreadState;

namespace MinecraftSharp
{
    public class UVThreadPoll
    {
        private static List<Thread> _threads;
        private bool background;

        public UVThreadPoll(bool background)
        {
            _threads = new List<Thread>();
        }

        public void addToQueue(Action queue)
        {
            Thread t = new Thread(() => queue());
            t.IsBackground = background;
            t.Start();
            _threads.Add(t);
        }

        public void collectGarbage()
        {
            _threads.ToList().ForEach(t =>
            {
                if (t.ThreadState == ThreadState.Stopped)
                {
                    Console.WriteLine("[UVPoll] Removed dead thread");
                    _threads.Remove(t);
                }
                else
                {
                    Console.WriteLine($"[UVPoll] Can't remove thread {t.ThreadState}");
                }
            });
        }

        public void forceKillPool()
        {
            Console.WriteLine("[UVPoll] Force kill pool");
            _threads.ToList().ForEach(t =>
            {
                if (t.ThreadState != ThreadState.Stopped)
                {
                    t.Interrupt();
                }

                _threads.Remove(t);
            });
        }
    }
}