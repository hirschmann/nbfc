using clipr;

namespace NbfcProbe.CommandLineOptions
{
    [ApplicationInfo(Name = "nbfc-probe.exe", Description = "NoteBook FanControl probe tool.")]
    public class Verbs
    {
        [Verb("ec-read", "Read from EC registers")]
        public ECReadVerb ECRead { get; set; }

        [Verb("ec-write", "Write to EC registers")]
        public ECWriteVerb ECWrite { get; set; }

        [Verb("ec-dump", "Dump all EC registers")]
        public ECDumpVerb ECDump { get; set; }
    }
}
