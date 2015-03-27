///////////////////////////////////////////////////////////////////////
// MT2Q2-ServiceLibrary.cs - Project #4 Service prototype            //
//                                                                   //
// Jim Fawcett, CSE681 - Software Modeling and Analysis, Fall 2014   //
///////////////////////////////////////////////////////////////////////

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

 // [ServiceBehavior(Namespace="DependencyAnalyzer")]
  
  //  public class MessageService : IMessageService
  //{
  //  public void PostMessage(SvcMsg msg)
  //  {
  //      if (msg.cmd.ToString() == "Projects" || msg.cmd.ToString() == "ProjectList") ;
  //      msg.ShowMessage();

  //      if (msg.cmd.ToString() == "DepAnal")
  //      { }
  //  }
  //}
//}
