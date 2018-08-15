/*
 
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.
 
  Copyright (C) 2010-2016 Michael Möller <mmoeller@openhardwaremonitor.org>
	
*/

using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Threading;
using System.Text;
using System.Diagnostics;

namespace OpenHardwareMonitor.Hardware
{
    internal static class Ring0
    {
        private const string KernelDriverId = "WinRing0_1_2_0";

        private static KernelDriver driver;
        private static string fileName;
        private static Mutex isaBusMutex;
        private static readonly StringBuilder report = new StringBuilder();
        private static readonly string driverFileName = OperatingSystem.Is64BitOperatingSystem()
            ? "WinRing0x64.sys"
            : "WinRing0.sys";

        private const uint OLS_TYPE = 40000;
        private static IOControlCode
          IOCTL_OLS_GET_REFCOUNT = new IOControlCode(OLS_TYPE, 0x801,
            IOControlCode.Access.Any),
          IOCTL_OLS_GET_DRIVER_VERSION = new IOControlCode(OLS_TYPE, 0x800,
            IOControlCode.Access.Any),
          IOCTL_OLS_READ_MSR = new IOControlCode(OLS_TYPE, 0x821,
            IOControlCode.Access.Any),
          IOCTL_OLS_WRITE_MSR = new IOControlCode(OLS_TYPE, 0x822,
            IOControlCode.Access.Any),
          IOCTL_OLS_READ_IO_PORT_BYTE = new IOControlCode(OLS_TYPE, 0x833,
            IOControlCode.Access.Read),
          IOCTL_OLS_WRITE_IO_PORT_BYTE = new IOControlCode(OLS_TYPE, 0x836,
            IOControlCode.Access.Write),
          IOCTL_OLS_READ_PCI_CONFIG = new IOControlCode(OLS_TYPE, 0x851,
            IOControlCode.Access.Read),
          IOCTL_OLS_WRITE_PCI_CONFIG = new IOControlCode(OLS_TYPE, 0x852,
            IOControlCode.Access.Write),
          IOCTL_OLS_READ_MEMORY = new IOControlCode(OLS_TYPE, 0x841,
            IOControlCode.Access.Read);

        private static string GetDriverFileName(string directoryName)
        {
            return Path.Combine(directoryName, driverFileName);
        }

        private static bool ExtractDriver(string fileName)
        {
            string resourceName = "OpenHardwareMonitor.Hardware." + driverFileName;
            byte[] buffer = null;

            foreach (string name in Assembly.GetExecutingAssembly().GetManifestResourceNames())
            {
                if (name.Replace('\\', '.') == resourceName)
                {
                    using (Stream stream = Assembly.GetExecutingAssembly().
                      GetManifestResourceStream(name))
                    {
                        buffer = new byte[stream.Length];
                        stream.Read(buffer, 0, buffer.Length);
                    }
                }
            }

            if (buffer == null)
            {
                return false;
            }

            try
            {
                using (FileStream target = new FileStream(fileName, FileMode.Create))
                {
                    target.Write(buffer, 0, buffer.Length);
                    target.Flush();
                }
            }
            catch (IOException)
            {
                // for example there is not enough space on the disk
                return false;
            }

            // make sure the file is actually writen to the file system
            for (int i = 0; i < 20; i++)
            {
                try
                {
                    if (File.Exists(fileName) && new FileInfo(fileName).Length == buffer.Length)
                    {
                        return true;
                    }

                    Thread.Sleep(100);
                }
                catch (IOException)
                {
                    Thread.Sleep(10);
                }
            }

            // file still has not the right size, something is wrong
            return false;
        }

        public static void Open()
        {
			System.PlatformID p = Environment.OSVersion.Platform;

            // try loading shipped kernel drivers on Unix
			// (will probably only work on Linux through)
			if (p == PlatformID.Unix) {
				// try loading the `msr` kernel module on Linux (required on ost kernels)
				Process modprobe = new Process();
				try {
					modprobe.StartInfo.FileName  = "modprobe";
					modprobe.StartInfo.Arguments = "msr";
					modprobe.Start();
					modprobe.WaitForExit();
				} catch(Exception e) {
					report.AppendLine(string.Format("Failed to load `msr` kernel driver: {0}", e.Message));
				}

				return;
			}

			// following code is Windows NT only
			if ((p != PlatformID.Win32NT) && (p != PlatformID.WinCE)) {
				return;
			}

            if (driver != null)
                return;

            // clear the current report
            report.Length = 0;

            InitializeKernelDriver();

            if (!driver.IsOpen)
            {
                // driver is not loaded, try to install and open
                InstallKernelDriverCore(
                    Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
            }

            if (!driver.IsOpen)
                driver = null;

            string mutexName = "Global\\Access_ISABUS.HTP.Method";
            try
            {
                isaBusMutex = new Mutex(false, mutexName);
            }
            catch (UnauthorizedAccessException)
            {
                try
                {
                    isaBusMutex = Mutex.OpenExisting(mutexName, MutexRights.Synchronize);
                }
                catch { }
            }
        }

        private static void InitializeKernelDriver()
        {
            driver = new KernelDriver(KernelDriverId);
            driver.Open();
        }

        private static void InstallKernelDriverCore(string directoryPath)
        {
            fileName = GetDriverFileName(directoryPath);

            if (fileName != null && ExtractDriver(fileName))
            {
                string installError;
                if (driver.Install(fileName, out installError))
                {
                    driver.Open();

                    if (!driver.IsOpen)
                    {
                        driver.Delete();
                        report.AppendLine("Status: Opening driver failed after install");
                    }
                }
                else
                {
                    string errorFirstInstall = installError;

                    // install failed, try to delete and reinstall
                    driver.Delete();

                    // wait a short moment to give the OS a chance to remove the driver
                    Thread.Sleep(2000);

                    string errorSecondInstall;
                    if (driver.Install(fileName, out errorSecondInstall))
                    {
                        driver.Open();

                        if (!driver.IsOpen)
                        {
                            driver.Delete();
                            report.AppendLine(
                              "Status: Opening driver failed after reinstall");
                        }
                    }
                    else
                    {
                        report.AppendLine("Status: Installing driver \"" +
                          fileName + "\" failed" +
                          (File.Exists(fileName) ? " and file exists" : ""));
                        report.AppendLine("First Exception: " + errorFirstInstall);
                        report.AppendLine("Second Exception: " + errorSecondInstall);
                    }
                }
            }
            else
            {
                try
                {
                    // try to delete the driver file
                    if (File.Exists(fileName))
                        File.Delete(fileName);
                    fileName = null;
                }
                catch (IOException) { }
                catch (UnauthorizedAccessException) { }

                report.AppendLine("Status: Extracting driver failed");
            }
        }

        public static void InstallKernelDriver(string directoryPath)
        {
            InitializeKernelDriver();
            InstallKernelDriverCore(directoryPath);
        }

        public static void UninstallKernelDriver()
        {
            InitializeKernelDriver();

            if (driver != null)
            {
                try
                {
                    if (driver.IsOpen)
                    {
                        driver.Close();
                    }

                    driver.Delete();
                    driver = null;
                }
                catch
                {
                    report.AppendLine("Status: Uninstalling driver failed");
                }
            }
        }

        public static bool IsOpen
        {
            get { return driver != null; }
        }

        public static void Close()
        {
            if (driver != null)
            {
                driver.Close();
                driver = null;
            }

            if (isaBusMutex != null)
            {
                isaBusMutex.Close();
                isaBusMutex = null;
            }
        }
        public static ulong ThreadAffinitySet(ulong mask)
        {
            return ThreadAffinity.Set(mask);
        }

        public static string GetReport()
        {
            if (report.Length > 0)
            {
                StringBuilder r = new StringBuilder();
                r.AppendLine("Ring0");
                r.AppendLine();
                r.Append(report);
                r.AppendLine();
                return r.ToString();
            }
            else
                return null;
        }

        public static bool WaitIsaBusMutex(int millisecondsTimeout)
        {
            if (isaBusMutex == null)
                return true;
            try
            {
                return isaBusMutex.WaitOne(millisecondsTimeout, false);
            }
            catch (AbandonedMutexException) { return true; }
            catch (InvalidOperationException) { return false; }
        }

        public static void ReleaseIsaBusMutex()
        {
            if (isaBusMutex == null)
                return;
            isaBusMutex.ReleaseMutex();
        }

		private static bool rdmsrLinux(uint index, out ulong value, uint cpuNumber=0U)
		{
			// Load mono's posix assembly
			Assembly posix = Assembly.Load("Mono.Posix");
			if (posix.EntryPoint != null) {
				posix.EntryPoint.Invoke(posix, new object[] {});
			}

			// Acquire references to stuff required to create and open block devices
			Type FilePermissions = posix.GetType("Mono.Unix.Native.FilePermissions");
			Type OpenFlags       = posix.GetType("Mono.Unix.Native.OpenFlags");
			Type Syscall         = posix.GetType("Mono.Unix.Native.Syscall");
			MethodInfo mknod = Syscall.GetMethod("mknod", new Type[] { typeof(string), FilePermissions, typeof(ulong) });
			MethodInfo open  = Syscall.GetMethod("open",  new Type[] { typeof(string), OpenFlags });

			// Acquire references to stuff required to interact with a file descriptor
			Type UnixStream         = posix.GetType("Mono.Unix.UnixStream");
			MethodInfo readAtOffset = UnixStream.GetMethod("ReadAtOffset", new Type[] { typeof(byte[]), typeof(int), typeof(int), typeof(long) });
			MethodInfo dispose      = UnixStream.GetMethod("Dispose",      new Type[] {});

			// Determine file path of the MSR block device node for the requested CPU core
			string msrFile = String.Format("/dev/cpu/{0}/msr", cpuNumber);

			// Open the MSR block device node for the requested CPU core
			int fd = (int)open.Invoke(null, new object[] {msrFile, 0});
			if (fd < 0)
			{
				if (Marshal.GetLastWin32Error() == 2) { // ENOENT (No such file or directory)
					// Device node might be missing (broken udev)
					if ((int)mknod.Invoke(null, new object[] {msrFile, (uint)(0600 | 0x0100), (ulong)(202 << 8 | (cpuNumber & 0xFF))}) >= 0) {
						// Node successfully created, retry opening it
						fd = (int)open.Invoke(null, new object[] {msrFile, 0});
					}
				}
			}

			if (fd >= 0) {
				byte[] buffer = new byte[8];

				// Read requested data from file descriptor
				object stream = Activator.CreateInstance(UnixStream, new object[] { fd });
				readAtOffset.Invoke(stream, new object[] { buffer, 0, buffer.Length, index });
				dispose.Invoke(stream, new object[] {}); // This also closes the file descriptor

				// Convert byte stream to number (byte-order should not be an issue)
				value = BitConverter.ToUInt64(buffer, 0);
				return true;
			}

			value = 0;
			return false;
		}

		private static bool rdmsrWindows(uint index, out ulong value, ulong threadAffinityMask=0) {
			value = 0;
			if (driver == null)
			{
				return false;
			}

			// Force current thread to run on the requested CPU core
			// This probably works because the kernel processes short running requests on
			// the CPU thread where they originated from to improve efficiency. While this
			// is probably a likely hack, changing this behavior would require significant
			// changes to the kernel driver. 
			ulong mask = 0xFFFFFFFFFFFFFFFF;
			if (threadAffinityMask != 0) {
				mask = ThreadAffinity.Set(threadAffinityMask);
			}

			try {
				// Communicate with WinRing0 driver
				return driver.DeviceIOControl(IOCTL_OLS_READ_MSR, index, ref value);
			} finally {
				if (threadAffinityMask != 0) {
					ThreadAffinity.Set(mask);
				}
			}
		}

		/**
		 * Read MSR value from any CPU core
		 * 
		 * On Linux this will read the MSR value from the first core of
		 * the first CPU, on Windows the used CPU and core are not defined.
		 * 
		 * @param index The index of the MSR value to read
		 * @param eax   Upper 32-bits of MSR value at the given index
		 * @param edx   Lower 32-bits of MSR value at the given index
		 */
        public static bool Rdmsr(uint index, out uint eax, out uint edx)
        {
			return Rdmsr(index, out eax, out edx, -1);
        }

		/**
		 * Read MSR value from the given CPU core
		 * 
		 * @param index The index of the MSR value to read
		 * @param eax   Upper 32-bits of MSR value at the given index
		 * @param edx   Lower 32-bits of MSR value at the given index
		 * @param cpuNum The CPU thread number to read the MSR value from
		 */
		public static bool Rdmsr(uint index, out uint eax, out uint edx, int cpuNum)
		{
			bool result = false;
			ulong value = 0UL;

			if (Environment.OSVersion.Platform == PlatformID.Unix) {
				// Use first CPU thread (0) if no specific CPU thread number was given under Linux
				result = rdmsrLinux(index, out value, (cpuNum >= 0) ? ((uint)cpuNum) : 0U);
			} else {
				// Switch current process thread to the given CPU thread number and hope we get
				// the right result
				result = rdmsrWindows(index, out value, (cpuNum >= 0) ? (1UL << cpuNum) : 0UL);
			}

			// Split result into the different registers
			edx = (uint)((value >> 32) & 0xFFFFFFFF);
			eax = (uint)((value >>  0) & 0xFFFFFFFF);
			return result;
		}

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct WrmsrInput
        {
            public uint Register;
            public ulong Value;
        }
    	public static bool RdmsrTx(uint index, out uint eax, out uint edx, ulong threadAffinityMask)
	{
            ulong mask = ThreadAffinity.Set(threadAffinityMask);

	    bool result = Rdmsr(index, out eax, out edx);

	    ThreadAffinity.Set(mask);
	    return result;
        }

        public static bool Wrmsr(uint index, uint eax, uint edx)
        {
            if (driver == null)
                return false;

            WrmsrInput input = new WrmsrInput();
            input.Register = index;
            input.Value = ((ulong)edx << 32) | eax;

            return driver.DeviceIOControl(IOCTL_OLS_WRITE_MSR, input);
        }

        public static byte ReadIoPort(uint port)
        {
            if (driver == null)
                return 0;

            uint value = 0;
            driver.DeviceIOControl(IOCTL_OLS_READ_IO_PORT_BYTE, port, ref value);

            return (byte)(value & 0xFF);
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct WriteIoPortInput
        {
            public uint PortNumber;
            public byte Value;
        }

        public static void WriteIoPort(uint port, byte value)
        {
            if (driver == null)
                return;

            WriteIoPortInput input = new WriteIoPortInput();
            input.PortNumber = port;
            input.Value = value;

            driver.DeviceIOControl(IOCTL_OLS_WRITE_IO_PORT_BYTE, input);
        }

        public const uint InvalidPciAddress = 0xFFFFFFFF;

        public static uint GetPciAddress(byte bus, byte device, byte function)
        {
            return
              (uint)(((bus & 0xFF) << 8) | ((device & 0x1F) << 3) | (function & 7));
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct ReadPciConfigInput
        {
            public uint PciAddress;
            public uint RegAddress;
        }

        public static bool ReadPciConfig(uint pciAddress, uint regAddress,
          out uint value)
        {
            if (driver == null || (regAddress & 3) != 0)
            {
                value = 0;
                return false;
            }

            ReadPciConfigInput input = new ReadPciConfigInput();
            input.PciAddress = pciAddress;
            input.RegAddress = regAddress;

            value = 0;
            return driver.DeviceIOControl(IOCTL_OLS_READ_PCI_CONFIG, input,
              ref value);
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct WritePciConfigInput
        {
            public uint PciAddress;
            public uint RegAddress;
            public uint Value;
        }

        public static bool WritePciConfig(uint pciAddress, uint regAddress,
          uint value)
        {
            if (driver == null || (regAddress & 3) != 0)
                return false;

            WritePciConfigInput input = new WritePciConfigInput();
            input.PciAddress = pciAddress;
            input.RegAddress = regAddress;
            input.Value = value;

            return driver.DeviceIOControl(IOCTL_OLS_WRITE_PCI_CONFIG, input);
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct ReadMemoryInput
        {
            public ulong address;
            public uint unitSize;
            public uint count;
        }

        public static bool ReadMemory<T>(ulong address, ref T buffer)
        {
            if (driver == null)
            {
                return false;
            }

            ReadMemoryInput input = new ReadMemoryInput();
            input.address = address;
            input.unitSize = 1;
            input.count = (uint)Marshal.SizeOf(buffer);

            return driver.DeviceIOControl(IOCTL_OLS_READ_MEMORY, input,
              ref buffer);
        }
    }
}
