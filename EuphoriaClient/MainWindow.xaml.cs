using Euphoria;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EuphoriaClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window,INotifier
    {
        public MainWindow()
        {
            InitializeComponent();
            new Thread(() =>
            {
                Bot b = new Bot();
                b.Start(this);
            }).Start();

        }

        public void Log(string log)
        {
            Dispatcher.Invoke(() => {
                txtLog.Text += log;
            });
        }


        public void NotifyPlayerKilled(DarkBotInstance instance)
        {
            throw new NotImplementedException();
        }

        public void NotifyProccessExit(DarkBotInstance instance)
        {
            Dispatcher.Invoke(() =>
            {
                foreach (Label item in listActiveInstance.Items)
                {
                    item.Foreground = Brushes.Red;
                }
            });
        }

        public void NotifyProcessStarted(DarkBotInstance instance)
        {
            Dispatcher.Invoke(() =>
            {
                Label lbl = new Label();
                lbl.Content = instance.username;
                lbl.Foreground = Brushes.Green;
                listActiveInstance.Items.Add(lbl);
            });
        }
    }
}
