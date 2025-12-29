# iOS Build Guide for "Did I Do That?" App

This guide walks you through building the iOS version of the app on a macOS machine.

## Prerequisites

### 1. macOS Environment
- **macOS Ventura (13.0)** or later
- **Xcode 15** or later (available from the Mac App Store)
- At least **50GB of free disk space** for Xcode and SDKs

### 2. Apple Developer Account
- An **Apple Developer account** is required for device testing and App Store distribution
- Enroll at [developer.apple.com](https://developer.apple.com)
- Cost: $99/year for individuals

### 3. .NET MAUI Workload
Install .NET 10 SDK and the MAUI workload:

```bash
# Install .NET 10 SDK
# Download from https://dotnet.microsoft.com/download/dotnet/10.0

# Install MAUI workloads
dotnet workload install maui-ios
```

## Setting Up Your Development Environment

### Step 1: Clone the Repository

```bash
git clone https://github.com/YOUR_USERNAME/DidIDoThatApp.git
cd DidIDoThatApp
```

### Step 2: Install Xcode Command Line Tools

```bash
xcode-select --install
```

### Step 3: Accept Xcode License

```bash
sudo xcodebuild -license accept
```

### Step 4: Verify .NET MAUI Installation

```bash
dotnet --list-sdks
dotnet workload list
```

You should see `maui-ios` in the installed workloads.

## Building for iOS Simulator

For development and testing without a physical device:

```bash
# Build for iOS Simulator (Debug)
dotnet build DidIDoThatApp/DidIDoThatApp.csproj -f net10.0-ios -c Debug

# Run on iOS Simulator
dotnet build DidIDoThatApp/DidIDoThatApp.csproj -f net10.0-ios -c Debug -t:Run -p:_DeviceName=:v2:runtime=com.apple.CoreSimulator.SimRuntime.iOS-17-0
```

### Common Simulator Targets

| Device | Simulator Command |
|--------|-------------------|
| iPhone 15 | `iossimulator-arm64` |
| iPhone 15 Pro | `iossimulator-arm64` |
| iPhone SE | `iossimulator-arm64` |
| iPad Pro | `iossimulator-arm64` |

## Building for Physical Device

### Step 1: Create Signing Certificates

1. Open **Xcode** → **Settings** → **Accounts**
2. Add your Apple ID
3. Click **Manage Certificates**
4. Create an **Apple Development** certificate

### Step 2: Create App ID

1. Go to [developer.apple.com/account](https://developer.apple.com/account)
2. Navigate to **Certificates, Identifiers & Profiles**
3. Click **Identifiers** → **+**
4. Select **App IDs** → Continue
5. Enter:
   - Description: `Did I Do That`
   - Bundle ID (Explicit): `com.companyname.dididothatapp`
6. Enable required capabilities:
   - **Push Notifications** (if using remote notifications)
   - **Background Modes** (select "Background fetch" and "Background processing")

### Step 3: Create Provisioning Profile

1. In the Developer Portal, go to **Profiles** → **+**
2. Select **iOS App Development**
3. Choose your App ID
4. Select your certificate and devices
5. Download and double-click to install

### Step 4: Configure the Project

Update `DidIDoThatApp.csproj` if needed:

```xml
<PropertyGroup Condition="$(TargetFramework.Contains('-ios'))">
    <CodesignKey>Apple Development: Your Name (XXXXXXXXXX)</CodesignKey>
    <CodesignProvision>Your Provisioning Profile Name</CodesignProvision>
</PropertyGroup>
```

### Step 5: Build and Deploy

```bash
# Build for device (Debug)
dotnet build DidIDoThatApp/DidIDoThatApp.csproj -f net10.0-ios -c Debug -r ios-arm64

# Build for device (Release)
dotnet build DidIDoThatApp/DidIDoThatApp.csproj -f net10.0-ios -c Release -r ios-arm64
```

## Building for App Store

### Step 1: Create Distribution Certificate

1. Open **Keychain Access** → **Certificate Assistant** → **Request a Certificate from a Certificate Authority**
2. Enter your email and select **Saved to disk**
3. In Developer Portal, go to **Certificates** → **+**
4. Select **Apple Distribution**
5. Upload your CSR file
6. Download and double-click to install

### Step 2: Create App Store Provisioning Profile

1. In Developer Portal, go to **Profiles** → **+**
2. Select **App Store Connect**
3. Choose your App ID
4. Select your distribution certificate
5. Download and install

### Step 3: Build for App Store

```bash
# Create IPA for App Store
dotnet publish DidIDoThatApp/DidIDoThatApp.csproj \
    -f net10.0-ios \
    -c Release \
    -r ios-arm64 \
    -p:ArchiveOnBuild=true \
    -p:CodesignKey="Apple Distribution: Your Name (XXXXXXXXXX)" \
    -p:CodesignProvision="Your Distribution Profile" \
    -o ./artifacts/ios
```

## Info.plist Configuration

The `Platforms/iOS/Info.plist` file has been configured with:

```xml
<!-- Background fetch for notification recalculation -->
<key>UIBackgroundModes</key>
<array>
    <string>fetch</string>
    <string>processing</string>
</array>

<!-- Background task identifier -->
<key>BGTaskSchedulerPermittedIdentifiers</key>
<array>
    <string>com.dididothat.notificationrecalc</string>
</array>
```

## Notification Permissions

The app requests notification permissions on first launch. For iOS 10+, this is handled automatically by the `Plugin.LocalNotification` package.

## Troubleshooting

### "No valid iOS code signing keys found"

```bash
# Open Xcode and sign in with your Apple ID
# Or import certificates from another machine
security import ~/Downloads/Certificates.p12 -P password
```

### "The code signature version is no longer supported"

Update to the latest Xcode version or add to your `.csproj`:

```xml
<PropertyGroup Condition="$(TargetFramework.Contains('-ios'))">
    <UseModernBuildSystem>true</UseModernBuildSystem>
</PropertyGroup>
```

### Simulator Won't Start

```bash
# Reset iOS Simulator
xcrun simctl erase all

# Or boot a specific simulator
xcrun simctl boot "iPhone 15"
```

### Build Fails with Provisioning Profile Errors

1. Revoke all existing certificates in Developer Portal
2. Delete all certificates from Keychain
3. In Xcode, go to Settings → Accounts → Download Manual Profiles
4. Rebuild

## CI/CD for iOS

To add iOS builds to GitHub Actions, you'll need:
1. A **macOS runner** (GitHub-hosted `macos-latest` or self-hosted)
2. Certificates and provisioning profiles as **secrets**
3. Apple ID credentials for notarization (optional)

Example workflow addition (requires secrets setup):

```yaml
# Add to .github/workflows/release-build.yml
jobs:
  build-ios:
    runs-on: macos-latest
    steps:
    - uses: actions/checkout@v4
    - uses: actions/setup-dotnet@v4
      with:
        dotnet-version: '10.0.x'
    - run: dotnet workload install maui-ios
    - run: dotnet build -f net10.0-ios -c Release
```

## Resources

- [.NET MAUI Documentation](https://learn.microsoft.com/dotnet/maui/)
- [Apple Developer Documentation](https://developer.apple.com/documentation/)
- [iOS Code Signing Guide](https://help.apple.com/xcode/mac/current/#/dev80cc24546)
- [MAUI iOS Deployment](https://learn.microsoft.com/dotnet/maui/ios/deployment/)
