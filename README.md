# Irrigation

Application pour avoir une trace des actions pour l'irrigation pour agriculteur.

## Generer Les Livrables

Les commandes suivantes sont a lancer depuis la racine du workspace.

### Executable PC (Windows)

Build desktop:

```powershell
dotnet build IrrigationApp/IrrigationApp.Desktop/IrrigationApp.Desktop.csproj -c Release
```

Generer un executable publie:

```powershell
dotnet publish IrrigationApp/IrrigationApp.Desktop/IrrigationApp.Desktop.csproj -c Release -r win-x64 --self-contained true
```

Sortie principale:

```text
IrrigationApp/IrrigationApp.Desktop/bin/Release/net10.0/win-x64/publish/
```

### APK Android

Generer l'APK:

```powershell
dotnet publish IrrigationApp/IrrigationApp.Android/IrrigationApp.Android.csproj -c Release -f net10.0-android
```

APK generees:

```text
IrrigationApp/IrrigationApp.Android/bin/Release/net10.0-android/com.CompanyName.IrrigationApp-Signed.apk
IrrigationApp/IrrigationApp.Android/bin/Release/net10.0-android/publish/com.CompanyName.IrrigationApp-Signed.apk
```

Option installation via ADB:

```powershell
adb install -r IrrigationApp/IrrigationApp.Android/bin/Release/net10.0-android/publish/com.CompanyName.IrrigationApp-Signed.apk
```
