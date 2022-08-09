using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Net;
using Discord;
using Discord.Commands;
using Discord.Net.WebSockets;
using Discord.WebSocket;
using System.Net;
using System.Diagnostics;
using Newtonsoft.Json;
using System.IO;
using System.Threading;
using EuphoriaClient;
using System.Net.Sockets;

namespace Euphoria
{
    public class Bot
    {
        Thread th;
        string token = File.ReadAllText("./token.txt");
        DiscordSocketClient _client;
        string darkbotpath = File.ReadAllText("./path.txt");
        BotConfig config = JsonConvert.DeserializeObject<BotConfig>(File.ReadAllText("./BotConfig.json"));
        List<DarkBotInstance> darkBotInstances = new List<DarkBotInstance>();
        INotifier notifier;
        public async Task Start(INotifier notifier)
        {
            new Thread(() =>
            {
                BeginReadOutput();
            }).Start();
            this.notifier = notifier;
            _client = new DiscordSocketClient();

            _client.Log += Log;
            _client.Ready += OnReady;


            await _client.LoginAsync(TokenType.Bot, token,false);
            await _client.StartAsync();
            
            // Block this task until the program is closed.
            await Task.Delay(-1);
        }
        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }
        private async Task OnReady()
        {
           
            
            _client.MessageReceived += _client_MessageReceived;
           
        }

        private Task _client_MessageReceived(SocketMessage arg)
        {
            
            string msg = arg.Content;
            ulong author = arg.Author.Id;
            var attachments = arg.Attachments as IReadOnlyCollection<Attachment>;
            
            if (msg.StartsWith("!"))
            {
                //remove first char of string
                
                msg = msg.Substring(1);
                string[] splitted = msg.Split('-');
                string command = splitted[0].Replace(" ","");
                Dictionary<string,string>args = new Dictionary<string,string>();
                for (int i = 1; i < splitted.Length; i++)
                {
                    string type = splitted[i].Split(' ')[0];
                    string value = splitted[i].Split(' ')[1];
                    args.Add(type, value);
                }

                switch (command)
                {
                    case "info":
                        {
                            arg.Channel.SendMessageAsync("Salut je suis Euphoria ! Mon but principal c'est de t'aider dans ton évolution sur le jeu darkorbit." +
                                "Grâce à moi tu peux lancer un bot qui va jouer pour toi sans te soucier de devoir laisser ton ordinateur allumé pour le faire sur ton PC. \n\n" +
                                "Pour commencer il va falloir que tu prépare la configuration du bot (c'est le .json dans darkbot) \n \n" +
                                "Ensuite envoie sur ce channel **ou en message privé** *(c'est plus safe)* le message `!start -username <TonNomDutilisateur> -password <TonMotDePasse> -config <LeNomDeTaConfig>` n'oublie pas de m'envoyer ton fichier de config dans le même message 🤩 \n \n" +
                                "😜 **Ne t'inquiète pas je ne sauvegarde aucun mot de passe! N'hésite pas à supprimer ton message juste après avoir reçut une confirmation si je ne l'ai pas fait moi même !**" +
                                "\n\n\n *currently client version "+config.majorVersion+"."+config.minorVersion+"* \n\n\n -");
                            break;
                        }
                    case "start":
                        {
                            StartBot(arg, args, attachments,arg.Channel);
                            break;
                        }
                    case "clear":
                        {
                            break;
                        }
                    default:
                        break;
                }

            }
            return Task.CompletedTask;
        }

        private Task StartBot(SocketMessage arg,Dictionary<string, string> args, IReadOnlyCollection<Attachment> attachments,ISocketMessageChannel channel)
        {
            try
            {
                foreach (var attachment in attachments)
                {
                    using (var client = new WebClient())
                    {
                        client.DownloadFile(attachment.Url, darkbotpath + @"\configs\" + args["config"] + ".json");
                    }
                }
                channel.SendMessageAsync("Ton bot va démmarrer sous peu merci de ta patience \uD83D\uDC4C");
                File.WriteAllText(darkbotpath + "/file.propreties", "username=" + args["username"] + "\n" + "password=" + args["password"] + "\n");
                
                Process p = startProcess(args);
                
                if (p != null)
                {
                    channel.SendMessageAsync("Ton bot à démmarrer tu le verra bientôt sur les maps \uD83D\uDC4C");
                    DarkBotInstance d = new DarkBotInstance();
                    d.discordId = arg.Author.Id;
                    d.process = p;
                    d.username = arg.Author.Username;
                    d.active = true;
                    d.notifier = arg.Channel;
                    darkBotInstances.Add(d);
                    notifier.NotifyProcessStarted(d);

                }
                else
                {
                    channel.SendMessageAsync("😣 Désolé mais le processus n'a pas pu démarrer. Tu peux essayer à nouveau plus tard.\n" +
                        "Si le problème persiste contactez @UZApocalyps#1570");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            channel.DeleteMessageAsync(arg);
            return Task.CompletedTask;
        }
        public void Stop()
        {
            th.Abort();
        }

        private Process startProcess(Dictionary<string,string> args)
        {
            var processStartInfo = new ProcessStartInfo();

            processStartInfo.WorkingDirectory = darkbotpath;
            

            processStartInfo.FileName = "cmd.exe";

            processStartInfo.Arguments = "/C java -jar ./DarkBot.jar -start -login file.propreties -config "+args["config"]+".json";
            processStartInfo.RedirectStandardOutput = false;
            processStartInfo.UseShellExecute = false;
            Process proc = new Process();
            proc.StartInfo = processStartInfo;
            proc.Start();
            proc.EnableRaisingEvents = true;
            proc.Exited += Proc_Exited;
            return proc;
        }
        private void BeginReadOutput()
        {
            string log = "";
            UdpClient listener = new UdpClient(7504);
            IPEndPoint groupEP = new IPEndPoint(IPAddress.Loopback, 8015);
            while (true)
            {
                byte[] bytes = listener.Receive(ref groupEP);
                log = Encoding.UTF8.GetString(bytes);
                Interpret(log).Start();
                notifier.Log(log);
            }
        }

        private Task Interpret(string log)
        {
            int pid = 0;
            string date = "";
            string[] infos = log.Split(']');
            string message = "";
            if (infos.Length > 0)
            {
                pid = int.Parse(infos[0].Remove(1));
                date = infos[1].Remove(1);
                message = infos[2];
            }
            
            return Task.CompletedTask;
        }

        private void Proc_Exited(object? sender, EventArgs e)
        {
            DarkBotInstance instance = darkBotInstances.Find(x => x.process == sender as Process);
            instance.notifier.SendMessageAsync(" ❌ Le compte '"+instance.username+"' a crash, c'est peut-être dû à une mauvaise configuration ou à des identifiants érronés ❌ ");
            notifier.NotifyProccessExit(darkBotInstances.Find(x=>x.process == sender));
        }

        


        
    }
}
