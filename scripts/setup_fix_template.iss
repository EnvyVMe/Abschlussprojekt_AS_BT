//!!! Hier anpassen !!!
#define ef3Version "3.9.056"

//!!! Hier anpassen !!!
#define ef3FixFrom "09"
#define ef3FixTo "09"

#define logfile "ef3_fix"

//!!! Hier anpassen !!!
#define app_path "K:\Exe\ef\3.9\3.9.056\efcom"

// -------------------------------------------------------------------------------------------
[Setup]
// -------------------------------------------------------------------------------------------
AppName=ef3
AppMutex=ef, efr2
SetupLogging=yes

AppVersion={#ef3Version}.{#ef3FixFrom}_{#ef3FixTo}

DefaultGroupName=ef3
VersionInfoCompany=efcom GmbH
DefaultDirName={sd}\efcom\Test
// -------------------------------------------------------------------------------------------

// -------------------------------------------------------------------------------------------
[Files]
// -------------------------------------------------------------------------------------------
//!!! Hier anpassen !!!
// Fixe
// Source: "{#app_path}\*.pbd"; DestDir: "{app}"; Flags: ignoreversion;
Source: "{#app_path}\3.9\Source\Fix_09\efmodkvd.pbd"; DestDir: "{app}"; Flags: ignoreversion

// -------------------------------------------------------------------------------------------

// -------------------------------------------------------------------------------------------
[Registry]
// -------------------------------------------------------------------------------------------
Root: HKLM; Subkey: "{code:KeyPath}"; ValueType: string; ValueName: {code:KeyNameFix}; ValueData: "{#ef3FixTo}"; Check: not IsWin64; Flags:uninsdeletevalue
Root: HKLM64; Subkey: "{code:KeyPath}"; ValueType: string; ValueName: {code:KeyNameFix}; ValueData: "{#ef3FixTo}"; Check: IsWin64; Flags:uninsdeletevalue
// -------------------------------------------------------------------------------------------

// -------------------------------------------------------------------------------------------
[Languages]
// -------------------------------------------------------------------------------------------
Name: "de"; MessagesFile: "compiler:Languages/German.isl"
Name: "en"; MessagesFile: "compiler:Default.isl"
// -------------------------------------------------------------------------------------------

// -------------------------------------------------------------------------------------------
[CustomMessages]
// -------------------------------------------------------------------------------------------
de.VersionFalsch_1=Es ist eine falsche Version (
en.VersionFalsch_1=There is a wrong version (
de.VersionFalsch_2=) installiert in "
en.VersionFalsch_2=) installed in "
de.VersionFalsch_3=". Das Fix kann nicht installiert werden, da es fuer Version 
en.VersionFalsch_3=". The fix can not be installed because it is determined for version 
de.VersionFalsch_4= bestimmt ist.
en.VersionFalsch_4=.
de.FixFalscheVersion_1=Es ist Fix 
en.FixFalscheVersion_1=Fix 
de.FixFalscheVersion_2= installiert. Diese Routine installiert jedoch Fix 
en.FixFalscheVersion_2= is installed. This routine installs fix 
de.FixFalscheVersion_3= bis Fix 
en.FixFalscheVersion_3= to fix 
de.FixFalscheVersion_4=.
en.FixFalscheVersion_4=.
de.VersionFehlend=An diesem Ort existiert noch keine ef3 Version, die gefixt werden koennte.
en.VersionFehlend=There is no ef3 version at this place which could be fixed.
// -------------------------------------------------------------------------------------------

// -------------------------------------------------------------------------------------------
[Code]
// -------------------------------------------------------------------------------------------
var
  s_KeyPathEf3, s_KeyPath, s_KeyNameVersion, s_KeyNameFix, s_Path: string;
  i_RegRoot: integer;

// ---------------------------------------------------------------------
// Initialisierung aller Variablen
// ---------------------------------------------------------------------
procedure InitializeVariables();
begin
	s_KeyPathEf3 := 'SOFTWARE\efcom\ef3\';
	s_KeyNameVersion := 'Version';
	s_KeyNameFix := 'Fix';
	if IsWin64 then begin
		i_RegRoot := HKLM64;
	end
	else
		i_RegRoot := HKLM;
end;
// ---------------------------------------------------------------------

// ---------------------------------------------------------------------
// Aus einem Pfad wird ein Schlüssel erzeugt
// ---------------------------------------------------------------------
function PathKey(param: string): string;
var
  ls_Path: string;
begin
  ls_Path := s_Path;
  StringChangeEx(ls_Path, ':', '', True);
  StringChangeEx(ls_Path, '\', '_', True);
  Result := ls_Path;
end;
// ---------------------------------------------------------------------

// ---------------------------------------------------------------------
// Der Pfad der Logdatei wird zurückgegeben
// ---------------------------------------------------------------------
function LogFile(param: string): string;
begin
  Result := s_Path + '\{#logfile}_{#ef3Version}.{#ef3FixFrom}_{#ef3FixTo}.log';
end;
// ---------------------------------------------------------------------

// ---------------------------------------------------------------------
// Beendet die Anwendung
// ---------------------------------------------------------------------
procedure ExitProcess(exitCode: integer);
  external 'ExitProcess@kernel32.dll stdcall';
// ---------------------------------------------------------------------

// ---------------------------------------------------------------------
// Der Registry-Pfad für die Einträge der ef3-Anwendung wird zurückgegeben
// ---------------------------------------------------------------------
function KeyPath(param: string): string;
begin
  Result := s_KeyPath;
end;
// ---------------------------------------------------------------------

// ---------------------------------------------------------------------
// Der Schlüsselname für die Fix-Version von ef3
// ---------------------------------------------------------------------
function KeyNameFix(param: string): string;
begin
  Result := s_KeyNameFix;
end;
// ---------------------------------------------------------------------

// ---------------------------------------------------------------------
// Einstiegspunkt für die InnoSetup-Routine
// ---------------------------------------------------------------------
function InitializeSetup(): boolean;
begin
	InitializeVariables();
	Result := true;
end;
// ---------------------------------------------------------------------

// ---------------------------------------------------------------------
// Funktion wird bei jedem Schrittwechsel des Setups aufgerufen
// ---------------------------------------------------------------------
procedure CurStepChanged(pt_CurStep: TSetupStep);
var
  ls_Version, ls_fix: string;
begin
	s_Path := ExpandConstant('{app}');
	s_KeyPath := s_KeyPathEf3 + PathKey('');
	filecopy(ExpandConstant('{log}'), LogFile(''), false);
	if (pt_CurStep = ssInstall) then begin
		if RegValueExists(i_RegRoot, s_KeyPath, s_KeyNameVersion) then
		begin
			if RegQueryStringValue(i_RegRoot, s_KeyPath, s_KeyNameVersion, ls_Version) then
				if not (ls_Version = '{#ef3Version}') then begin
					MsgBox(ExpandConstant('{cm:VersionFalsch_1}') + ls_Version + ExpandConstant('{cm:VersionFalsch_2}') + 
						s_Path + ExpandConstant('{cm:VersionFalsch_3}') + '{#ef3Version}' + ExpandConstant('{cm:VersionFalsch_4}'), mbConfirmation, MB_OK);
					ExitProcess(0);
				end
				else if RegQueryStringValue(i_RegRoot, s_KeyPath, s_KeyNameFix, ls_fix) then
					if not ((((ls_fix = '') or (ls_fix = '01')) and ('{#ef3FixFrom}' = '01')) or
						((StrToIntDef('{#ef3FixFrom}', 1) <= StrToIntDef(ls_fix, 1) + 1) and (StrToIntDef(ls_fix, 1) <= StrToIntDef('{#ef3FixTo}', 1)))) then begin
							MsgBox(ExpandConstant('{cm:FixFalscheVersion_1}') + ls_fix + ExpandConstant('{cm:FixFalscheVersion_2}') +
								'{#ef3FixFrom}' + ExpandConstant('{cm:FixFalscheVersion_3}') + '{#ef3FixTo}' +
								ExpandConstant('{cm:FixFalscheVersion_4}'), mbConfirmation, MB_OK)
							ExitProcess(0);
					end;
		end
		else begin
			MsgBox(ExpandConstant('{cm:VersionFehlend}'), mbConfirmation, MB_OK);
			ExitProcess(0);
		end;
	end
end;
// ---------------------------------------------------------------------
