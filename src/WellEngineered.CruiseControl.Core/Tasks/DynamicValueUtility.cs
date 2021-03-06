﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

using WellEngineered.CruiseControl.PrivateBuild.NetReflector;
using WellEngineered.CruiseControl.PrivateBuild.NetReflector.Attributes;
using WellEngineered.CruiseControl.PrivateBuild.NetReflector.Serialisers;
using WellEngineered.CruiseControl.Remote;
using WellEngineered.CruiseControl.Remote.Parameters;

namespace WellEngineered.CruiseControl.Core.Tasks
{
	/// <summary>
    /// Utility class for setting dynamic values.
    /// </summary>
    public static class DynamicValueUtility
    {
        #region Private fields
        private static readonly Regex parameterRegex = new Regex(@"\$\[[^\]]*\]", RegexOptions.Compiled);
        private static readonly Regex paramPartRegex = new Regex(@"(?<!\\)\|", RegexOptions.Compiled);
        #endregion

        #region Public methods
        #region FindProperty()
        /// <summary>
        /// Attempts to find a property on an objec using reflection attributes.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        public static PropertyValue FindProperty(object value, string property)
        {
            PropertyValue actualProperty = null;
            object currentValue = value;
            MemberInfo currentProperty = null;
            int lastIndex = 0;

            if (value != null)
            {
                PropertyPart[] parts = SplitPropertyName(property);
                int position = 0;
                while ((position < parts.Length) && (currentValue != null))
                {
                    actualProperty = null;
                    if (currentProperty != null)
                    {
                        currentValue = GetValue(currentProperty, currentValue);
                        currentProperty = null;
                    }

                    var curValue = currentValue as IEnumerable;
                    if (curValue != null)
                    {
                        currentValue = FindTypedValue(curValue, parts[position].Name);
                    }
                    else
                    {
                        currentProperty = FindActualProperty(currentValue, parts[position].Name);
                        if (!string.IsNullOrEmpty(parts[position].KeyName))
                        {
                            IEnumerable values = GetValue(currentProperty, currentValue) as IEnumerable;
                            currentValue = FindKeyedValue(values, parts[position].KeyName, parts[position].KeyValue);
                            currentProperty = null;
                        }
                        else if (parts[position].Index >= 0)
                        {
                            // If the current item is expecting an indexed value then treat the source as an array
                            Array values = GetValue(currentProperty, currentValue) as Array;
                            if (values != null)
                            {
                                lastIndex = parts[position].Index;
                                if (lastIndex < values.GetLength(0))
                                {
                                    // Generate the property here, it will get wiped later if there are further parts
                                    actualProperty = new PropertyValue(currentValue, currentProperty, lastIndex);
                                    currentValue = values.GetValue(lastIndex);
                                    currentProperty = null;
                                }
                            }
                            currentProperty = null;
                        }
                    }

                    position++;
                }
                if (currentProperty != null)
                {
                    actualProperty = new PropertyValue(currentValue, currentProperty, lastIndex);
                }
            }

            return actualProperty;
        }
        #endregion

        #region FindActualProperty()
        /// <summary>
        /// Attempts to find a reflector property.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="reflectorProperty">The reflector property.</param>
        /// <returns></returns>
        public static MemberInfo FindActualProperty(object value, string reflectorProperty)
        {
            MemberInfo actualProperty = null;

            if (value != null)
            {
                // Iterate through all the properties
                List<MemberInfo> allProperties = new List<MemberInfo>(value.GetType().GetProperties());
                allProperties.AddRange(value.GetType().GetFields());
                foreach (MemberInfo property in allProperties)
                {
                    // Iterate through all the attributes
                    object[] attributes = property.GetCustomAttributes(true);
                    foreach (object attribute in attributes)
                    {
                        // Get the name of the property
                        string name = null;
                        var attrib = attribute as ReflectorPropertyAttribute;
                        if (attrib != null)
                        {
                            name = attrib.Name;
                        }

                        // Check to see whether the property has been found
                        if (name == reflectorProperty)
                        {
                            actualProperty = property;
                            break;
                        }
                    }

                    if (actualProperty != null) break;
                }
            }

            return actualProperty;
        }
        #endregion

        #region FindTypedValue()
        /// <summary>
        /// Finds a keyed value.
        /// </summary>
        /// <param name="values">The enumeration containing the values.</param>
        /// <param name="typeName"></param>
        /// <returns>The matching value, if found, null otherwise.</returns>
        public static object FindTypedValue(IEnumerable values, string typeName)
        {
            object actualValue = null;

            foreach (object value in values)
            {
                // Get the attributes
                object[] attributes = value.GetType().GetCustomAttributes(true);
                foreach (object attribute in attributes)
                {
                    var attrib = attribute as ReflectorTypeAttribute;
                    if (attrib != null)
                    {
                        string name = attrib.Name;
                        if (name == typeName)
                        {
                            actualValue = value;
                            break;
                        }
                    }
                }
            }

            return actualValue;
        }
        #endregion

        #region FindKeyedValue()
        /// <summary>
        /// Finds a keyed value.
        /// </summary>
        /// <param name="values">The enumeration containing the values.</param>
        /// <param name="keyName">The name of the key.</param>
        /// <param name="keyValue">The value of the key.</param>
        /// <returns>The matching value, if found, null otherwise.</returns>
        public static object FindKeyedValue(IEnumerable values, string keyName, string keyValue)
        {
            object actualValue = null;

            if (values != null)
            {
                foreach (object value in values)
                {
                    // First, attempt to find a property that matches
                    MemberInfo property = FindActualProperty(value, keyName);
                    if (property != null)
                    {
                        // Then see if the property has a value
                        object propertyValue = GetValue(property, value);
                        if (propertyValue != null)
                        {
                            // Finally, check if the values match
                            if (string.Equals(keyValue, propertyValue.ToString()))
                            {
                                actualValue = value;
                                break;
                            }
                        }
                    }
                }
            }

            return actualValue;
        }
        #endregion

        #region SplitPropertyName()
        /// <summary>
        /// Splits a property name into its component parts.
        /// </summary>
        /// <param name="propertyName">The property to split.</param>
        /// <returns>An array of component parts.</returns>
        public static PropertyPart[] SplitPropertyName(string propertyName)
        {
            // Split into the dot separated parts
            string[] parts = propertyName.Split('.');
            List<PropertyPart> results = new List<PropertyPart>();

            foreach (string part in parts)
            {
                PropertyPart newPart = new PropertyPart();

                // Check if each part has a key
                int keyIndex = part.IndexOf('[');
                if (keyIndex >= 0)
                {
                    // If so, find the different parts
                    newPart.Name = part.Substring(0, keyIndex);
                    string key = part.Substring(keyIndex + 1);
                    int equalsIndex = key.IndexOf('=');
                    if (equalsIndex >= 0)
                    {
                        newPart.KeyName = key.Substring(0, equalsIndex);
                        newPart.KeyValue = key.Substring(equalsIndex + 1);
                        newPart.KeyValue = newPart.KeyValue.Remove(newPart.KeyValue.Length - 1);
                    }
                    else
                    {
                        int index;
                        if (int.TryParse(key.Substring(0, key.Length - 1), out index))
                        {
                            newPart.Index = index;
                        }
                    }
                }
                else
                {
                    // Otherwise the whole is the name
                    newPart.Name = part;
                }
                results.Add(newPart);
            }

            return results.ToArray();
        }
        #endregion

        #region ConvertValue()
        /// <summary>
        /// Performs any conversion required by the original parameter definition.
        /// </summary>
        /// <param name="parameterName">The name of the parameter.</param>
        /// <param name="inputValue">The input value.</param>
        /// <param name="parameterDefinitions">The definitions.</param>
        /// <returns>The converted value.</returns>
        public static object ConvertValue(string parameterName, string inputValue, IEnumerable<ParameterBase> parameterDefinitions)
        {
            object actualValue = inputValue;
            if (parameterDefinitions != null)
            {
                foreach (var parameter in parameterDefinitions)
                {
                    if (parameter.Name == parameterName)
                    {
                        try
                        {
                            actualValue = parameter.Convert(inputValue ?? parameter.DefaultValue);
                        }
                        catch (InvalidCastException)
                        {
                            throw actualValue == null
                                      ? new CruiseControlException(
                                            "Unable to set dynamic value for " + parameterName +
                                            ", unable to find a valid default value")
                                      : new CruiseControlException(
                                            "Unable to set dynamic value for " + parameterName +
                                            ", unable to convert value");
                        }
                        break;
                    }
                }
            }
            return actualValue;
        }
        #endregion

        #region ConvertXmlToDynamicValues()
        private static void buildMembers(ref Dictionary<string, XmlMemberSerialiser> members, NetReflectorTypeTable typeTable, XmlNode node)
        {
            if (node == null)
                return;

            var nodeType = typeTable.ContainsType(node.Name) ? typeTable[node.Name] : null;
            if (nodeType != null)
            {
                foreach (XmlMemberSerialiser value in nodeType.MemberSerialisers)
                {
                    if (value != null)
                    {
                        if (!members.ContainsKey(value.Attribute.Name))
                            members.Add(value.Attribute.Name, value);
                    }
                }
            }

            foreach (XmlNode childNode in node.ChildNodes)
            {
                buildMembers(ref members, typeTable, childNode);
            }
        }

        private static void addParameters(ref List<XmlElement> parameters, XmlNode node)
        {
            var doc = node.OwnerDocument;
            if (parameters.Count > 0)
            {
                var parametersEl = node.SelectSingleNode("dynamicValues");
                if (parametersEl == null)
                {
                    parametersEl = doc.CreateElement("dynamicValues");
                    node.AppendChild(parametersEl);
                }

                foreach (var element in parameters)
                {
                    parametersEl.AppendChild(element);
                }

                parameters.Clear();
            }
        }

        private static XmlNode getParentNode(XmlNode node)
        {
            var nodeAsAttribute = node as XmlAttribute;
            if (nodeAsAttribute != null)
            {
                return nodeAsAttribute.OwnerElement;
            }
            else
            {
                return node.ParentNode;
            }
        }

        /// <summary>
        /// Check for and convert inline XML dynamic value notation into <see cref="IDynamicValue"/> definitions.
        /// </summary>
        /// <param name="typeTable">The type table.</param>
        /// <param name="inputNode">The node to process.</param>
        /// <param name="exclusions">Any elements to exclude.</param>
        /// <returns></returns>
        public static XmlNode ConvertXmlToDynamicValues(NetReflectorTypeTable typeTable, XmlNode inputNode, params string[] exclusions)
        {
            var resultNode = inputNode;
            var doc = inputNode.OwnerDocument;
            var parameters = new List<XmlElement>();

            // Initialise the values from the reflection
            var inputMembers = new Dictionary<string, XmlMemberSerialiser>();
            buildMembers(ref inputMembers, typeTable, inputNode);

            var nodes = inputNode.SelectNodes("descendant::text()|descendant-or-self::*[@*]/@*");
            foreach (XmlNode nodeWithParam in nodes)
            {
                var text = nodeWithParam.Value;
                var isExcluded = CheckForExclusion(nodeWithParam, exclusions);
                if (!isExcluded && parameterRegex.Match(text).Success)
                {
                    // Generate the format string
                    var parametersEl = doc.CreateElement("parameters");
                    var index = 0;
                    var lastReplacement = string.Empty;
                    var format = parameterRegex.Replace(text, (match) =>
                    {
                        // Split into it's component parts
                        var parts = paramPartRegex.Split(match.Value.Substring(2, match.Value.Length - 3));

                        // Generate the value element
                        var dynamicValueEl = doc.CreateElement("namedValue");
                        dynamicValueEl.SetAttribute("name", parts[0].Replace("\\|", "|"));
                        if (parts.Length > 1)
                        {
                            dynamicValueEl.SetAttribute("value", parts[1].Replace("\\|", "|"));
                        }

                        parametersEl.AppendChild(dynamicValueEl);

                        // Generate the replacement
                        lastReplacement = string.Format(CultureInfo.CurrentCulture, "{{{0}{1}}}",
                            index++,
                            parts.Length > 2 ? ":" + parts[2].Replace("\\|", "|") : string.Empty);
                        return lastReplacement;
                    });

                    // Generate the dynamic value element
                    var replacementValue = string.Empty;
                    XmlElement replacementEl;
                    if (lastReplacement != format)
                    {
                        replacementEl = doc.CreateElement("replacementValue");
                        AddElement(replacementEl, "format", format);
                        replacementEl.AppendChild(parametersEl);
                    }
                    else
                    {
                        replacementEl = doc.CreateElement("directValue");
                        AddElement(replacementEl, "parameter", parametersEl.SelectSingleNode("namedValue/@name").InnerText);
                        var innerValue = parametersEl.SelectSingleNode("namedValue/@value");
                        if (innerValue != null)
                        {
                            replacementValue = innerValue.InnerText;
                            AddElement(replacementEl, "default", replacementValue);
                        }
                    }
                    parameters.Add(replacementEl);

                    // Generate the path
                    var propertyName = new StringBuilder();
                    var currentNode = nodeWithParam is XmlAttribute ? nodeWithParam : nodeWithParam.ParentNode;
                    var previousNode = currentNode;
                    var lastName = string.Empty;
                    bool hasDynamicValues = false;
                    while ((currentNode != null) && (currentNode.NodeType != XmlNodeType.Document))
                    {
                        var nodeName = currentNode.Name;

                        var currentNodeType = typeTable.ContainsType(nodeName) ? typeTable[nodeName] : null;
                        if (currentNodeType != null)
                        {
                            hasDynamicValues = currentNodeType.Type.
                                GetInterface(typeof(IWithDynamicValuesItem).Name) != null;
                            if (hasDynamicValues)
                                break;
                        }
                        
                        // we don't want to take into account the input node, but we need to know if it has
                        // dynamic values, hence the break here and not in the while loop
                        if (currentNode == inputNode)
                            break;

                        // Check if we are dealing with an array
                        if (inputMembers.ContainsKey(nodeName) && inputMembers[nodeName].ReflectorMember.MemberType.IsArray)
                        {
                            // Do some convoluted processing to handle array items
                            propertyName.Remove(0, lastName.Length + 1); // Remove the previous name, since this is now an index position
                            var position = 0;
                            var indexNode = previousNode;

                            // Find the index of the node
                            while (indexNode.PreviousSibling != null)
                            {
                                if (indexNode.NodeType != XmlNodeType.Comment)
                                    position++;
                                indexNode = indexNode.PreviousSibling;
                            }

                            // Add the node as an indexed node
                            propertyName.Insert(0, "." + nodeName + "[" + position.ToString(CultureInfo.CurrentCulture) + "]");
                        }
                        else
                        {
                            // Just add the node name
                            propertyName.Insert(0, "." + nodeName);
                        }

                        // Move to the parent
                        lastName = nodeName;
                        previousNode = currentNode;
                        currentNode = getParentNode(currentNode);
                    }
                    propertyName.Remove(0, 1);
                    AddElement(replacementEl, "property", propertyName.ToString());

                    // Set a replacement value
                    nodeWithParam.Value = replacementValue;

                    // add the parameters already there if the current node has dynamic values
                    if (hasDynamicValues)
                        addParameters(ref parameters, currentNode);
                }
            }

            // Add the remaining parameters to the root element
            addParameters(ref parameters, inputNode);

            return resultNode;
        }
        #endregion
        #endregion

        #region Private methods
        #region GetValue()
        private static object GetValue(MemberInfo member, object source)
        {
            object value = null;

            var pi = member as PropertyInfo;
            if (pi != null)
            {
                value = pi.GetValue(source, new object[0]);
            }
            else
            {
                value = (member as FieldInfo).GetValue(source);
            }

            return value;
        }
        #endregion

        #region AddElement()
        /// <summary>
        /// Adds an XML element.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private static void AddElement(XmlElement parent, string name, string value)
        {
            var element = parent.OwnerDocument.CreateElement(name);
            element.InnerText = value;
            parent.AppendChild(element);
        }
        #endregion

        #region CheckForExclusion()
        /// <summary>
        /// Check to see if the node should be excluded.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="exclusions"></param>
        /// <returns></returns>
        private static bool CheckForExclusion(XmlNode node, string[] exclusions)
        {
            var isExcluded = false;
            if ((exclusions != null) && (exclusions.Length > 0))
            {
                foreach (var exclusion in exclusions)
                {
                    var ancestor = node.SelectSingleNode("ancestor::" + exclusion + "[1]");
                    if (ancestor != null)
                    {
                        isExcluded = true;
                        break;
                    }
                }
            }
            return isExcluded;
        }
        #endregion
        #endregion

        #region Private methods
        #region PropertyValue
        /// <summary>
        /// Defines a property value.
        /// </summary>
        public class PropertyValue
        {
            private readonly object mySource;
            private readonly MemberInfo myProperty;
            private readonly int myArrayIndex;

            internal PropertyValue(object source, MemberInfo property, int arrayIndex)
            {
                this.mySource = source;
                this.myProperty = property;
                this.myArrayIndex = arrayIndex;
            }

            /// <summary>
            /// The source of the property.
            /// </summary>
            public object Source
            {
                get { return this.mySource; }
            }

            /// <summary>
            /// The property.
            /// </summary>
            public MemberInfo Property
            {
                get { return this.myProperty; }
            }

            /// <summary>
            /// The current value of the property.
            /// </summary>
            public object Value
            {
                get
                {
                    return this.myProperty is PropertyInfo
                               ? (this.myProperty as PropertyInfo).GetValue(this.mySource, new object[0])
                               : (this.myProperty as FieldInfo).GetValue(this.mySource);
                }
            }

            /// <summary>
            /// Changes the value of the property.
            /// </summary>
            /// <param name="value">The new value to set.</param>
            public void ChangeProperty(object value)
            {
                if (this.myProperty is PropertyInfo)
                {
                    this.ChangePropertyValue(value);
                }
                else
                {
                    this.ChangeFieldValue(value);
                }
            }

            /// <summary>
            /// Change the value when the source is a property.
            /// </summary>
            /// <param name="value"></param>
            private void ChangePropertyValue(object value)
            {
                var actualValue = value;
                var index = new object[0];
                var property = (this.myProperty as PropertyInfo);

                // If it is an array then reset the index and use the array item type instead
                if (property.PropertyType.IsArray)
                {
                    if (property.PropertyType.GetElementType() != value.GetType())
                    {
                        actualValue = Convert.ChangeType(value, property.PropertyType.GetElementType(), CultureInfo.CurrentCulture);
                    }

                    var array = property.GetValue(this.mySource, index) as Array;
                    array.SetValue(actualValue, this.myArrayIndex);
                }
                else
                {
                    if ((value != null) && (property.PropertyType != value.GetType()))
                    {
                        // See if there are any converters on either the property or the underlying type
                        var converters = property.GetCustomAttributes(typeof(TypeConverterAttribute), true);
                        if (converters.Length == 0)
                        {
                            converters = property.PropertyType.GetCustomAttributes(typeof(TypeConverterAttribute), true);
                        }

                        // Convert the value
                        var converted = false;
                        if (converters.Length > 0)
                        {
                            // Check the converters
                            var valueType = value.GetType();
                            foreach (TypeConverterAttribute converterAttrib in converters)
                            {
                                var converter = Activator.CreateInstance(Type.GetType(converterAttrib.ConverterTypeName)) as TypeConverter;
                                if (converter.CanConvertFrom(valueType))
                                {
                                    actualValue = converter.ConvertFrom(value);
                                    converted = true;
                                    break;
                                }
                            }
                        }

                        // Finally, if all else fails, use the standard convert
                        if (!converted)
                        {
                            actualValue = Convert.ChangeType(value, property.PropertyType, CultureInfo.CurrentCulture);
                        }
                    }

                    property.SetValue(this.mySource, actualValue, index);
                }
            }

            /// <summary>
            /// Change the value when the source is a field.
            /// </summary>
            /// <param name="value"></param>
            /// <returns></returns>
            private void ChangeFieldValue(object value)
            {
                FieldInfo property = (this.myProperty as FieldInfo);
                object actualValue = value;

                // If it is an array then just set the array value instead of changing the entire value
                if (property.FieldType.IsArray)
                {
                    if (property.FieldType.GetElementType() != value.GetType())
                    {
                        actualValue = Convert.ChangeType(value, property.FieldType, CultureInfo.CurrentCulture);
                    }
                    Array array = property.GetValue(this.mySource) as Array;
                    array.SetValue(actualValue, this.myArrayIndex);
                }
                else
                {
                    if (property.FieldType != value.GetType())
                    {
                        actualValue = Convert.ChangeType(value, property.FieldType, CultureInfo.CurrentCulture);
                    }
                    property.SetValue(this.mySource, actualValue);
                }
            }
        }
        #endregion

        #region PropertyPart
        /// <summary>
        /// Defines a part of a property.
        /// </summary>
        public class PropertyPart
        {
            /// <summary>
            /// The name of the property.
            /// </summary>
            public string Name;

            /// <summary>
            /// The name of the key
            /// </summary>
            public string KeyName;

            /// <summary>
            /// The value of the key
            /// </summary>
            public string KeyValue;

            /// <summary>
            /// The index of the item in the array.
            /// </summary>
            public int Index = -1;
        }
        #endregion
        #endregion
    }
}
