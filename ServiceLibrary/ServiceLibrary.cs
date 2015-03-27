/*----------------------------------------------------------------------
 * ServiceLibrary.cs - Server for hosting services of dependency analyzer
 * Ver 1.0
 * Language - C#, 2013, .Net Framework 4.5
 * Platform - Sony Vaio T14, Win 8.1
 * Application - Dependency Analyzer| Project #4| Fall 2014|
 * Author - Alok Arya (alarya@syr.edu)
 * ---------------------------------------------------------------------
 * 
 * Package Operations:
 * This is the service contract for server - client, server-server operations
 * This package defines an interface that needs to be implemented by both the client and server
 * This package also defines a Datacontract which is used for exchanging information
 * 
 * Note: No test stub
 * 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using System.Runtime.Serialization;
using System.ServiceModel.Web;

namespace DependencyAnalyzer
{
    [DataContract(Namespace = "DependencyAnalyzer")]
    public class SvcMsg
    {
        public enum Command { Projects, ProjectList, DepAnal,UpdateTable,RelAnal };
        [DataMember]
        public Command cmd;
        [DataMember]
        public Uri src;
        [DataMember]
        public Uri dst;
        [DataMember]
        public string body;

        public void ShowMessage()
        {
            Console.Write("\n  Received Message:");
            Console.Write("\n    src = {0}\n    dst = {1}", src.ToString(), dst.ToString());
            Console.Write("\n    cmd = {0}", cmd.ToString());
            Console.Write("\n    body:\n{0}", body);
        }
    }


    [ServiceContract(Namespace = "DependencyAnalyzer")]
    public interface IMessageService
    {
        [OperationContract]
        void PostMessage(SvcMsg msg);
    }
}


