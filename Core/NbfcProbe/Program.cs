using clipr;
using NbfcProbe.CommandLineOptions;
using StagWare.FanControl;
using StagWare.FanControl.Plugins;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace NbfcProbe
{
    public class Program
    {
        #region Nested Types

        struct RegisterLog : IEquatable<RegisterLog>
        {
            public bool Print;
            public List<byte> Values;

            public bool Equals(RegisterLog other)
            {
                if(other.Print != this.Print)
                {
                    return false;
                }

                return other.Values.SequenceEqual(this.Values);
            }
        }

        #endregion

        #region Constants

        const string RowHeaderFormatHex = "0x{0:X2}: ";
        const string RowHeaderFormatDec = "{0:d3}: ";

        const string ValueFormatHex = "{0:X2}";
        const string ValueFormatDec = "{0:d3}";

        #endregion

        #region Private Fields

        static IEmbeddedController ec;

        #endregion

        #region Main

        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.ProcessExit += (s, e) =>
            {
                if (ec != null)
                {
                    try
                    {
                        ec.ReleaseLock();
                        ec.Dispose();
                    }
                    catch { }
                }
            };

            try
            {
                ParseArgs(args);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.Message);
            }
        }

        #endregion

        #region Private Methods

        private static void ParseArgs(string[] args)
        {
            var opt = new Verbs();
            var helpGen = new NbfcCli.CommandLineOptions.HelpGenerator<Verbs>();
            helpGen.DescriptionDistance = 29;
            helpGen.GenericDescription = "All input values are interpreted as decimal numbers by default."
                + Environment.NewLine
                + "Hexadecimal values may be entered by prefixing them with \"0x\".";

            var parser = new CliParser<Verbs>(opt, ParserOptions.CaseInsensitive, helpGen);
            parser.StrictParse(args);

            if (opt.ECDump != null)
            {
                ECDump();
            }
            else if (opt.ECRead != null)
            {
                ECRead(opt.ECRead.Register);
            }
            else if (opt.ECWrite != null)
            {
                ECWrite(opt.ECWrite.Register, opt.ECWrite.Value, opt.ECWrite.Verbose);
            }
            else if (opt.ECMonitor != null)
            {
                if (opt.ECMonitor.Interval < 1)
                {
                    Console.Error.WriteLine("The interval must be at least 1 second");
                    return;
                }

                if ((opt.ECMonitor.Timespan < 2 * opt.ECMonitor.Interval) && (opt.ECMonitor.Timespan > 0))
                {
                    Console.Error.WriteLine("The monitored timespan must be at least (2 * interval)");
                    return;
                }

                ECMonitor(
                    opt.ECMonitor.Timespan,
                    opt.ECMonitor.Interval,
                    opt.ECMonitor.ReportPath,
                    opt.ECMonitor.Clearly,
                    opt.ECMonitor.Decimal);
            }
            else
            {
                Console.WriteLine(helpGen.GetHelp(parser.Config));
            }
        }

        private static void ECWrite(byte register, byte value, bool verbose)
        {
            AccessEcSynchronized(ec =>
            {
                if (verbose)
                {
                    Console.WriteLine("Writing at {0}: {1} (0x{1:X2})", register, value);
                }

                ec.WriteByte(register, value);

                if (verbose)
                {
                    byte b = ec.ReadByte(register);
                    Console.WriteLine("Current value at {0}: {1} (0x{1:X2})", register, b);
                }
            });
        }

        private static void ECRead(byte register)
        {
            AccessEcSynchronized(ec =>
            {
                byte b = ec.ReadByte(register);
                Console.WriteLine("{0} (0x{0:X2})", b);
            });
        }

        private static void ECDump()
        {
            AccessEcSynchronized(ec =>
            {
                ConsoleColor defaultColor = Console.ForegroundColor;
                Console.WriteLine("   | 00 01 02 03 04 05 06 07 08 09 0A 0B 0C 0D 0E 0F");
                Console.WriteLine("---|------------------------------------------------");

                // Read all register bytes
                for (int i = 0; i <= 0xF0; i += 0x10)
                {
                    Console.ForegroundColor = defaultColor;
                    Console.Write("{0:X2} | ", i);

                    for (int j = 0; j <= 0xF; j++)
                    {
                        byte b = ec.ReadByte((byte)(i + j));

                        if (b == 0)
                        {
                            Console.ForegroundColor = ConsoleColor.DarkGray;
                        }
                        else if (b == 0xFF)
                        {
                            Console.ForegroundColor = ConsoleColor.DarkGreen;
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                        }

                        Console.Write("{0:X2} ", b);
                    }

                    Console.WriteLine();
                }

                Console.ForegroundColor = defaultColor;
            });
        }

        private static void ECMonitor(int timespan, int interval, string reportPath, bool clearly, bool decimalFormat)
        {
            var logs = new RegisterLog[byte.MaxValue];

            Console.CancelKeyPress += (sender, e) =>
            {
                if (reportPath != null)
                {
                    SaveRegisterLogs(reportPath, logs, clearly, decimalFormat);
                }
            };

            using (ec = LoadEC())
            {
                if (ec == null)
                {
                    return;
                }

                Console.WriteLine("monitoring...");

                for (byte b = 0; b < logs.Length; b++)
                {
                    AccessEcSynchronized(ec =>
                    {
                        logs[b].Values = new List<byte>();
                        logs[b].Values.Add(ec.ReadByte(b));
                    },
                    ec);
                }

                int loopCount = 0;

                while ((timespan < 1) || (loopCount < Math.Ceiling(((double)timespan / interval) - 1)))
                {
                    Thread.Sleep(interval * 1000);
                    AccessEcSynchronized(ec =>
                    {
                        for (int i = 0; i < logs.Length; i++)
                        {
                            byte value = ec.ReadByte((byte)i);
                            logs[i].Values.Add(value);

                            if (value != logs[i].Values[0])
                            {
                                logs[i].Print = true;
                            }
                        }
                    },
                    ec);

                    Console.Clear();
                    PrintRegisterLogs(logs, clearly, decimalFormat);
                    loopCount++;
                }
            }

            if (reportPath != null)
            {
                SaveRegisterLogs(reportPath, logs, clearly, decimalFormat);
            }
        }

        private static void SaveRegisterLogs(
            string path,
            RegisterLog[] registerLogs,
            bool clearly,
            bool decimalFormat)
        {
            string dir = Path.GetDirectoryName(path);

            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            int filenameChanges = 0;
            string validatedPath = path;

            while (File.Exists(validatedPath))
            {
                filenameChanges++;
                string filename = Path.GetFileNameWithoutExtension(path)
                    + (filenameChanges + 1).ToString()
                    + Path.GetExtension(path);

                validatedPath = Path.Combine(Path.GetDirectoryName(path), filename);
            }

            try
            {
                string valueFormat = decimalFormat ? ValueFormatDec : ValueFormatHex;
                var sb = new StringBuilder();

                // write header
                for (int i = 0; i < registerLogs.Length; i++)
                {
                    if (!registerLogs[i].Print)
                    {
                        continue;
                    }

                    sb.AppendFormat(valueFormat, i);
                    sb.Append(",");
                }

                // remove last comma
                sb.Remove(sb.Length - 1, 1);
                sb.AppendLine();

                // write values
                for (int i = 0; i < registerLogs[0].Values.Count; i++)
                {
                    for (int j = 0; j < registerLogs.Length; j++)
                    {
                        if (!registerLogs[j].Print)
                        {
                            continue;
                        }

                        sb.AppendFormat(valueFormat, registerLogs[j].Values[i]);
                        sb.Append(",");
                    }

                    //remove last comma
                    sb.Remove(sb.Length - 1, 1);
                    sb.AppendLine();
                }

                sb.AppendLine();
                File.WriteAllText(validatedPath, sb.ToString());
                Console.WriteLine($"Report saved: {validatedPath}");
            }
            catch (Exception ex)
            {
                string msg = "Could not save report";

                if (!string.IsNullOrWhiteSpace(ex.Message))
                {
                    msg += $": {ex.Message}";
                }

                Console.Error.WriteLine(msg);
            }
        }

        private static void PrintRegisterLogs(
            RegisterLog[] logs,
            bool clearly,
            bool decimalFormat)
        {
            ConsoleColor defaultColor = Console.ForegroundColor;
            string headerFormat = decimalFormat ? RowHeaderFormatDec : RowHeaderFormatHex;
            string valueFormat = decimalFormat ? ValueFormatDec : ValueFormatHex;
            int headerLength = string.Format(headerFormat, 0).Length;
            int valueLength = string.Format(valueFormat, 0).Length + 1;

            for (int i = 0; i < logs.Length; i++)
            {
                if (!logs[i].Print)
                {
                    continue;
                }

                int start = 0;
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.Write(headerFormat, i);
                Console.ForegroundColor = defaultColor;

                if (headerLength + logs[i].Values.Count * valueLength > Console.BufferWidth)
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.Write("...,");
                    Console.ForegroundColor = defaultColor;
                    start = logs[i].Values.Count - ((Console.BufferWidth - 10) / valueLength);
                }

                byte? prev = null;

                for (int j = start; j < logs[i].Values.Count; j++)
                {
                    bool valueChanged = logs[i].Values[j] != prev;
                    prev = logs[i].Values[j];

                    if (clearly && !valueChanged)
                    {
                        Console.Write(string.Empty.PadRight(valueLength));
                        continue;
                    }

                    if (j > start)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        Console.Write(",");
                    }

                    Console.ForegroundColor = valueChanged ? ConsoleColor.Red : ConsoleColor.DarkGray;
                    Console.Write(valueFormat, logs[i].Values[j]);
                    Console.ForegroundColor = defaultColor;
                }

                Console.WriteLine();
            }
        }

        private static IEmbeddedController LoadEC()
        {
            var ecLoader = new FanControlPluginLoader<IEmbeddedController>(FanControl.PluginsDirectory);

            if (ecLoader.FanControlPlugin == null)
            {
                Console.Error.WriteLine("Could not load EC plugin. Try to run ec-probe with elevated privileges.");
                return null;
            }

            ecLoader.FanControlPlugin.Initialize();

            if (ecLoader.FanControlPlugin.IsInitialized)
            {
                return ecLoader.FanControlPlugin;
            }
            else
            {
                Console.Error.WriteLine("EC initialization failed. Try to run ec-probe with elevated privileges.");
                ecLoader.FanControlPlugin.Dispose();
            }

            return null;
        }

        private static void AccessEcSynchronized(Action<IEmbeddedController> callback)
        {
            using (ec = LoadEC())
            {
                if (ec != null)
                {
                    AccessEcSynchronized(callback, ec);
                }
            }
        }

        private static void AccessEcSynchronized(Action<IEmbeddedController> callback, IEmbeddedController ec)
        {
            if (ec.AcquireLock(200))
            {
                try
                {
                    callback(ec);
                }
                finally
                {
                    ec.ReleaseLock();
                }
            }
            else
            {
                Console.Error.WriteLine("Could not acquire EC lock");
            }
        }

        #endregion
    }
}
