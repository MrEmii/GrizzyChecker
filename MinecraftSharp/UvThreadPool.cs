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
        private List<UVThread> _threads;
        private bool background;
        private Action _onEndHook = null;
        private int currentThreads = 0;
        private int deadThreads = 0;

        public UVThreadPoll(bool background)
        {
            _threads = new List<UVThread>();
        }

        public void addToQueue(Action<UVThread> queue)
        {
            UVThread t = null;
            t = new UVThread(() => queue(t), background);
            this._threads.Add(t);

            if (this._onEndHook != null) {
                t.addKillHook(endHookHandler);
                this.currentThreads++;
            }
        }

        private void endHookHandler()
        {
            this.deadThreads++;
            
            if (this.deadThreads >= this.currentThreads)
            {
                _onEndHook?.Invoke();
                this.deadThreads = 0;
                return;
            }
            Console.WriteLine(this.deadThreads);
        }
        
        public void setEndHook(Action a)
        {
            this._onEndHook = a;
        }
        
        public void collectGarbage()
        {
            _threads.ToList().ForEach(t =>
            {
                if (t.IsAlive)
                {
                    Debug.WriteLine("[UVPoll] Removed dead thread");
                    _threads.Remove(t);
                }
                else
                {
                    Debug.WriteLine($"[UVPoll] Can't remove thread {t.getState()}");
                }
            });
            this.deadThreads = 0;
            this.currentThreads = 0;
        }

        public void forceKillPool()
        {
            Console.WriteLine("[UVPoll] Force kill pool");
            _threads.ToList().ForEach(t =>
            {
                if (t.IsAlive)
                {
                    t.killThread();
                }

                _threads.Remove(t);
            });
        }
    }

}