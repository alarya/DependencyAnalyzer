/////////////////////////////////////////////////////////////////////
///  delegateDemo.cs - demonstrate basic use of delegates         ///
///  ver 2.0                                                      ///
///  Language:      C#                                            ///
///  Platform:      Dell Dimension 9200, Windows Vista Ulitmate   ///
///  Application:   CSE681 - Software Modeling and Analysis demo  ///
///  Author:        Jim Fawcett, CST 2-187, Syracuse University   ///
///                 (315) 443-3948, jfawcett@twcny.rr.com         ///
/////////////////////////////////////////////////////////////////////

using System;

namespace delegateDemo
{
  /////////////////////////////////////////////////////////////////
  // Publisher creates events, defines delegates, invokes callbacks

  public class Publisher
  {
    public delegate void invoker(string msg);
    
      // The preceding statement is a declaration of a delegete TYPE!
      // It says that the invoker type can hold (only) pointers to
      //   functions with the signature: void func(string)

    public invoker inv1 = null;   // called when event #1 happens
    public invoker inv2 = null;   // called when event #2 happens

      // The two statements above declare instances of the invoker type
      // Clients register for a callback from the invoker with this:
      //   inv1 += new invoker(someFunctionPointer);
      // The function pointer is just the name of the function the client
      // wants to have called (back) when Publisher publishes.
      //
      // When Publisher publishes an event it simply calls:
      //   inv1(someString);
      // The delegate, inv1, calls every function that has been registered
      // by some client, passing each function the string someString.

    public Publisher()
    {
      Console.Write("\n  Constructing Publisher object\n");
    }
    public void Publish() 
    {
      Console.Write("\n  Publisher invoking callbacks for first event\n");
      if(inv1 != null)
        inv1("  invoked by Publisher.Publish() for event #1\n");

      Console.Write("\n  Publisher invoking callbacks for second event\n");
      if(inv2 != null)
        inv2("  invoked by Publisher.Publish() for event #2\n");
    }
  }
  //
  /////////////////////////////////////////////////////////////////
  // Subscriber defines handlers for Publishers events, registers them with 
  // Publisher's delegate - Publisher instance is passed to its constructor

  public class Subscriber
  {
    public Subscriber(Publisher pub)
    {
      // Subscriber registers for Publisher's event #1
      pub.inv1 += new Publisher.invoker(instanceHandler);
      pub.inv1 += new Publisher.invoker(staticHandler);

      // Subscriber registers for Publisher's event #2
      pub.inv2 += new Publisher.invoker(instanceHandler);
    }
    void instanceHandler(string msg)
    {
      Console.Write("\n  Subscriber.instanceHandler called");
      Console.Write("\n  " + msg);
    }
    static void staticHandler(string msg)
    {
      Console.Write("\n  Subscriber.staticHandler called");
      Console.Write("\n  " + msg);
    }
  }
  /////////////////////////////////////////////////////////////////
  // delegateDemo is the executive for this demo.
  // It creates the Publisher and Subscriber instances and
  // starts the Publisher publishing.

  class delegateDemo
  {
    [STAThread]
    static void Main(string[] args)
    {
      Console.Write("\n  Demonstrating use of Delegates ");
      Console.Write("\n ================================\n");

      Publisher pub = new Publisher();
      Subscriber sub = new Subscriber(pub);
      pub.Publish();

        // Note that we see no calls to the subscriber's functions.
        // The calls occur in the Publish() function.

      Console.Write("\n\n");
      Console.ReadLine();
    }
  }
}
