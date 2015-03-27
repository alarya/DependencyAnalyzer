/*-------------------------------------------------------------
 * FileMgr.cs - Fetching files for code analysis
 * Platform:    Sony vaio T14, Win 8.1 
 * Application: CodeAnalyzer| Project #2| FALL 2014
 * Author:      Alok Arya
 * -----------------------------------------------------------
 * Package operations:
 * This package receives a relative path and list of patterns 
 * from the analyzer.
 * In return, it returns all the file references back to the 
 * analyzer.
 * If required, it searches for files in the sub directory 
 * as well
 * 
 * Required Files:
 * Analyzer.cs
 * 
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace CodeAnalysis
{
    public class FileMgr
    {
        private List<string> files = new List<string>();
        private List<string> patterns = new List<string>();
        
        
        //----------find files based on the path & patterns received-----------------------------------------------------------------------
        public void findFiles(string path, bool recurse)
        {           
            foreach(string pattern in patterns)
            {
                try
                {
                    string[] newFiles = Directory.GetFiles(path, pattern);
                    for (int i = 0; i < newFiles.Length; ++i)
                        newFiles[i] = Path.GetFullPath(newFiles[i]);
                    files.AddRange(newFiles);
                }
                catch(Exception e)
                {
                    Console.WriteLine("\nSome error occured in fetching files: {0}", e.Message);
                }
            }
            try
            {
                if (recurse)
                {
                    string[] dirs = Directory.GetDirectories(path);
                    foreach (string dir in dirs)
                        findFiles(dir, recurse);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("\nSome error occured in fetching files: {0}", e.Message);
            }
        }
        public static List<string> getDirs(string path)
        {
            List<string> dirs = new List<string>();
            string[] dirsTemp = Directory.GetDirectories(path);
            foreach (string dir in dirsTemp)
                dirs.Add(Path.GetFullPath(dir));
            return dirs;
            
        }
        public void addPattern(string pattern)
        {
            patterns.Add(pattern);
        }
        

        public List<string> returnFiles()
        {
            return files;
        }

        //--------------test stub----------------------------------------------------------------------------------------------------------
#if(TEST_FILEMGR)
        static void Main(string[] args)
        {
            Console.Write("\n  Testing FileMgr Class");
            Console.Write("\n =======================\n");

            FileMgr fm = new FileMgr();
            string packageName = "Parser.cs";
            fm.addPattern(packageName);
            fm.findFiles("../../../../",true);
            List<string> files = fm.returnFiles();
            foreach (string file in files)
            Console.Write("\n  {0}", file);
            Console.Write("\n\n");
            Console.ReadLine();           
        }
#endif
    }
}


