/*----------------------------------------------------------------------
 * Server.cs - Server for hosting services of dependency analyzer
 * Ver 1.0
 * Language - C#, 2013, .Net Framework 4.5
 * Platform - Sony Vaio T14, Win 8.1
 * Application - Dependency Analyzer| Project #4| Fall 2014|
 * Author - Alok Arya (alarya@syr.edu)
 * ---------------------------------------------------------------------
 * 
 * Package Operations:
 * This package hosts the service implementation of Dependency analyzer.
 * This server assumes a the root of repository as "../../../Repository".
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
  class Server: IMessageService
  {
      static BlockingQueue<SvcMsg> rcvBlockingQ = null;

      //-----------Constructor-------------------------------------------------------------------------------------------------------------//
      static Server()
      {
          if (rcvBlockingQ == null)
              rcvBlockingQ = new BlockingQueue<SvcMsg>();
      }
      //--------------- Create Client channel using ChannelFactory ------------------------------------------------------------------------//
      static IMessageService CreateClientChannel(string url)
    {
      BasicHttpBinding binding = new BasicHttpBinding();
      EndpointAddress address = new EndpointAddress(url);
      ChannelFactory<IMessageService> factory =
        new ChannelFactory<IMessageService>(binding, address);
      return factory.CreateChannel();
    }

    //------------Create Service Channel using Service host -------------------------------------------------------------------------------------//
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
                Console.WriteLine("\n\nServer 1 requested update table..");
                Thread.Sleep(500);
                DepAnal.MergeXmlTypes(msg.body);
                DepAnal.ShowDeps();
                break;
            case "RelAnal":
                Console.WriteLine("\n\nReceived request for Relationship Analysis from Client...");
                try
                {

                    Console.WriteLine("\nSending Client {0} Relationship results...", msg.src);
                    string path1 = "../../../Repository/" + msg.body;
                    SvcMsg msgReply1 = new SvcMsg();
                    msgReply1.src = new Uri("http://localhost:8080/MessageService");
                    msgReply1.dst = new Uri(msg.src.ToString());
                    msgReply1.cmd = SvcMsg.Command.RelAnal;
                    msgReply1.body = DepAnal.RelationshipAnalysis(path1);
                    IMessageService proxyClient1 = CreateClientChannel(msg.src.ToString());
                    proxyClient1.PostMessage(msgReply1);
                }
                catch
                {
                    Console.WriteLine("\n Directory not found: ");
                    return;
                }
                break;
        }
    }
    //--------------------------------Handles request for Project list----------------------------------------------------------------//
    public static void getProjectList(SvcMsg msg)
    {
        Console.WriteLine("\n\nReceived request for Project list from Client : {0}", msg.src);
        string s = DepAnal.getProjects("../../../Repository");
        Console.WriteLine("\nSending available project list to Client {0}", msg.src);

        SvcMsg msgReply2 = new SvcMsg();
        msgReply2.src = new Uri("http://localhost:8080/MessageService");
        msgReply2.dst = new Uri(msg.src.ToString());
        msgReply2.cmd = SvcMsg.Command.ProjectList;
        msgReply2.body = s;
        IMessageService proxyClient2 = CreateClientChannel(msg.src.ToString());
        proxyClient2.PostMessage(msgReply2);
    }
    //-----------------------------Handles request for Dependency Analysis------------------------------------------------------------//
      public static void getDepAnal(SvcMsg msg)
      {
          Console.WriteLine("\n\nReceived request for Dependency analysis from Client for directory : {0}", msg.body);
          try
          {
              string path = "../../../Repository/" + msg.body;
              string result = DepAnal.getDepAnalXML(DepAnal.DependencyAnalysis(path));
              Console.WriteLine("\nSending Client {0} dependency results for directory {1}", msg.src, msg.body);
              SvcMsg msgReply = new SvcMsg();
              msgReply.src = new Uri("http://localhost:8080/MessageService");
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
        while(true)
        {
            SvcMsg msg = rcvBlockingQ.deQ();
            ProcessMessage(msg);
        }
    }

    //----------------------Main --------------------------------------------------------------------------------------------------------//
    static void Main(string[] args)
    {
      "Starting Message Service on Server".Title();

      string server0 = args[0];
      string server1 = args[1];
      IMessageService proxyServer1 = null;
      ServiceHost host = null;

      try
      {
          host = CreateServiceChannel(server0);
          host.Open();
      }
      catch
      {
          Console.WriteLine("\nServer could not be started..Please check URL");
          Console.ReadLine();
          return;
      }

      try { proxyServer1 = CreateClientChannel(server1); }
      catch (Exception e) { Console.WriteLine("\n{0}",e.Message); }
       
      //IMessageService proxyClient = CreateClientChannel("http://localhost:8082/MessageService");
      
      DepAnal D1 = new DepAnal();

      Thread Receiver = new Thread(DequeueMessage);
      Receiver.IsBackground = true;
      Receiver.Start();
  
      Console.WriteLine("\nMy Type table:  ");
      DepAnal.UpdateTT("../../../Repository");
      DepAnal.show();

      Thread.Sleep(1500);
      
      SvcMsg msg = new SvcMsg();
      msg.src = new Uri(server0);
      msg.dst = new Uri(server1);
      msg.cmd = SvcMsg.Command.UpdateTable;
      msg.body = DepAnal.getXmlTypes();

      for (int i = 0; i < 5; i++)
      {
          try 
          { 
            proxyServer1.PostMessage(msg);
            break;
          }
          catch 
          { 
              Console.WriteLine("\n Server 1 not started..");
              Console.WriteLine("\n Retrying....");
              Thread.Sleep(1000);
          }
          
      }
      
      //DepAnal.show();
      //SvcMsg<string> msg = new SvcMsg<string>();
      //msg.cmd = SvcMsg<string>.Command.Projects;
      //msg.src = new Uri("http://localhost:8080/MessageService");
      //msg.dst = new Uri("http://localhost:8081/MessageService");
      //msg.body = "body";
      //proxy.PostMessage(msg);

      Console.Write("\n");
      Console.ReadKey();      
      host.Close();
    }
  }
}
