
/*----------------------------------------------------------------
 * CmdLineParser.cs - Command Line parser
 * Ver 1.0
 * Language - C#, 2013, .Net Framework 4.5
 * Platform - Sony Vaio T14, Win 8.1
 * Application - CodeAnalyzer| Project #2| Fall 2014|
 * Author - Alok Arya (alarya@syr.edu)
 * ---------------------------------------------------------------
 * Package operations:
 * This package receives input from the executive for command line
 * parsing.
 * It does the type checking of the options and returns the results
 * to the executive.
 * 
 * 
 * Required files:
 * Executive.cs 
 * 
 * 
 */


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace CmdLineParser
{
    public class CmdLineParser
    {
        
       //-----------Splits all the patterns in the argument into an List-------------------------------------------------------------------
        public static List<String> splitPattern(string patterns)
        {
            string[] pattern = patterns.Split(',');
  
          List<String> temp = new List<String>() ;
            temp = pattern.ToList<string>();            
            return temp;
        }

        //-----------------check validity of options---------------------------------------------------------------------------------------
        public static bool checkOptions(string options)
        {
            if (options[0] != '/')
                return false;
            
            if(options.Length > 4)
                return false;
         
            for (int i=1; i < options.Length; i++)
                    if ( options[i] == 'x' || options[i] == 'r' || options[i] == 's' )
                    {
                        return true;
                    }
            
            return false;
        }

        //-----------------check if relationship option exists-----------------------------------------------------------------------------
        public static bool checkRelationship(string options)
        {
            foreach(char c in options)
                if(c == 'r')
                {
                    return true;
                }
            return false;
        }

        //------------------check if subdirectory option exists---------------------------------------------------------------------------
        public static bool checkSubdir(string options)
        {
            foreach (char c in options)
                if (c == 's')
                {
                    return true;
                }
            return false;
        }

        //-----------------check if xmloutput option exists-------------------------------------------------------------------------------
        public static bool checkXml(string options)
        {
            foreach (char c in options)
                if (c == 'x')
                {
                    return true;
                }
            return false;
        }


//--------------test stub------------------------------------------------------------------------------------//
#if(TEST_CmdLineParser)        
        static void Main(string[] args)
        {
            String temp = "*.cs,*.txt,*.doc";
            List<String> Patterns = splitPattern(temp);
            foreach (String s in Patterns)
                Console.WriteLine("\nPattern:{0}", s);

            String temp1 = "/xsr";
            bool validity = checkOptions(temp1);
            Console.WriteLine("\nOptions are valid:{0}", validity);
        }
#endif
    }
}
