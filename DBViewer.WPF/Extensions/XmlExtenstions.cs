using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace DBViewer.WPF
{
    public static class XmlExtenstions
    {
        #region Properties

        public enum XmlElementType : int
        {
            Unknown = 0,
            XmlCharacterData = 1,
            XmlCDATASection = 2,
            XmlComment = 3,
            XmlElement = 4,
            XmlEntity = 5,
            XmlDeclaration = 6,
            XmlNotation = 7
        }

        #endregion

        #region Retrieval

        public static List<XmlElement> AllElements(this XmlDocument document, string strOnlyWithTag, bool boolRecurse)
        {
            // Validation
            if (document == null || document.DocumentElement == null) { return null; }

            // Get Child Elements
            List<XmlElement> listElements = document.DocumentElement.ChildElements(strOnlyWithTag, boolRecurse);

            return listElements;
        }

        public static List<XmlElement> ChildElements(this XmlElement xmlParent, string strOnlyWithTag = "", bool boolRecurse = false)
        {
            List<XmlElement> listElements = new List<XmlElement>();

            // Loop Child Nodes
            foreach (System.Xml.XmlNode elementChild in xmlParent.ChildNodes)
            {
                // Check Element Name
                if (elementChild.Name == strOnlyWithTag || strOnlyWithTag == "")
                {
                    XmlElement childElement = (XmlElement)(elementChild.Clone());
                    listElements.Add(childElement);
                }
                else if (elementChild.ChildNodes.Count > 0 && boolRecurse == true)
                {
                    XmlElement xmlE = (XmlElement)elementChild;
                    List<XmlElement> listChildElements = ChildElements(xmlE, strOnlyWithTag, true);

                    listElements.AddRange(listChildElements.ToArray());
                }
            }

            return listElements;
        }

        public static List<XmlElement> ChildElements(this XmlElement xmlParent, XmlElementType xmlElementType = XmlElementType.XmlElement, string strOnlyWithTag = "", bool boolRecurse = false)
        {
            // Get Child Elements
            List<XmlElement> listElements = xmlParent.ChildElements(strOnlyWithTag, boolRecurse);

            listElements = listElements.Where(element => element.GetType() == GetElementType(xmlElementType)).ToList();

            return listElements;
        }

        #endregion

        #region Serialization / Deserialization

        public static string Serialize<T>(this T objSerialize, string strPrefix = "", string strNameSpace = "")
        {
            string strXml = "";

            try
            {
                // Create Empty Xml Namespace
                System.Xml.Serialization.XmlSerializerNamespaces xmlNamespace = new System.Xml.Serialization.XmlSerializerNamespaces();
                xmlNamespace.Add(strPrefix, strNameSpace);

                // Instantiate Serializer
                System.Xml.Serialization.XmlSerializer xmlSerializer = new System.Xml.Serialization.XmlSerializer(typeof(T));

                // Intstantiate XmlWriter Settings
                XmlWriterSettings xmlSettings = new XmlWriterSettings();
                xmlSettings.OmitXmlDeclaration = false;
                xmlSettings.Encoding = new UTF8Encoding(false);
                xmlSettings.Indent = true;

                // Serialize Object
                System.IO.MemoryStream stream = new System.IO.MemoryStream();
                XmlWriter writer = XmlWriter.Create(stream, xmlSettings);
                xmlSerializer.Serialize(writer, objSerialize, xmlNamespace);

                // Get Output Xml
                strXml = Encoding.UTF8.GetString(stream.ToArray());
            }
            catch (Exception ex)
            {
                Console.Write(ex.ToString());
                return "";
            }

            return strXml;
        }

        public static T Deserialize<T>(string strXml)
        {
            try
            {
                // Create Serializer
                XmlSerializer serializer = new XmlSerializer(typeof(T));

                // Create Stream
                byte[] byteArray = System.Text.Encoding.ASCII.GetBytes(strXml);
                Stream stream = new MemoryStream(byteArray);

                // Create Stream
                XmlReader reader = XmlReader.Create(stream);

                // Deserialize Object
                T objValue = (T)serializer.Deserialize(reader);

                // Close Stream
                stream.Close();

                return objValue;
            }
            catch (Exception ex)
            {
                Console.Write(ex.ToString());
                return default(T);
            }
        }

        #endregion

        #region Type

        private static Type GetElementType(XmlElementType xmlElementType)
        {
            switch (xmlElementType)
            {
                case XmlElementType.XmlCDATASection:
                    return typeof(XmlCDataSection);
                case XmlElementType.XmlCharacterData:
                    return typeof(XmlCharacterData);
                case XmlElementType.XmlComment:
                    return typeof(XmlComment);
                case XmlElementType.XmlDeclaration:
                    return typeof(XmlDeclaration);
                case XmlElementType.XmlElement:
                    return typeof(XmlElement);
                case XmlElementType.XmlEntity:
                    return typeof(XmlEntity);
                case XmlElementType.XmlNotation:
                    return typeof(XmlNotation);
            }

            return null;
        }

        #endregion
    }
}
