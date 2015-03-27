///////////////////////////////////////////////////////////////////////
// Utilities.cs - MT code helpers                                    //
//                                                                   //
// Jim Fawcett, CSE681 - Software Modeling and Analysis, Fall 2014   //
///////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities
{
  public static class Extensions
  {
    public static void Title(this string title, char underlineChar = '=', bool linefeedAtEnd = true)
    {
      Console.Write("\n  {0}\n {1}", title, new string(underlineChar, title.Length + 2));
      if (linefeedAtEnd)
        Console.Write("\n");
    }
  }
#if(TEST_UTILITIES)
  class Utilities
  {
    static void Main(string[] args)
    {
      "hello world".Title();
    }
  }
#endif
}
