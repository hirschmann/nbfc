using System;
using StagWare.FanControl;
using StagWare.FanControl.Plugins;
using System.IO;
using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace NbfcProbe
{
	class MainClass
	{
		private readonly IEmbeddedController ec;

		public MainClass()
		{
			var ecLoader = new FanControlPluginLoader<IEmbeddedController>(FanControl.PluginsDirectory);
			this.ec = ecLoader.FanControlPlugin;
			this.ec.Initialize();
		}

		public int ProcessArguments(string[] args) {
			// Make errors printed by EC plugin visible
			var traceListener = new ConsoleTraceListener(true);
			Debug.Listeners.Add(traceListener);

			var converter = new System.ComponentModel.ByteConverter();
			try {
				if (args.Length > 0 && args[0] == "ec-write") {
					if (args.Length == 3) {
						return this.ECWrite(
							(byte)converter.ConvertFromString(args[1]),
							(byte)converter.ConvertFromString(args[2])
						);
					} else {
						Console.Error.WriteLine("Action `ec-write` requires exactly 2 arguments");
						return 2;
					}
				} else if (args.Length > 0 && args[0] == "ec-read") {
					if (args.Length == 2) {
						return this.ECRead((byte)converter.ConvertFromString(args[1]));
					} else {
						Console.Error.WriteLine("Action `ec-read` requires exactly 1 arguments");
						return 2;
					}
				} else if (args.Length > 0 && args[0] == "ec-dump") {
					if (args.Length == 1) {
						return this.ECDump();
					} else {
						Console.Error.WriteLine("Action `ec-dump` requires no arguments");
						return 2;
					}
				} else {
					int returnValue = 0;
					TextWriter console = Console.Out;
					if (args.Length > 0 && (args[0] != "--help" || args[0] != "-h")) {
						console = Console.Error;

						console.WriteLine("Unknown action `{0}`!", args[0]);
						console.WriteLine();

						returnValue = 2;
					}

					console.WriteLine("Usage: nbfc-probe [-h|--help] [ec-dump|ec-read|ec-write] {arguments}");
					console.WriteLine();
					console.WriteLine("Possible modes:");
					console.WriteLine(" * ec-dump");
					console.WriteLine("    Dump all registers that are present in EC memory as hexadecimal table.");
					console.WriteLine(" * ec-read [register]");
					console.WriteLine("    Print the value of the given EC register number.");
					console.WriteLine(" * ec-write [register] [value]");
					console.WriteLine("    Write the given value to the given EC RAM register number,");
					console.WriteLine("    then read the given value from the EC and print the new value.");
					console.WriteLine();
					console.WriteLine("All numbers are in decimal format by default; hexadecimal values may be");
					console.WriteLine("entered by prefixing them with \"0x\".");

					return returnValue;
				}
			} finally {
				Debug.Listeners.Remove(traceListener);
			}
		}

		private delegate int withECDelegate();
		private int withEC(withECDelegate callback) {
			if (this.ec.AcquireLock(200)) {
				try {
					return callback();
				} finally {
					this.ec.ReleaseLock();
				}
			} else {
				Console.Error.WriteLine("Error connecting to Embedded Controller");
				return 3;
			}
		}

		public int ECWrite(byte register, byte value)
		{
			return this.withEC(delegate {
				// Write something
				Console.WriteLine("Writing at {0}: {1} (0x{1:X2})", register, value);
				this.ec.WriteByte(register, value);

				// Read back the value to check if it was written successfully
				byte b = this.ec.ReadByte(register);
				Console.WriteLine("Value: {0:D3} (0x{0:X2})", b); 

				Console.WriteLine();

				return 0;
			});
		}

		public int ECRead(byte register)
		{
			return this.withEC(delegate {
				// Read the requested value
				byte b = this.ec.ReadByte(register);
				Console.WriteLine("Value: {0:D3} (0x{0:X2})", b);

				return 0;
			});
		}

		public int ECDump()
		{
			return this.withEC(delegate {
                StringBuilder sb = new StringBuilder(16 * 54);

				// Read all register bytes
                for (int i = 0; i <= 0xF0; i += 0x10)
                {
                    sb.AppendFormat("{0:X2}: ", i);

                    for (int j = 0; j <= 0xF; j++)
                    {
                        byte b = this.ec.ReadByte((byte)(i + j));
                        sb.AppendFormat("{0:X2} ", b);
                    }

                    sb.AppendLine();
                }

                Console.WriteLine(sb);
				return 0;
			});
		}

		public static int Main(string[] args)
		{
			MainClass main = new MainClass();
			return main.ProcessArguments(args);
		}
	}
}
