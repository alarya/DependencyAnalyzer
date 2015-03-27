/*----------------------------------------------------------------------
 * Server1.cs - Server for hosting services of dependency analyzer
 * Ver 1.0
 * Language - C#, 2013, .Net Framework 4.5
 * Platform - Sony Vaio T14, Win 8.1
 * Application - Dependency Analyzer| Project #4| Fall 2014|
 * Author - Alok Arya (alarya@syr.edu)
 * ---------------------------------------------------------------------
 * 
 * Package Operations:
 * This package hosts the service implementation of Dependency analyzer.
 * This server assumes a the root of repository as "../../../Repository1".
 * Place all code to be analyzed by server in repository directory. 
 * This package implements the service contract defined in package ServiceLibrary.cs
 * This package makes calls on DepAnal.cs for processing requests received from the client
 * This package maintains a receiving queue(for receiving messages) defined by BlockingQueue.cs 
 * 
 * Required Packages:
 * BlockingQueue.cs DepAnal.cs ServerLibrary.cs
 * 
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using System.ServiceModel.Channels;
using Utilities;
using System.Xml.Serialization;
using System.IO;
using System.Threading;
namespace DependencyAnalyzer
{
  class Server : IMessageService
  {

    static BlockingQueue<SvcMsg> rcvBlockingQ = null;
    //------------------------Construct------------------------------------------------------------------------------------------//
    static Server()
    {
        if (rcvBlockingQ == null)
            rcvBlockingQ = new BlockingQueue<SvcMsg>();
    }
    static IMessageService CreateClientChannel(string url)
    {
      BasicHttpBinding binding = new BasicHttpBinding();
      EndpointAddress address = new EndpointAddress(url);
      ChannelFactory<IMessageService> factory =
        new ChannelFactory<IMessageService>(binding, address);
      return factory.CreateChannel();
    }
    static ServiceHost CreateServiceChannel(string url)
    {
      BasicHttpBinding binding = new BasicHttpBinding();
      Uri baseAddress = new Uri(url);
      Type service = typeof(Server);
      ServiceHost host = new ServiceHost(service, baseAddress);
      host.AddServiceEndpoint(typeof(IMessageService), binding, baseAddress);
      return host;
    }

    public static string ConvertToXml(object toSerialize)
    {
      string temp;
      XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
      ns.Add(string.Empty, string.Empty);
      var serializer = new XmlSerializer(toSerialize.GetType());
      using (StringWriter writer = new StringWriter())
      {
        serializer.Serialize(writer, toSerialize, ns);
        temp = writer.ToString();
      }
      return temp;
    }

    //-----------------------Enqueues incoming message --------------------------------------------------------------------------------//
    public void PostMessage(SvcMsg msg)
    {
        rcvBlockingQ.enQ(msg);
    } 
    //-----------------------Handles the type of requests --------------------------------------------------------------------------------//
    public static void ProcessMessage(SvcMsg msg)
    {
        switch (msg.cmd.ToString())
        {
            case "ProjectList":
                getProjectList(msg);
                break;
            case "DepAnal":
                getDepAnal(msg);
                break;
            case "UpdateTable":
                Console.WriteLine("\nServer 0 requested update table..");
                //Console.WriteLine(msg.body);
                Thread.Sleep(500);
                DepAnal.MergeXmlTypes(msg.body);
                //Console.WriteLine("\n\nMerged Table: ");
                break;
            case "RelAnal":
                Console.WriteLine("\n\nReceived request for Relationship Analysis from Client...");
                try
                {
                    Console.WriteLine("\nSending Client {0} Relationship results...", msg.src);
                    string path1 = "../../../Repository1/" + msg.body;
                    SvcMsg msgReply1 = new SvcMsg();
                    msgReply1.src = new Uri("http://localhost:8081/MessageService");
                    msgReply1.dst = new Uri(msg.src.ToString());
                    msgReply1.cmd = SvcMsg.Command.RelAnal;
                    msgReply1.body = DepAnal.RelationshipAnalysis(path1);
                    IMessageService proxyClient_ = CreateClientChannel(msg.src.ToString());
                    proxyClient_.PostMessage(msgReply1);
                }
                catch
                {
                    Console.WriteLine("\n Directory not found: ");
                    return;
                }
                break;
        }
    }
    //-------------------------Handles request for project list ----------------------------------------------------------------------//
    public static void getProjectList(SvcMsg msg)
    {
        Console.WriteLine("\n\nReceived request for Project list from Client : {0}", msg.src);
        string s = DepAnal.getProjects("../../../Repository1");
        Console.WriteLine("\nSending Client available project list to Client {0}", msg.src);

        SvcMsg msgReply2 = new SvcMsg();
        msgReply2.src = new Uri("http://localhost:8081/MessageService");
        msgReply2.dst = new Uri(msg.src.ToString());
        msgReply2.cmd = SvcMsg.Command.ProjectList;
        msgReply2.body = s;
        IMessageService proxyClient2 = CreateClientChannel(msg.src.ToString());
        proxyClient2.PostMessage(msgReply2);
    }
    //-------------------------------------Handles request for Dependency analysis -----------------------------------------------------//
    public static void getDepAnal(SvcMsg msg)
    {
        Console.WriteLine("\n\nReceived request for Dependency analysis from Client for directory : {0}", msg.body);
        try
        {
            string path = "../../../Repository1/" + msg.body;
            string result = DepAnal.getDepAnalXML(DepAnal.DependencyAnalysis(path));
            Console.WriteLine("\nSending Client {0} dependency results for directory {1}", msg.src, msg.body);
            SvcMsg msgReply = new SvcMsg();
            msgReply.src = new Uri("http://localhost:8081/MessageService");
            msgReply.dst = new Uri(msg.src.ToString());
            msgReply.cmd = SvcMsg.Command.DepAnal;
            msgReply.body = result;
            IMessageService proxyClient = CreateClientChannel(msg.src.ToString());
            proxyClient.PostMessage(msgReply);
        }
        catch
        {
            Console.WriteLine("\n Directory not found: ");
            return;
        }
    }
    //------------------------Thread process method for dequeing incoming messages-----------------------------------------------------//
    public static void DequeueMessage()
    {
        while (true)
        {
            SvcMsg msg = rcvBlockingQ.deQ();
            ProcessMessage(msg);
        }
    }
    static void Main(string[] args)
    {
      "Starting Message Service on Server".Title();

      string server1 = args[0];
      string server0 = args[1];
      IMessageService proxyServer0 = null;
      ServiceHost host = null;

      try
      {
          host = CreateServiceChannel(server1);
          host.Open();
      }
      catch
      {
          Console.WriteLine("\nServer could not be started..Please check URL");
          Console.ReadLine();
          return;
      }

      try { proxyServer0 = CreateClientChannel(server0); }
      catch (Exception e) { Console.WriteLine("\n{0}", e.Message); }
      
        //IMessageService proxyClient = CreateClientChannel("http://localhost:8082/MessageService");

      Thread Receiver = new Thread(DequeueMessage);
      Receiver.IsBackground = true;
      Receiver.Start(); 

      DepAnal D2 = new DepAnal();
      Console.WriteLine("\nMy Type table:  ");
      DepAnal.UpdateTT("../../../Repository1");
      DepAnal.show();

      Thread.Sleep(1500);
      
      SvcMsg msg = new SvcMsg();
      msg.src = new Uri(server1);
      msg.dst = new Uri(server0);
      msg.cmd = SvcMsg.Command.UpdateTable;
      msg.body = DepAnal.getXmlTypes();
      for (int i = 0; i < 5; i++)
      {
          try 
          { 
              proxyServer0.PostMessage(msg);
              break;
          }
          catch 
          { 
              Console.WriteLine("\n Server 0 not started");
              Console.WriteLine("\nRetrying....");
              Thread.Sleep(1000);
          }
      }

      //proxy.PostMessage(msg);

      ////SvcMsg<string> msg = new SvcMsg<string>();
      ////msg.cmd = SvcMsg<string>.Command.Projects;
      ////msg.src = new Uri("http://localhost:8080/MessageService");
      ////msg.dst = new Uri("http://localhost:8081/MessageService");
      ////msg.body = "body";
      ////proxy.PostMessage(msg);

      Console.Write("\n");
      Console.ReadKey();
      host.Close();
    }
  }
}
