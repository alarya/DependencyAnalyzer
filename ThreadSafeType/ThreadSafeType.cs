/*----------------------------------------------------------------------
 * ThreadSafeType.cs - Thread safe type table
 * Ver 1.0
 * Language - C#, 2013, .Net Framework 4.5
 * Platform - Sony Vaio T14, Win 8.1
 * Application - Dependency Analyzer| Project #4| Fall 2014|
 * Original Author:      Jim Fawcett, CST 4-187, Syracuse University          
 *                      (315) 443-3948, jfawcett@twcny.rr.com  
 * ---------------------------------------------------------------------
 * 
 * Package Operations:
 * 
 * This package defines a thread safe type table using object locker_.
 * The blocking queue provides standard operations like add, remove etc.
 * 
 * 
 * Required Packages:
 * None (This packages is used by other packages to define and implement a thread safe type table)
 *
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DependencyAnalyzer
{
  public class TypeElem
  {
    public string Namespace { get; set; }
    public string Filename { get; set; }
  }
  public class TypeTable
  {
    object locker_ = new object();
    public Dictionary<string, TypeElem> types { get; set; }

    public TypeTable()
    {
      types = new Dictionary<string, TypeElem>();
    }
    //----< f will use lambda capture to acquire its arguments >-------

    T lockit<T>(Func<T> f)
    {
      lock (locker_) { return f.Invoke(); }
    }
    public bool add(string Type, string Namespace, string Filename)
    {
      TypeElem elem = new TypeElem();
      elem.Namespace = Namespace;
      elem.Filename = Filename;
      return lockit<bool>(() =>
      {
        if (types.Keys.Contains(Type) && types[Type].Namespace == Namespace)
          return false;
        types[Type] = elem;
        return true;
      });
    }
    public bool remove(string Type)
    {
      return lockit<bool>(() => { return types.Remove(Type); });
    }
    string namespce(string Type)
    {
      return lockit<string>(() =>
      {
         return (types.Keys.Contains(Type)) ? types[Type].Namespace : "";
      });
    }
    public string filename(string Type)
    {
      return lockit<string>(() =>
      {
        return (types.Keys.Contains(Type)) ? types[Type].Filename : "";
      });
    }
    public bool contains(string Type)
    {
      return lockit<bool>(() => types.Keys.Contains(Type));
    }
  }
 
    
#if(TEST_TYPE)   
    class Test
  {
    //----< partial test of type-safe TypeTable >----------------------

    static void Main(string[] args)
    {
      TypeTable tt = new TypeTable();
      tt.add("Type1", "myNamespace", " someFile.cs");
      tt.add("Type2", "myNamespace", "someOtherFile.cs");
      foreach(string key in tt.types.Keys)
      {
        Console.Write("\n  {0, -15} {1, -20} {2, 20}", key, tt.types[key].Namespace, tt.types[key].Filename);
      }
      Console.Write("\n\n");
      Console.ReadLine();
    }
  }
#endif
}
