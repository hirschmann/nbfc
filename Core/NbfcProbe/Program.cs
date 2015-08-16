using clipr;
using NbfcProbe.CommandLineOptions;
using StagWare.FanControl;
using StagWare.FanControl.Plugins;
using System;
using System.Text;

namespace NbfcProbe
{
    public class Program
    {
        #region Main

        static void Main(string[] args)
        {
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
            var parser = new CliParser<Verbs>(opt, ParserOptions.CaseInsensitive, new CustomHelpGenerator());
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
            else
            {
                Console.WriteLine((new CustomHelpGenerator()).GetHelp(null));
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
                StringBuilder sb = new StringBuilder(16 * 54);

                // Read all register bytes
                for (int i = 0; i <= 0xF0; i += 0x10)
                {
                    sb.AppendFormat("{0:X2}: ", i);

                    for (int j = 0; j <= 0xF; j++)
                    {
                        byte b = ec.ReadByte((byte)(i + j));
                        sb.AppendFormat("{0:X2} ", b);
                    }

                    sb.AppendLine();
                }

                Console.WriteLine(sb);
            });
        }

        private static void AccessEcSynchronized(Action<IEmbeddedController> callback)
        {
            var ecLoader = new FanControlPluginLoader<IEmbeddedController>(FanControl.PluginsDirectory);
            IEmbeddedController ec = ecLoader.FanControlPlugin;
            ec.Initialize();

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
                Console.Error.WriteLine("Error connecting to Embedded Controller");
            }
        }

        private static void PrintHelp()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("Usage: nbfc-probe [-h|--help] <command> [<args>]");
            sb.AppendLine();
            sb.AppendLine("Commands:");
            sb.AppendLine(" * ec-dump");
            sb.AppendLine("    Dump all registers that are present in EC memory as hexadecimal table.");
            sb.AppendLine(" * ec-read <register>");
            sb.AppendLine("    Print the value of the given EC register number.");
            sb.AppendLine(" * ec-write <register> <value> [-v|--verbose]");
            sb.AppendLine("    Write the given value to the given EC register number.");
            sb.AppendLine("    If the verbose option is set, read and print the updated value.");
            sb.AppendLine();
            sb.AppendLine("Input values are interpreted as decimal numbers by default.");
            sb.AppendLine("Hexadecimal values may be entered by prefixing them with \"0x\".");

            Console.WriteLine(sb);
        }

        #endregion
    }
}
