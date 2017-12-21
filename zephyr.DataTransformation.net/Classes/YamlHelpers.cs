using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

using Newtonsoft.Json;

using YamlDotNet.Serialization;

namespace zephyr.DataTransformation
{
    public class YamlHelpers
    {
        public string Transform(string yaml, string xslt)
        {
            if( string.IsNullOrWhiteSpace( xslt ) )
                return yaml;

            string yamlAsXml = ConvertToFormat( yaml, FormatType.Xml );
            yamlAsXml = XmlHelpers.Transform( yamlAsXml, xslt );
            return XmlHelpers.ConvertToFormat( yamlAsXml, FormatType.Yaml );
        }

        public static string ConvertToFormat(string yaml, FormatType targetFormatType)
        {
            string serializedData = "";

            switch( targetFormatType )
            {
                case FormatType.Json:
                {
                    serializedData = Serialize( yaml, serializeAsJson: true, formatJson: true, emitDefaultValues: false );

                    break;
                }
                case FormatType.Yaml:
                {
                    serializedData = yaml;
                    break;
                }
                case FormatType.Xml:
                {
                    Dictionary<object, object> yamlAsDict = Deserialize( yaml );
                    serializedData = Serialize( yamlAsDict, serializeAsJson: true, formatJson: true, emitDefaultValues: false );
                    XmlDocument doc = JsonConvert.DeserializeXmlNode( serializedData );
                    serializedData = XmlHelpers.Serialize<string>( doc );

                    break;
                }
                case FormatType.None:
                {
                    serializedData = yaml;
                    break;
                }
            }

            return serializedData;
        }


        #region Serialize/Deserialize
        public static void Serialize(TextWriter tw, object data, bool serializeAsJson = false, bool emitDefaultValues = false)
        {
            Serializer serializer = null;
            SerializerBuilder builder = new SerializerBuilder();

            if( serializeAsJson )
                builder.JsonCompatible();

            if( emitDefaultValues )
                builder.EmitDefaults();

            serializer = builder.Build();

            serializer.Serialize( tw, data );
        }

        public static string Serialize(object data, bool serializeAsJson = false, bool formatJson = true, bool emitDefaultValues = false)
        {
            string result = null;

            if( !string.IsNullOrWhiteSpace( data?.ToString() ) )
                using( StringWriter writer = new StringWriter() )
                {
                    Serialize( writer, data, serializeAsJson, emitDefaultValues );
                    result = serializeAsJson && formatJson ? JsonHelpers.Format( writer.ToString() ) : writer.ToString();
                }

            return result;
        }

        public static void SerializeFile(string path, object data, bool serializeAsJson = false, bool formatJson = true, bool emitDefaultValues = false)
        {
            if( !serializeAsJson )
            {
                using( StreamWriter writer = new StreamWriter( path ) )
                    Serialize( writer, data, serializeAsJson, emitDefaultValues );
            }
            else //gets formatted json
            {
                string result = Serialize( data, serializeAsJson, formatJson, emitDefaultValues );
                File.WriteAllText( path, result );
            }
        }

        public static T Deserialize<T>(string yaml, bool ignoreUnmatchedProperties = true)
        {
            using( StringReader reader = new StringReader( yaml ) )
            {
                DeserializerBuilder builder = new DeserializerBuilder();
                if( ignoreUnmatchedProperties )
                    builder.IgnoreUnmatchedProperties();
                Deserializer deserializer = builder.Build();
                return deserializer.Deserialize<T>( reader );
            }
        }

        public static T Deserialize<T>(TextReader reader, bool ignoreUnmatchedProperties = true)
        {
            DeserializerBuilder builder = new DeserializerBuilder();
            if( ignoreUnmatchedProperties )
                builder.IgnoreUnmatchedProperties();
            Deserializer deserializer = builder.Build();
            return deserializer.Deserialize<T>( reader );
        }

        public static T DeserializeFile<T>(string path, bool ignoreUnmatchedProperties = true) where T : class
        {
            T ssc = null;
            using( StreamReader reader = new StreamReader( path ) )
            {
                DeserializerBuilder builder = new DeserializerBuilder();
                if( ignoreUnmatchedProperties )
                    builder.IgnoreUnmatchedProperties();
                Deserializer deserializer = builder.Build();
                ssc = deserializer.Deserialize<T>( reader );
            }
            return ssc;
        }

        public static Dictionary<object, object> Deserialize(string source)
        {
            return Deserialize<Dictionary<object, object>>( source );
        }
        #endregion
    }
}