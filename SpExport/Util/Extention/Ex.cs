using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using SpExport.Core;

namespace SpExport.Util.Extention
{
    public class Serializer
    {
        #region Functions
        public static void SerializeToXML<T>(T t, String inFilename)
        {
            XmlSerializer serializer = new XmlSerializer(t.GetType());
            TextWriter textWriter = new StreamWriter(inFilename);
            serializer.Serialize(textWriter, t);
            textWriter.Close();
        }

        public static T DeserializeFromXML<T>(String inFilename)
        {
            XmlSerializer deserializer = new XmlSerializer(typeof(T));
            TextReader textReader = new StreamReader(inFilename);
            T retVal = (T)deserializer.Deserialize(textReader);
            textReader.Close();
            return retVal;
        }
        #endregion
    }
    public static class Extensions
    {
        public static string SerializeAnObject(this object AnObject)
        {
            XmlSerializer Xml_Serializer = new XmlSerializer(AnObject.GetType());
            StringWriter Writer = new StringWriter();      

            Xml_Serializer.Serialize(Writer, AnObject);
            return Writer.ToString();
        } 

  
        public static Object DeSerializeAnObject(string XmlOfAnObject, Type ObjectType)
        {       
            StringReader StrReader = new StringReader(XmlOfAnObject);
            XmlSerializer Xml_Serializer = new XmlSerializer(ObjectType);
            XmlTextReader XmlReader = new XmlTextReader(StrReader);
            try
            {
                Object AnObject = Xml_Serializer.Deserialize(XmlReader);
                return AnObject;
            }
            finally
            {
                XmlReader.Close();
                StrReader.Close();
            }
        }
        public static Dictionary<string, string> ToDictionary(this string keyValue)
        {
            return keyValue.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                          .Select(part => part.Split('='))
                          .ToDictionary(split => split[0], split => split[1]);
        }
    }
    public static class CoreExtensions
    {
        public static void Merge<T>(this ObservableCollection<T> source, IEnumerable<T> collection)
        {
            Merge<T>(source, collection, false);
        }

        public static void Merge<T>(this ObservableCollection<T> source, IEnumerable<T> collection, bool ignoreDuplicates)
        {
            if (collection != null)
            {
                foreach (T item in collection)
                {
                    bool addItem = true;

                    if (ignoreDuplicates)
                        addItem = !source.Contains(item);

                    if (addItem)
                        source.Add(item);
                }
            }
        }

        public static bool IsNavigable(this PropertyInfo property)
        {
            bool navigable = true;

            object[] attributes = property.GetCustomAttributes(typeof(NotNavigableAttribute), true);
            if (attributes.Length > 0)
                navigable = false;

            return navigable;
        }

        public static bool IsNavigable(this ObjectBase obj, string propertyName)
        {
            PropertyInfo propertyInfo = obj.GetType().GetProperty(propertyName);
            return propertyInfo.IsNavigable();
        }

        public static bool IsNavigable<T>(this ObjectBase obj, Expression<Func<T>> propertyExpression)
        {
            string propertyName = PropertySupport.ExtractPropertyName(propertyExpression);
            PropertyInfo propertyInfo = obj.GetType().GetProperty(propertyName);
            return propertyInfo.IsNavigable();
        }

        static Dictionary<string, bool> BrowsableProperties = new Dictionary<string, bool>();
        static Dictionary<string, PropertyInfo[]> BrowsablePropertyInfos = new Dictionary<string, PropertyInfo[]>();

        public static bool IsBrowsable(this object obj, PropertyInfo property)
        {
            string key = string.Format("{0}.{1}", obj.GetType(), property.Name);

            if (!BrowsableProperties.ContainsKey(key))
            {
                bool browsable = property.IsNavigable();
                BrowsableProperties.Add(key, browsable);
            }

            return BrowsableProperties[key];
        }

        public static PropertyInfo[] GetBrowsableProperties(this object obj)
        {
            string key = obj.GetType().ToString();

            if (!BrowsablePropertyInfos.ContainsKey(key))
            {
                List<PropertyInfo> propertyInfoList = new List<PropertyInfo>();
                PropertyInfo[] properties = obj.GetType().GetProperties();
                foreach (PropertyInfo property in properties)
                {
                    if ((property.PropertyType.IsSubclassOf(typeof(ObjectBase)) || property.PropertyType.GetInterface("IList") != null))
                    {
                        // only add to list of the property is NOT marked with [NotNavigable]
                        if (IsBrowsable(obj, property))
                            propertyInfoList.Add(property);
                    }
                }

                BrowsablePropertyInfos.Add(key, propertyInfoList.ToArray());
            }

            return BrowsablePropertyInfos[key];
        }
    }
}
