using System;
using System.IO;
using NUnit.Framework;

using zephyr.DataTransformation;

namespace zephyr.DataTransformation.UnitTests
{
    [TestFixture]
    public class UnitTests
    {
        string _root = null;
        string _xslt = null;
        string _baseXml = null;
        string _serversXml = null;
        string _baseJson = null;
        string _serversJson = null;
        string _baseYaml = null;
        string _serversYaml = null;


        [OneTimeSetUp]
        public void Init()
        {
            _root = Path.GetDirectoryName( System.Reflection.Assembly.GetExecutingAssembly().Location );
            Directory.SetCurrentDirectory( $@"{_root}\..\..\Resources" );
            _root = Directory.GetCurrentDirectory();

            _xslt = File.ReadAllText( "servers.xslt" );
            _baseXml = File.ReadAllText( "base.xml" );
            _serversXml = File.ReadAllText( "servers.xml" );
            _baseJson = File.ReadAllText( "base.json" );
            _baseYaml = File.ReadAllText( "base.yaml" );
            //_serversJson = File.ReadAllText( "servers.json" );
        }

        [Test]
        [Category( "xml" )]
        public void TransformXml()
        {
            string result = XmlHelpers.Transform( _baseXml, _xslt );

            Assert.AreEqual( _serversXml, result );
        }

        [Test]
        [Category( "xml" )]
        [TestCase( FormatType.Json )]
        [TestCase( FormatType.Yaml )]
        public void ConvertXml(FormatType format)
        {
            string result = XmlHelpers.ConvertToFormat( _baseXml, format );
            CompareConvertResult( result, format );
        }

        [Test]
        [Category( "josn" )]
        [TestCase( FormatType.Xml )]
        [TestCase( FormatType.Yaml )]
        public void ConvertJson(FormatType format)
        {
            string result = JsonHelpers.ConvertToFormat( _baseJson, format );
            CompareConvertResult( result, format );
        }

        void CompareConvertResult(string actual, FormatType format)
        {
            switch( format )
            {
                case FormatType.Yaml:
                {
                    Assert.AreEqual( _baseYaml, actual );
                    break;
                }
                case FormatType.Json:
                {
                    Assert.AreEqual( _baseJson, actual );
                    break;
                }
                case FormatType.Xml:
                {
                    Assert.AreEqual( _baseXml, actual );
                    break;
                }
            }
        }
    }
}