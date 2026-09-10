# Cross Server Portals Continued

Connect Valheim servers and local worlds through ordinary portals.

> **Community-maintained fork:** This project is a Valheim 1.0-compatible continuation of
> [lunar91/lunarbin's Cross Server Portals](https://github.com/lunar91/CrossServerPortals).
> The original design and implementation belong to lunar91 and contributors. This fork is
> distributed under the same GPL-3.0 license and is not an official lunar91 release.

## Recommended companion: PortalPass

For seamless travel between password-protected servers, pair this mod with
[PortalPass on Thunderstore](https://thunderstore.io/c/valheim/p/jcruse03/PortalPass/)
([source on GitHub](https://github.com/jcruse03/PortalPass)). PortalPass is an
optional client-only companion that supplies passwords from a private file the
player controls, so cross-server portal trips do not stop at a password prompt.
It never records or learns passwords automatically.

## Compatibility

- Valheim 1.0 / dedicated server network version 39
- BepInExPack Valheim 5.4.2350 or newer compatible release
- Install on every client and server participating in cross-server travel

The plugin GUID and DLL name remain unchanged so existing configuration is retained when
upgrading from the original mod. Remove the original DLL before installing this package;
do not run both versions together.

## Installation

Copy `ValheimCrossServerPortals.dll` into `BepInEx/plugins/CrossServerPortals/` on each
client and server.

## Portal tags

Use one of these formats:

```text
SourceTag|server.example.com:2456|TargetTag
SourceTag|192.0.2.10:2456
SourceTag|[2001:db8::10]:2456|TargetTag
SourceTag|world:My Local World|TargetTag
```

- **SourceTag** identifies the departure portal.
- **TargetTag** is optional. On arrival, the mod looks for either that ordinary tag or a
  cross-server portal whose source tag matches it.
- A server address without a port uses Valheim's default port, `2456`.
- IPv6 addresses with a port must use brackets.
- `world:` destinations switch to a local world by name.

Cross-server portals ignore Valheim's ordinary item teleport restrictions, matching the
behavior of the original mod.

## Configuration

- `PreserveStatusEffects` — preserve effects such as rested and wet across a mod-driven transfer.
- `RecolorPortalGlyphs` / `CustomPortalGlyphColor` — identify cross-server portals visually.
- `RecolorPortalEffects` / `CustomPortalEffectColor` — customize their particle effects.
- `PromptBeforeTeleport` — request confirmation before leaving the current server.
- `RequireAdminToRename` — server-synchronized restriction for cross-server portal tags.
- `LockAdminConfig` — lock synchronized administrator configuration.

## What changed in this continuation

- Restored Valheim 1.0 compatibility using the new flattened portal-list API.
- Replaced asynchronous thread-pool delays with Unity coroutines.
- Preserved the calculated safe exit position after the destination finishes loading.
- Limited periodic portal enumeration to servers and compare portals by stable ZDO ID.
- Reworked portal-tag validation for safer DNS, port, world-name, and IPv6 handling.
- Updated deprecated particle-system calls and improved null safety.
- Added repeatable Linux builds, parser tests, CI, and Thunderstore-ready packaging.

See [CHANGELOG.md](CHANGELOG.md) for release history and [docs/PUBLISHING.md](docs/PUBLISHING.md)
for maintainer release instructions.

## Building

Set `VALHEIM_GAME_PATH` to a Valheim installation containing BepInEx, then:

```bash
dotnet test tests/CrossServerPortals.Tests.csproj
dotnet build CrossServerPortals.csproj -c Release
dotnet msbuild CrossServerPortals.csproj -t:Package -p:Configuration=Release
```

The Thunderstore-ready archive is written to `artifacts/`.

## Credits and license

- [lunar91](https://github.com/lunar91) — original author and architecture
- [Wacky-Mole](https://github.com/Wacky-Mole) — original admin permission work
- [ServerSync](https://github.com/blaxxun-boop/ServerSync) — server-authoritative configuration

Licensed under [GPL-3.0](LICENSE). Git history from the upstream repository is preserved.
