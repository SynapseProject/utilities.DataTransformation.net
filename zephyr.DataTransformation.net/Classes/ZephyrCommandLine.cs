using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Zephyr.DataTransformation;

public class ZephyrCommandLine
{
    public void Main(string[] args)
    {
        {
            Arguments a = null;
            try
            {
                a = new Arguments(args);

                if (!a.IsParsed)
                    WriteHelpAndExit(a.Message);

                if (a.SerializationFormat == FormatType.None)
                    return;

                switch (a.Action)
                {
                    case Action.Convert:
                        Convert(a.File, a.SerializationFormat, a.OutputFormat, a.ShowActionHelp);
                        break;
                    case Action.XslTransform:
                        XslTransform(a.File, a.SerializationFormat, a.Xslt, a.ShowActionHelp);
                        break;
                    case Action.JsonSelect:
                        JsonSelect(a.File, a.SerializationFormat, a.Expression, a.ShowActionHelp);
                        break;
                    case Action.RegexMatch:
                        RegexMatch(a.File, a.Pattern, a.Options, a.ShowActionHelp);
                        break;
                    case Action.RegexMatches:
                        RegexMatch(a.File, a.Pattern, a.Options, a.ShowActionHelp, true);
                        break;
                }
            }
            catch (Exception ex)
            {
                WriteHelpAndExit(UnwindException(ex));
            }
        }

    }

    #region Action Methods
    static void Convert(string data, FormatType inputFormat, FormatType outputFormat, bool showHelp)
    {
        if (showHelp)
        {
            //Dictionary<string, Type> parms = new Dictionary<string, Type>
            //{
            //    { "file", typeof( string ) },
            //    { "outputFormat", typeof( FormatType ) }
            //};
            List<Parameter> parms = new List<Parameter>
                {
                    new Parameter{ Key = "file", Type = typeof( string ), HelpText = "Path to file" },
                    new Parameter{ Key = "outputFormat", Type = typeof( FormatType )}
                };
            ConsoleColor defaultColor = Console.ForegroundColor;
            Console_WriteLine("Parameter options for Convert action:\r\n", ConsoleColor.Green);
            WriteMethodParametersHelp(parms);
            Console.ForegroundColor = defaultColor;
        }
        else if (outputFormat != FormatType.None)
        {
            Console.WriteLine($"Converting file to {outputFormat} format.\r\n");
            Console.WriteLine((inputFormat == outputFormat) ? data : WrapperUtility.ConvertToFormat(inputFormat, data, outputFormat));
        }
    }

    static void XslTransform(string data, FormatType inputFormat, string xslt, bool showHelp)
    {
        if (showHelp)
        {
            //Dictionary<string, Type> parms = new Dictionary<string, Type>
            //{
            //    { "file", typeof( string ) },
            //    { "xslt", typeof( string ) }
            //};

            List<Parameter> parms = new List<Parameter>
                {
                    new Parameter{Key = "file", Type = typeof(string), HelpText = "Path to file"},
                    new Parameter{Key = "xslt", Type = typeof(string), HelpText = "Path to xslt file"}
                };
            ConsoleColor defaultColor = Console.ForegroundColor;
            Console_WriteLine("Parameter options for XslTransform action:\r\n", ConsoleColor.Green);
            WriteMethodParametersHelp(parms);
            Console.ForegroundColor = defaultColor;
        }
        else //if (inputFormat == FormatType.Xml && !string.IsNullOrWhiteSpace(xslt))
        {
            Console.WriteLine($"Executing XslTransform on file.\r\n");
            Console.WriteLine(WrapperUtility.Transform(inputFormat, data, xslt));
        }
    }

    static void JsonSelect(string data, FormatType inputFormat, string expression, bool showHelp)
    {
        if (showHelp)
        {
            //Dictionary<string, Type> parms = new Dictionary<string, Type>
            //{
            //    { "file", typeof( string ) },
            //    { "expression", typeof( string ) }
            //};
            List<Parameter> parms = new List<Parameter>
                {
                    new Parameter{ Key = "file", Type = typeof(string), HelpText = "Path to file"},
                    new Parameter{ Key = "expression", Type = typeof(string), HelpText = "SelectToken expression"}
                };
            ConsoleColor defaultColor = Console.ForegroundColor;
            Console_WriteLine("Parameter options for JsonSelect action:\r\n", ConsoleColor.Green);
            WriteMethodParametersHelp(parms);
            Console.ForegroundColor = defaultColor;
        }
        else if (inputFormat == FormatType.Json && !string.IsNullOrWhiteSpace(expression))
        {
            Console.WriteLine($"Executing JsonSelect on file.\r\n");
            object resultAsObject = JsonHelpers.Select(data, expression);
            if (resultAsObject is string)
                Console.WriteLine(resultAsObject.ToString());
            else if (resultAsObject is List<string> resultAsList)
            {
                //List<string> resultAsList = (List<string>)resultAsObject;
                resultAsList.ForEach(Console.WriteLine);
            }
        }
    }

    static void RegexMatch(string data, string pattern, RegexOptions options, bool showHelp, bool matchAll = false)
    {
        if (showHelp)
        {
            List<Parameter> parms = new List<Parameter>
                {
                    new Parameter{ Key = "file", Type = typeof(string), HelpText = "Path to file"},
                    new Parameter{ Key = "pattern", Type = typeof(string), HelpText = "Match pattern"},
                    new Parameter{ Key = "[options]", Type = typeof(string), HelpText = "RegexOptions. Default: default behavior"}
                };
            ConsoleColor defaultColor = Console.ForegroundColor;
            Console_WriteLine($"Parameter options for {(matchAll ? "RegexMatches" : "RegexMatch")} action:\r\n", ConsoleColor.Green);
            WriteMethodParametersHelp(parms);
            Console.ForegroundColor = defaultColor;
        }
        else if (!string.IsNullOrWhiteSpace(pattern))
        {
            if (matchAll)
            {
                Console.WriteLine($"Executing RegexMatches on file.\r\n");
                MatchCollection mc = RegexHelpers.Matches(data, pattern, options, Regex.InfiniteMatchTimeout);
                foreach (Match m in mc)
                    Console.WriteLine(m.ToString());
            }
            else
            {
                Console.WriteLine($"Executing RegexMatch on file.\r\n");
                Match m = RegexHelpers.Match(data, pattern, options, Regex.InfiniteMatchTimeout);
                Console.WriteLine(m.ToString());
            }
        }
    }
    #endregion

    #region Help
    static void WriteMethodParametersHelp(Dictionary<string, Type> parms, string prefix = null)
    {
        int count = 0;
        foreach (string key in parms.Keys)
        {
            count++;
            Console.WriteLine("\t{0,-30}{1}", key, GetTypeFriendlyName(parms[key], prefix));
        }
        if (count == 0)
            Console.WriteLine($"\tNo additional parameter options.");
    }

    static void WriteMethodParametersHelp(List<Parameter> parms, string prefix = null)
    {
        if (parms.Count == 0)
            Console.WriteLine($"\tNo additional parameter options.");
        else
        {
            foreach (Parameter p in parms)
            {
                //Console.WriteLine("\t{0,-30}{1}", p.Key, GetTypeFriendlyName(p.Type, prefix));
                Console.WriteLine($"    {p.Key,-20} {GetTypeFriendlyName(p.Type, prefix),-20} {p.HelpText}");
            }
        }
    }

    static string GetTypeFriendlyName(Type type, string prefix)
    {
        string typeName = type.ToString().ToLower();
        if (typeName.Contains("guid"))
        {
            if (typeName.Contains("generic.list"))
                return "Csv list of Guids or JSON list of Guids";
            else
                return "Guid";
        }
        else if (typeName.Contains("int"))
        {
            return "int";
        }
        else if (typeName.Contains("bool"))
        {
            return "bool";
        }
        else if (typeName.Contains("string"))
        {
            return "string";
        }
        else if (typeName.Contains("datetime"))
        {
            return "DateTime";
        }
        else if (type.IsEnum)
        {
            return GetEnumValuesCsv(type);
        }
        else
        {
            return type.ToString().Replace(prefix, "");
        }
    }

    static string GetEnumValuesCsv(Type enumType)
    {
        Array values = Enum.GetValues(enumType);
        List<object> av = new List<object>();
        foreach (object v in values) av.Add(v);
        return string.Join(",", av);
    }

    static void WriteHelpAndExit(string message)
    {
        bool haveError = !string.IsNullOrWhiteSpace(message);

        ConsoleColor defaultColor = Console.ForegroundColor;
        Console_WriteLine($"zephyr.datatransformation.dll, Version: {typeof(ZephyrCommandLine).Assembly.GetName().Version}\r\n", ConsoleColor.Green);
        Console.WriteLine("Syntax:");
        Console_WriteLine("  zephyr datatransformation {0}serializationFormat{1} {0}action{1} {0}parameters{1}\r\n", ConsoleColor.Cyan, "{", "}");
        Console_WriteLine("  serializationFormat: json|yaml|xml\r\n", ConsoleColor.Green);
        Console.WriteLine($"  {"action:",-23}parameters\r\n", "");
        Console.WriteLine($"      {"Convert",-19}Convert file to outputFormat");
        Console.WriteLine($"      {"",-19}file:{{filePath}} outputFormat:{{json|yaml|xml}}\r\n");
        Console.WriteLine($"      {"XslTransform",-19}Transform file using xlst");
        Console.WriteLine($"      {"",-19}file:{{filePath}} xslt:{{filePath}}\r\n");
        Console.WriteLine($"      {"JsonSelect",-19}Query file using SelectToken expression");
        Console.WriteLine($"      {"",-19}file:{{filePath}} expression:{{string}}\r\n");
        Console.WriteLine($"      {"RegexMatch",-19}Query file using Regex return first match");
        Console.WriteLine($"      {"",-19}file:{{filePath}} pattern:{{string}}");
        Console.WriteLine($"      {"",-19}[options:{{RegexOptions}}]\r\n");
        Console.WriteLine($"      {"RegexMatches",-19}Query file using Regex return all matches");
        Console.WriteLine($"      {"",-19}file:{{filePath}} pattern:{{string}}");
        Console.WriteLine($"      {"",-19}[options:{{RegexOptions}}]\r\n");

        Console.WriteLine("  Examples:");
        Console.WriteLine("     zephyr datatransformation json convert file:c:\\temp\\products.json");
        Console.WriteLine("         outputFormat:yaml\r\n");
        Console.WriteLine("     zephyr datatransformation json xsltransform file:c:\\temp\\products.json");
        Console.WriteLine("         xslt:c:\\temp\\foo.xslt\r\n");
        Console.WriteLine("     zephyr datatransformation json jsonselect file:c:\\temp\\products.json");
        Console.WriteLine("        expression:$..Products[?(@.Price >= 50)].Name\r\n");
        Console.WriteLine("     zephyr datatransformation json regexmatch file:c:\\temp\\products.json");
        Console.WriteLine("        pattern:\\d+ options:ignorecase,compiled");

        if (haveError)
            Console_WriteLine($"\r\n\r\n*** Last error:\r\n{message}\r\n", ConsoleColor.Red);

        Console.ForegroundColor = defaultColor;

        Environment.Exit(haveError ? 1 : 0);
    }

    static void Console_WriteLine(string s, ConsoleColor color, params object[] args)
    {
        Console.ForegroundColor = color;
        Console.WriteLine(s, args);
    }

    // copied from Synapse.Core.Utilities.ExceptionHelpers
    public static string UnwindException(Exception ex)
    {
        return UnwindException(null, ex);
    }

    public static string UnwindException(string context, Exception ex, bool asSingleLine = false)
    {
        //string lineEnd = asSingleLine ? "|" : @"\r\n";
        string lineEnd = asSingleLine ? "|" : "\r\n";

        StringBuilder msg = new StringBuilder();
        if (!string.IsNullOrWhiteSpace(context))
            msg.Append($"An error occurred in: {context}{lineEnd}");

        msg.Append($"{ex.Message}{lineEnd}");

        if (ex.InnerException != null)
        {
            if (ex.InnerException is AggregateException)
            {
                AggregateException ae = ex.InnerException as AggregateException;
                foreach (Exception wcx in ae.InnerExceptions)
                {
                    Stack<Exception> exceptions = new Stack<Exception>();
                    exceptions.Push(wcx);

                    while (exceptions.Count > 0)
                    {
                        Exception e = exceptions.Pop();

                        if (e.InnerException != null)
                            exceptions.Push(e.InnerException);

                        msg.Append($"{e.Message}{lineEnd}");
                    }
                }
            }
            else
            {
                Stack<Exception> exceptions = new Stack<Exception>();
                exceptions.Push(ex.InnerException);

                while (exceptions.Count > 0)
                {
                    Exception e = exceptions.Pop();

                    if (e.InnerException != null)
                        exceptions.Push(e.InnerException);

                    msg.Append($"{e.Message}{lineEnd}");
                }
            }
        }

        return asSingleLine ? msg.ToString().TrimEnd('|') : msg.ToString();
    }
    #endregion


}

internal class Arguments
{
    public bool IsParsed { get; internal set; }
    public string Message { get; internal set; }
    public bool ShowActionHelp { get; internal set; }

    public FormatType SerializationFormat { get; internal set; }
    public Action Action { get; internal set; }
    public Dictionary<string, string> Parms { get; internal set; }

    public string File { get; internal set; }
    public string FilePath { get; internal set; }
    public FormatType OutputFormat { get; internal set; }
    public string Expression { get; internal set; }
    public string Xslt { get; internal set; }
    public string XsltPath { get; internal set; }
    public string Pattern { get; internal set; }
    public RegexOptions Options { get; internal set; }

    // parameters
    const string __file = "file";
    const string __outputformat = "outputformat";
    const string __xslt = "xslt";
    const string __expression = "expression";
    const string __pattern = "pattern";
    const string __options = "options";

    public Arguments(string[] args)
    {
        IsParsed = false;
        ShowActionHelp = false;

        if (args.Length == 0 || IsHelp(args[0]))
        {
            return;
        }
        #region SerializationFormat
        if (Enum.TryParse<FormatType>(args[0], true, out FormatType f))
        {
            SerializationFormat = f;
            if (SerializationFormat == FormatType.None)
            {
                Message += "  * Not a valid Serialization Format.\r\n";
                return;
            }
        }
        else
        {
            Message += "  * Unknown Serialization Format.\r\n";
            return;
        }
        #endregion

        #region Action
        if (args.Length < 2)
        {
            Message += "  * Action not specified.\r\n";
            return;
        }
        if (IsHelp(args[1]))
        {
            return;
        }
        if (Enum.TryParse<Action>(args[1], true, out Action a))
        {
            Action = a;
            if (Action == Action.None)
            {
                Message += "  * Not a valid action.\r\n";
                return;
            }
        }
        else
        {
            Message += "  * Unknown Action.\r\n";
            return;
        }
        //if (Action == Action.XslTransform && SerializationFormat != FormatType.Xml)
        //{
        //    Message += $"  * {Action.ToString()} only works on {FormatType.Xml.ToString()} data.\r\n";
        //    return;
        //}
        if (Action == Action.JsonSelect && SerializationFormat != FormatType.Json)
        {
            Message += $"  * {Action.ToString()} only works on {FormatType.Json.ToString()} data.\r\n";
            return;
        }
        #endregion

        #region Parameters
        if (args.Length > 2 && IsHelp(args[2]))
        {
            ShowActionHelp = true;
            IsParsed = true;
            return;
        }

        bool error = false;
        Parms = ParseCmdLine(args, 2, ref error);
        if (error)
            return;

        switch (Action)
        {
            case Action.Convert:
                if (!GetConvertParameters())
                    return;

                break;
            case Action.XslTransform:
                if (!GetXslTransformParameters())
                    return;

                break;
            case Action.JsonSelect:
                if (!GetJsonSelectParameters())
                    return;

                break;
            case Action.RegexMatch:
            case Action.RegexMatches:
                if (!GetRegexMatchParameters())
                    return;

                break;
        }
        #endregion

        IsParsed = true;
    }


    bool IsHelp(string p)
    {
        p = p.ToLower();
        return (p.Equals("?") || p.Equals("help")) ? true : false;
    }

    bool GetConvertParameters()
    {
        bool ok = true;

        // file, outputformat
        if (Parms.Keys.Contains(__file))
        {
            if (System.IO.File.Exists(Parms[__file]))
            {
                File = System.IO.File.ReadAllText(Parms[__file]);
                FilePath = Parms[__file];
            }
            else
            {
                Message += "  * Unable to resolve File as path.\r\n";
                ok = false;
            }

            Parms.Remove(__file);
        }
        else
        {
            Message += "  * File not specified.\r\n";
            ok = false;
        }

        if (Parms.Keys.Contains(__outputformat))
        {
            if (Enum.TryParse<FormatType>(Parms[__outputformat], true, out FormatType outputFormat))
                OutputFormat = outputFormat;
            else
            {
                Message += "  * Unknown OutputFormat.\r\n";
                ok = false;
            }

            Parms.Remove(__outputformat);
        }
        else
        {
            Message += "  * OutputFormat not specified.\r\n";
            ok = false;
        }

        return ok;
    }

    bool GetXslTransformParameters()
    {
        bool ok = true;

        // file, xslt
        if (Parms.Keys.Contains(__file))
        {
            if (System.IO.File.Exists(Parms[__file]))
            {
                File = System.IO.File.ReadAllText(Parms[__file]);
                FilePath = Parms[__file];
            }
            else
            {
                Message += "  * Unable to resolve File as path.\r\n";
                ok = false;
            }

            Parms.Remove(__file);
        }
        else
        {
            Message += "  * File not specified.\r\n";
            ok = false;
        }

        if (Parms.Keys.Contains(__xslt))
        {
            if (System.IO.File.Exists(Parms[__xslt]))
            {
                Xslt = System.IO.File.ReadAllText(Parms[__xslt]);
                XsltPath = Parms[__xslt];
            }
            else
            {
                Message += "  * Unable to resolve Xslt as path.\r\n";
                ok = false;
            }

            Parms.Remove(__file);
        }
        else
        {
            Message += "  * Xslt not specified.\r\n";
            ok = false;
        }

        return ok;
    }

    bool GetJsonSelectParameters()
    {
        bool ok = true;

        // file, expression
        if (Parms.Keys.Contains(__file))
        {
            if (System.IO.File.Exists(Parms[__file]))
            {
                File = System.IO.File.ReadAllText(Parms[__file]);
                FilePath = Parms[__file];
            }
            else
            {
                Message += "  * Unable to resolve File as path.\r\n";
                ok = false;
            }

            Parms.Remove(__file);
        }
        else
        {
            Message += "  * File not specified.\r\n";
            ok = false;
        }

        if (Parms.Keys.Contains(__expression))
        {
            Expression = Parms[__expression];

            Parms.Remove(__expression);
        }
        else
        {
            Message += "  * Expression not specified.\r\n";
            ok = false;
        }

        return ok;
    }

    bool GetRegexMatchParameters()
    {
        bool ok = true;

        // file, pattern, [regexoptions]
        if (Parms.Keys.Contains(__file))
        {
            if (System.IO.File.Exists(Parms[__file]))
            {
                File = System.IO.File.ReadAllText(Parms[__file]);
                FilePath = Parms[__file];
            }
            else
            {
                Message += "  * Unable to resolve File as path.\r\n";
                ok = false;
            }

            Parms.Remove(__file);
        }
        else
        {
            Message += "  * File not specified.\r\n";
            ok = false;
        }

        if (Parms.Keys.Contains(__pattern))
        {
            Pattern = Parms[__pattern];

            Parms.Remove(__pattern);
        }
        else
        {
            Message += "  * Pattern not specified.\r\n";
            ok = false;
        }

        if (Parms.Keys.Contains(__options))
        {
            if (Enum.TryParse<RegexOptions>(Parms[__options], true, out RegexOptions o))
                Options = o;
            else
            {
                Message += "  * Unable to parse Options as regexoptions.\r\n";
                ok = false;
            }

            Parms.Remove(__options);
        }
        else
            Options = RegexOptions.None;        

        return ok;
    }

    Dictionary<string, string> ParseCmdLine(string[] args, int startIndex, ref bool error)
    {
        Dictionary<string, string> options = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        if (args.Length < (startIndex + 1))
            Message += "Not enough arguments specified.";
        else
        {
            string pattern = "(?<argname>.*?):(?<argvalue>.*)";
            //string pattern = @"(?<argname>/\w+):(?<argvalue>.*)";
            for (int i = startIndex; i < args.Length; i++)
            {
                Match match = Regex.Match(args[i], pattern);

                // If match not found, command line args are improperly formed.
                if (match.Success)
                {
                    options[match.Groups["argname"].Value.TrimStart('/')] =       //.ToLower()
                        match.Groups["argvalue"].Value;
                }
                else
                {
                    Message = "The command line arguments are not valid or are improperly formed. Use 'argname:argvalue' for extended arguments.\r\n";
                    break;
                }
            }
        }
        error = !string.IsNullOrWhiteSpace(Message);
        return options;
    }

}

enum Action
{
    None,
    Convert,
    XslTransform,
    JsonSelect,
    RegexMatch,
    RegexMatches
}

class Parameter
{
    public string Key { get; set; }
    public Type Type { get; set; }
    public string HelpText { get; set; }
}
