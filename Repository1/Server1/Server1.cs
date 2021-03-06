﻿///////////////////////////////////////////////////////////////////////
// MT2Q2-Server.cs - Project #4 Service Server prototype             //
//                                                                   //
// Jim Fawcett, CSE681 - Software Modeling and Analysis, Fall 2014   //
///////////////////////////////////////////////////////////////////////

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
                Console.WriteLine("\n\nReceived request for Project list from Client : {0}", msg.src);
                string s = DepAnal.getProjects("../../../../");
                Console.WriteLine("\nSending Client available project list to Client {0}", msg.src);

                SvcMsg msgReply2 = new SvcMsg();
                msgReply2.src = new Uri("http://localhost:8081/MessageService");
                msgReply2.dst = new Uri(msg.src.ToString());
                msgReply2.cmd = SvcMsg.Command.ProjectList;
                msgReply2.body = s;
                IMessageService proxyClient2 = CreateClientChannel(msg.src.ToString());
                proxyClient2.PostMessage(msgReply2);
                break;
            case "DepAnal":
                Console.WriteLine("\n\nReceived request for Dependency analysis from Client for directory...{0}", msg.body);
                string result = DepAnal.getDepAnalXML(DepAnal.DependencyAnalysis(msg.body));
                Console.WriteLine("\nSending Client {0} dependency results for {1}", msg.src, msg.body);

                SvcMsg msgReply = new SvcMsg();
                msgReply.src = new Uri("http://localhost:8080/MessageService");
                msgReply.dst = new Uri(msg.src.ToString());
                msgReply.cmd = SvcMsg.Command.DepAnal;
                msgReply.body = result;
                IMessageService proxyClient = CreateClientChannel(msg.src.ToString());
                proxyClient.PostMessage(msgReply);
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

                Console.WriteLine("\nSending Client {0} Relationship results...", msg.src);

                SvcMsg msgReply1 = new SvcMsg();
                msgReply1.src = new Uri("http://localhost:8080/MessageService");
                msgReply1.dst = new Uri(msg.src.ToString());
                msgReply1.cmd = SvcMsg.Command.RelAnal;
                msgReply1.body = DepAnal.RelationshipAnalysis();
                IMessageService proxyClient_ = CreateClientChannel(msg.src.ToString());
                proxyClient_.PostMessage(msgReply1);
                break;
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

      ServiceHost host = CreateServiceChannel("http://localhost:8081/MessageService");
      host.Open();

      IMessageService proxyServer = CreateClientChannel("http://localhost:8080/MessageService");
      IMessageService proxyClient = CreateClientChannel("http://localhost:8082/MessageService");

      DepAnal D2 = new DepAnal();
      Console.WriteLine("\nMy Type table:  ");
      DepAnal.UpdateTT("../../../Dependency_Analyzer");
      DepAnal.show();


      SvcMsg msg = new SvcMsg();
      msg.src = new Uri("http://localhost:8081/MessageService");
      msg.dst = new Uri("http://localhost:8080/MessageService");
      msg.cmd = SvcMsg.Command.UpdateTable;
      msg.body = DepAnal.getXmlTypes();
      proxyServer.PostMessage(msg);

      Thread Receiver = new Thread(DequeueMessage);
      Receiver.IsBackground = true;
      Receiver.Start();  
  

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
