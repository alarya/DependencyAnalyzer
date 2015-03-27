
/*----------------------------------------------------------------
 * Executive.cs - Executive file for the CodeAnalyzer application
 * Ver 1.0
 * Language - C#, 2013, .Net Framework 2013
 * Platform - Sony Vaio T14, Win 8.1
 * Application - CodeAnalyzer| Project #2| Fall 2014|
 * Author - Alok Arya (alarya@syr.edu)
 * ---------------------------------------------------------------
 * Package operations:
 * This package is the entry point for the codeanalyzer application.
 * It does some minor type checking and calls commandline parser for 
 * parsing the arguments.
 * Calls Analyzer for further processing.
 * 
 * Note: No test stub required as the main function is used as the 
 * .starting point for the application
 * 
 * Required files:
 * Display.cs Analyzer.cs
 * 
 * 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Executive
{
    class Executive
    {
        
        static void Main(string[] args)
        {
            bool checkoptions = false;
            bool relationship = false;
            bool subdir = false;
            bool xml = false;
            String path;
            List<String> patterns;
            try
            {
                path = Path.GetFullPath(args[0]);
                string[] files = Directory.GetFiles(path); //to check if path is correct
                Console.WriteLine("\nPath: {0}\n",path);
            }
            catch
            {
                Console.WriteLine("\nPlease provide a valid path...");
                Console.ReadLine();
                return;
            }

            try
            {
                patterns = CmdLineParser.CmdLineParser.splitPattern(args[1]);
            }
            catch
            {
                Console.WriteLine("\nInvalid command.Please provide below format\npath pattern1,pattern2,...   [/[x][s][r]] ");
                Console.ReadLine();
                return;
            }
            
            Console.WriteLine("Patterns:");
            for (int i = 0; i < patterns.Count; i++ )
                Console.WriteLine("{0}", patterns[i]);

            if (args.Length > 3)
            {   
                Console.WriteLine("\nInvalid command.Please provide below format\npath pattern1,pattern2,...   [/[x][s][r]] ");
                Console.ReadLine();
                return;
            }

            if (args.Length == 3)
            {
                checkoptions = CmdLineParser.CmdLineParser.checkOptions(args[2]);
                relationship = CmdLineParser.CmdLineParser.checkRelationship(args[2]);
                subdir = CmdLineParser.CmdLineParser.checkSubdir(args[2]);
                xml = CmdLineParser.CmdLineParser.checkXml(args[2]);
                Console.WriteLine("\nOptions: {0}", args[2]);
            }
            else
                checkoptions = true;
            
            if (!checkoptions)
            {
                Console.WriteLine("\nInvalid options.\n\"path\" \"pattern1,pattern2,...\"   [/[x][s][r]] ");
                Console.ReadLine();
                return;
            }

            CodeAnalysis.Analyzer.doAnalysis(args[0], patterns, relationship, subdir, xml);
        }
    }
}
