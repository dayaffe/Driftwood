using System;
using System.Collections;
using System.IO;
using System.Net;
using BepInEx;
using FishNet;
using FishNet.Managing.Client;
using Steamworks;
using UnityEngine;

namespace Driftwood.Identity
{
    [BepInPlugin(Guid, "Driftwood Steam Identity", Version)]
    [BepInDependency("com.humangenome.driftwood.connect", BepInDependency.DependencyFlags.HardDependency)]
    public sealed class IdentityClaimPlugin : BaseUnityPlugin
    {
        public const string Guid = "com.dayaffe.driftwood.identity";
        public const string Version = "0.1.0";
        private const ulong FirstIndividualSteamId = 76561197960265728UL;
        private ClientManager? _client;
        private bool _claimAttempted;
        private bool _claimSucceeded;
        private string? _failure;

        private void Awake()
        {
            StartCoroutine(AttachWhenReady());
        }

        private IEnumerator AttachWhenReady()
        {
            while (InstanceFinder.ClientManager == null) yield return null;
            _client = InstanceFinder.ClientManager;
            _client.OnAuthenticated += OnAuthenticated;
            Logger.LogInfo("Steam identity support is ready.");
        }

        private void OnDestroy()
        {
            if (_client != null) _client.OnAuthenticated -= OnAuthenticated;
        }

        // OnAuthenticated runs after FishNet assigns Connection.ClientId and before it
        // registers scene objects. The game's Client component can only begin its spawn
        // coroutine after that registration, so this synchronous POST finishes first.
        private void OnAuthenticated()
        {
            _claimAttempted = true;
            _claimSucceeded = false;
            _failure = null;
            try
            {
                int clientId = _client == null || _client.Connection == null
                    ? -1
                    : _client.Connection.ClientId;
                ulong steamId = SteamUser.GetSteamID().m_SteamID;
                if (steamId < FirstIndividualSteamId)
                {
                    Fail("Steam did not provide an individual-account SteamID64.");
                    return;
                }

                IdentityClaimCoordinator coordinator = new IdentityClaimCoordinator(new HttpIdentityClaimSender());
                string connectConfig = Path.Combine(Paths.ConfigPath, "com.humangenome.driftwood.connect.cfg");
                ConnectTarget target = ConnectTarget.Parse(File.ReadAllText(connectConfig));
                string persona = SteamFriends.GetPersonaName() ?? string.Empty;
                if (!coordinator.ClaimBeforeSpawn(target.Host, target.GamePort, clientId, steamId, persona, out string? failure))
                {
                    Fail(failure ?? "The identity request failed.");
                    return;
                }

                _claimSucceeded = true;
                Logger.LogInfo("Claimed Steam identity " + steamId + " for FishNet connection " + clientId + ".");
            }
            catch (Exception exception)
            {
                Fail(exception.GetType().Name + ": " + exception.Message);
            }
        }

        internal bool SpawnMayProceed(out string? failure)
        {
            if (_claimSucceeded)
            {
                failure = null;
                return true;
            }
            failure = _claimAttempted
                ? (_failure ?? "The Steam identity claim failed.")
                : "The Steam identity claim has not completed yet.";
            return false;
        }

        private void Fail(string reason)
        {
            _failure = reason;
            Logger.LogError("Steam identity claim failed before player spawn: " + reason);
        }

        private sealed class HttpIdentityClaimSender : IIdentityClaimSender
        {
            public bool TrySend(IdentityClaimRequest claim, out string? failure)
            {
                try
                {
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(claim.Endpoint);
                    request.Method = "POST";
                    request.ContentType = "application/json";
                    request.ContentLength = claim.Body.Length;
                    request.Timeout = 3000;
                    request.ReadWriteTimeout = 3000;
                    request.KeepAlive = false;
                    request.Proxy = null;
                    using (Stream body = request.GetRequestStream())
                        body.Write(claim.Body, 0, claim.Body.Length);
                    using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                    {
                        int status = (int)response.StatusCode;
                        if (status < 200 || status >= 300)
                        {
                            failure = "The server returned HTTP " + status + ".";
                            return false;
                        }
                    }
                    failure = null;
                    return true;
                }
                catch (Exception exception)
                {
                    failure = exception.Message;
                    return false;
                }
            }
        }
    }
}
