/*----------------------------------------------------------------------
 * Analyzer.cs - Analyzer acts like an executive for code analysis
 * Ver 1.0
 * Language - C#, 2013, .Net Framework 4.5
 * Platform - Sony Vaio T14, Win 8.1
 * Application - CodeAnalyzer| Project #2| Fall 2014|
 * Author - Alok Arya (alarya@syr.edu)
 * ---------------------------------------------------------------------
 * Package operations:
 * Analyzer interacts with the File manger module to get file references.
 * Analyzer calls the Semi Expressions package for converting source code
 * files into semi expressions.
 * This package calls the parser via builder for code analysis.
 * The package also calls Display packge for displaying analysis results
 * and saving output to XML files.
 * 
 * Required files:
 * Semi.cs parser.cs toker.cs Display.cs
 * 
 * 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;


namespace CodeAnalysis
{  
    public class Analyzer
    {
        
        //-------------------Calls Filemanager to get all files which need to be analyzed------------------------------------------
        public static List<String> getFiles(string path, List<String> patterns, bool recurse)
        {
            FileMgr fm = new FileMgr();
            foreach (string pattern in patterns)
                fm.addPattern(pattern);
                
            fm.findFiles(path,recurse);
            return fm.returnFiles();
        }

        //----------------------Parse 1 for type and function analysis------------------------------------------------------------------
        public static void parse1(List<String> files)
        {
            List<Elem> typefunc = new List<Elem>();
            DependencyAnalyzer.TypeTable TT = new DependencyAnalyzer.TypeTable();
      
            foreach (object file in files)
            {
                
                CSsemi.CSemiExp semi = new CSsemi.CSemiExp();
                semi.displayNewLines = false;
                if (!semi.open(file as string))
                {
                    Console.Write("\n  Can't open {0}\n\n", file);
                }

                BuildCodeAnalyzer builder = new BuildCodeAnalyzer(semi);
                Parser parser = builder.build();
                Repository rep = Repository.getInstance();
                List<Elem> temp = Repository.gettypes();
                Elem E = new Elem();
                E.type = "file";
                E.name = getFileName(file.ToString());
                temp.Add(E);       //add file name 
                try
                {
                    while (semi.getSemi())
                        parser.parse(semi);
                }
                catch (Exception ex)
                {
                    Console.Write("\n\n  {0}\n", ex.Message);
                }

                
                //typefunc.AddRange(rep.typefunc); //will be passed to Xml output

                
                semi.close();
            }            
            List<Elem> types = new List<Elem>();
            types = Repository.gettypes();
            
          
        }
        //---------------------Merge type tables----------------------------------------------------------------------------------//
        public static DependencyAnalyzer.TypeTable returnTypeSafe()
        {
            //Add new types to thread safe type table
            List<Elem> Types = Repository.gettypes();
            DependencyAnalyzer.TypeTable TT = new DependencyAnalyzer.TypeTable();
            string namespaceName = "abc";
            string fileName = "xyz";
            foreach (Elem t in Types)
            {
                if (t.type == "file")
                {
                    fileName = t.name;
                    continue;
                }
                if (t.type == "namespace")
                {
                    namespaceName = t.name;
                    continue;
                }
                bool add = TT.add(t.name, namespaceName, fileName);
            }
            return TT;
        }
        //---------------------DependencyAnalysis --------------------------------------------------------------------------------//
        public static Dictionary<string, List<string>> DepAnalysis(string path, bool subdir)
        {
            List<string> files = new List<string>();
            List<string> patterns = new List<string>();
            patterns.Add("*.cs");           
            Dictionary<string, List<string>> DepList = new Dictionary<string, List<string>>();
            DependencyAnalyzer.TypeTable TT = returnTypeSafe();
            string dependentPackage = null;
            files = Analyzer.getFiles(path, patterns, subdir);
            if (files.Count == 0) {                    
                Console.WriteLine("\nNo files found during dependency analysis");
                Console.ReadLine();
                return null;}           
            foreach (string file in files)
            {DepList[getFileName(file)] = new List<string>();}
            foreach (object file in files)
            {
                    CSsemi.CSemiExp semi = new CSsemi.CSemiExp();
                    semi.displayNewLines = false;
                    if (!semi.open(file as string)){
                        Console.Write("\n  Can't open {0}\n\n", file);
                        return null;}
                    BuildCodeAnalyzer builder = new BuildCodeAnalyzer(semi);
                    Repository rep = Repository.getInstance();
                    int index = -1;
                    try
                    {
                        while (semi.getSemi())
                        {
                            foreach (string type in TT.types.Keys)
                            {
                                index = semi.Contains(type);
                                if (index != -1 && TT.types[type].Filename != getFileName(file.ToString()))
                                {
                                    dependentPackage = TT.types[type].Filename;
                                    if(!(DepList[getFileName(file.ToString())].Exists(x => x == dependentPackage)))
                                    DepList[getFileName(file.ToString())].Add(dependentPackage);                                  
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {Console.Write("\n\n  {0}\n", ex.Message);}
                    if (index != -1)
                        DepList[getFileName(file.ToString())].Add(dependentPackage);
                    semi.close();
            }
            return DepList;
        }
  
        //----------------------Second parse... to find type relationships-------------------------------------------------------
        public static Dictionary<string, List<Relationships>> parse2(string path, bool subdir, string packageName)
        {
            //Console.Write("\n  Type Relationships ");
            //Console.Write("\n ----------------------------\n");
            Dictionary<string, List<Relationships>> reln = new Dictionary<string, List<Relationships>>();
            List<string> files = new List<string>();
            List<string> patterns = new List<string>();
            patterns.Add("*.cs");
            files = Analyzer.getFiles(path, patterns, subdir);
            
            if (files.Count == 0)
            {
                Console.WriteLine("\nNo files found during dependency analysis");
                Console.ReadLine();
                return null;
            }

            foreach (object file in files)
            {
                CSsemi.CSemiExp semi = new CSsemi.CSemiExp();
                semi.displayNewLines = false;

                if (!semi.open(file as string))
                {
                    Console.Write("\n  Can't open {0} during relationship analysis\n\n", file);
                    return null;
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
                    Console.Write("\n\nException encountered:{0}\n", ex.Message);
                }
                semi.close();
                //ordering relationship list by type1 and the relationship
                List<Relationships> temp = FinalRelationships.getrelationships().OrderBy(x => x.type1).ThenBy(x => x.relationship).ToList<Relationships>();
                reln.Add(file as string, temp);   // adding relationship analysis file wise in a dictionary
            }
            return reln;
            //Display d2 = new Display();
            //d2.displayrelationships(reln);
            //if (xml)
            //    d2.displayXML2(reln);
   
        }
        
        
        //-------------------------------------------creates type table--------------------------------------------------------------
        public static void createTypeTable(string path, List<string> patterns, bool subdir)
        {
            
            List<string> files = new List<string>();
            files = Analyzer.getFiles(path,patterns,subdir);
            if (files.Count == 0)
            {
                Console.WriteLine("\nNo files found....Please recheck the patterns provided");
                Console.ReadLine();
            }
                      
           parse1(files);           
        }
        //-----------------------------------gets file name from fully qualified path--------------------------------------------------//
        public static String getFileName(String path)
        {
            string[] name = path.Split('\\');
            int count = name.Count();
            return name[count - 1];
        }
        //----------------------------------print Dep Analysis list-----------------------------------------------------------//
        public static void ShowDeps(Dictionary<string, List<string>> DepList)
        {
            Console.WriteLine("\n\nPackage Dependencies");
            foreach (string key in DepList.Keys)
            {
                Console.Write("\n\n\n  {0}", key);
                foreach (string pkg in DepList[key])
                {
                    Console.Write("\n    {0}", pkg);
                }
            }
            Console.Write("\n");
        }
        // -------------------------------Finalrelationships to RelnTable for specified package----------------------------------------//
        public static DependencyAnalyzer.RelationshipsTable Convert(Dictionary<String, List<Relationships>> RT, string packageName)
        {
            DependencyAnalyzer.RelationshipsTable RTnew = new DependencyAnalyzer.RelationshipsTable();
            DependencyAnalyzer.RelInfo Rtemp = new DependencyAnalyzer.RelInfo();
            foreach (KeyValuePair<String, List<Relationships>> pair in RT)
            {
                if(getFileName(pair.Key) == packageName)
                foreach (Relationships R in pair.Value)
                {
                    Rtemp.relationship = R.relationship;
                    Rtemp.instance = R.type2;
                    RTnew.add(R.type1, Rtemp);
                    Console.Write("\n  {0,-18} : {1,-18} : {2,-18}", R.type1, R.relationship, R.type2);
                }
            }
            return RTnew;
        }
        //----------------------------get XML format of type tables ----------------------------------------------------------------//
        public static string getTypesXML()
        {
            List<Elem> types = Repository.gettypes();
            XDocument doc = new XDocument();
            XElement root = new XElement("typeInfo");
            doc.Add(root);
            foreach (Elem type in types)
            {
                XElement type_ = new XElement("Type");
                root.Add(type_);
                XElement T1 = new XElement("type");
                XElement T2 = new XElement("Name");
                T1.Value = type.type;
                T2.Value = type.name;
                type_.Add(T1);
                type_.Add(T2);
            }
            return doc.ToString();
        }
        //-------------------------Update table from XML ----------------------------------------------------------------//
        public static void MergeXmlTypes(string xml)
        {
            XDocument doc = XDocument.Parse(xml);
            List<Elem> types = Repository.gettypes();
            foreach(XElement T in doc.Descendants("Type"))
            {
                Elem elem = new Elem(); 
                elem.type = T.Element("type").Value;
                elem.name = T.Element("Name").Value;
                types.Add(elem);
                //Console.WriteLine("\n{0}  {1}",elem.type, elem.name);
            }

            //Printing old type table --- for testing
             //foreach (Elem type in types)
            //{
            //    Console.WriteLine("\n {0} {1}", type.type, type.name);
            //}

            Console.WriteLine("\n\n Merged type table :-");
            DependencyAnalyzer.TypeTable TT = returnTypeSafe();
            foreach (string key in TT.types.Keys)
            {
                Console.Write("\n  {0, -20} {1, -25} {2, 25}", key, TT.types[key].Namespace, TT.types[key].Filename);
            }
           
        }
//<-------------test stub----------------------------------------------------------------------------->
#if(TEST_ANALYZER)     
        static void Main(string[] args)
        {
            string path = "../../../../CodeAnalyzer";
            List<string> patterns = new List<string>();
            patterns.Add("*.cs");
                       
            createTypeTable(path, patterns,true);
            DependencyAnalyzer.TypeTable TT = returnTypeSafe();
            foreach (string key in TT.types.Keys)
            {
                Console.Write("\n  {0, -20} {1, -25} {2, 25}", key, TT.types[key].Namespace, TT.types[key].Filename);
            }
            Console.ReadLine();

            //sending only types of the package for dependencies
            //List<string> packageTypes = new List<string>();
            //foreach (string key in TT.types.Keys)
            //{
            //    if (TT.types[key].Filename == "Analyzer.cs") //simulating package dependencies on parser package
            //        packageTypes.Add(key);
            //}
            Dictionary<string, List<string>> DepList = new Dictionary<string, List<string>>();
            DepList = DepAnalysis(path, true); // do dependency analysis in Parser.cs
            ShowDeps(DepList);
            Console.ReadLine();
            
            //Console.WriteLine("\n\nType table in XML");
            //string xml = getTypesXML();
            //Console.WriteLine("\n{0}", xml);

            Dictionary<string, List<Relationships>> RT = parse2(path, true, "*.cs");
            //DependencyAnalyzer.RelationshipsTable RTnew = new DependencyAnalyzer.RelationshipsTable();
            //RTnew = Analyzer.Convert(RT, "Parser.cs");
            //Diplay relationships
            Console.WriteLine("\n\nRelationships: ");
            foreach (KeyValuePair<String, List<Relationships>> pair in RT)
            {
                    foreach (Relationships R in pair.Value)
                    {
                        Console.Write("\n  {0,-18} : {1,-18} : {2,-18}", R.type1, R.relationship, R.type2);
                    }
            }
            //DependencyAnalyzer.RelationshipsTable.display(RTnew);
            Console.ReadLine();

            XDocument doc = new XDocument();
            XElement root = new XElement("typeInfo");
            doc.Add(root);
            XElement type_ = new XElement("Type");
            root.Add(type_);
            XElement T1 = new XElement("type");
            XElement T2 = new XElement("Name");
            T1.Value = "class";
            T2.Value = "A";
            type_.Add(T1);
            type_.Add(T2);
            type_ = new XElement("Type");
            root.Add(type_);
            T1 = new XElement("type");
            T2 = new XElement("Name");
            T1.Value = "class";
            T2.Value = "B";
            type_.Add(T1);
            type_.Add(T2);
            Console.WriteLine("\n{0}",doc.ToString());
            Console.ReadLine();

            MergeXmlTypes(doc.ToString());

            
            Console.ReadLine();   
        }
#endif    
    }
}