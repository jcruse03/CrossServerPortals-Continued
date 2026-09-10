# Changelog

## 1.3.2 - 2026-09-10

- Recommend and link the optional PortalPass companion for seamless travel between
  password-protected servers.
- Documentation-only package update. The included plugin DLL is unchanged from the
  tested 1.3.1 release; there are no gameplay or configuration changes.

## 1.3.1 - 2026-09-09

- Replace the inherited package icon with an original two-world portal emblem.
- No gameplay or configuration changes.

## 1.3.0 - 2026-09-09

- Add Valheim 1.0 compatibility.
- Replace background-thread delays with Unity coroutines.
- Keep players at the calculated safe portal exit after loading.
- Restrict portal tracking to the server and compare stable ZDO identifiers.
- Improve portal-tag validation, including world names with spaces and bracketed IPv6.
- Default omitted server ports to 2456.
- Update particle APIs and improve visual-effect null safety.
- Add automated parser tests and reproducible release packaging.

## Earlier releases

Versions 0.1.0 through 1.2.0 were released by lunar91. See the preserved Git history and
[upstream repository](https://github.com/lunar91/CrossServerPortals) for their full notes.
