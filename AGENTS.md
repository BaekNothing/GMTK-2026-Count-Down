# COUNT DOWN agent release contract

Any agent that changes playable code, scenes, project settings, packages,
resources, or deployment tooling must finish the task with this release
sequence unless the user explicitly requests local-only or no-publish work:

1. Inspect `git status` and preserve unrelated user changes.
2. Run the local Unity runtime validation:
   `Unity.exe -batchmode -projectPath <repo> -executeMethod CountDownValidation.ValidateRuntimeAssembly -logFile <repo>/countdown-validation-final.log`
3. Run the local Unity WebGL production build:
   `Unity.exe -batchmode -projectPath <repo> -executeMethod WebGLProjectSetup.Build -logFile <repo>/countdown-build.log -quit`
4. Confirm both logs report success and `Builds/WebGL/index.html` exists.
5. Stage only the intended files, commit them, and push the current branch.
6. Run `Tools/Publish-Itch.ps1` to publish the exact successful WebGL output.
7. Report the commit, pushed branch, build result, and itch.io channel/version.

Never place Unity credentials or the itch.io API key in tracked files. Local
Unity Personal activation is authoritative for builds. itch.io credentials
must come from `BUTLER_API_KEY` or butler's local credential store, and the
destination must come from `ITCH_TARGET` or an explicit script argument.

If itch.io credentials or the target are missing, complete validation, build,
commit, and push, then report publishing as the only blocker with the exact
missing variable. Do not claim that a release was published.
