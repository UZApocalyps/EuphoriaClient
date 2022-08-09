using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Euphoria
{
    public class DarkBotInstance
    {
        public ulong discordId;
        public Process process;
        public string username;
        public bool active;
        public ISocketMessageChannel notifier;
    }
}
