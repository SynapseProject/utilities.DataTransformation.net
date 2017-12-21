using System;
using System.Dynamic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.XPath;
using System.Xml.Xsl;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

using YamlDotNet.Serialization;

namespace zephyr.DataTransformation
{
    public class XmlHelpers
    {
        public static string Transform(XPathDocument xpathDocument, string xslt,
            bool omitXmlDeclaration = true, ConformanceLevel conformanceLevel = ConformanceLevel.Auto,
            Encoding encoding = null, bool formatOutputIndented = true)
        {
            if( encoding == null )
                encoding = Encoding.UTF8;

            // process the xslt
            XmlTextReader xmlTextReaderXslt = new XmlTextReader( new StringReader( xslt ) );
            XslCompiledTransform xslCompiledTransform = new XslCompiledTransform();
            xslCompiledTransform.Load( xmlTextReaderXslt );

            // handle the output stream
            XmlWriterSettings settings = new XmlWriterSettings()
            {
                OmitXmlDeclaration = omitXmlDeclaration,
                ConformanceLevel = conformanceLevel,
                Encoding = encoding,
                CloseOutput = true,
                Indent = formatOutputIndented
            };

            // setup the output buffer
            StringBuilder buf = new StringBuilder();
            XmlWriter writer = XmlWriter.Create( buf, settings );

            // do the needful
            xslCompiledTransform.Transform( xpathDocument, null, writer );

            return buf.ToString();
        }

        public static XmlDocument Transform(XmlDocument xml, string xslt,
            bool omitXmlDeclaration = true, ConformanceLevel conformanceLevel = ConformanceLevel.Auto,
            Encoding encoding = null, bool formatOutputIndented = true)
        {
            if( string.IsNullOrWhiteSpace( xslt ) || xml == null )
                return xml;

            // process the xml
            XPathDocument xpathDocument = new XPathDocument( new XmlNodeReader( xml ) );

            // transform
            string buf = Transform( xpathDocument, xslt, omitXmlDeclaration, conformanceLevel, encoding, formatOutputIndented );

            // return doc
            XmlDocument doc = new XmlDocument();
            doc.LoadXml( buf );
            return doc;
        }

        public static string Transform(string xml, string xslt,
            bool omitXmlDeclaration = true, ConformanceLevel conformanceLevel = ConformanceLevel.Auto,
            Encoding encoding = null, bool formatOutputIndented = true)
        {
            if( string.IsNullOrWhiteSpace( xslt ) || string.IsNullOrWhiteSpace( xml ) )
                return xml;

            // Process the XML
            XmlTextReader xmlTextReader = new XmlTextReader( new StringReader( xml ) );
            XPathDocument xpathDocument = new XPathDocument( xmlTextReader );

            // transform
            string buf = Transform( xpathDocument, xslt, omitXmlDeclaration, conformanceLevel, encoding, formatOutputIndented );

            // return string
            return buf;
        }

        public static bool IsValidXml(string xml)
        {
            bool isValid = true;
            try
            {
                if( !string.IsNullOrEmpty( xml ) )
                {
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.LoadXml( xml );
                    isValid = true;
                }
            }
            catch( Exception )
            {
                isValid = false;
            }
            return isValid;
        }

        public static string ConvertToFormat(string xml, FormatType targetFormatType)
        {
            string serializedData = "";

            switch( targetFormatType )
            {
                case FormatType.Json:
                {
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml( xml );
                    if( doc.FirstChild.NodeType == XmlNodeType.XmlDeclaration )
                        doc.RemoveChild( doc.FirstChild );
                    serializedData = JsonConvert.SerializeXmlNode( doc );

                    break;
                }
                case FormatType.Yaml:
                {
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml( xml );
                    if( doc.FirstChild.NodeType == XmlNodeType.XmlDeclaration )
                        doc.RemoveChild( doc.FirstChild );
                    serializedData = JsonConvert.SerializeXmlNode( doc );

                    //convert the string JSON data into an object so yaml.net can serialize it to YAML nicely
                    ExpandoObjectConverter expConverter = new ExpandoObjectConverter();
                    dynamic deserializedObject = JsonConvert.DeserializeObject<ExpandoObject>( serializedData, expConverter );

                    Serializer serializer = new Serializer();
                    serializedData = serializer.Serialize( deserializedObject );

                    break;
                }
                case FormatType.Xml:
                case FormatType.None:
                {
                    serializedData = xml;
                    break;
                }
            }

            return serializedData;
        }

        public static string Serialize<T>(object data, bool omitXmlDeclaration = true, ConformanceLevel conformanceLevel = ConformanceLevel.Auto,
            Encoding encoding = null, bool formatOutputIndented = true, bool omitXmlNamespace = true)
        {
            if( string.IsNullOrWhiteSpace( data?.ToString() ) )
                return null;

            if( encoding == null )
                encoding = Encoding.UTF8;

            // handle the output stream
            XmlWriterSettings settings = new XmlWriterSettings()
            {
                OmitXmlDeclaration = omitXmlDeclaration,
                ConformanceLevel = conformanceLevel,
                Encoding = encoding,
                CloseOutput = true,
                Indent = formatOutputIndented
            };

            XmlSerializer s = new XmlSerializer( typeof( T ) );
            StringBuilder buf = new StringBuilder();
            XmlWriter w = XmlWriter.Create( buf, settings );
            if( data is XmlDocument )
            {
                ((XmlDocument)data).Save( w );
            }
            else
            {
                if( omitXmlNamespace )
                {
                    XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                    ns.Add( "", "" );
                    s.Serialize( w, data, ns );
                }
                else
                {
                    s.Serialize( w, data );
                }
            }
            w.Close();

            return buf.ToString();
        }
    }
}