# Android maintenance release (October 2026)

This release exists to maintain the Google Play listing and developer-account activity. Avoid feature changes.

## Changes
- Application display version 1.0.1; Android version code 12 (confirm 12 is unused in Play Console).
- Stable .NET 10 MAUI/EF Core package updates; Plugin.LocalNotification is intentionally held at 13.0.0 because 14.x breaks the existing NotificationService API.
- CI validates pull requests with an unsigned Android Release build; signing and AAB upload run on pushes to main only.

## Release verification (manual)
1. Confirm developer account email and telephone verification in Play Console and read the exact inactivity notice.
2. Check Play Console's highest previously uploaded Android version code; if 12 already exists, increment ApplicationVersion before merging.
3. Verify produced bundle targets API 36 or higher and meets Play Console requirements. The project does not explicitly pin a target SDK; inspect compiled output and Play Console feedback.
4. Run existing tests, build the Android Release, then install the app on an Android emulator or physical device.
5. Test initial startup; create/edit/delete tasks; close/reopen the app; verify database data persists.
6. Test permission prompt and local notification scheduling, delivery, rescheduling and cancellation on Android 13+; specifically verify behavior after reboot when practical. The notification plugin stays on 13.0.0, so confirm existing notification behavior is unchanged.
7. After CI passes and user review, merge the PR. Download the signed .aab artifact from the **main branch** release workflow run (PR runs intentionally do not expose signing secrets).
8. Submit the AAB using the existing Play App Signing identity. Confirm Google Play accepts it and that the account warning resolves. Do not assume that uploading alone meets all account-verification requirements.

## Scope/limitations
NuGet versions were changed statically. The first CI attempt failed due to breaking LocalNotification 14.x APIs, so it was reverted to 13.0.0; CI must validate the corrected commit. No Android emulator, Play Console submission, or signed AAB validation was performed by the PR author.
