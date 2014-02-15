using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NI.Ioc.Examples.ConsoleApp
{
    public class TestClass
    {
        private int _SomeIntProperty;
        private double _SomeDoubleProperty;

        public int SomeIntProperty
        {
            get { return _SomeIntProperty; }
            set { _SomeIntProperty = value; }
        }

        public double SomeDoubleProperty
        {
            get { return _SomeDoubleProperty; }
            set { _SomeDoubleProperty = value; }
        }

        public string SomeStringProperty { get; set; }
        public IList SomeListProperty { get; set; }
        public IDictionary SomeMapProperty { get; set; }

        public TestClass(int intProp, double doubleProp, string id)
        {
            _SomeIntProperty = intProp;
            _SomeDoubleProperty = doubleProp;
            Console.WriteLine(
                    String.Format(
                            "Created TestClass object with id {0}", id
                        )
                );
        }

        public override string ToString()
        {
            return String.Format(
                    @"{0} SomeIntProperty: {1} 
                    {0} SomeDoubleProperty: {2} 
                    {0} SomeStringProperty: {3} 
                    {0} SomeListProperty: {4},
                    {0} SomeMapProperty: {5}",
                    Environment.NewLine,
                    SomeIntProperty,
                    SomeDoubleProperty,
                    SomeStringProperty,
                    StringifyList(SomeListProperty),
                    StringifyMap(SomeMapProperty)
                );
        }

        private string StringifyList(IList list)
        {
            if (list == null)
            {
                return "null";
            }
            StringBuilder sb = new StringBuilder();
            foreach (object obj in list)
            {
                sb.Append(obj.ToString());
                sb.Append(", ");
            }
            string res = sb.ToString();
            return res.Substring(0, res.Length - 2);
        }

        private string StringifyMap(IDictionary map)
        {
            if (map == null)
            {
                return "null";
            }
            StringBuilder sb = new StringBuilder();
            foreach (DictionaryEntry e in map)
            {
                sb.AppendFormat("{0}:{1}", e.Key, e.Value);
                sb.Append(", ");
            }
            string res = sb.ToString();
            return res.Substring(0, res.Length - 2);
        }
    }
}
