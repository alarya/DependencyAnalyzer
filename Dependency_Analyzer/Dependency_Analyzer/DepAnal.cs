/*----------------------------------------------------------------------
 * DepAnal.cs - Server for hosting services of dependency analyzer
 * Ver 1.0
 * Language - C#, 2013, .Net Framework 4.5
 * Platform - Sony Vaio T14, Win 8.1
 * Application - Dependency Analyzer| Project #4| Fall 2014|
 * Author - Alok Arya (alarya@syr.edu)
 * ---------------------------------------------------------------------
 * 
 * Package Operations:
 * This package acts as an intermediator between Server and CodeAnalyzer(developed in Pr#2).
 * This package receives requests from the server.
 * It returns the results of requests like pacakage dependency and relationship analysis to 
 * the server in XML format.
 * This package interacts with Analyzer and FileMgr package of the code Analyzer for processig
 * requests.
 * 
 * Required Packages:
 * Analyzer.cs FileMgr.cs Parser.cs ThreadSafeType.cs
 * 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace DependencyAnalyzer
{ 
    public class DepAnal
    {
        static TypeTable TT ;
        private static Dictionary<string, List<string>> Dependencies{get; set;}
        //--------------Constructor----------------------------------------------------------------------------------------------------//
        static DepAnal()
        {
            TT = new TypeTable();
            Dependencies = new Dictionary<string, List<string>>();
        }
        //-----------------------------------------------------------------------------------------------------------------------------//
        public static TypeTable getTT()
        {
            return TT;
        }

        //--------------Creates type table of all packages in the Repository of the calling server---------------------------------------//
        public static void UpdateTT(string path)
        {
            List<string> patterns = new List<string>();
            patterns.Add("*.cs");
            CodeAnalysis.Analyzer.createTypeTable(path, patterns, true);
            
        }
        //---------------Returns dependency analysis results------------------------------------------------------------------------//
        public static Dictionary<string, List<string>> DependencyAnalysis(string dir)
        {
            //DepAnal.UpdateTT("../../");
            Dependencies = new Dictionary<string, List<string>>();
            string path = dir;        
            Console.WriteLine(path);
            Dependencies = CodeAnalysis.Analyzer.DepAnalysis(path, true);
            return Dependencies;
        }
        //-----------------------Return XML form of Dependency analysis result -------------------------------------------------//
        public static string getDepAnalXML(Dictionary<string, List<string>> DepList)
        {
            XDocument doc = new XDocument();
            XElement root = new XElement("DepAnal");
            doc.Add(root);
            foreach(KeyValuePair<string, List<string>> pair in DepList)
            {
                XElement package = new XElement("package", new XAttribute("name",pair.Key));
                root.Add(package);
                foreach(string dependentPackage in pair.Value)
                {
                    XElement depPackage = new XElement("dependentPackage");
                    depPackage.Value = dependentPackage ;
                    package.Add(depPackage);
                }
            }

            return doc.ToString();
        }
        //------------------------Returns relationship analysis request as xml string----------------------------------------------------------------------//
        public static string RelationshipAnalysis(string path)
        {
            Dictionary<string,List<CodeAnalysis.Relationships>> temp = CodeAnalysis.Analyzer.parse2(path, true, "*.cs"); 
            XDocument doc = new XDocument();
            XElement root = new XElement("RelnInfo");
            doc.Add(root);
            foreach (KeyValuePair<String, List<CodeAnalysis.Relationships>> pair in temp)
                 {
                    XElement file = new XElement("file", new XAttribute("name",getFileName(pair.Key)));
                    //file.Value = getFileName(pair.Key);
                    root.Add(file);
                    foreach(CodeAnalysis.Relationships R in pair.Value)
                    {
                        XElement Relationship = new XElement("Relationship");
                        file.Add(Relationship);
                        XElement type1 = new XElement("type1");
                        XElement relationship = new XElement("relationship");
                        XElement type2 = new XElement("type2");
                        type1.Value = R.type1;
                        type2.Value = R.type2;
                        relationship.Value = R.relationship;
                        Relationship.Add(type1);
                        Relationship.Add(relationship);
                        Relationship.Add(type2);
                    }
             
                 }

            return doc.ToString();
            
        }
        //-------------------Test display of type tables-----------------------------------------------------------------------------//
        public static void show()
        {
            TT = CodeAnalysis.Analyzer.returnTypeSafe();
            foreach (string key in TT.types.Keys)
            {
                Console.Write("\n  {0, -20} {1, -25} {2, 25}", key, TT.types[key].Namespace, TT.types[key].Filename);
            }
        }
        //------------------Displaying package dependencies--------------------------------------------------------------------------//
        public static void ShowDeps()
        {
            //Console.WriteLine("\nPackage Dependencies");
            foreach (string key in DepAnal.Dependencies.Keys)
            {
                Console.Write("\n  {0}", key);
                foreach (string pkg in DepAnal.Dependencies[key])
                {
                    Console.Write("\n    {0}", pkg);
                }
            }
            Console.Write("\n");
        }
        //---------return Dependencies found-------------------------------------------------------------------------------//
        public static Dictionary<string, List<string>> returnDependencies()
        {
            return Dependencies;
        }
        //--------------------------XML type table ---------------------------------------------------------------------//
        public static string getXmlTypes()
        {
            return CodeAnalysis.Analyzer.getTypesXML();
        }
        //-------------------------Update table from XML ----------------------------------------------------------------//
        public static void MergeXmlTypes(string xml)
        {
            CodeAnalysis.Analyzer.MergeXmlTypes(xml);
        }
        //-----------------------------------gets file name from fully qualified path--------------------------------------------------//
        public static string getFileName(string path)
        {
            string[] name = path.Split('\\');
            int count = name.Count();
            return name[count - 1];
        }
        //-----------------------------------gets directory name from fully qualified ppath-----------------------------//
        public static string getDirName(string path)
        {
            string[] name = path.Split('\\');
            int count = name.Count();
            return name[count - 1];
        }
        //-----------------------get Project list --------------------------------------------------------------------------------//
        public static string getProjects(string path)
        {
            List<string> projects = new List<string>();
            projects = CodeAnalysis.FileMgr.getDirs(path);
            XDocument doc = new XDocument();
            XElement root = new XElement("Projects");
            doc.Add(root);
            foreach(string project in projects)
            {
                XElement T = new XElement("project");
                T.Value = getDirName(project);
                root.Add(T);
            }
            return doc.ToString();
        }
    
    }
 
//----------------------test stub --------------------------------------------------------------------------------------------------------//
#if(Test_DepAnal)   
    class Test
    {
        static void Main(string[] args)
        {
            DepAnal.UpdateTT("../../../../");
            DepAnal.show();
            string projects = DepAnal.getProjects("../../../../");
            Console.WriteLine("\n");
            Console.WriteLine(projects);
            XDocument doc = XDocument.Parse(projects);
            foreach (XElement T in doc.Descendants("project"))
            {
                Console.WriteLine("{0}", T.Value);
            }
            Console.ReadLine();

            //fetch Dependency analysis results for Code Analyzer package
            string DepAnalResult = DepAnal.getDepAnalXML(DepAnal.DependencyAnalysis("CodeAnalyzer"));
            Console.WriteLine("\nPackage Dependencies");
            //show dependency analysis in XML
            DepAnal.ShowDeps();
            Console.WriteLine("\n{0}", DepAnalResult);

            //Test for Parse XML result of dependency analysis
            Console.WriteLine("\n\n");
            XDocument doc1 = XDocument.Parse(DepAnalResult);
            foreach (XElement T in doc1.Descendants("package"))
            {
                Console.WriteLine("\nPackage: {0}\n", T.Attribute("name"));
                foreach (XElement T1 in T.Descendants("dependentPackage"))
                {
                    Console.WriteLine("\t{0}", T1.Value);
                }
            }

            Console.ReadLine();
        }
    }
#endif
}
