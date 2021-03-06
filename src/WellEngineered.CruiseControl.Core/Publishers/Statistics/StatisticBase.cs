﻿using System.Xml.XPath;

using WellEngineered.CruiseControl.PrivateBuild.NetReflector.Attributes;

namespace WellEngineered.CruiseControl.Core.Publishers.Statistics
{
    /// <summary>
    /// Provides the base functionality for statistics.
    /// </summary>
    /// <title>Statistics</title>
    public abstract class StatisticBase
    {
        /// <summary>
        /// The name of this statistic.
        /// </summary>
        protected string name;
        /// <summary>
        /// The XML XPath to locate the value of this statistic.
        /// </summary>
        protected string xpath;
        /// <summary>
        /// Should a graph be generated for this statistic?
        /// </summary>
        private bool generateGraph;
        /// <summary>
        /// Should this statistic be collected and published?
        /// </summary>
        private bool include = true;

        private StatisticsNamespaceMapping[] nameSpaces;


        /// <summary>
        /// Initializes a new instance of the <see cref="StatisticBase" /> class.	
        /// </summary>
        /// <remarks></remarks>
        protected StatisticBase()
        {
            this.NameSpaces = new StatisticsNamespaceMapping[0];
        }

        /// <summary>
        /// Create a statistic that extracts all items that match the specifed XML XPath.
        /// </summary>
        /// <param name="name">The name of the statistic.</param>
        /// <param name="xpath">The XML XPath to locate the values.</param>
        protected StatisticBase(string name, string xpath)
        {
            this.name = name;
            this.xpath = xpath;
            this.NameSpaces = new StatisticsNamespaceMapping[0];
        }

        /// <summary>
        /// The XML XPath to locate the value of this statistic.
        /// </summary>
        /// <default>n/a</default>
        /// <version>1.0</version>
        [ReflectorProperty("xpath")]
        public string Xpath
        {
            get { return this.xpath; }
            set { this.xpath = value; }
        }

        /// <summary>
        /// The name of the statistic.
        /// </summary>
        /// <default>n/a</default>
        /// <version>1.0</version>
        [ReflectorProperty("name")]
        public string Name
        {
            get { return this.name; }
            set { this.name = value; }
        }

        /// <summary>
        /// Should a graph be generated for this statistic?
        /// </summary>
        /// <default>false</default>
        /// <version>1.3</version>
        [ReflectorProperty("generateGraph", Required = false)]
        public bool GenerateGraph
        {
            get { return this.generateGraph; }
            set { this.generateGraph = value; }
        }

        /// <summary>
        /// Provides support for the use of xml namespaces.
        /// </summary>
        /// <default>none</default>
        /// <version>1.7</version>        
        [ReflectorProperty("namespaces", Required = false)]
        public StatisticsNamespaceMapping[] NameSpaces
        {
            get { return this.nameSpaces; }
            set { this.nameSpaces = value;}
        }


        /// <summary>
        /// Should this statistic be collected and published?
        /// </summary>
        /// <default>true</default>
        /// <version>1.3</version>
        [ReflectorProperty("include", Required = false)]
        public bool Include
        {
            get { return this.include; }
            set { this.include = value; }
        }

        /// <summary>
        /// Extract the value of the statistic from the specified XML data.
        /// </summary>
        /// <param name="nav">A navigator into an XML document containing the statistic data.</param>
        /// <returns>The statistic value.</returns>
        public StatisticResult Apply(XPathNavigator nav)
        {
            object value = this.Evaluate(nav);
            return new StatisticResult(this.name, value);
        }

        /// <summary>
        /// Extract the value of the statistic from the specified XML data.
        /// </summary>
        /// <param name="nav">A navigator into an XML document containing the statistic data.</param>
        /// <returns>The statistic value.</returns>
        protected virtual object Evaluate(XPathNavigator nav)
        {
            if (this.NameSpaces.Length == 0) return nav.Evaluate(this.xpath);

            System.Xml.XmlNamespaceManager nmsp = new System.Xml.XmlNamespaceManager(nav.NameTable);

            foreach (var s in this.NameSpaces)
            {
                if (s.Url == "default")
                {
                    nmsp.AddNamespace(s.Prefix, string.Empty);
                }
                else
                {
                    nmsp.AddNamespace(s.Prefix, s.Url);
                }
            }

            return nav.Evaluate(this.xpath,nmsp);
        }

        /// <summary>
        /// Equalses the specified obj.	
        /// </summary>
        /// <param name="obj">The obj.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public override bool Equals(object obj)
        {
            var o = (StatisticBase)obj;
            return this.name.Equals(o.Name);
        }

        /// <summary>
        /// Gets the hash code.	
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        public override int GetHashCode()
        {
            return this.name.GetHashCode();
        }
    }
}
