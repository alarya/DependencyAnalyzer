/*----------------------------------------------------------------------
 * MainWindow.xaml.cs - Render view for GUI 
 * Ver 1.0
 * Language - C#, 2013, .Net Framework 4.5
 * Platform - Sony Vaio T14, Win 8.1
 * Application - Dependency Analyzer| Project #4| Fall 2014|
 * Author - Alok Arya (alarya@syr.edu)
 * ---------------------------------------------------------------------
 * 
 * Package Operations:
 * This package renders the view for the client GUI
 * This package receives messsages from a receiver queue from the server
 * This package also receives input from the user and sends requests to the server 
 * 
 * Required Packages:
 * BlockingQueue.cs Client.cs ServerLibrary.cs
 * 
 */
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
using System.Net;

namespace Client_GUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static DependencyAnalyzer.IMessageService proxyServer0;
        static DependencyAnalyzer.IMessageService proxyServer1;
        static string server0;
        static string server1;
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
        //------------------------------handles dequeuing of messages --------------------------------------------------------------//
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
            //results from server 0
            if (msg.src.ToString() == server0)
            {
                ((MainWindow)Application.Current.MainWindow).rsltServer0.Items.Clear();
                string relResult = msg.body;
                XDocument doc = XDocument.Parse(relResult);
                foreach (XElement T in doc.Descendants("file"))
                {
                    int items = ((MainWindow)Application.Current.MainWindow).rsltServer0.Items.Count;
                    ((MainWindow)Application.Current.MainWindow).rsltServer0.Items.Insert(items, T.Attribute("name"));
                    foreach (XElement T1 in T.Descendants("Relationship"))
                    {
                        string S = "  " + T1.Element("type1").Value + ":" + T1.Element("relationship").Value + ":" + T1.Element("type2").Value;
                        int items1 = ((MainWindow)Application.Current.MainWindow).rsltServer0.Items.Count;
                        ((MainWindow)Application.Current.MainWindow).rsltServer0.Items.Insert(items1, S);
                    }
                }
            }
            //results from server 1
            if (msg.src.ToString() == server1)
            {
                ((MainWindow)Application.Current.MainWindow).rsltServer1.Items.Clear();
                string relResult = msg.body;
                XDocument doc = XDocument.Parse(relResult);
                foreach (XElement T in doc.Descendants("file"))
                {
                    int items = ((MainWindow)Application.Current.MainWindow).rsltServer1.Items.Count;
                    ((MainWindow)Application.Current.MainWindow).rsltServer1.Items.Insert(items, T.Attribute("name"));
                    foreach (XElement T1 in T.Descendants("Relationship"))
                    {
                        string S = "  " + T1.Element("type1").Value + ":" + T1.Element("relationship").Value + ":" + T1.Element("type2").Value;
                        int items1 = ((MainWindow)Application.Current.MainWindow).rsltServer1.Items.Count;
                        ((MainWindow)Application.Current.MainWindow).rsltServer1.Items.Insert(items1, S);
                    }
                }
            }

        }
        //--------------------event to handle displaying package dependency results-----------------------------------------------------//
        public static void DisplayDepAnal(DependencyAnalyzer.SvcMsg msg)
        {
            //results from server 0
            if (msg.src.ToString() == server0)
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
            }
            //results from server 1
            if(msg.src.ToString() == server1)
            {
                ((MainWindow)Application.Current.MainWindow).rsltServer1.Items.Clear();
                string DepAnalResult = msg.body;
                XDocument doc = XDocument.Parse(DepAnalResult);
                foreach (XElement T in doc.Descendants("package"))
                {
                    int items = ((MainWindow)Application.Current.MainWindow).rsltServer1.Items.Count;
                    ((MainWindow)Application.Current.MainWindow).rsltServer1.Items.Insert(items, T.Attribute("name"));
                    foreach (XElement T1 in T.Descendants("dependentPackage"))
                    {
                        string S = "   " + T1.Value;
                        int items1 = ((MainWindow)Application.Current.MainWindow).rsltServer1.Items.Count;
                        ((MainWindow)Application.Current.MainWindow).rsltServer1.Items.Insert(items1, S);
                    }
                }
            }
        }
        //--------------------event to handle displaying projectList results-----------------------------------------------------//
        public static void DisplayProjectList(DependencyAnalyzer.SvcMsg msg)
        {
            //results for server 0
            if (msg.src.ToString() == server0)
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

            //results for server 1
            if (msg.src.ToString() == server1)
            {
                ((MainWindow)Application.Current.MainWindow).rsltServer1.Items.Clear();
                string projects = msg.body;
                XDocument doc = XDocument.Parse(projects);
                foreach (XElement T in doc.Descendants("project"))
                {
                    int items = ((MainWindow)Application.Current.MainWindow).rsltServer1.Items.Count;
                    ((MainWindow)Application.Current.MainWindow).rsltServer1.Items.Insert(items, T.Value);
                }
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
            if (host == null)
            {
                try
                {
                    receiverAddress = "http://" + getIP() + ":" + ListenPort.Text + "/MessageService";
                    host = DependencyAnalyzer.MessageClient.CreateServiceChannel(receiverAddress);
                    host.Open();

                    Thread Receiver = new Thread(DequeueMessage);
                    Receiver.IsBackground = true;
                    Receiver.Start();
                    ((MainWindow)Application.Current.MainWindow).___Listen___Button_.IsEnabled = false;
                }
                catch(Exception E)
                {
                    host = null;
                    MessageBox.Show(E.Message);
                }
            }
        }
        //----------------------------get IP address of the client ----------------------------------------------------------------//
        public static string getIP()
        {
            IPHostEntry host;
            string localIP = "?";
            host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily.ToString() == "InterNetwork")
                {
                    localIP = ip.ToString();
                }
            }
            return localIP;
        }
        //----------------Connect to server 0 --------------------------------------------------------------------------------------//
        private void ___Connect0___Connect__Click(object sender, RoutedEventArgs e)
        {
            try
            {
                server0 = Server0Addr.Text + ":" + Server0Port.Text + "/MessageService";
                proxyServer0 = DependencyAnalyzer.MessageClient.CreateClientChannel(server0);
                ((MainWindow)Application.Current.MainWindow).___Connect0___Connect_.IsEnabled = false;
            }
            catch(Exception E)
            {
                MessageBox.Show(E.Message);
            }
        }
        //-------------------------------------------------------------------------------------------------------------------------//

        //---------------Connect to server 1 --------------------------------------------------------------------------------------//
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                server1 = Server1Addr.Text + ":" + Server1Port.Text + "/MessageService";
                proxyServer1 = DependencyAnalyzer.MessageClient.CreateClientChannel(server1);
                ((MainWindow)Application.Current.MainWindow).Connect1.IsEnabled = false;
            }
            catch (Exception E)
            {
                MessageBox.Show(E.Message);
            }
        }
        //-------------------------------------------------------------------------------------------------------------------------//
        
        //---------------Handle Send Request button--------------------------------------------------------------------------------//
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //Message to send to server 0
            if ((bool)Server0chk.IsChecked)
            {
                if (host != null && proxyServer0 != null)
                {
                    sendServer0();
                }
            }
            //Message to send to server 1
            if ((bool)Server1chk.IsChecked)
            {
                if (host != null && proxyServer1 != null)
                {
                    sendServer1();
                }
            }
        }
        //----------------------------Send request to server 1 -------------------------------------------------------------------------//
        private void sendServer1()
        {
            DependencyAnalyzer.SvcMsg msg1 = new DependencyAnalyzer.SvcMsg();

            if ((bool)ProjectList.IsChecked)
                msg1.cmd = DependencyAnalyzer.SvcMsg.Command.ProjectList;
            if ((bool)Relationships.IsChecked)
            {
                msg1.cmd = DependencyAnalyzer.SvcMsg.Command.RelAnal;
                if (((MainWindow)Application.Current.MainWindow).rsltServer1.SelectedItem != null)
                    msg1.body = ((MainWindow)Application.Current.MainWindow).rsltServer1.SelectedItem.ToString();
            }
            if ((bool)PackageDep.IsChecked)
            {
                msg1.cmd = DependencyAnalyzer.SvcMsg.Command.DepAnal;
                if (((MainWindow)Application.Current.MainWindow).rsltServer1.SelectedItem != null)
                    msg1.body = ((MainWindow)Application.Current.MainWindow).rsltServer1.SelectedItem.ToString();
            }
            msg1.src = new Uri(receiverAddress);
            msg1.dst = new Uri(server0);
            try { proxyServer1.PostMessage(msg1); }
            catch { MessageBox.Show("Server 1 is not up"); }
        }
        //-----------------------------------Send request to server 0 -------------------------------------------------------------------//
        private void sendServer0()
        {
            DependencyAnalyzer.SvcMsg msg = new DependencyAnalyzer.SvcMsg();

            if ((bool)ProjectList.IsChecked)
                msg.cmd = DependencyAnalyzer.SvcMsg.Command.ProjectList;
            if ((bool)Relationships.IsChecked)
            {
                msg.cmd = DependencyAnalyzer.SvcMsg.Command.RelAnal;
                if (((MainWindow)Application.Current.MainWindow).rsltServer0.SelectedItem != null)
                    msg.body = ((MainWindow)Application.Current.MainWindow).rsltServer0.SelectedItem.ToString();
            }
            if ((bool)PackageDep.IsChecked)
            {
                msg.cmd = DependencyAnalyzer.SvcMsg.Command.DepAnal;
                if (((MainWindow)Application.Current.MainWindow).rsltServer0.SelectedItem != null)
                    msg.body = ((MainWindow)Application.Current.MainWindow).rsltServer0.SelectedItem.ToString();
            }
            msg.src = new Uri(receiverAddress);
            msg.dst = new Uri(server0);
            try { proxyServer0.PostMessage(msg); }
            catch { MessageBox.Show("Server 0 is not up"); }
        }
    }
}
