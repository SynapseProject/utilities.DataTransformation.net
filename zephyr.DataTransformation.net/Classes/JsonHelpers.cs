using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Xml;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

using YamlDotNet.Serialization;

namespace zephyr.DataTransformation
{
    public class JsonHelpers
    {
        public string Transform(string json, string xslt)
        {
            string jsonAsXml = ConvertToFormat( json, FormatType.Xml );
            jsonAsXml = XmlHelpers.Transform( jsonAsXml, xslt );
            return XmlHelpers.ConvertToFormat( jsonAsXml, FormatType.Json );
        }

        public static string ConvertToFormat(string inputJson, FormatType targetFormatType)
        {
            string serializedData = "";

            switch( targetFormatType )
            {
                case FormatType.Json:
                {
                    serializedData = inputJson;
                    break;
                }
                case FormatType.Yaml:
                {
                    //convert the string JSON data into an object so yaml.net can serialize it to YAML nicely
                    ExpandoObjectConverter expConverter = new ExpandoObjectConverter();
                    dynamic deserializedObject = JsonConvert.DeserializeObject<ExpandoObject>( inputJson, expConverter );

                    Serializer serializer = new Serializer();
                    serializedData = serializer.Serialize( deserializedObject );

                    break;
                }
                case FormatType.Xml:
                {
                    XmlDocument doc = JsonConvert.DeserializeXmlNode( inputJson );
                    serializedData = XmlHelpers.Serialize<string>( doc );

                    break;
                }
                case FormatType.None:
                {
                    serializedData = inputJson;
                    break;
                }
            }

            return serializedData;
        }


        //taken from accepted answer on
        //  http://stackoverflow.com/questions/4580397/json-formatter-in-c
        //minor code cleanup, plus added --> s = s.Replace( " \"", "\"" ).Replace( ": {", ":{" );
        //to deal with output of yaml.net
        private const string __indent = "  ";
        public static string FormatJson(string s)
        {
            s = s.Replace( " \"", "\"" ).Replace( ": {", ":{" );
            int indent = 0;
            bool quoted = false;
            StringBuilder sb = new StringBuilder();

            for( int i = 0; i < s.Length; i++ )
            {
                var ch = s[i];
                switch( ch )
                {
                    case '{':
                    case '[':
                    {
                        sb.Append( ch );
                        if( !quoted )
                        {
                            sb.AppendLine();
                            Enumerable.Range( 0, ++indent ).ForEach( item => sb.Append( __indent ) );
                        }
                        break;
                    }

                    case '}':
                    case ']':
                    {
                        if( !quoted )
                        {
                            sb.AppendLine();
                            Enumerable.Range( 0, --indent ).ForEach( item => sb.Append( __indent ) );
                        }
                        sb.Append( ch );
                        break;
                    }

                    case '"':
                    {
                        sb.Append( ch );
                        bool escaped = false;

                        int index = i;
                        while( index > 0 && s[--index] == '\\' )
                            escaped = !escaped;

                        if( !escaped )
                            quoted = !quoted;
                        break;
                    }

                    case ',':
                    {
                        sb.Append( ch );
                        if( !quoted )
                        {
                            sb.AppendLine();
                            Enumerable.Range( 0, indent ).ForEach( item => sb.Append( __indent ) );
                        }
                        break;
                    }

                    case ':':
                    {
                        sb.Append( ch );
                        if( !quoted )
                            sb.Append( " " );
                        break;
                    }

                    default:
                    {
                        sb.Append( ch );
                        break;
                    }
                }
            }
            return sb.ToString();
        }
    }

    static class Extensions
    {
        public static void ForEach<T>(this IEnumerable<T> ie, Action<T> action)
        {
            foreach( var i in ie )
            {
                action( i );
            }
        }
    }
}