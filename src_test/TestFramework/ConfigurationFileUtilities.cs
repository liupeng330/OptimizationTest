using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Xml;
using System.Diagnostics;
using System.Globalization;
using System.Xml.Linq;

namespace AdSage.Concert.Test.Framework
{
    /// <summary>
    /// Set of utility methods to deal with configuration files.
    /// </summary>
    public static class ConfigurationFileUtilities
    {
        /// <summary>
        /// Update the value for a given key in the config file
        /// </summary>
        /// <param name="fileName">Full path to the config file</param>
        /// <param name="xpath">Xml path to the node that need to be updated</param>
        /// <param name="value">The value to be set</param>
        public static void UpdateConfigurationValue(string fileName, string xpath, string value)
        {
            if (!File.Exists(fileName))
            {
                throw new FileNotFoundException("File Not Found", fileName);
            }

            XmlDocument doc = new XmlDocument();

            doc.Load(fileName);

            XmlNodeList nodeList = doc.SelectNodes(xpath);
            if (nodeList.Count == 0)
            {
                throw new ArgumentException(xpath + " not found in " + fileName);
            }

            foreach (XmlNode node in nodeList)
            {
                if (node != null)
                {
                    if (node.NodeType == XmlNodeType.Attribute)
                    {
                        node.Value = value;
                    }
                    else
                    {
                        node.InnerText = value;
                    }
                }
            }

            doc.Save(fileName);
        }

        /// <summary>
        /// Update the value for a given key in the config file for a give list of path/value pairs
        /// </summary>
        /// <param name="fileName">Full path to the config file</param>
        /// <param name="replacements">Path/value pairs to replace in the config file</param>
        public static void UpdateConfigurationValues(string fileName, Dictionary<string, string> replacements)
        {
            if (!File.Exists(fileName))
            {
                throw new FileNotFoundException("File Not Found", fileName);
            }

            XmlDocument doc = new XmlDocument();

            doc.Load(fileName);

            XmlNodeList nodeList;
            XmlNode node;
            foreach (var kvp in replacements)
            {
                string xpath = kvp.Key;
                string value = kvp.Value;

                nodeList = doc.SelectNodes(xpath);

                //We require the xpath to be unique, which means, we should find one and only one node
                if (nodeList.Count == 0)
                {
                    throw new ArgumentException(xpath + " not found in " + fileName);
                }
                else if (nodeList.Count > 1)
                {
                    throw new ArgumentException(xpath + " return multiple results in " + fileName);
                }

                node = nodeList[0];

                if (node.NodeType == XmlNodeType.Attribute)
                {
                    node.Value = value;
                }
                else
                {
                    node.InnerText = value;
                }
            }

            doc.Save(fileName);
        }

        /// <summary>
        /// Get the value for a given key in the config file
        /// </summary>
        /// <param name="fileName">Full path to the config file</param>
        /// <param name="xpath">Xml path to the node that want to be got</param>
        /// <returns>The value you want to get</returns>
        public static string GetConfigurationValue(string fileName, string xpath)
        {
            string[] valueList = GetConfigurationValues(fileName, xpath);
            if (valueList.Length < 1)
            {
                throw new ArgumentOutOfRangeException(xpath, "There is no value");
            }
            return valueList[0];
        }

        /// <summary>
        /// Get the value for a given key in the config file from the appSettings section
        /// </summary>
        /// <param name="fileName">Full path to the config file</param>
        /// <param name="keyName">key name in the <add key="keyName" value="value"/> pair </param>
        /// <returns>The value you want to get</returns>
        public static string GetAppConfigurationValue(string fileName, string keyName)
        {
            string xpath = String.Format(CultureInfo.InvariantCulture, "/configuration/appSettings/add[@key=\"{0}\"]/@value", keyName);

            return GetConfigurationValue(fileName, xpath);
        }
        
        /// <summary>
        /// Get the value for a given key in the config file
        /// </summary>
        /// <param name="fileName">Full path to the config file</param>
        /// <param name="xpath">Xml path to the node that want to be got</param>
        /// <returns>The value you want to get</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
        public static T? GetConfigurationValue<T>(string fileName, string xpath) where T: struct
        {
            string[] valueList = GetConfigurationValues(fileName, xpath);
            if (valueList.Length == 0 || String.IsNullOrEmpty(valueList[0]))
            {
                return null;
            }
            return (T)Convert.ChangeType(valueList[0], typeof(T), CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Get the value for a given key in the config file
        /// </summary>
        /// <param name="fileName">Full path to the config file</param>
        /// <param name="xpath">Xml path to the node that want to be got</param>
        /// <param name="defaultValue">Value to return if the node is not found</param>
        /// <returns>The value you want to get</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
        public static T GetConfigurationValue<T>(string fileName, string xpath, T defaultValue)
        {
            string[] valueList = GetConfigurationValues(fileName, xpath);
            if (valueList.Length == 0 || String.IsNullOrEmpty(valueList[0]))
            {
                return defaultValue;
            }
            return (T)Convert.ChangeType(valueList[0], typeof(T), CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Get the value list for a given key in the config file
        /// </summary>
        /// <param name="fileName">Full path to the config file</param>
        /// <param name="xpath">Xml path to the node that want to be got</param>
        /// <returns>The value list you want to get</returns>
        public static string[] GetConfigurationValues(string fileName, string xpath)
        {
            if (!File.Exists(fileName))
            {
                throw new FileNotFoundException("File Not Found: " + fileName, fileName);
            }

            XmlDocument doc = new XmlDocument();
            doc.Load(fileName);
            XmlNodeList nodeList = doc.SelectNodes(xpath);

            return
                (from XmlNode node in nodeList
                 select (node.NodeType == XmlNodeType.Attribute) ? node.Value : node.InnerText
                 ).ToArray();
        }

        /// <summary>
        /// Update the config files with newly added items, or update the old items with new value.
        /// </summary>
        /// <param name="fileName">Full path to the config file</param>
        /// <param name="key">Add the config item if key doesn't exist.</param>
        /// <param name="value">The value to be set</param>
        public static void AddConfigurationItem(string fileName, string key, string value)
        {
            if (!File.Exists(fileName))
            {
                throw new FileNotFoundException("File Not Found: " + fileName, fileName);
            }

            ExeConfigurationFileMap configurationMap = new ExeConfigurationFileMap { ExeConfigFilename = fileName };
            Configuration config = ConfigurationManager.OpenMappedExeConfiguration(configurationMap, ConfigurationUserLevel.None);
            KeyValueConfigurationCollection settings = config.AppSettings.Settings;
            if (settings[key] != null)
            {
                settings.Remove(key);
            }
            settings.Add(key, value);
            config.Save();
        }

        public static void RemoveConfigurationItem(string fileName, string key)
        {
            if (!File.Exists(fileName))
            {
                throw new FileNotFoundException("File Not Found: " + fileName, fileName);
            }
            XElement removeElement = XElement.Load(fileName);
            removeElement.Elements(key).Remove();
            removeElement.Save(fileName);
        }
    }
}
