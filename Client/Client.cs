/*----------------------------------------------------------------------
 * Client.cs - Client for requesting services of dependency analyzer
 * Ver 1.0
 * Language - C#, 2013, .Net Framework 4.5
 * Platform - Sony Vaio T14, Win 8.1
 * Application - Dependency Analyzer| Project #4| Fall 2014|
 * Author - Alok Arya (alarya@syr.edu)
 * ---------------------------------------------------------------------
 * 
 * Package Operations:
 * This package implements the service contract from the client perspective.
 * It provides services for setting up the client and channels for communication
 * It defines a  blocking queue for recieving replies from the servers.
 * 
 * Required Packages:
 * BlockingQueue.cs ServerLibrary.cs Utilities.cs
 * 
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Threading;
using Utilities;
using System.Xml;
using System.Xml.Linq;

namespace DependencyAnalyzer
{
  public class MessageClient: IMessageService
  {
      public static BlockingQueue<SvcMsg> rcvQueue = null;

    static MessageClient()
    {
       rcvQueue = new BlockingQueue<SvcMsg>();
    }
    //-----------Create Client channel for the client using channel factory---------------------------------------------------------------//
    public static IMessageService CreateClientChannel(string url)
    {
      BasicHttpBinding binding = new BasicHttpBinding();
      EndpointAddress address = new EndpointAddress(url);
      ChannelFactory<IMessageService> factory = 
        new ChannelFactory<IMessageService>(binding, address);
      return factory.CreateChannel(); 
    }
    //------------Create Service channel for the client using Service Host------------------------------------------------------------------//
     public static ServiceHost CreateServiceChannel(string url)
    {
      BasicHttpBinding binding = new BasicHttpBinding();
      Uri baseAddress = new Uri(url);
      Type service = typeof(MessageClient);
      ServiceHost host = new ServiceHost(service, baseAddress);
      host.AddServiceEndpoint(typeof(IMessageService), binding, baseAddress);
      return host;
    }
    //-------------Service Implementation--------------------------------------------------------------------------------------------------//
     public void PostMessage(SvcMsg msg)
     {
         switch (msg.cmd.ToString())
         {
             case "ProjectList":
                 rcvQueue.enQ(msg);
                 //Console.WriteLine("\nReceived reply from {0}", msg.src);
                 //string projects = msg.body;
                 //Console.WriteLine("\nProject List");
                 //XDocument doc = XDocument.Parse(projects);
                 //foreach (XElement T in doc.Descendants("project"))
                 //{
                 //    Console.WriteLine("{0}", T.Value);
                 //}
                 break;
             case "DepAnal":
                 rcvQueue.enQ(msg);
                 //Console.WriteLine("\nReceived reply from {0}", msg.src);
                 //Console.WriteLine("\nPackage Dependencies");
                 //string DepAnalResult = msg.body;
                 //XDocument doc1 = XDocument.Parse(DepAnalResult);
                 //foreach (XElement T in doc1.Descendants("package"))
                 //{
                 //    Console.WriteLine("\nPackage: {0}\n", T.Attribute("name"));
                 //    foreach (XElement T1 in T.Descendants("dependentPackage"))
                 //    {
                 //     Console.WriteLine("\t{0}", T1.Value);
                 //    }
                 //}
                 //Console.Write("\n");
                 break;
             case "RelAnal":
                 rcvQueue.enQ(msg);
                 //Console.WriteLine("\nReceived reply from {0}", msg.src);
                 //string Reln = msg.body;
                 //Console.WriteLine("\nRelationship Analysis");
                 //XDocument doc = XDocument.Parse(Reln);
                 //foreach(XElement T in doc.Descendants("file"))
                 //{
                 //    Console.WriteLine("\nFile: {0}\n",T.Attribute("name"));
                 //      foreach(XElement T1 in T.Descendants("Relationship"))
                 //      {
                 //          Console.WriteLine("\t{0} {1} {2}", T1.Element("type1").Value,T1.Element("relationship").Value,T1.Element("type2").Value);
                 //      }
                 //}
                 break;       
         }
     }
#if(TEST_CLIENT)
    //-----------Client Executive----------------------------------------------------------------------------------------------------------//
    static void Main(string[] args)
    {
      "Starting Message Service on Client".Title();

      ServiceHost host = CreateServiceChannel("http://localhost:8082/MessageService");
      host.Open();

      IMessageService proxyServer = CreateClientChannel("http://localhost:8080/MessageService");
      IMessageService proxyServer1 = CreateClientChannel("http://localhost:8081/MessageService");
      Thread.Sleep(3000);

      //Checking result for dependency Analysis
      Console.WriteLine("\n\n Requesting Project List from Server 0..");
      SvcMsg msg5 = new SvcMsg();
      msg5.cmd = SvcMsg.Command.DepAnal;
      msg5.src = new Uri("http://localhost:8082/MessageService");
      msg5.dst = new Uri("http://localhost:8080/MessageService");
      msg5.body = "CodeAnalyzer";
      proxyServer.PostMessage(msg5);
        
      ////Requesting project list from Server 0                    
      //Console.WriteLine("\n\n Requesting Project List from Server 0..");
      //SvcMsg msg4 = new SvcMsg();
      //msg4.cmd = SvcMsg.Command.ProjectList;
      //msg4.src = new Uri("http://localhost:8082/MessageService");
      //msg4.dst = new Uri("http://localhost:8080/MessageService");
      //proxyServer.PostMessage(msg4);

      //Requesting Dependency Analysis to Server 0
      //Console.WriteLine("\n\nSending Dependency analysis request to server 0...");  
      //SvcMsg msg = new SvcMsg();
      //msg.cmd = SvcMsg.Command.DepAnal;
      //msg.src = new Uri("http://localhost:8082/MessageService");
      //msg.dst = new Uri("http://localhost:8080/MessageService");
      //msg.body = "Parser.cs";
      //proxyServer.PostMessage(msg);

      //Thread.Sleep(4000);

      //Requesting Dependency Analysis to Server 1
      //Console.WriteLine("\n\nSending Dependency analysis request to server 1...");
      //SvcMsg msg2 = new SvcMsg();
      //msg2.cmd = SvcMsg.Command.DepAnal;
      //msg2.src = new Uri("http://localhost:8082/MessageService");
      //msg2.dst = new Uri("http://localhost:8081/MessageService");
      //msg2.body = "DepAnal.cs";
      //proxyServer1.PostMessage(msg2);

      //Thread.Sleep(4000);

      //Requesting Relationship Analysis to Server 0
      //Console.WriteLine("\n\nSending Relationship analysis request to server 0...");
      //SvcMsg msg3 = new SvcMsg();
      //msg3.cmd = SvcMsg.Command.RelAnal;
      //msg3.src = new Uri("http://localhost:8082/MessageService");
      //msg3.dst = new Uri("http://localhost:8080/MessageService");
      //proxyServer.PostMessage(msg3);

      Console.ReadKey();
      Console.Write("\n");
    }
#endif
  }

}
