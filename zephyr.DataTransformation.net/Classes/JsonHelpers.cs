using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Xml;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

using YamlDotNet.Serialization;

namespace Zephyr.DataTransformation
{
    public class JsonHelpers
    {
        public static string Transform(string json, string xslt, bool preserveOutputAsIs = true)
        {
            if ( string.IsNullOrWhiteSpace( json ) )
                return null;

            if( string.IsNullOrWhiteSpace( xslt ) )
                return json;

            string jsonAsXml = ConvertToFormat( json, FormatType.Xml );
            jsonAsXml = XmlHelpers.Transform( jsonAsXml, xslt );
            return preserveOutputAsIs ? jsonAsXml : XmlHelpers.ConvertToFormat( jsonAsXml, FormatType.Json );
        }

        public static string ConvertToFormat(string json, FormatType targetFormatType)
        {
            if (string.IsNullOrWhiteSpace(json))
                return null;

            string serializedData = "";

            switch( targetFormatType )
            {
                case FormatType.Json:
                {
                    serializedData = json;
                    break;
                }
                case FormatType.Yaml:
                {
                    //convert the string JSON data into an object so yaml.net can serialize it to YAML nicely
                    ExpandoObjectConverter expConverter = new ExpandoObjectConverter();
                    dynamic deserializedObject = JsonConvert.DeserializeObject<ExpandoObject>( json, expConverter );

                    Serializer serializer = new Serializer();
                    serializedData = serializer.Serialize( deserializedObject );

                    break;
                }
                case FormatType.Xml:
                {
                    XmlDocument doc = JsonConvert.DeserializeXmlNode( json );
                    serializedData = XmlHelpers.Serialize<string>( doc );

                    break;
                }
                case FormatType.None:
                {
                    serializedData = json;
                    break;
                }
            }

            return serializedData;
        }

        public static object Select(string json, string expression)
        {
            if (string.IsNullOrWhiteSpace(json))
                return null;

            List<string> result = new List<string>();

            IEnumerable<JToken> tokens = JObject.Parse( json ).SelectTokens( expression );

            foreach( JToken token in tokens )
                result.Add( token.ToString() );

            if( result.Count == 0 )
                return null;
            else if( result.Count == 1 )
                return result[0];
            else
                return result;
        }

        public static string Format(string json, Newtonsoft.Json.Formatting format = Newtonsoft.Json.Formatting.Indented)
        {
            return JObject.Parse( json ).ToString( format );
        }
    }
}