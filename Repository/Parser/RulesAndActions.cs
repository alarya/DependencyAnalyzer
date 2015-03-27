/*--------------------------------------------------------------------
* RulesAndActions.cs - Parser rules specific to an application      
* ver 2.2                                                           
* Language:    C#, 2013, .Net Framework 4.5                         
* Platform:    Sony Vaio T14, Win 8.1                     
* Application: Demonstration for CSE681, Project #2, Fall 2011      
* Original Author:      Jim Fawcett, CST 4-187, Syracuse University          
*                      (315) 443-3948, jfawcett@twcny.rr.com  
* Modified by: alok arya (alarya@syr.edu)
*---------------------------------------------------------------------
*
* Package Operations:
* -------------------
* RulesAndActions package contains all of the Application specific
* code required for most analysis tools.
*
* It defines the following Four rules which each have a
* grammar construct detector and also a collection of IActions:
*   - DetectNameSpace rule
*   - DetectClass rule
*   - DetectFunction rule
*   - DetectScopeChange
*   
*   Three actions - some are specific to a parent rule:
*   - Print
*   - PrintFunction
*   - PrintScope
* 
* The package also defines a Repository class for passing data between
* actions and uses the services of a ScopeStack, defined in a package
* of that name.
*
* Note:
* This package does not have a test stub since it cannot execute
* without requests from Parser.
*  
*
* Required Files:
*   IRuleAndAction.cs, RulesAndActions.cs, Parser.cs, ScopeStack.cs,
*   Semi.cs, Toker.cs
*   
* Build command:
*   csc /D:TEST_PARSER Parser.cs IRuleAndAction.cs RulesAndActions.cs \
*                      ScopeStack.cs Semi.cs Toker.cs
*   
* Maintenance History:
* --------------------
* ver 2.2 : 24 Sep 2011
* - modified Semi package to extract compile directives (statements with #)
*   as semiExpressions
* - strengthened and simplified DetectFunction
* - the previous changes fixed a bug, reported by Yu-Chi Jen, resulting in
* - failure to properly handle a couple of special cases in DetectFunction
* - fixed bug in PopStack, reported by Weimin Huang, that resulted in
*   overloaded functions all being reported as ending on the same line
* - fixed bug in isSpecialToken, in the DetectFunction class, found and
*   solved by Zuowei Yuan, by adding "using" to the special tokens list.
* - There is a remaining bug in Toker caused by using the @ just before
*   quotes to allow using \ as characters so they are not interpreted as
*   escape sequences.  You will have to avoid using this construct, e.g.,
*   use "\\xyz" instead of @"\xyz".  Too many changes and subsequent testing
*   are required to fix this immediately.
* ver 2.1 : 13 Sep 2011
* - made BuildCodeAnalyzer a public class
* ver 2.0 : 05 Sep 2011
* - removed old stack and added scope stack
* - added Repository class that allows actions to save and 
*   retrieve application specific data
* - added rules and actions specific to Project #2, Fall 2010
* ver 1.1 : 05 Sep 11
* - added Repository and references to ScopeStack
* - revised actions
* - thought about added folding rules
* ver 1.0 : 28 Aug 2011
* - first release
*
* Planned Modifications (not needed for Project #2):
* --------------------------------------------------
* - add folding rules:
*   - CSemiExp returns for(int i=0; i<len; ++i) { as three semi-expressions, e.g.:
*       for(int i=0;
*       i<len;
*       ++i) {
*     The first folding rule folds these three semi-expression into one,
*     passed to parser. 
*   - CToker returns operator[]( as four distinct tokens, e.g.: operator, [, ], (.
*     The second folding rule coalesces the first three into one token so we get:
*     operator[], ( 
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace CodeAnalysis
{
    //-----------------------------------holds scope information---------------------------------------------------------------------------
    public class Elem  
    {
        public string type { get; set; }
        public string name { get; set; }
        public int begin { get; set; }
        public int end { get; set; }
        public int loc { get; set; }
        public int complexity { get; set; }

        public override string ToString()
        {
            StringBuilder temp = new StringBuilder();
            temp.Append("{");
            temp.Append(String.Format("{0,-10}", type)).Append(" : ");
            temp.Append(String.Format("{0,-10}", name)).Append(" : ");
            temp.Append(String.Format("{0,-5}", begin.ToString()));  // line of scope start
            temp.Append(String.Format("{0,-5}", end.ToString()));    // line of scope end
            temp.Append(String.Format("{0,-5}", loc.ToString()));
            temp.Append("}");
            return temp.ToString();
        }
    }

    //-----------------------------structure for storing type relationships----------------------------------------------------------------
    public class Relationships              
    {
        public string type1 { get; set; }
        public string relationship { get; set; }
        public string type2 { get; set; }
    
    }

    //--------------------------------Store relationships found here------------------------------------------------------------------------
    public static class FinalRelationships          
    {
        static List<Relationships> relationships = new List<Relationships>();
        //static DependencyAnalyzer.RelationshipsTable RT = new DependencyAnalyzer.RelationshipsTable();
        public static List<Relationships> getrelationships()
        {
            return relationships;
        }
        public static void ReinitializeList()
        {
            relationships = new List<Relationships>();
        }
    }

    //------------------------------------Central repository for parsing----------------------------------------------------------------------
    public class Repository
    {
        ScopeStack<Elem> stack_ = new ScopeStack<Elem>();
        List<Elem> locations_ = new List<Elem>();
        static Repository instance;
        public static int scopes;
        List<Elem> typefunc_ = new List<Elem>();
        static List<Elem> Types = new List<Elem>();
        static List<string> types = new List<string>(); //This is list of types for which relationship analysis happens
        public static int cycle = 1 ;
        
        public Repository()
        {
            instance = this;
        }
        public static void setTypes(List<string> T)
        {
            types = T;
        }
        public static Repository getInstance()
        {
            return instance;
        }

        //<-------------------------------provides all actions access to current semiExp--------------------------------------------------->
        public CSsemi.CSemiExp semi
        {
            get;
            set;
        }
        //<---------------------------------------saved by newline rule's action------------------------------------------------------------>
        public int lineCount  
        {
            get { return semi.lineCount; }
        }
        
        public int prevLineCount  
        {
            get;
            set;
        }
        //<----------------------enables recursively tracking entry and exit from scopes------------------------------------------------------>
        public ScopeStack<Elem> stack  // pushed and popped by scope rule's action
        {
            get { return stack_; }
        }
    
        public List<Elem> locations
        {
            get { return locations_; }
        }

        public List<Elem> typefunc
        {
            get { return typefunc_; }
        }

        public static void addtypes(Elem elem)
        {
            Repository.Types.Add(elem);
        }

        public static List<Elem> gettypes()
        {
            return Repository.Types;
        }
        public static List<string> getPackageTypes()
        {
            return types;
        }
    }

    //---------pushes scope info on stack when entering new scope------------------------------------------------------------------
    public class PushStack : AAction
    {
        Repository repo_;

        public PushStack(Repository repo)
        {
            repo_ = repo;
        }
        public override void doAction(CSsemi.CSemiExp semi)
        {
            Elem elem = new Elem();
            elem.type = semi[0];  // expects type
            elem.name = semi[1];  // expects name
            elem.begin = repo_.semi.lineCount - 1;
            elem.end = 0;
           
            repo_.stack.push(elem);        //pushing element into stack

            if (elem.type == "control" || elem.name == "anonymous")
                return;

            repo_.locations.Add(elem);

            if (AAction.displaySemi)
            {
                switch (elem.type)
                {

                    case "namespace":
                      //Console.Write("\n{0}:{1}", elem.type.ToString(), elem.name.ToString());
                        if(Repository.cycle == 1)
                        Repository.addtypes(elem);
                        repo_.typefunc.Add(elem);
                        break;
                    case "interface":
                      //Console.Write("\n\n{0}:{1}", elem.type.ToString(), elem.name.ToString());
                        if (Repository.cycle == 1)
                        Repository.addtypes(elem);
                        break;
                    case "struct":
                      //Console.Write("\n\n{0}:{1}", elem.type.ToString(), elem.name.ToString());
                        if (Repository.cycle == 1)
                        Repository.addtypes(elem);
                        repo_.typefunc.Add(elem);
                        break;
                    case "enum":
                        if (Repository.cycle == 1)
                        Repository.addtypes(elem);
                        repo_.typefunc.Add(elem);
                        break;
                    case "class":
                      //Console.Write("\n{0}:{1}", elem.type.ToString(), elem.name.ToString());
                        if (Repository.cycle == 1)
                        Repository.addtypes(elem);                
                        repo_.typefunc.Add(elem);
                        break;
                    case "function":
                      //Console.Write("\n\t{0}:{1}", elem.type.ToString(), elem.name.ToString());
                        Repository.scopes = 1;   
                        break;

                }
            }
            if (AAction.displayStack)
                repo_.stack.display();
        }
    }
    
    //----------------pops scope info from stack when leaving scope------------------------------------------------------------------------
    public class PopStack : AAction
    {
        Repository repo_;

        public PopStack(Repository repo)
        {
            repo_ = repo;
        }
        //-----------------------Add functions into type function analysis list--------------------------------------------------------------
        public void checkForFunction(Elem elem)
        {
            switch (elem.type)
            {

                case "namespace":
                    break;
                case "class":
                    break;
                case "function":
                    Elem temp = new Elem();
                    temp.type = elem.type;
                    temp.name = elem.name;
                    temp.loc = elem.end - elem.begin;
                    temp.complexity = Repository.scopes;
                    repo_.typefunc.Add(temp);   // adding function analysis into functype list
                    break;
            }
        }
        public override void doAction(CSsemi.CSemiExp semi)
        {
            Elem elem;
            try
            {
                elem = repo_.stack.pop();
                for (int i = 0; i < repo_.locations.Count; ++i)
                {
                    Elem temp = repo_.locations[i];
                    if (elem.type == temp.type)
                    {
                        if (elem.name == temp.name)
                        {
                            if ((repo_.locations[i]).end == 0)
                            {
                                (repo_.locations[i]).end = repo_.semi.lineCount;
                                break;
                            }
                        }
                    }
                }
            }
            catch
            {
                //Console.Write("popped empty stack on semiExp: ");
                //semi.display();
                return;
            }
            CSsemi.CSemiExp local = new CSsemi.CSemiExp();
            local.Add(elem.type).Add(elem.name);
            if (local[0] == "control")
                return;

            if (AAction.displaySemi)
            {
                checkForFunction(elem);                
            }
        }
    }

    //----------------------Action to store aggregation found---------------------------------------------------------------------------------
    public class Aggfound : AAction
    {
        Repository repo_;

        public Aggfound(Repository repo)
        {
            repo_ = repo;
        }

        public override void doAction(CSsemi.CSemiExp semi)
        {
            List<Elem> types = new List<Elem>();
            types = Repository.gettypes();

            if (types.Exists(x => x.name == semi[1])) //checking if aggregating one of existing types 
            {
                String temp;
                Elem temp1 = new Elem();
                Relationships temp2 = new Relationships();

                //look for last class in locations table
                for (int i = (repo_.locations.Count - 1); i >= 0; i--)
                {
                    temp1 = repo_.locations[i];
                    temp = temp1.type;
                    if (temp.Equals("class"))
                    {
                        temp2.type1 = temp1.name;
                        temp2.type2 = semi[1];
                        temp2.relationship = "aggregates";
                        break;
                    }
                }
                List<Relationships> temp3 = FinalRelationships.getrelationships();
                //don't add duplicate relationships
                if (!(temp3.Exists(x => x.type1 == temp2.type1 && x.relationship == temp2.relationship && x.type2 == temp2.type2)))              //notworking
                    FinalRelationships.getrelationships().Add(temp2);
            }
        }
    }   

    //---------------------Action to store inheritance found-------------------------------------------------------------------------------
    public class Inheritancefound : AAction
    {
        Repository repo_;

        public Inheritancefound(Repository repo)
        {
            repo_ = repo;
        }
        public override void doAction(CSsemi.CSemiExp semi)
        {
            List<Elem> types = new List<Elem>();
            types = Repository.gettypes();
            for (int i = 1; i < semi.count; i++)
                if (types.Exists(x => x.name == semi[i]))
                {
                    Relationships temp2 = new Relationships();
                    temp2.type1 = semi[0];
                    temp2.type2 = semi[i];
                    temp2.relationship = "Inherits";
                    FinalRelationships.getrelationships().Add(temp2);
                }

            Elem elem = new Elem();
            elem.type = "class";  // expects type
            elem.name = semi[0];  // expects name
            elem.begin = repo_.semi.lineCount - 1;
            elem.end = 0;

            repo_.stack.push(elem);      // push the class into scopestack
            repo_.locations.Add(elem);   //push the class into locations table 
        } 
    }

    //-----------------Action for detect using relationship------------------------------------------------------------------------------------------------
    public class Usingaction : AAction
    {
        Repository repo_;

        public Usingaction(Repository repo)
        {
            repo_ = repo;
        }

        public override void doAction(CSsemi.CSemiExp semi)
        {
            List<Elem> types = new List<Elem>();
            types = Repository.gettypes();
            Elem temp1 = new Elem();
            String temp;
            Relationships temp2 = new Relationships();

            //look for latest class scope in locations table
            for (int i = (repo_.locations.Count - 1); i >= 0; i--)
            {
                temp1 = repo_.locations[i];
                temp = temp1.type;
                if (temp.Equals("class"))
                {
                    temp2.type1 = temp1.name;                 // store the class name
                    break;
                }
            }

            for (int i = 0; i < semi.count; i++)            //checking all tokens in semi expression with existing user defined value types
            {
                if (types.Exists(x => x.name == semi[i] && (x.type == "class" || x.type == "struct" || x.type == "enum")))
                {
                    temp2.type2 = semi[i];
                    temp2.relationship = "Uses";
                    List<Relationships> temp3 = FinalRelationships.getrelationships();
                    //don't add duplicate relationships
                    if (!(temp3.Exists(x => x.type1 == temp2.type1 && x.relationship == temp2.relationship && x.type2 == temp2.type2)))
                        FinalRelationships.getrelationships().Add(temp2);
                }
            }
      
        }
    }

    //------------------Action for detect composition----------------------------------------------------------------------------------
    public class CompositionAction : AAction
    {
        Repository repo_;

        public CompositionAction(Repository repo)
        {
            repo_ = repo;
        }

        public override void doAction(CSsemi.CSemiExp semi)
        {
            List<Elem> types = new List<Elem>();
            types = Repository.gettypes();
            Elem temp1 = new Elem();
            String temp;
            Relationships temp2 = new Relationships();
            //look for latest class scope in locations table
            for (int i = (repo_.locations.Count - 1); i >= 0; i--)
            {
                temp1 = repo_.locations[i];
                temp = temp1.type;
                if (temp.Equals("class"))
                {
                    temp2.type1 = temp1.name;                 // store the class name
                    break;
                }
            }

            temp2.type2 = semi[0];
            temp2.relationship = "Composes";
            //don't add duplicate relationships
            List<Relationships> temp3 = FinalRelationships.getrelationships();
            if (!(temp3.Exists(x => x.type1 == temp2.type1 && x.relationship == temp2.relationship && x.type2 == temp2.type2)))
                FinalRelationships.getrelationships().Add(temp2);
        }
    }

    //--------------action to print function signatures--------------------------------------------------------------------------------
    public class PrintFunction : AAction
        {
            Repository repo_;

            public PrintFunction(Repository repo)
            {
                repo_ = repo;
            }
            public override void display(CSsemi.CSemiExp semi)
            {
                for (int i = 0; i < semi.count; ++i)
                    if (semi[i] != "\n" && !semi.isComment(semi[i]))
                        Console.Write("{0} ", semi[i]);
            }
            public override void doAction(CSsemi.CSemiExp semi)
            {
                this.display(semi);
            }
        }

    //--------------concrete printing action, useful for debugging-------------------------------------------------------------------------
    public class Print : AAction
        {
            Repository repo_;

            public Print(Repository repo)
            {
                repo_ = repo;
            }
            public override void doAction(CSsemi.CSemiExp semi)
            {
                Console.Write("\n  line# {0}", repo_.semi.lineCount - 1);
                this.display(semi);
            }
        }

    //-----------------Braceless scopes-----------------------------------------------------------------------------------------------------
    public class Braceless : AAction
    {
        Repository repo_;

        public Braceless(Repository repo)
        {
            repo_ = repo;
        }
        public override void doAction(CSsemi.CSemiExp semi)
        {
            Repository.scopes++;
        }
    }

    //----------------------rule to detect namespace declarations--------------------------------------------------------------------------
    public class DetectNamespace : ARule
        {
            public override bool test(CSsemi.CSemiExp semi)
            {
                int index = semi.Contains("namespace");
                if (index != -1)
                {
                    CSsemi.CSemiExp local = new CSsemi.CSemiExp();
                    // create local semiExp with tokens for type and name
                    local.displayNewLines = false;
                    local.Add(semi[index]).Add(semi[index + 1]);
                    doActions(local);
                    return true;
                }
                return false;
            }
        }

    //-------------------Rule to detect an aggregation---------------------------------------------------------------------------------------
    public class DetectAggregation : ARule
        {
            public override bool test(CSsemi.CSemiExp semi)
            {
                int index = semi.Contains("new");
                int index1 = semi.Contains(".");  // if qualified name is used in aggregation    
                int index2 = semi.Contains("<");  // class aggregating List of another class
                if (index != -1)
                {                    
                    CSsemi.CSemiExp local = new CSsemi.CSemiExp();
                    local.displayNewLines = false;
                    if (index1 == -1 && index2 == -1)
                        local.Add(semi[index]).Add(semi[index + 1]);
                    else if(index1 != -1)
                    local.Add(semi[index]).Add(semi[index1 + 1]);
                    else if(index2 != -1)
                        local.Add(semi[index]).Add(semi[index2 + 1]);
                    //Console.WriteLine("\n {0} {1}", semi[index],semi[index+1]); //for testing
                    doActions(local);
                    return true;
                }
                return false;
            }
        }

     //-----------------Rule to detect an Composition------------------------------------------------------------------------------------------
     public class DetectComposition : ARule
        {
            public override bool test(CSsemi.CSemiExp semi)
            {
                List<Elem> types = Repository.gettypes();
                Elem elem = new Elem();
                CSsemi.CSemiExp local = new CSsemi.CSemiExp();
                local.displayNewLines = false;
                for (int i = 0; i < semi.count; i++)
                { 
                    for(int j = 0 ; j < types.Count ; j++ )
                    {
                        elem = types[j];
                        if (semi[i] == elem.name && (elem.type == "struct" || elem.type == "enum") && Repository.getPackageTypes().Contains(semi[i]))
                        {
                            local.Add(elem.name);
                            doActions(local);
                            return true;
                        }
                    }
                }

                return false;
            }
        }

   //-----------------Rule to Detect inheritance-----------------------------------------------------------------------------------------------
   public class DetectInheritance : ARule
        {
            public override bool test(CSsemi.CSemiExp semi)
            {
                int index = semi.Contains("class");
                int index1 = semi.Contains(":");
                int count = semi.Contains(",");    // check multiple implementations of interfaces
                if(index!= -1 && index1 != -1)
                {
                    CSsemi.CSemiExp local = new CSsemi.CSemiExp();
                    local.displayNewLines = false;
                    local.Add(semi[index+1]);
                    for (int i = index1+1; i < semi.count; i = i + 2 )
                        local.Add(semi[i]);
                    //for(int i= 0; i<semi.count;i++)
                       // Console.WriteLine("\ntest {0}", semi[i]);
                    doActions(local);
                    return true;
                }
                return false;
            }
        }

    //------------------rule to dectect class definitions----------------------------------------------------------------------------------
    public class DetectClass : ARule
        {
            public override bool test(CSsemi.CSemiExp semi)
            {
                int indexCL = semi.Contains("class");
                int indexIF = semi.Contains("interface");  
                int indexST = semi.Contains("struct");
                int indexEN = semi.Contains("enum");
                int index = Math.Max(indexCL, indexIF);
                index = Math.Max(index, indexST);
                index = Math.Max(index, indexEN);
                if (index != -1)
                {
                    CSsemi.CSemiExp local = new CSsemi.CSemiExp();
                    // local semiExp with tokens for type and name
                    local.displayNewLines = false;
                    local.Add(semi[index]).Add(semi[index + 1]);
                    doActions(local);
                    return true;
                }
                return false;
            }
        }

    //---------------rule to dectect function definitions------------------------------------------------------------------------------------
    public class DetectFunction : ARule
        {
            public static bool isSpecialToken(string token)
            {
                string[] SpecialToken = { "if", "for", "foreach", "while", "catch", "using","switch" };
                foreach (string stoken in SpecialToken)
                    if (stoken == token)
                        return true;
                return false;
            }
            public override bool test(CSsemi.CSemiExp semi)
            {
                if (semi[semi.count - 1] != "{")
                    return false;

                int index = semi.FindFirst("(");

                if (index > 0 && !isSpecialToken(semi[index - 1]))
                {
                    CSsemi.CSemiExp local = new CSsemi.CSemiExp();
                    local.Add("function").Add(semi[index - 1]);
                    doActions(local);
                    return true;
                 }
                return false;
            }
         }

    //------------------Detect braceless scope--------------------------------------------------------------------------------------------=
    public class DetectbracelessScope: ARule
    {
        public static bool isSpecialToken(string token)
        {
            string[] SpecialToken = { "if", "for", "foreach", "catch", "else","while"};
            foreach (string stoken in SpecialToken)
                if (stoken == token)
                    return true;
            return false;
        }
        public override bool test(CSsemi.CSemiExp semi)
        {
            if (semi[semi.count - 1] == "{")
                return false;

            int index = semi.FindFirst("(");

            if (index > 0 && isSpecialToken(semi[index - 1]))
            {
                CSsemi.CSemiExp local = new CSsemi.CSemiExp();
                doActions(local); 
                return true;
            }
            return false;
        }
    }
    //-----------------rule to dectect Using in function definitions-------------------------------------------------------------------------
    public class DetectUsing : ARule
        {
            public static bool isSpecialToken(string token)
            {
                string[] SpecialToken = { "if", "for", "foreach", "while", "catch", "using" };
                foreach (string stoken in SpecialToken)
                    if (stoken == token)
                        return true;
                return false;
            }
            public override bool test(CSsemi.CSemiExp semi)
            {
                if (semi[semi.count - 1] != "{")   
                    return false;
 
                int index = semi.FindFirst("(");

                if (index > 0 && !isSpecialToken(semi[index - 1]))
                {
                    CSsemi.CSemiExp local = new CSsemi.CSemiExp();
                    if (semi[index + 1] == ")")                    //no parameters existing, no need to check for using relationship
                    {
                        return false;
                    }
                    else                                          //parameters exist, send semi containing parameters
                    {                        
                        for (int i = index+1 ; i < semi.count-1; i++)
                        {
                            if (semi[i] != "ref" && semi[i] != "out" && semi[i] != "in" && semi[i] != "," && semi[i] != ".")
                            {
                                local.Add(semi[i]);
                            }
                        }
                        doActions(local);
                        return false;      //so function name can be detected
                    }
                }
                return false;
            }
        }
  
     //-----------detect entering anonymous scope------------------------------------------------------------------------------------------
     public class DetectAnonymousScope : ARule
        {
            public override bool test(CSsemi.CSemiExp semi)
            {
                int index = semi.Contains("{");
                if (index != -1)
                {
                    CSsemi.CSemiExp local = new CSsemi.CSemiExp();
                    // create local semiExp with tokens for type and name
                    local.displayNewLines = false;
                    local.Add("control").Add("anonymous");
                    doActions(local);
                    return true;
                }
                return false;
            }
        }

     //-----------detect leaving scope----------------------------------------------------------------------------------------------------
     public class DetectLeavingScope : ARule
        {
            public override bool test(CSsemi.CSemiExp semi)
            {
                int index = semi.Contains("}");
                if (index != -1)
                {
                    doActions(semi);
                    ++Repository.scopes;     //incrementing scopes by 1 when leaving a scope
                    return true;

                }
                return false;
            }
        }
     //----------------aggregates the parser and rules and action classes-----------------------------------------------------------------   
    public class BuildCodeAnalyzer
        {
            Repository repo = new Repository();

            public BuildCodeAnalyzer(CSsemi.CSemiExp semi)
            {
                repo.semi = semi;
                FinalRelationships.ReinitializeList();
            }
            public virtual Parser build()
            {
                Parser parser = new Parser();

                // decide what to show
                AAction.displaySemi = true;
                AAction.displayStack = false;  // this is default so redundant

                // action used for namespaces, classes, and functions
                PushStack push = new PushStack(repo);

                // capture namespace info
                DetectNamespace detectNS = new DetectNamespace();
                detectNS.add(push);
                parser.add(detectNS);

                // capture class info
                DetectClass detectCl = new DetectClass();
                detectCl.add(push);
                parser.add(detectCl);

                // capture function info
                DetectFunction detectFN = new DetectFunction();
                detectFN.add(push);
                parser.add(detectFN);

                //Braceless scope
                Braceless bsa = new Braceless(repo);
                DetectbracelessScope bs = new DetectbracelessScope();
                bs.add(bsa);
                parser.add(bs);
                
                // handle entering anonymous scopes, e.g., if, while, etc.
                DetectAnonymousScope anon = new DetectAnonymousScope();
                anon.add(push);
                parser.add(anon);

                // handle leaving scopes
                DetectLeavingScope leave = new DetectLeavingScope();
                PopStack pop = new PopStack(repo);
                leave.add(pop);
                parser.add(leave);

                // parser configured
                return parser;
            }

            public virtual Parser build1()
            {
                
                Parser parser = new Parser();
                Repository.cycle = 2;

                // decide what to show
                AAction.displaySemi = true;
                AAction.displayStack = false;  // this is default so redundant

                // action used for namespaces, classes, and functions
                PushStack push = new PushStack(repo);
                Aggfound aggfound = new Aggfound(repo);
                Inheritancefound Ifound = new Inheritancefound(repo);
                Usingaction Ufound = new Usingaction(repo);
                CompositionAction compAct = new CompositionAction(repo);
                

                // capture namespace info
                DetectNamespace detectNS = new DetectNamespace();
                detectNS.add(push);
                parser.add(detectNS);

                // capture Inheritance relationship(will detect inheritancce as well as push class into the stack)
                DetectInheritance detectI = new DetectInheritance();
                detectI.add(Ifound);
                parser.add(detectI);

                // capture class info
                DetectClass detectCl = new DetectClass();
                detectCl.add(push);
                parser.add(detectCl);

                //handle aggregation within class definition
                DetectAggregation agg = new DetectAggregation();
                agg.add(aggfound);
                parser.add(agg);

                //Handle using using(should be placed before detecting function)
                DetectUsing detectU = new DetectUsing();
                detectU.add(Ufound);
                parser.add(detectU);
                
                //handle composition
                DetectComposition detectC = new DetectComposition();
                detectC.add(compAct);
                parser.add(detectC);

                // capture function info
                DetectFunction detectFN = new DetectFunction();
                detectFN.add(push);
                parser.add(detectFN);

                // handle entering anonymous scopes, e.g., if, while, etc.
                DetectAnonymousScope anon = new DetectAnonymousScope();
                anon.add(push);
                parser.add(anon);

                // handle leaving scopes
                DetectLeavingScope leave = new DetectLeavingScope();
                PopStack pop = new PopStack(repo);
                leave.add(pop);
                parser.add(leave);

                // parser configured
                return parser;
            }
        }
    }


