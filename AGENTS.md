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
7. If controls or player-facing behavior changed, update `ITCH_PAGE.md`.
   Do not use browser automation for itch.io pages or devlogs. Record patch
   details in Git commits and publish builds only through `Tools/Publish-Itch.ps1`.
8. Report the commit, pushed branch, build result, and itch.io channel/version.

Do not use browser automation for build validation, gameplay checks, visual QA,
itch.io pages, or devlogs. Rely on Unity validation and build logs for automated
checks. When a browser-only or visual confirmation is still useful, hand that
check to the user with concise reproduction steps.

Never place Unity credentials or the itch.io API key in tracked files. Local
Unity Personal activation is authoritative for builds. itch.io credentials
and destination may be read from the Git-ignored `.env.itch.local`, process
environment variables, butler's local credential store, or an explicit script
argument. `.env.itch.example` is the only dotenv file that may be committed.

If itch.io credentials or the target are missing, complete validation, build,
commit, and push, then report publishing as the only blocker and point to
`.env.itch.example`. Do not claim that a release was published.
