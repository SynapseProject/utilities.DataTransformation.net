using System;

namespace Zephyr.DataTransformation
{
    public class WrapperUtility
    {
        public static string Transform(FormatType format, string data, string xslt)
        {
            switch( format )
            {
                case FormatType.Yaml:
                {
                    return YamlHelpers.Transform( data, xslt );
                }
                case FormatType.Json:
                {
                    return JsonHelpers.Transform( data, xslt );
                }
                case FormatType.Xml:
                {
                    return XmlHelpers.Transform( data, xslt );
                }
                default:
                {
                    return data;
                }
            }
        }

        public static string ConvertToFormat(FormatType inputFormat, string data, FormatType outputFormat)
        {
            switch( inputFormat )
            {
                case FormatType.Yaml:
                {
                    return YamlHelpers.ConvertToFormat( data, outputFormat );
                }
                case FormatType.Json:
                {
                    return JsonHelpers.ConvertToFormat( data, outputFormat );
                }
                case FormatType.Xml:
                {
                    return XmlHelpers.ConvertToFormat( data, outputFormat );
                }
                default:
                {
                    return data;
                }
            }
        }
    }
}