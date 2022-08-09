using Euphoria;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EuphoriaClient
{
    public interface INotifier
    {
        public void NotifyProccessExit(DarkBotInstance instance);
        public void NotifyPlayerKilled(DarkBotInstance instance);

        public void NotifyProcessStarted(DarkBotInstance instance);


        public void Log(string log);
        
    }
}
