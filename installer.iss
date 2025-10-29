; Inno Setup Script for Mongolian Phonetic Keyboard
; Download Inno Setup from: https://jrsoftware.org/isdl.php

#define MyAppName "Mongolian Phonetic Keyboard"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "Amgaa G."
#define MyAppExeName "MongolianPhonetic.exe"

[Setup]
AppId={{8F9D5C2E-1A3B-4F7E-9D2C-5E8A9B3C7D1F}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
AllowNoIcons=yes
LicenseFile=LICENSE.txt
OutputDir=installer
OutputBaseFilename=MongolianPhoneticKeyboard-Setup-{#MyAppVersion}
Compression=lzma
SolidCompression=yes
WizardStyle=modern
PrivilegesRequired=lowest
; SetupIconFile=icon.ico
UninstallDisplayIcon={app}\{#MyAppExeName}

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "autostart"; Description: "Start with Windows"; GroupDescription: "Additional options:"

[Files]
Source: "publish\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "Launch {#MyAppName}"; Flags: nowait postinstall skipifsilent

[Registry]
Root: HKCU; Subkey: "Software\Microsoft\Windows\CurrentVersion\Run"; ValueType: string; ValueName: "MongolianPhonetic"; ValueData: """{app}\{#MyAppExeName}"""; Flags: uninsdeletevalue; Tasks: autostart

[Code]
function InitializeSetup(): Boolean;
var
  ResultCode: Integer;
begin
  // Check if already running
  if CheckForMutexes('MongolianPhoneticKeyboard') then
  begin
    if MsgBox('Mongolian Phonetic Keyboard is currently running. Would you like to close it and continue with installation?',
              mbConfirmation, MB_YESNO) = IDYES then
    begin
      // Kill the process
      Exec('taskkill', '/F /IM MongolianPhonetic.exe', '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
      Result := True;
    end
    else
      Result := False;
  end
  else
    Result := True;
end;
