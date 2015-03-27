/*----------------------------------------------------------------------
* Parser.cs -   Parser detects code constructs defined by rules       
* ver 1.0                                                           
* Language:     C#, 2013, .Net Framework 4.5                         
* Platform:     Sony Vaio T14, Win 8.1                      
* Application:  CodeAnalyzer| Project #2| Fall 2014|      
* Original Author:      Jim Fawcett, CST 4-187, Syracuse University          
*              (315) 443-3948, jfawcett@twcny.rr.com  
* Author: Alok Arya (alarya@syr.edu)             
*-----------------------------------------------------------------------
*
 * Module Operations:
 * ------------------
 * This module defines the following class:
 *   Parser  - a collection of IRules
 */
/* Required Files:
 *   IRulesAndActions.cs, RulesAndActions.cs, Parser.cs, Semi.cs, Toker.cs
 *   
 * Build command:
 *   csc /D:TEST_PARSER Parser.cs IRulesAndActions.cs RulesAndActions.cs \
 *                      Semi.cs Toker.cs
 *   
 * Maintenance History:
 * --------------------
 * ver 1.3 : 24 Sep 2011
 * - Added exception handling for exceptions thrown while parsing.
 *   This was done because Toker now throws if it encounters a
 *   string containing @".
 * - RulesAndActions were modified to fix bugs reported recently
 * ver 1.2 : 20 Sep 2011
 * - removed old stack, now replaced by ScopeStack
 * ver 1.1 : 11 Sep 2011
 * - added comments to parse function
 * ver 1.0 : 28 Aug 2011
 * - first release
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;

namespace CodeAnalysis
{
  
    //------------------rule-based parser used for code analysis----------------------------------------------------------------------------------------
    public class Parser
    {
    private List<IRule> Rules;
    public Parser()
    {
          Rules = new List<IRule>();
    }
    public void add(IRule rule)
    {
      Rules.Add(rule);
    }
    
    //---------Rule returns true to tell parser to stop processing the current semiExp------------------------------------------------------------------
    public void parse(CSsemi.CSemiExp semi)
    {
      foreach (IRule rule in Rules)
      {
        if (rule.test(semi))
          break;
      }
    }
  }

  class TestParser
  {
    //----process commandline to get file references------------------------------------------------------------------------------------------------------

    static List<string> ProcessCommandline(string[] args)
    {
      List<string> files = new List<string>();
      if (args.Length == 0)
      {
        Console.Write("\n  Please enter file(s) to analyze\n\n");
        return files;
      }
      string path = args[0];
      path = Path.GetFullPath(path);
      for (int i = 1; i < args.Length; ++i)
      {
        string filename = Path.GetFileName(args[i]);
        files.AddRange(Directory.GetFiles(path, filename));
      }
      return files;
    }

    static void ShowCommandLine(string[] args)
    {
      Console.Write("\n  Commandline args are:\n");
      foreach (string arg in args)
      {
        Console.Write("  {0}", arg);
      }
      Console.Write("\n\n  current directory: {0}", System.IO.Directory.GetCurrentDirectory());
      Console.Write("\n\n");
    }
    //----< Test Stub >-------------------------------------------------------------------------------------------------------------------

#if(TEST_PARSER)

    static void Main(string[] args)
    {
      Console.Write("\n  Demonstrating Parser");
      Console.Write("\n ======================\n");

      ShowCommandLine(args);

      List<string> files = TestParser.ProcessCommandline(args);
          
        //1st pass
       foreach (object file in files)
       {
             Console.Write("\n  Processing file {0}\n", file as string);

             CSsemi.CSemiExp semi = new CSsemi.CSemiExp();
             semi.displayNewLines = false;
             if (!semi.open(file as string))
             {
                Console.Write("\n  Can't open {0}\n\n", args[0]);
                return;
             }

             Console.Write("\n  Type and Function Analysis");
             Console.Write("\n ----------------------------\n");

             BuildCodeAnalyzer builder = new BuildCodeAnalyzer(semi);
             Parser parser = builder.build();
     
             try
             {
                while (semi.getSemi())
                parser.parse(semi);
             }
             catch (Exception ex)
             {
                Console.Write("\n\n  {0}\n", ex.Message);
             }
        
             Repository rep = Repository.getInstance();
             List<Elem> typefuncs = rep.typefunc;
             foreach (Elem typefunc in typefuncs)
             {
                 switch (typefunc.type)
                 {

                     case "namespace":
                         Console.Write("\n{0}:{1}", typefunc.type.ToString(), typefunc.name.ToString());
                         break;
                     case "interface":
                         Console.Write("\n\n{0}:{1}", typefunc.type.ToString(), typefunc.name.ToString());
                         break;
                     case "struct":
                         Console.Write("\n\n{0}:{1}", typefunc.type.ToString(), typefunc.name.ToString());
                         break;
                     case "enum":
                         Console.Write("\n\n{0}:{1}", typefunc.type.ToString(), typefunc.name.ToString());
                         break;                        
                     case "class":
                         Console.Write("\n{0}:{1}", typefunc.type.ToString(), typefunc.name.ToString());
                         break;
                     case "function":
                         Console.Write("\n\t{0}:{1}", typefunc.type.ToString(), typefunc.name.ToString());
                         Console.Write("\n\t-->Lines of code: {0}", typefunc.loc);
                         Console.Write("\n\t-->Complexity: {0}:", typefunc.complexity);
                         Console.WriteLine("\n");
                         break;
                 }
             }
        Console.WriteLine();
        Console.Write("\n\n File analyzed..\n\n");
        semi.close();
        Console.ReadLine();
      }
      
      Console.WriteLine("\n\n All the user types found :-");
      List<Elem> types = new List<Elem>();
      types = Repository.gettypes();
      foreach(Elem type in types)
      {
          Console.WriteLine("\n {0} {1}", type.type, type.name);
      }

    
      Console.ReadLine();
      Console.Write("\n  Type Relationships ");
      Console.Write("\n ----------------------------\n");        
      //2nd pass
      foreach (object file in files)
      {

          CSsemi.CSemiExp semi = new CSsemi.CSemiExp();
          semi.displayNewLines = false;
          
          if (!semi.open(file as string))
          {
              Console.Write("\n  Can't open {0}\n\n", args[0]);
              return;
          }
          
          BuildCodeAnalyzer builder = new BuildCodeAnalyzer(semi);
          Parser parser = builder.build1();         
          
          try
          {
              while (semi.getSemi())
                  parser.parse(semi);
          }
          
          catch (Exception ex)
          {
              Console.Write("\n\n  {0}\n", ex.Message);
          }
          semi.close();
      } 

            List<Relationships> relations = new List<Relationships>();
          //  relations = FinalRelationships.getrelationships() ;
          //  List<Relationships> relations1 = relations.Distinct().ToList<Relationships>() ;

          //foreach (Relationships R in relations1)
          //{
          //    Console.Write("\n  {0} : {1} : {2}", R.type1, R.relationship,R.type2);
          //}
          Console.ReadLine(); 
      }       
    }
#endif
}

