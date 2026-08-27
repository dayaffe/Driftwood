using System;
using System.Globalization;
using System.Text;

namespace Driftwood.Identity
{
    public sealed class IdentityClaimRequest
    {
        private IdentityClaimRequest(Uri endpoint, byte[] body, string clientId, string steamId)
        {
            Endpoint = endpoint;
            Body = body;
            ClientId = clientId;
            SteamId = steamId;
        }

        public Uri Endpoint { get; }
        public byte[] Body { get; }
        public string ClientId { get; }
        public string SteamId { get; }

        public static IdentityClaimRequest Create(
            string host,
            int gamePort,
            int clientId,
            ulong steamId,
            string personaName)
        {
            if (string.IsNullOrWhiteSpace(host)
                || host.IndexOfAny(new[] { '/', '\\', '\r', '\n', '\t' }) >= 0)
            {
                throw new ArgumentException("Host must be a hostname or IP address.", nameof(host));
            }
            if (gamePort < 1 || gamePort >= 65535)
                throw new ArgumentOutOfRangeException(nameof(gamePort));
            if (clientId < 0)
                throw new ArgumentOutOfRangeException(nameof(clientId));
            if (steamId == 0UL)
                throw new ArgumentOutOfRangeException(nameof(steamId));

            string client = clientId.ToString(CultureInfo.InvariantCulture);
            string steam = steamId.ToString(CultureInfo.InvariantCulture);
            string json = "{\"clientId\":\"" + client
                + "\",\"steamId\":\"" + steam
                + "\",\"name\":\"" + EscapeJson(personaName ?? string.Empty) + "\"}";
            Uri endpoint = new UriBuilder("http", host.Trim(), gamePort + 1, "/api/v1/identity").Uri;
            return new IdentityClaimRequest(endpoint, Encoding.UTF8.GetBytes(json), client, steam);
        }

        private static string EscapeJson(string value)
        {
            StringBuilder result = new StringBuilder(value.Length + 8);
            foreach (char c in value)
            {
                switch (c)
                {
                    case '"': result.Append("\\\""); break;
                    case '\\': result.Append("\\\\"); break;
                    case '\b': result.Append("\\b"); break;
                    case '\f': result.Append("\\f"); break;
                    case '\n': result.Append("\\n"); break;
                    case '\r': result.Append("\\r"); break;
                    case '\t': result.Append("\\t"); break;
                    default:
                        if (char.IsControl(c))
                            result.Append("\\u").Append(((int)c).ToString("x4", CultureInfo.InvariantCulture));
                        else
                            result.Append(c);
                        break;
                }
            }
            return result.ToString();
        }
    }

    public interface IIdentityClaimSender
    {
        bool TrySend(IdentityClaimRequest request, out string? failure);
    }

    public sealed class IdentityClaimCoordinator
    {
        private readonly IIdentityClaimSender _sender;

        public IdentityClaimCoordinator(IIdentityClaimSender sender)
        {
            _sender = sender ?? throw new ArgumentNullException(nameof(sender));
        }

        public bool ClaimBeforeSpawn(
            string host,
            int gamePort,
            int clientId,
            ulong steamId,
            string personaName,
            out string? failure)
        {
            IdentityClaimRequest request;
            try
            {
                request = IdentityClaimRequest.Create(host, gamePort, clientId, steamId, personaName);
            }
            catch (Exception exception)
            {
                failure = exception.Message;
                return false;
            }
            return _sender.TrySend(request, out failure);
        }
    }
}
