using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace Driftwood.Identity
{
    public sealed class ConnectTarget
    {
        private ConnectTarget(string host, int gamePort)
        {
            Host = host;
            GamePort = gamePort;
        }

        public string Host { get; }
        public int GamePort { get; }

        public static ConnectTarget Parse(string text)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));
            Dictionary<string, string> values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            string section = string.Empty;
            using (StringReader reader = new StringReader(text))
            {
                for (;;)
                {
                    string? line = reader.ReadLine();
                    if (line is null) break;
                    string trimmed = line.Trim();
                    if (trimmed.Length == 0 || trimmed[0] == '#' || trimmed[0] == ';') continue;
                    if (trimmed[0] == '[' && trimmed[trimmed.Length - 1] == ']')
                    {
                        section = trimmed.Substring(1, trimmed.Length - 2).Trim();
                        continue;
                    }
                    if (!section.Equals("Client", StringComparison.OrdinalIgnoreCase)) continue;
                    int equals = trimmed.IndexOf('=');
                    if (equals <= 0) continue;
                    values[trimmed.Substring(0, equals).Trim()] = trimmed.Substring(equals + 1).Trim();
                }
            }

            int port;
            if (!values.TryGetValue("ConnectAddress", out string? host) || string.IsNullOrWhiteSpace(host))
                throw new InvalidDataException("ConnectAddress is missing from the Driftwood target.");
            if (!values.TryGetValue("ConnectPort", out string? portText)
                || !int.TryParse(portText, NumberStyles.None, CultureInfo.InvariantCulture, out port)
                || port < 1 || port >= 65535)
                throw new InvalidDataException("ConnectPort is missing or invalid in the Driftwood target.");
            return new ConnectTarget(host.Trim(), port);
        }
    }
}
