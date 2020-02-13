using System;
using System.Threading;

namespace MinecraftSharp {
    public class UVThread {
        private readonly Thread _thread;
        public bool IsAlive;
        private Action _onHook = null;
        
        public UVThread(ThreadStart a, bool background){
            this._thread = new Thread(a);
            this._thread.IsBackground = background;
            this._thread.Start();
            this.IsAlive = true;
        }

        public void killThread(){
            this._thread.Interrupt();
            this.IsAlive = false;
            _onHook?.Invoke();
        }

        public void addKillHook(Action a)
        {
            this._onHook = a;
        }
        

        public ThreadState getState()
        {
            return this._thread.ThreadState;
        }
       
    }
}