using clipr;

namespace NbfcProbe.CommandLineOptions
{
    [ApplicationInfo(Name = "ec-probe.exe", Description = "NoteBook FanControl EC probing tool")]
    public class Verbs
    {
        [Verb("dump", "Dump all EC registers")]
        public ECDumpVerb ECDump { get; set; }

        [Verb("read", "Read a byte from a EC register")]
        public ECReadVerb ECRead { get; set; }

        [Verb("write", "Write a byte to a EC register")]
        public ECWriteVerb ECWrite { get; set; }

        [Verb("monitor", "Monitor all EC registers for changes")]
        public ECMonitorVerb ECMonitor { get; set; }
    }
}
