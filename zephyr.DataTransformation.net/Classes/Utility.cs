using System;

namespace Zephyr.DataTransformation
{
    public class WrapperUtility
    {
        public static string Transform(FormatType format, string data, string xslt, bool preserveOutputAsIs = true)
        {
            switch( format )
            {
                case FormatType.Yaml:
                {
                    return YamlHelpers.Transform( data, xslt, preserveOutputAsIs );
                }
                case FormatType.Json:
                {
                    return JsonHelpers.Transform( data, xslt, preserveOutputAsIs );
                }
                case FormatType.Xml:
                {
                    return XmlHelpers.Transform( data, xslt, formatOutputIndented: preserveOutputAsIs );
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