# Security Policy

## Reporting a vulnerability

Report privately through GitHub's private vulnerability reporting:

**https://github.com/HumanGenome/Driftwood/security/advisories/new**

There is no security mailing address for this project. Please do not open a public issue for a
vulnerability.

## What this repo is

Driftwood is the hub: this repo publishes the Windows installer players run, plus documentation.
The dedicated server package has its own policy at
[DriftwoodServer](https://github.com/HumanGenome/DriftwoodServer/security/policy).

## In scope

- Anything that lets a published Driftwood download be replaced, spoofed or downgraded: the
  installer, the auto-update package, or the signed update manifest the app polls.
- Signature or version checks in the app's updater that can be bypassed, including a manifest that
  a launcher accepts without validating its signature against the key the launcher embeds.
- Code execution reachable from a server address a player is told to add — anything a hostile
  server can do to a client that connects to it.
- Local privilege escalation through the installer, or through a path the app writes.

## Out of scope

- Cheating, item duplication or other in-game exploits in How to Fish itself. Report those to the
  game's developer.
- Vulnerabilities in the game's own code, in BepInEx, or in Avalonia. Report those upstream.
- A server operator doing anything they like to their own server.
- Denial of service against a server you have been given the address of.
