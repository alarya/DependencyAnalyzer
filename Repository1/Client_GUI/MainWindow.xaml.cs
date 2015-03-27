using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Threading;
using System.Xml;
using System.Xml.Linq;

namespace Client_GUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static DependencyAnalyzer.IMessageService proxyServer0;
        static string server0;
        static string receiverAddress;
        static ServiceHost host = null;
        delegate void NewMessage(DependencyAnalyzer.SvcMsg msg);
        event NewMessage reln = new NewMessage(DisplayRelationships);
        event NewMessage projectList = new NewMessage(DisplayProjectList);
        event NewMessage DepAnal = new NewMessage(DisplayDepAnal);
        public MainWindow()
        {
            InitializeComponent();
        }
        //------------------------------handes dequeuing of messages --------------------------------------------------------------//
        public void DequeueMessage()
        {
            while (true)
            {
                DependencyAnalyzer.SvcMsg msg = DependencyAnalyzer.MessageClient.rcvQueue.deQ();
                switch(msg.cmd.ToString())
                { 
                    case "RelAnal":
                    this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, reln, msg);
                    break;
                    case "ProjectList":
                    this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, projectList, msg);
                    break;
                    case "DepAnal":
                    this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, DepAnal, msg);
                    break;
                }
            }
        }
        //--------------------event to handle displaying relationship results-----------------------------------------------------//
        public static void DisplayRelationships(DependencyAnalyzer.SvcMsg msg)
        {
            ((MainWindow)Application.Current.MainWindow).rsltServer0.Items.Clear();
            string relResult = msg.body;
            XDocument doc = XDocument.Parse(relResult);
            foreach(XElement T in doc.Descendants("file"))
            {
                int items = ((MainWindow)Application.Current.MainWindow).rsltServer0.Items.Count;
                ((MainWindow)Application.Current.MainWindow).rsltServer0.Items.Insert(items, T.Attribute("name"));
                  foreach(XElement T1 in T.Descendants("Relationship"))
                  {
                      string S = "  " + T1.Element("type1").Value +  ":" + T1.Element("relationship").Value + ":" + T1.Element("type2").Value ;
                      int items1 = ((MainWindow)Application.Current.MainWindow).rsltServer0.Items.Count;
                      ((MainWindow)Application.Current.MainWindow).rsltServer0.Items.Insert(items1, S);
                  }
            }
        }
        //--------------------event to handle displaying relationship results-----------------------------------------------------//
        public static void DisplayDepAnal(DependencyAnalyzer.SvcMsg msg)
        {
            ((MainWindow)Application.Current.MainWindow).rsltServer0.Items.Clear();
            string DepAnalResult = msg.body;
            XDocument doc = XDocument.Parse(DepAnalResult);
            foreach (XElement T in doc.Descendants("package"))
            {
                int items = ((MainWindow)Application.Current.MainWindow).rsltServer0.Items.Count;
                ((MainWindow)Application.Current.MainWindow).rsltServer0.Items.Insert(items, T.Attribute("name"));
                foreach (XElement T1 in T.Descendants("dependentPackage"))
                {
                    string S = "   " + T1.Value;
                    int items1 = ((MainWindow)Application.Current.MainWindow).rsltServer0.Items.Count;
                    ((MainWindow)Application.Current.MainWindow).rsltServer0.Items.Insert(items1, S);
                }
            }
            Console.Write("\n");

        }
        //--------------------event to handle displaying projectList results-----------------------------------------------------//
        public static void DisplayProjectList(DependencyAnalyzer.SvcMsg msg)
        {
            ((MainWindow)Application.Current.MainWindow).rsltServer0.Items.Clear();
            string projects = msg.body;
            XDocument doc = XDocument.Parse(projects);
            foreach (XElement T in doc.Descendants("project"))
            {
                int items = ((MainWindow)Application.Current.MainWindow).rsltServer0.Items.Count;
                ((MainWindow)Application.Current.MainWindow).rsltServer0.Items.Insert(items, T.Value);
            }
        }
        private void ___Server0Addr__TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void ___Server0___CheckBox__Checked(object sender, RoutedEventArgs e)
        {

        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void ___Listen___Button__Click(object sender, RoutedEventArgs e)
        {
            receiverAddress = "http://localhost:" + ListenPort.Text + "/MessageService";
            host = DependencyAnalyzer.MessageClient.CreateServiceChannel(receiverAddress);
            host.Open();

            Thread Receiver = new Thread(DequeueMessage);
            Receiver.IsBackground = true;
            Receiver.Start();
        }
        public void displayDepResult(DependencyAnalyzer.SvcMsg msg)
        {

        }
        private void ___Connect0___Connect__Click(object sender, RoutedEventArgs e)
        {
            server0 = Server0Addr.Text + ":" + Server0Port.Text + "/MessageService";            
            proxyServer0 = DependencyAnalyzer.MessageClient.CreateClientChannel(server0);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (host != null && proxyServer0 != null)
            {
                DependencyAnalyzer.SvcMsg msg = new DependencyAnalyzer.SvcMsg();
                bool projects = (bool)ProjectList.IsChecked;
                if (projects)
                    msg.cmd = DependencyAnalyzer.SvcMsg.Command.ProjectList;
                if ((bool)Relationships.IsChecked)
                {
                    msg.cmd = DependencyAnalyzer.SvcMsg.Command.RelAnal;
                    msg.body = "Analyzer.cs";
                }
                if ((bool)PackageDep.IsChecked)
                {
                    msg.cmd = DependencyAnalyzer.SvcMsg.Command.DepAnal;
                    if (((MainWindow)Application.Current.MainWindow).rsltServer0.SelectedItem != null)
                        msg.body = ((MainWindow)Application.Current.MainWindow).rsltServer0.SelectedItem.ToString();
                }
                msg.src = new Uri(receiverAddress);
                msg.dst = new Uri(server0);
                proxyServer0.PostMessage(msg);
            }
        }
    }
}
