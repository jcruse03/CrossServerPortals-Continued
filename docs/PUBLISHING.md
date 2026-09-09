# Publishing

## Release checklist

1. Confirm `Version` in `CrossServerPortals.csproj` matches `version_number` in `manifest.json`.
2. Run `mise run check` against current Valheim/BepInEx assemblies.
3. Smoke-test direct login and portal travel on a disposable server/world.
4. Commit, tag `v<version>`, and push the tag.
5. Attach `artifacts/ValheimCrossServerPortals-v<version>.zip` to the GitHub release.
6. Upload the same ZIP to the Valheim community on Thunderstore.

## First Thunderstore publication

1. Sign in at <https://thunderstore.io> using Discord.
2. Create or choose a team/namespace that Jim controls.
3. Open the Valheim community's package upload page.
4. Upload the generated ZIP and verify the dependency and README preview.
5. Publish only after the Kujamaton client/server portal test succeeds.

Keep the title, description, and first README paragraph explicit that this is a community
continuation/fork of lunar91's original GPL-3.0 project. Do not imply endorsement by lunar91.
