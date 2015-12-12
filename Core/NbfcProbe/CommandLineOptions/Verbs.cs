using clipr;

namespace NbfcProbe.CommandLineOptions
{
    [ApplicationInfo(Name = "ec-probe.exe", Description = "NoteBook FanControl EC probe tool.")]
    public class Verbs
    {
        [Verb("read", "Read from EC register")]
        public ECReadVerb ECRead { get; set; }

        [Verb("write", "Write to EC register")]
        public ECWriteVerb ECWrite { get; set; }

        [Verb("dump", "Dump all EC registers")]
        public ECDumpVerb ECDump { get; set; }
    }
}
