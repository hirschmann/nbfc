using System;
using System.Globalization;
using System.Linq;

namespace NbfcCli
{
    public static class Parser
    {
        delegate Tuple<bool, T> TryParseAction<T>(string s);

        public static bool TryGetOption(string[] args, string[] aliases, out int value)
        {
            TryParseAction<int> action =
                s =>
                {
                    int v;
                    bool b = int.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out v);
                    return new Tuple<bool, int>(b, v);
                };

            return TryGetOption(action, args, aliases, out value);
        }

        public static bool TryGetOption(string[] args, string[] aliases, out string value)
        {
            TryParseAction<string> action =
                s =>
                {
                    return new Tuple<bool, string>(true, s);
                };

            return TryGetOption(action, args, aliases, out value);
        }

        public static bool TryGetOption(string[] args, string[] aliases, out float value)
        {
            TryParseAction<float> action =
                s =>
                {
                    float v;
                    bool b = float.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out v);
                    return new Tuple<bool, float>(b, v);
                };

            return TryGetOption(action, args, aliases, out value);
        }

        private static bool TryGetOption<T>(TryParseAction<T> action, string[] args, string[] aliases, out T value)
        {
            for (int i = 0; i < args.Length - 1; i++)
            {
                string s = args[i].Trim();

                if (aliases.Any(x => x.Equals(s, StringComparison.OrdinalIgnoreCase)))
                {
                    var result = action(args[i + 1]);
                    value = result.Item2;

                    return result.Item1;
                }
            }

            value = default(T);
            return false;
        }
    }
}
