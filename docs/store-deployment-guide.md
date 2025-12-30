# Store Deployment Guide for "Did I Do That?" App

This comprehensive guide walks you through deploying the app to both Google Play Store and Apple App Store.

---

## Table of Contents

1. [Pre-Deployment Checklist](#pre-deployment-checklist)
2. [Google Play Store Deployment](#google-play-store-deployment)
3. [Apple App Store Deployment](#apple-app-store-deployment)
4. [Post-Deployment Tasks](#post-deployment-tasks)
5. [Updating Your App](#updating-your-app)

---

## Pre-Deployment Checklist

Before submitting to any store, ensure you have:

- [ ] **App Icon**: High-quality app icons for all required sizes
- [ ] **Screenshots**: Device-specific screenshots for store listings
- [ ] **App Description**: Short and full descriptions
- [ ] **Privacy Policy**: Hosted URL for privacy policy (required by both stores)
- [ ] **Content Rating**: Prepared answers for content rating questionnaires
- [ ] **Version Numbers**: Updated version in `DidIDoThatApp.csproj`
- [ ] **Tested**: Thoroughly tested on real devices
- [ ] **All Tests Passing**: Run `dotnet test` to confirm

### Updating Version Numbers

In `DidIDoThatApp/DidIDoThatApp.csproj`:

```xml
<PropertyGroup>
    <!-- Display version shown to users (e.g., "1.0.0") -->
    <ApplicationDisplayVersion>1.0.0</ApplicationDisplayVersion>
    
    <!-- Internal version code (must increment for each release) -->
    <ApplicationVersion>1</ApplicationVersion>
</PropertyGroup>
```

---

## Google Play Store Deployment

### Step 1: Create Google Play Developer Account

1. Go to [play.google.com/console](https://play.google.com/console)
2. Sign in with your Google account
3. Pay the **one-time $25 registration fee**
4. Complete account setup (can take 48 hours for verification)

### Step 2: Create Signing Keys

Android apps must be signed. You have two options:

#### Option A: Let Google Manage Signing (Recommended)

Google Play App Signing manages your app's keys. You just need an upload key:

```bash
# Create upload keystore
keytool -genkey -v -keystore upload-keystore.jks -alias upload -keyalg RSA -keysize 2048 -validity 10000
```

When prompted, enter:
- Keystore password (remember this!)
- Your name, organization, etc.
- Key password (can be same as keystore password)

**⚠️ IMPORTANT**: Back up `upload-keystore.jks` securely. If lost, you cannot update your app!

#### Option B: Manage Your Own Keys (Not Recommended)

Follow the same process but understand you're fully responsible for key security.

### Step 3: Configure the Project for Signing

Add to `DidIDoThatApp.csproj`:

```xml
<PropertyGroup Condition="$(TargetFramework.Contains('-android')) and '$(Configuration)' == 'Release'">
    <AndroidKeyStore>true</AndroidKeyStore>
    <AndroidSigningKeyStore>upload-keystore.jks</AndroidSigningKeyStore>
    <AndroidSigningKeyAlias>upload</AndroidSigningKeyAlias>
    <AndroidSigningKeyPass>YOUR_KEY_PASSWORD</AndroidSigningKeyPass>
    <AndroidSigningStorePass>YOUR_KEYSTORE_PASSWORD</AndroidSigningStorePass>
</PropertyGroup>
```

**For CI/CD, use secrets instead:**

```yaml
# In GitHub Actions workflow
env:
  ANDROID_SIGNING_KEY_PASS: ${{ secrets.ANDROID_KEY_PASSWORD }}
  ANDROID_SIGNING_STORE_PASS: ${{ secrets.ANDROID_KEYSTORE_PASSWORD }}
```

### Step 4: Build Release AAB

Google Play requires **Android App Bundle (AAB)** format:

```bash
dotnet publish DidIDoThatApp/DidIDoThatApp.csproj \
    -f net10.0-android \
    -c Release \
    -p:AndroidPackageFormat=aab \
    -o ./artifacts/android
```

The output will be in `./artifacts/android/com.companyname.dididothatapp-Signed.aab`

### Step 5: Create App in Google Play Console

1. Go to **Play Console** → **Create app**
2. Fill in:
   - **App name**: Did I Do That?
   - **Default language**: English (US)
   - **App or game**: App
   - **Free or paid**: Free (or Paid)
3. Accept policies and click **Create app**

### Step 6: Complete Store Listing

Navigate to **Grow** → **Store presence** → **Main store listing**:

| Field | Value |
|-------|-------|
| **App name** | Did I Do That? |
| **Short description** | Track recurring maintenance tasks and never miss a deadline. |
| **Full description** | (See below) |
| **App icon** | 512x512 PNG |
| **Feature graphic** | 1024x500 PNG |
| **Screenshots** | At least 2 phone screenshots (16:9 or 9:16) |

**Suggested Full Description:**
```
Did I Do That? helps you stay on top of recurring maintenance tasks for your home, car, pets, and more.

Features:
• Create recurring tasks with customizable frequencies (daily, weekly, monthly, yearly)
• Organize tasks into categories like Home, Car, Personal, Pet, and Business
• Get timely reminders before tasks are due
• Track completion history for all your tasks
• See overdue and upcoming tasks at a glance on your dashboard

Stop wondering when you last changed your oil, replaced your air filters, or gave your pet their medication. Did I Do That? keeps track so you don't have to.

Perfect for:
🏠 Home maintenance (HVAC filters, smoke detector batteries, gutter cleaning)
🚗 Vehicle care (oil changes, tire rotations, inspections)
🐕 Pet care (medications, vet visits, grooming)
💼 Business tasks (license renewals, equipment maintenance)
👤 Personal care (doctor appointments, prescription refills)
```

### Step 7: Complete App Content

Navigate to **Policy** → **App content** and complete:

1. **Privacy policy**: Add your privacy policy URL
2. **Ads declaration**: Declare if app contains ads
3. **App access**: "All functionality is available without login"
4. **Content ratings**: Complete the questionnaire (likely "Everyone" rating)
5. **Target audience**: Select appropriate age groups
6. **News apps**: "Not a news app"
7. **Data safety**: Declare data collection practices

**Data Safety Declaration for this app:**
- Data collected: None shared with third parties
- Data stored locally on device only
- No account required
- Data can be deleted by uninstalling app

### Step 8: Set Up Testing Tracks

Before releasing to production, use testing tracks:

1. **Internal testing**: Up to 100 testers, instant rollout
2. **Closed testing**: Invite-only, for broader testing
3. **Open testing**: Anyone can join via link

Navigate to **Release** → **Testing** → **Internal testing**:
1. Click **Create new release**
2. Upload your AAB file
3. Add release notes
4. Save and review

### Step 9: Submit for Production

Once testing is complete:

1. Go to **Release** → **Production**
2. Click **Create new release**
3. Upload your AAB (or promote from testing track)
4. Write release notes
5. Click **Review release**
6. Click **Start rollout to Production**

**Review Timeline**: 
- First submission: 3-7 days
- Updates: Usually 1-3 days

---

## Apple App Store Deployment

### Step 1: Create Apple Developer Account

1. Go to [developer.apple.com/programs](https://developer.apple.com/programs)
2. Click **Enroll**
3. Sign in with your Apple ID
4. Pay the **$99/year membership fee**
5. Complete enrollment (can take 1-2 days for verification)

### Step 2: Prepare Certificates and Profiles

See [iOS Build Guide](ios-build-guide.md) for detailed instructions on:
- Creating Distribution certificates
- Creating App Store provisioning profiles
- Configuring code signing

### Step 3: Create App in App Store Connect

1. Go to [appstoreconnect.apple.com](https://appstoreconnect.apple.com)
2. Click **Apps** → **+** → **New App**
3. Fill in:
   - **Platforms**: iOS
   - **Name**: Did I Do That?
   - **Primary language**: English (US)
   - **Bundle ID**: com.companyname.dididothatapp
   - **SKU**: dididothat-001 (unique identifier)
   - **User Access**: Full Access

### Step 4: Complete App Information

Navigate to your app and fill in:

#### App Information
| Field | Value |
|-------|-------|
| **Subtitle** | Maintenance Task Tracker |
| **Category** | Productivity |
| **Secondary Category** | Lifestyle |
| **Content Rights** | Does not contain third-party content |
| **Age Rating** | 4+ (complete questionnaire) |

#### Pricing and Availability
- Set price tier (Free or choose price)
- Select availability by country/region

#### App Privacy
- Privacy Policy URL (required)
- Data collection declarations

### Step 5: Prepare App Store Listing

In **App Store** → **iOS App** → your version:

| Field | Value |
|-------|-------|
| **Screenshots** | 6.5" and 5.5" iPhone required |
| **Promotional Text** | (optional, can change anytime) |
| **Description** | (same as Google Play) |
| **Keywords** | maintenance, tasks, reminders, recurring, home, car, pet |
| **Support URL** | Your support webpage |
| **Marketing URL** | (optional) |

**Screenshot Requirements:**
- 6.7" iPhone (1290 x 2796 px)
- 6.5" iPhone (1284 x 2778 px) - **Required**
- 5.5" iPhone (1242 x 2208 px) - **Required**
- iPad Pro 12.9" (2048 x 2732 px) - If supporting iPad

### Step 6: Build for App Store

On your Mac:

```bash
# Build IPA for App Store
dotnet publish DidIDoThatApp/DidIDoThatApp.csproj \
    -f net10.0-ios \
    -c Release \
    -r ios-arm64 \
    -p:ArchiveOnBuild=true \
    -p:RuntimeIdentifier=ios-arm64 \
    -p:CodesignKey="Apple Distribution: Your Name (XXXXXXXXXX)" \
    -p:CodesignProvision="Your App Store Profile"
```

### Step 7: Upload to App Store Connect

#### Option A: Using Transporter App (Recommended)
1. Download **Transporter** from Mac App Store
2. Sign in with your Apple ID
3. Drag and drop your IPA file
4. Click **Deliver**

#### Option B: Using Xcode
1. Open Xcode
2. Go to **Window** → **Organizer**
3. Select your archive
4. Click **Distribute App**
5. Choose **App Store Connect**
6. Follow the wizard

### Step 8: Submit for Review

1. In App Store Connect, go to your app
2. Select the build you uploaded
3. Complete all required metadata
4. Answer export compliance questions:
   - "Does your app use encryption?" → No (unless you added it)
5. Click **Submit for Review**

**Review Timeline:**
- Average: 24-48 hours
- First submission: Up to 7 days
- Rejections require resubmission

### Common App Store Rejection Reasons

| Reason | Solution |
|--------|----------|
| **Crashes or bugs** | Test thoroughly before submission |
| **Incomplete metadata** | Fill in all required fields |
| **Privacy policy missing** | Add hosted privacy policy URL |
| **Notification permission** | Explain why notifications are needed |
| **Background modes** | Justify background task usage in review notes |

**Review Notes for This App:**
> This app uses background fetch to recalculate notification times for recurring maintenance tasks. Users can disable notifications in Settings. Local notifications remind users of upcoming and overdue tasks.

---

## Post-Deployment Tasks

### Monitor Your App

**Google Play Console:**
- Check **Quality** → **Android vitals** for crashes
- Monitor **Statistics** for downloads and ratings
- Respond to reviews

**App Store Connect:**
- Check **Analytics** for impressions and downloads
- Monitor **App Analytics** for crashes
- Respond to reviews

### Set Up Crash Reporting

Consider adding crash reporting:
- [App Center](https://appcenter.ms) (Microsoft, free tier available)
- [Firebase Crashlytics](https://firebase.google.com/products/crashlytics) (Google, free)
- [Sentry](https://sentry.io) (cross-platform)

---

## Updating Your App

### Version Number Updates

For each update, increment:
1. `ApplicationDisplayVersion` (user-visible, e.g., "1.0.0" → "1.1.0")
2. `ApplicationVersion` (internal, must be unique and incrementing)

```xml
<ApplicationDisplayVersion>1.1.0</ApplicationDisplayVersion>
<ApplicationVersion>2</ApplicationVersion>
```

### Release Notes

Write clear, user-friendly release notes:

```
What's New in Version 1.1.0:

🎉 New Features
• Added task search functionality
• New category icons

🐛 Bug Fixes
• Fixed notification timing issue
• Improved app startup performance

Thank you for using Did I Do That?
```

### Update Submission

**Google Play:**
1. Go to **Release** → **Production** → **Create new release**
2. Upload new AAB
3. Add release notes
4. Submit for review

**App Store:**
1. Create new version in App Store Connect
2. Upload new IPA via Transporter
3. Update "What's New" text
4. Submit for review

---

## Checklist Summary

### Google Play Store
- [ ] Developer account ($25 one-time)
- [ ] Keystore created and backed up
- [ ] App signed and built as AAB
- [ ] Store listing complete with screenshots
- [ ] Privacy policy URL added
- [ ] Content rating completed
- [ ] Data safety declaration completed
- [ ] Tested on internal track
- [ ] Submitted for review

### Apple App Store
- [ ] Developer account ($99/year)
- [ ] Certificates and profiles created
- [ ] App built and signed for App Store
- [ ] Store listing complete with screenshots
- [ ] Privacy policy URL added
- [ ] Age rating completed
- [ ] Export compliance answered
- [ ] Submitted for review

---

## Resources

- [Google Play Console Help](https://support.google.com/googleplay/android-developer)
- [App Store Connect Help](https://developer.apple.com/app-store-connect/)
- [.NET MAUI Deployment Guide](https://learn.microsoft.com/dotnet/maui/deployment/)
- [Android App Bundle Guide](https://developer.android.com/guide/app-bundle)
- [iOS App Distribution](https://developer.apple.com/documentation/xcode/distributing-your-app-for-beta-testing-and-releases)
