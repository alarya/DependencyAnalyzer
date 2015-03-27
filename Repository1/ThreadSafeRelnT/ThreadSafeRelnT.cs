///////////////////////////////////////////////////////////////////////
// MT2Q1a.cs - Relationships Table                                   //
//                                                                   //
// Jim Fawcett, CSE681 - Software Modeling and Analysis, Fall 2014   //
///////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace DependencyAnalyzer
{
    public class RelInfo
    {
        public string instance { get; set; }
        public string relationship { get; set; }
    }
    public class RelationshipsTable
    {
        Dictionary<string, List<RelInfo>> relationships = new Dictionary<string, List<RelInfo>>();
        object locker_ = new object();
        T lockit<T>(Func<T> f)
        {
            lock (locker_) { return f.Invoke(); }
        }

        public void add(string src, RelInfo tgtRif)
        {
            lockit<bool>(() =>
            {
                if (relationships.Keys.Contains(src))
                    relationships[src].Add(tgtRif);
                else
                {
                    List<RelInfo> list = new List<RelInfo>();
                    list.Add(tgtRif);
                    relationships[src] = list;
                }
                return true;  // not used
            });
        }
        public bool remove(string src)
        {
            return lockit<bool>(() => relationships.Remove(src));
        }
        public Dictionary<string, List<RelInfo>> getRelationships()
        {
            return relationships;
        }
        public void setRelationships(Dictionary<string, List<RelInfo>> RT)
        {
            relationships = RT;
        }
        public static void display(RelationshipsTable table)
        {
            foreach (string key in table.getRelationships().Keys)
            {
                Console.Write("\n  {0}", key);
                foreach (RelInfo rInfo in table.getRelationships()[key])
                {
                    Console.Write("\n    {0} with {1}", rInfo.relationship, rInfo.instance);
                }
            }
        }
    }

//-------------------------test stub ---------------------------------------------------------------------------------//
#if(TEST_RELN)
    class Test
    {
        static void Main(string[] args)
        {
            Console.WriteLine("\nRelationshipsTable");
            RelationshipsTable table = new RelationshipsTable();

            string src = "RelationshipsTable";
            RelInfo ri = new RelInfo();
            ri.instance = "relationships";
            ri.relationship = "Composition";
            table.add(src, ri);

            ri = new RelInfo();
            ri.instance = "locker_";
            ri.relationship = "aggregation";
            table.add(src, ri);

            src = "RelInfo";
            ri = new RelInfo();
            ri.instance = "instance";
            ri.relationship = "aggregation";
            table.add(src, ri);

            ri = new RelInfo();
            ri.instance = "relationship";
            ri.relationship = "aggregation";
            table.add(src, ri);

            foreach (string key in table.getRelationships().Keys)
            {
                Console.Write("\n  {0}", key);
                foreach (RelInfo rInfo in table.getRelationships()[key])
                {
                    Console.Write("\n    {0} with {1}", rInfo.relationship, rInfo.instance);
                }
            }
            Console.Write("\n\n");
            Console.ReadLine();
        }
    }
#endif 
}