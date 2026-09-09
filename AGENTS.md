# AGENTS.md

## Project

Community-maintained Valheim 1.0 continuation of lunar91's Cross Server Portals.

## Routine commands

- `mise run test` — parser unit tests
- `mise run build` — release build against `.game/`
- `mise run package` — Thunderstore-ready archive
- `mise run check` — tests plus package build

Do not change the plugin GUID or assembly name: both are intentionally retained for
drop-in compatibility. Keep upstream attribution prominent. Never deploy to a live server
from a build target; deployment is a separate, explicit operational step.
