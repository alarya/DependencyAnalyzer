/*----------------------------------------------------------------
 * Display.cs - Display package
 * Ver 1.0
 * Language - C#, 2013, .Net Framework 4.5
 * Platform - Sony Vaio T14, Win 8.1
 * Application - CodeAnalyzer| Project #2| Fall 2014|
 * Author - Alok Arya (alarya@syr.edu)
 * ---------------------------------------------------------------
 * Package operations:
 * This package is used for displaying the output on the console.
 * The analyzer calls the display package for displaying the output
 * on the console(for type function analysis or relationship analysis.
 * 
 * If required, the package also saves the output to an XML file.
 * 
 * Required files:
 * Parser.cs 
 * 
 * Note: No test stub as the input is generated from parser for displaying
 * Build command:
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.IO;
namespace CodeAnalysis
{
    public class Display
    {
        //------------------Display type and function analysis---------------------------------
        public void displaytypefunc(List<Elem> typefuncs)
        {
            foreach(Elem typefunc in typefuncs)
            {
                if (typefunc.type == "file")
                    Console.ReadLine();
                switch (typefunc.type)
                {   
                    case "file":
                        Console.WriteLine("\nAnalysis of File: {0} \n\n\n", typefunc.name);
                        break;
                    case "namespace":
                        Console.Write("\n{0}:{1}", typefunc.type.ToString(), typefunc.name.ToString());
                        break;
                    case "interface":
                        Console.Write("\n\n{0}:{1}", typefunc.type.ToString(), typefunc.name.ToString());
                        break;
                    case "struct":
                        Console.Write("\n\n{0}:{1}\n\n", typefunc.type.ToString(), typefunc.name.ToString());
                        break;
                    case "enum":
                        Console.Write("\n\n{0}:{1}\n\n", typefunc.type.ToString(), typefunc.name.ToString());
                        break;
                    case "class":
                        Console.Write("\n{0}:{1}", typefunc.type.ToString(), typefunc.name.ToString());
                        break;
                    case "function":
                        Console.Write("\n\t{0}:{1}", typefunc.type.ToString(), typefunc.name.ToString());
                        Console.Write("\n\t-->Lines of code: {0}", typefunc.loc);
                        Console.Write("\n\t-->Complexity: {0}", typefunc.complexity);
                        Console.WriteLine("\n");
                        break;
                }
            }
            Console.WriteLine();
            Console.Write("\n\n File analyzed..\n\n");
        }

        //---------------------Display relationships----------------------------------------------------------------------------------
        public void displayrelationships(Dictionary<String, List<Relationships>> relationships)
        {
            foreach(KeyValuePair<String, List<Relationships>> pair in relationships)
            {
                Console.WriteLine("\n\n{0}", pair.Key);
                  foreach(Relationships R in pair.Value)
                  {
                      Console.Write("\n  {0,-18} : {1,-18} : {2,-18}", R.type1, R.relationship, R.type2);
                  }
                  Console.ReadLine();
            }
        }
        //-----------------------------Output type func analysis to XML----------------------------------------------------------------
        public void diplayXML1(List<Elem> typefuncs)
        {
            using (StreamWriter stream = new StreamWriter("typefunc.xml"))
            {
                XmlWriterSettings settings = new XmlWriterSettings();
                settings.Indent = true;
                XmlWriter xml = XmlWriter.Create(stream,settings);
                xml.WriteStartDocument();
                xml.WriteStartElement("Type_function_analysis");
                foreach (Elem typefunc in typefuncs)
                {
                    switch (typefunc.type)
                    {
                        case "file":
                            xml.WriteElementString("file", typefunc.name);
                            break;
                        case "namespace":
                            xml.WriteElementString("namespace", typefunc.name.ToString());
                            break;
                        case "interface":
                            xml.WriteElementString("interface", typefunc.name.ToString());
                            break;
                        case "struct":
                            xml.WriteElementString("struct", typefunc.name.ToString());
                            break;
                        case "enum":
                            xml.WriteElementString("enum", typefunc.name.ToString());
                            break;
                        case "class":
                            xml.WriteElementString("class", typefunc.name.ToString());
                            break;
                        case "function":
                            xml.WriteStartElement("function");
                            xml.WriteString(typefunc.name.ToString());
                            xml.WriteStartElement("Lines_of_code");
                            xml.WriteString(typefunc.loc.ToString());
                            xml.WriteEndElement();
                            xml.WriteStartElement("Complexity");
                            xml.WriteString(typefunc.complexity.ToString());
                            xml.WriteEndElement();
                            xml.WriteEndElement();
                            break;
                    }
                    xml.Flush();
                }
                xml.WriteEndDocument();
            }
            Console.WriteLine("\nOutput saved to XML file: {0}\\typefunc.xml", Path.GetFullPath("."));
            Console.ReadLine();
        }

    //-------------output relationships to XML---------------------------------------------------------------------------------------------
        public void displayXML2(Dictionary<String, List<Relationships>> relationships)
    {
        XmlTextWriter xml = new XmlTextWriter("relationship.xml", System.Text.Encoding.UTF8);
        xml.Formatting = Formatting.Indented;
        xml.Indentation = 2;
        xml.WriteStartElement("Relationship analysis");
        foreach (KeyValuePair<String, List<Relationships>> pair in relationships)
        {
            xml.WriteElementString("File",pair.Key);
            foreach (Relationships R in pair.Value)
            {          
                xml.WriteElementString("Type 1",R.type1);
                xml.WriteElementString("Relationship",R.relationship);
                xml.WriteElementString("Type 2",R.type2);
            }
            xml.Flush();
        }
        xml.WriteEndElement();
        Console.WriteLine("\nOutput saved to XML file: {0}\\relationship.xml", Path.GetFullPath("."));
        Console.ReadLine();
    }
 }

    //-----------------test stub------------------------------------------------------------------------------------------------------------
    class testDisplay
    {
#if(TEST_DISPLAY)       
        static void Main(string[] args)
        {
        }
#endif
    }
}
