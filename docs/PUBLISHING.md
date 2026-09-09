# Publishing

## Release checklist

1. Confirm `Version` in `CrossServerPortals.csproj` matches `version_number` in `manifest.json`.
2. Run `mise run check` against current Valheim/BepInEx assemblies.
3. Smoke-test direct login and portal travel on a disposable server/world.
4. Commit, tag `v<version>`, and push the tag.
5. Attach `artifacts/ValheimCrossServerPortals-v<version>.zip` to the GitHub release.
6. Upload the same ZIP to the Valheim community on Thunderstore.

## First Thunderstore publication

1. Sign in at <https://thunderstore.io> using GitHub.
2. Publish under the `jcruse03` team/namespace.
3. Create a team service account and supply its token through `TCLI_AUTH_TOKEN`;
   never commit or print the token.
4. Run `mise run check` and publish the generated ZIP with Thunderstore's official
   `tcli` using the settings in `thunderstore.toml`.
5. Verify the live package metadata, dependency, README, and downloadable archive.

The initial package passed direct login and two-way Kujamaton/Everville portal
traversal before publication.

Keep the title, description, and first README paragraph explicit that this is a community
continuation/fork of lunar91's original GPL-3.0 project. Do not imply endorsement by lunar91.
