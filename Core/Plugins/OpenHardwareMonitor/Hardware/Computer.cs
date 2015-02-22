/*
 
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.
 
  Copyright (C) 2009-2012 Michael Möller <mmoeller@openhardwaremonitor.org>
	
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Security.Permissions;
using System.Reflection;
using System.Security;

namespace OpenHardwareMonitor.Hardware
{
    public class Computer : IComputer
    {
        #region Nested Types

        private class Settings : ISettings
        {
            public bool Contains(string name)
            {
                return false;
            }

            public void SetValue(string name, string value)
            {
            }

            public string GetValue(string name, string value)
            {
                return value;
            }

            public void Remove(string name)
            {
            }
        }

        #endregion

        #region Private Fields

        private readonly List<IGroup> groups = new List<IGroup>();
        private readonly ISettings settings;

        private SMBIOS smbios;

        private bool open;

        private bool mainboardEnabled;
        private bool cpuEnabled;
        private bool ramEnabled;
        private bool gpuEnabled;
        private bool fanControllerEnabled;
        private bool hddEnabled;

        #endregion

        #region Constructor

        public Computer()
        {
            this.settings = new Settings();
        }

        public Computer(ISettings settings)
        {
            this.settings = settings ?? new Settings();
        }

        #endregion

        #region Finalizer

        ~Computer()
        {
            Close();
        }

        #endregion

        #region Events

        public event HardwareEventHandler HardwareAdded;
        public event HardwareEventHandler HardwareRemoved;

        #endregion

        #region Properties

        public bool MainboardEnabled
        {
            get { return mainboardEnabled; }            
            set
            {
                if (open && value != mainboardEnabled)
                {
                    if (value)
                    {
                        AddMainboard();
                    }
                    else
                    {
                        RemoveType<Mainboard.MainboardGroup>();
                    }
                }

                mainboardEnabled = value;
            }
        }

        public bool CPUEnabled
        {
            get { return cpuEnabled; }            
            set
            {
                if (open && value != cpuEnabled)
                {
                    if (value)
                    {
                        AddCPU();
                    }
                    else
                    {
                        RemoveType<CPU.CPUGroup>();
                    }
                }

                cpuEnabled = value;
            }
        }

        public bool RAMEnabled
        {
            get { return ramEnabled; }            
            set
            {
                if (open && value != ramEnabled)
                {
                    if (value)
                    {
                        AddRAM();
                    }
                    else
                    {
                        RemoveType<RAM.RAMGroup>();
                    }
                }

                ramEnabled = value;
            }
        }

        public bool GPUEnabled
        {
            get { return gpuEnabled; }            
            set
            {
                if (open && value != gpuEnabled)
                {
                    if (value)
                    {
                        AddGPU();
                    }
                    else
                    {
                        RemoveType<ATI.ATIGroup>();
                        RemoveType<Nvidia.NvidiaGroup>();
                    }
                }

                gpuEnabled = value;
            }
        }

        public bool FanControllerEnabled
        {
            get { return fanControllerEnabled; }
            set
            {
                if (open && value != fanControllerEnabled)
                {
                    if (value)
                    {
                        AddFanController();
                    }
                    else
                    {
                        RemoveType<TBalancer.TBalancerGroup>();
                        RemoveType<Heatmaster.HeatmasterGroup>();
                    }
                }

                fanControllerEnabled = value;
            }
        }

        public bool HDDEnabled
        {
            get { return hddEnabled; }
            set
            {
                if (open && value != hddEnabled)
                {
                    if (value)
                    {
                        AddHDD();
                    }
                    else
                    {
                        RemoveType<HDD.HarddriveGroup>();
                    }
                }

                hddEnabled = value;
            }
        }

        public IHardware[] Hardware
        {
            get
            {
                List<IHardware> list = new List<IHardware>();

                foreach (IGroup group in this.groups)
                {
                    foreach (IHardware hardware in group.Hardware)
                    {
                        list.Add(hardware);
                    }
                }

                return list.ToArray();
            }
        }

        #endregion

        #region Public Methods
        
        public static void InstallDriver(string directoryPath)
        {
            Ring0.InstallKernelDriver(directoryPath);
        }
        
        public static void UninstallDriver()
        {
            Ring0.UninstallKernelDriver();
        }
        
        public bool WaitIsaBusMutex(int timeout)
        {
            return Ring0.WaitIsaBusMutex(timeout);
        }
        
        public void ReleaseIsaBusMutex()
        {
            Ring0.ReleaseIsaBusMutex();
        }
        
        public void WriteIoPort(int port, byte value)
        {
            if (port < 0)
            {
                throw new ArgumentOutOfRangeException("port", "port must be greater or equal to 0");
            }

            Ring0.WriteIoPort((uint)port, value);
        }
        
        public byte ReadIoPort(int port)
        {
            if (port < 0)
            {
                throw new ArgumentOutOfRangeException("port", "port must be greater or equal to 0");
            }

            return Ring0.ReadIoPort((uint)port);
        }
        
        public void Open()
        {
            if (!open)
            {
                Ring0.Open();
                Opcode.Open();
                open = true;

                if (this.mainboardEnabled)
                {
                    AddMainboard();
                }

                if (this.cpuEnabled)
                {
                    AddCPU();
                }

                if (this.ramEnabled)
                {
                    AddRAM();
                }

                if (this.gpuEnabled)
                {
                    AddGPU();
                }

                if (this.fanControllerEnabled)
                {
                    AddFanController();
                }

                if (this.hddEnabled)
                {
                    AddHDD();
                }
            }
        }
        
        public void Close()
        {
            if (open)
            {
                while (groups.Count > 0)
                {
                    IGroup group = groups[groups.Count - 1];
                    Remove(group);
                }

                Opcode.Close();
                Ring0.Close();

                this.smbios = null;
                open = false;
            }
        }

        public void Accept(IVisitor visitor)
        {
            if (visitor == null)
            {
                throw new ArgumentNullException("visitor");
            }

            visitor.VisitComputer(this);
        }

        public void Traverse(IVisitor visitor)
        {
            foreach (IGroup group in this.groups)
            {
                foreach (IHardware hardware in group.Hardware)
                {
                    hardware.Accept(visitor);
                }
            }
        }

        #endregion

        #region Private Methods

        private void InitializeSMBios()
        {
            if (this.smbios == null)
            {
                this.smbios = new SMBIOS();
            }
        }

        private void AddMainboard()
        {
            InitializeSMBios();
            Add(new Mainboard.MainboardGroup(smbios, settings));
        }

        private void AddCPU()
        {
            InitializeSMBios();
            Add(new CPU.CPUGroup(settings));
        }

        private void AddRAM()
        {
            InitializeSMBios();
            Add(new RAM.RAMGroup(smbios, settings));
        }

        private void AddGPU()
        {
            Add(new ATI.ATIGroup(settings));
            Add(new Nvidia.NvidiaGroup(settings));
        }

        private void AddFanController()
        {
            Add(new TBalancer.TBalancerGroup(settings));
            Add(new Heatmaster.HeatmasterGroup(settings));
        }

        private void AddHDD()
        {
            Add(new HDD.HarddriveGroup(settings));
        }


        private void Add(IGroup group)
        {
            if (!groups.Contains(group))
            {
                groups.Add(group);

                if (HardwareAdded != null)
                {
                    foreach (IHardware hardware in group.Hardware)
                    {
                        HardwareAdded(hardware);
                    }
                }
            }
        }

        private void Remove(IGroup group)
        {
            if (groups.Contains(group))
            {
                groups.Remove(group);

                if (HardwareRemoved != null)
                {
                    foreach (IHardware hardware in group.Hardware)
                    {
                        HardwareRemoved(hardware);
                    }
                }

                group.Close();
            }
        }

        private void RemoveType<T>() where T : IGroup
        {
            for (int i = 0; i < this.groups.Count; i++)
            {
                if (this.groups[i] is T)
                {
                    this.groups.RemoveAt(i);
                }
            }
        }

        private static void NewSection(TextWriter writer)
        {
            for (int i = 0; i < 8; i++)
            {
                writer.Write("----------");
            }

            writer.WriteLine();
            writer.WriteLine();
        }

        private static int CompareSensor(ISensor a, ISensor b)
        {
            int c = a.SensorType.CompareTo(b.SensorType);

            if (c == 0)
            {
                c = a.Index.CompareTo(b.Index);
            }

            return c;
        }

        private static void ReportHardwareSensorTree(IHardware hardware, TextWriter w, string space)
        {
            w.WriteLine("{0}|", space);
            w.WriteLine("{0}+- {1} ({2})", space, hardware.Name, hardware.Identifier);
            ISensor[] sensors = hardware.Sensors;
            Array.Sort(sensors, CompareSensor);

            foreach (ISensor sensor in sensors)
            {
                w.WriteLine(
                    "{0}|  +- {1,-14} : {2,8:G6} {3,8:G6} {4,8:G6} ({5})",
                    space,
                    sensor.Name,
                    sensor.Value,
                    sensor.Min,
                    sensor.Max,
                    sensor.Identifier);
            }

            foreach (IHardware subHardware in hardware.SubHardware)
            {
                ReportHardwareSensorTree(subHardware, w, "|  ");
            }
        }

        private static void ReportHardwareParameterTree(IHardware hardware, TextWriter w, string space)
        {
            w.WriteLine("{0}|", space);
            w.WriteLine("{0}+- {1} ({2})", space, hardware.Name, hardware.Identifier);
            ISensor[] sensors = hardware.Sensors;
            Array.Sort(sensors, CompareSensor);

            foreach (ISensor sensor in sensors)
            {
                string innerSpace = space + "|  ";

                if (sensor.Parameters.Length > 0)
                {
                    w.WriteLine("{0}|", innerSpace);
                    w.WriteLine("{0}+- {1} ({2})", innerSpace, sensor.Name, sensor.Identifier);

                    foreach (IParameter parameter in sensor.Parameters)
                    {
                        string innerInnerSpace = innerSpace + "|  ";
                        w.WriteLine(
                            "{0}+- {1} : {2}",
                            innerInnerSpace, parameter.Name,
                            string.Format(CultureInfo.InvariantCulture, "{0} : {1}",
                            parameter.DefaultValue, parameter.Value));
                    }
                }
            }

            foreach (IHardware subHardware in hardware.SubHardware)
            {
                ReportHardwareParameterTree(subHardware, w, "|  ");
            }
        }

        private static void ReportHardware(IHardware hardware, TextWriter w)
        {
            string hardwareReport = hardware.GetReport();

            if (!string.IsNullOrEmpty(hardwareReport))
            {
                NewSection(w);
                w.Write(hardwareReport);
            }

            foreach (IHardware subHardware in hardware.SubHardware)
            {
                ReportHardware(subHardware, w);
            }
        }

        public string GetReport()
        {
            using (StringWriter w = new StringWriter(CultureInfo.InvariantCulture))
            {
                w.WriteLine();
                w.WriteLine("Open Hardware Monitor Report");
                w.WriteLine();

                Version version = typeof(Computer).Assembly.GetName().Version;

                NewSection(w);
                w.Write("Version: "); w.WriteLine(version.ToString());
                w.WriteLine();

                NewSection(w);
                w.Write("Common Language Runtime: ");
                w.WriteLine(Environment.Version.ToString());
                w.Write("Operating System: ");
                w.WriteLine(Environment.OSVersion.ToString());
                w.Write("Process Type: ");
                w.WriteLine(IntPtr.Size == 4 ? "32-Bit" : "64-Bit");
                w.WriteLine();

                string r = Ring0.GetReport();
                if (r != null)
                {
                    NewSection(w);
                    w.Write(r);
                    w.WriteLine();
                }

                NewSection(w);
                w.WriteLine("Sensors");
                w.WriteLine();
                foreach (IGroup group in groups)
                {
                    foreach (IHardware hardware in group.Hardware)
                    {
                        ReportHardwareSensorTree(hardware, w, "");
                    }
                }
                w.WriteLine();

                NewSection(w);
                w.WriteLine("Parameters");
                w.WriteLine();
                foreach (IGroup group in groups)
                {
                    foreach (IHardware hardware in group.Hardware)
                    {
                        ReportHardwareParameterTree(hardware, w, "");
                    }
                }
                w.WriteLine();

                foreach (IGroup group in groups)
                {
                    string report = group.GetReport();
                    if (!string.IsNullOrEmpty(report))
                    {
                        NewSection(w);
                        w.Write(report);
                    }

                    IHardware[] hardwareArray = group.Hardware;
                    foreach (IHardware hardware in hardwareArray)
                    {
                        ReportHardware(hardware, w);
                    }

                }
                return w.ToString();
            }
        }

        #endregion
    }
}
