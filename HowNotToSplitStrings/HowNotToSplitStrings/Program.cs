using CommandLine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HowNotToSplitStrings
{
    class Program
    {
        const string _delimiter = ",";
        const string _file5FieldCountName = "ImportTestFile-5Delimiter.csv";
        const string file50FieldCountName = "ImportTestFile-50Delimiter.csv";

        public class Options
        {
            [Option('v', "verbose", Required = false, Default = false, HelpText = "Set output to verbose messages.")]
            public bool Verbose { get; set; }

            [Option('f', "filename", Required = false, Default = _file5FieldCountName, HelpText = "Specifies delimiter file.")]
            public string FileName { get; set; }

            [Option('d', "delimiter", Required = false, Default = _delimiter, HelpText = "String delimiter.")]
            public string Delimiter{ get; set; }

            [Option('d', "duration", Required = false, Default = 20, HelpText = "Duration that the test should run..")]
            public int Duration { get; set; }
        }

        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
                   .WithParsed<Options>(RunOptions)
                   .WithNotParsed(HandleParseError);
        }

        static void RunOptions(Options opts) {//handle options
            //Console.WriteLine("Press any key when ready to run program.");
            //Console.ReadKey();

            StringArrayKeepEmpties(opts);
            StringArrayDontKeepEmpties(opts);

            CharArrayKeepEmpties(opts);
            CharArrayDontKeepEmpties(opts);

            Console.WriteLine("Completed running program, press any key to close.");
        }

        // Divide duration by 4 since there are 4 strings.

        static void StringArrayKeepEmpties(Options opts) => ReadFileAndParse(opts.FileName, opts.Delimiter, false, true, opts.Duration / 4, opts.Verbose);
        static void StringArrayDontKeepEmpties(Options opts) => ReadFileAndParse(opts.FileName, opts.Delimiter, false, false, opts.Duration / 4, opts.Verbose);

        static void CharArrayKeepEmpties(Options opts) => ReadFileAndParse(opts.FileName, opts.Delimiter, true, true, opts.Duration / 4, opts.Verbose);
        static void CharArrayDontKeepEmpties(Options opts) => ReadFileAndParse(opts.FileName, opts.Delimiter, true, false, opts.Duration / 4, opts.Verbose);

        static void ReadFileAndParse(string fileName, string delimiterParam, bool useCharArray, bool keepEmptyEntries, int durationSecs, bool verbose) {
            if (verbose)
                Console.WriteLine($"File {fileName} is size {new FileInfo(fileName).Length} in bytes.");

            var charDelims = delimiterParam.ToCharArray();
            var stringDelims = new string[] { delimiterParam };
            Func<string, string[]> splitLineDelegate;
            var splitOptions = keepEmptyEntries ? StringSplitOptions.None : StringSplitOptions.RemoveEmptyEntries;

            if (useCharArray) {
                splitLineDelegate = (s) => s.Split(charDelims, splitOptions);
            } else {
                splitLineDelegate = (s) => s.Split(stringDelims, splitOptions);
            }

            string[] lineOutput;

            using (var stream = new StreamReader(File.OpenRead(fileName))) {
                lineOutput = stream.ReadToEnd().Split(new char[] { '\n' });
            }
            var lines = lineOutput.Length;

            ParseFields(lineOutput, lines, splitLineDelegate, durationSecs, verbose);
        }

        static void ParseFields(string[] lineOutput, int lines, Func<string, string[]> splitLineDelegate, int durationSecs, bool verbose) {
            var startTime = DateTime.Now;

            var i = 0;
            // Check every 1000 tries whether it's passed 
            while (1 == 1) {
                for (var lineCount = 0; lineCount < lines; lineCount++) {
                    // Perform split and discard results.
                    splitLineDelegate(lineOutput[lineCount]);
                    lineCount++;

                    if (i % 1000 == 0) {
                        if (verbose)
                            Console.WriteLine($"Parsed line {lineCount}");
                        
                        if (DateTime.Now.Subtract(startTime).TotalSeconds > durationSecs) {
                            break;
                        }
                    }
                    i++;
                }

                if (DateTime.Now.Subtract(startTime).TotalSeconds > durationSecs) {
                    break;
                }
            }
        }

        static void HandleParseError(IEnumerable<Error> errs)
        {
            //handle errors
            Console.WriteLine($"Errors: {string.Join(",", errs)}");
        }
    }
}
