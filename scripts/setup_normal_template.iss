//!!! Hier anpassen !!!
#define ef3Version "3.9.056"

//!!! Hier anpassen !!!
// Nur ben�tigt, falls Version Fix beinhaltet
#define ef3Fix ""

#define logfile "ef3_install"

//!!! Hier anpassen !!!
#define app_path "K:\Exe\ef\3.9\3.9.056\efcom"

// -------------------------------------------------------------------------------------------
[Setup]
// -------------------------------------------------------------------------------------------
AppName=ef3
AppMutex=ef, efr2
SetupLogging=yes

//!!! Hier anpassen !!!
// ".{#ef3Fix}" nur anh�ngen, falls Version Fix beinhaltet
AppVersion={#ef3Version}.{#ef3Fix}

DefaultGroupName=ef3
DisableProgramGroupPage=yes
VersionInfoCompany=efcom GmbH
AppendDefaultDirName=no
DefaultDirName={sd}\
// -------------------------------------------------------------------------------------------

// -------------------------------------------------------------------------------------------
[InstallDelete]
// -------------------------------------------------------------------------------------------
// Type: files; Name: "{app}\*.*"
// -------------------------------------------------------------------------------------------

// -------------------------------------------------------------------------------------------
[Files]
// -------------------------------------------------------------------------------------------

//!!! Hier anpassen !!!
// Kunden
// Source: "{#app_path}\3.9\Source\Kunde\efcuefcom.pbd"; DestDir: "{app}"; Flags: ignoreversion


//!!! Hier anpassen !!!
// Module
// Source: "{#app_path}\3.9\Source\Module\efmodgeb.pbd"; DestDir: "{app}"; Flags: ignoreversion


//!!! Hier anpassen !!!
// Fixe
// Source: "{#app_path}\3.9\Source\Fix_01\efapp.pbd"; DestDir: "{app}"; Flags: ignoreversion


// Ini
Source: "W:\MigrationDB\Installationsroutine\1.0\Ini\Deutsch\vorlage_ef.ini"; DestDir: "{app}"; Flags: ignoreversion
// -------------------------------------------------------------------------------------------

// -------------------------------------------------------------------------------------------
[UninstallDelete]
// -------------------------------------------------------------------------------------------
Type: files; Name: "{app}\*.log"
// -------------------------------------------------------------------------------------------

// -------------------------------------------------------------------------------------------
[Registry]
// -------------------------------------------------------------------------------------------
Root: HKLM; Subkey: "{code:KeyPath}"; Check: not IsWin64; Flags: uninsdeletekey
Root: HKLM64; Subkey: "{code:KeyPath}"; Check: IsWin64; Flags: uninsdeletekey
Root: HKLM; Subkey: "{code:KeyPath}"; ValueType: string; ValueName: {code:KeyNameVersion}; ValueData: "{#ef3Version}"; Check: not IsWin64; Flags: uninsdeletevalue
Root: HKLM64; Subkey: "{code:KeyPath}"; ValueType: string; ValueName: {code:KeyNameVersion}; ValueData: "{#ef3Version}"; Check: IsWin64; Flags: uninsdeletevalue

//!!! Hier anpassen !!!
// " ValueData: "{#ef3Fix}";" nur einf�gen, falls Version Fix beinhaltet
Root: HKLM; Subkey: "{code:KeyPath}"; ValueType: string; ValueName: {code:KeyNameFix}; ValueData: "{#ef3Fix}"; Check: not IsWin64; Flags:uninsdeletevalue
Root: HKLM64; Subkey: "{code:KeyPath}"; ValueType: string; ValueName: {code:KeyNameFix}; ValueData: "{#ef3Fix}"; Check: IsWin64; Flags:uninsdeletevalue
// -------------------------------------------------------------------------------------------

// -------------------------------------------------------------------------------------------
[Icons]
// -------------------------------------------------------------------------------------------
Name: "{group}\{code:PathKey}\ef3_{code:PathLastElement}"; Filename: "{app}\ef.exe"
Name: "{group}\{code:PathKey}\ef3_{code:PathLastElement}_uninstaller"; Filename: "{uninstallexe}"
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
de.EnvironmentSelectionTitle=Auswahl der Umgebung
en.EnvironmentSelectionTitle=Selection of environment
de.EnvironmentSelectionDescription=Bitte w�hlen Sie die gew�nschte Umgebung aus:
en.EnvironmentSelectionDescription=Please select the desired environment:
de.VersionUeberschreibenFrage_1=Es ist bereits eine Version (
en.VersionUeberschreibenFrage_1=There already is a version (
de.VersionUeberschreibenFrage_2=) installiert in "
en.VersionUeberschreibenFrage_2=) installed in "
de.VersionUeberschreibenFrage_3=". M�chten Sie sie durch Version 
en.VersionUeberschreibenFrage_3=". Do you want to overwrite it with version 
de.VersionUeberschreibenFrage_4= ersetzen?
en.VersionUeberschreibenFrage_4=?
de.BereitsInstalliert_1=Die zu installierende Version (
en.BereitsInstalliert_1=The version (
de.BereitsInstalliert_2=) ist schon installiert. �berschreiben?
en.BereitsInstalliert_2=) to be installed already is installed. Overwrite?
// -------------------------------------------------------------------------------------------

// -------------------------------------------------------------------------------------------
[Code]
// -------------------------------------------------------------------------------------------

var
	gs_keyPathEf3, gs_keyPath, gs_keyNameVersion, gs_keyNameFix, gs_path, gs_information, gs_environment: string;
	gi_regRoot: integer;
	gl_description: TLabel;

// ---------------------------------------------------------------------
// Initialisierung aller Variablen
// ---------------------------------------------------------------------
procedure InitializeVariables();
begin
	gs_keyPathEf3 := 'SOFTWARE\efcom\ef3\';
	gs_keyNameVersion := 'Version';
	gs_keyNameFix := 'Fix';
	gs_information := 'The following components will be installed into the following locations,' + #13#10 +
		'if not already present:' + #13#10 +
		'' + #13#10 +
		'Client: %s\EFCOM\%s' + #13#10 +
		'Service Monitor: %s\EFCOM\EF3SERVICE\EFSVCMON' + #13#10 +
		'Batch Service: %s\EFCOM\EF3SERVICE\%s' + #13#10 +
		'Batch: ?'
	gs_environment := 'LIVE';
	if IsWin64 then begin
		gi_regRoot := HKLM64;
	end
	else
		gi_regRoot := HKLM;
end;
// ---------------------------------------------------------------------

// ---------------------------------------------------------------------
// Aus einem Pfad wird ein Schl�ssel erzeugt
// ---------------------------------------------------------------------
function PathKey(param: string): string;
var
	ls_path: string;
begin
	ls_path := gs_path;
	StringChangeEx(ls_path, ':', '', true);
	StringChangeEx(ls_path, '\', '_', true);
	Result := ls_path;
end;
// ---------------------------------------------------------------------

// ---------------------------------------------------------------------
// Aus einem Pfad wird das letzte Element zur�ckgegeben
// ---------------------------------------------------------------------
function PathLastElement(param: string): string;
begin
  Result := ExtractFileName(gs_path);
end;
// ---------------------------------------------------------------------

// ---------------------------------------------------------------------
// Der Pfad der Logdatei wird zur�ckgegeben
// ---------------------------------------------------------------------
function LogFile(param: string): string;
begin
// ".{#ef3Fix}" nur einbauen, wenn Version Fix beinhaltet
  Result := gs_path + '\{#logfile}_{#ef3Version}.{#ef3Fix}.log';
end;
// ---------------------------------------------------------------------

// ---------------------------------------------------------------------
// Beendet die Anwendung
// ---------------------------------------------------------------------
procedure ExitProcess(exitCode: integer);
  external 'ExitProcess@kernel32.dll stdcall';
// ---------------------------------------------------------------------

// ---------------------------------------------------------------------
// Der Registry-Pfad f�r die Eintr�ge der ef3-Anwendung wird zur�ckgegeben
// ---------------------------------------------------------------------
function KeyPath(param: string): string;
begin
	Result := gs_keyPath;
end;
// ---------------------------------------------------------------------

// ---------------------------------------------------------------------
// Der Schl�sselname f�r die Version von ef3
// ---------------------------------------------------------------------
function KeyNameVersion(param: string): string;
begin
	Result := gs_keyNameVersion;
end;
// ---------------------------------------------------------------------

// ---------------------------------------------------------------------
// Der Ordnerpfad f�r die Schl�ssel von allen ef3-Installationen
// ---------------------------------------------------------------------
function KeyPathEf3(param: string): string;
begin
	Result := gs_keyPathEf3;
end;
// ---------------------------------------------------------------------

// ---------------------------------------------------------------------
// Der Schl�sselname f�r die Fix-Version von ef3
// ---------------------------------------------------------------------
function KeyNameFix(param: string): string;
begin
	Result := gs_keyNameFix;
end;
// ---------------------------------------------------------------------

// ---------------------------------------------------------------------
// Einstiegspunkt f�r die Routine
// ---------------------------------------------------------------------
function InitializeSetup(): boolean;
begin
	InitializeVariables();
	Result := true;
end;
// ---------------------------------------------------------------------

procedure LiveSelectedHandler(ao_sender: TObject);
begin
	gs_environment := 'LIVE';
	gl_description.Caption := Format(gs_information, [gs_path, gs_environment, gs_path, gs_path, gs_environment]);
end;

procedure TestSelectedHandler(ao_sender: TObject);
begin
	gs_environment := 'TEST';
	gl_description.Caption := Format(gs_information, [gs_path, gs_environment, gs_path, gs_path, gs_environment]);
end;

procedure MigSelectedHandler(ao_sender: TObject);
begin
	gs_environment := 'MIG';
	gl_description.Caption := Format(gs_information, [gs_path, gs_environment, gs_path, gs_path, gs_environment]);
end;

procedure EnvironmentSelectionHandler(awp_sender: TWizardPage);
begin
	gs_path := ExpandConstant('{app}');
	if (gs_environment = 'LIVE') then begin
		LiveSelectedHandler(nil);
	end else if (gs_environment = 'TEST') then begin
		TestSelectedHandler(nil);
	end else if (gs_environment = 'MIG') then begin
		MigSelectedHandler(nil)
	end
end;

// ---------------------------------------------------------------------
// Einstiegspunkt f�r die Routine, nachdem Wizard-Form erstellt wurde
// ---------------------------------------------------------------------
procedure InitializeWizard;
var
	lwp_environmentSelection: TWizardPage;
	lnrb_live, lnrb_test, lnrb_mig: TNewRadioButton;
begin
	lwp_environmentSelection := CreateCustomPage(wpSelectDir, ExpandConstant('{cm:EnvironmentSelectionTitle}'), ExpandConstant('{cm:EnvironmentSelectionDescription}'));
	lwp_environmentSelection.OnActivate := @EnvironmentSelectionHandler;
	lnrb_live := TNewRadioButton.Create(WizardForm);
	lnrb_live.Parent := lwp_environmentSelection.Surface;
	lnrb_live.Checked := true;
	lnrb_live.Top := 0;
	lnrb_live.Width := lwp_environmentSelection.SurfaceWidth;
	lnrb_live.Font.Style := [fsBold];
	lnrb_live.Font.Size := 9;
	lnrb_live.Caption := 'Live';
	lnrb_live.OnClick := @LiveSelectedHandler;
	lnrb_test := TNewRadioButton.Create(WizardForm);
	lnrb_test.Parent := lwp_environmentSelection.Surface;
	lnrb_test.Checked := false;
	lnrb_test.Top := lnrb_live.Top + lnrb_live.Height + 4;
	lnrb_test.Width := lwp_environmentSelection.SurfaceWidth;
	lnrb_test.Font.Style := lnrb_live.Font.Style;
	lnrb_test.Font.Size := lnrb_live.Font.Size;
	lnrb_test.Caption := 'Test';
	lnrb_test.OnClick := @TestSelectedHandler;
	lnrb_mig := TNewRadioButton.Create(WizardForm);
	lnrb_mig.Parent := lwp_environmentSelection.Surface;
	lnrb_mig.Checked := false;
	lnrb_mig.Top := lnrb_test.Top + lnrb_test.Height + 4;
	lnrb_mig.Width := lwp_environmentSelection.SurfaceWidth;
	lnrb_mig.Font.Style := lnrb_test.Font.Style;
	lnrb_mig.Font.Size := lnrb_test.Font.Size;
	lnrb_mig.Caption := 'Mig';
	lnrb_mig.OnClick := @MigSelectedHandler;
	gl_description := TLabel.Create(WizardForm);
	gl_description.Parent := lwp_environmentSelection.Surface;
	gl_description.Top := lnrb_mig.Top + lnrb_mig.Height + 8;
	gl_description.Width := lwp_environmentSelection.SurfaceWidth;
	gl_description.AutoSize := true;
	gl_description.Wordwrap := false;
end;
// ---------------------------------------------------------------------

// ---------------------------------------------------------------------
// Funktion wird bei jedem Schrittwechsel des Setups aufgerufen
// ---------------------------------------------------------------------
procedure CurStepChanged(pt_CurStep: TSetupStep);
var
	ls_version: string;
begin
	if (pt_CurStep = ssInstall) then begin
		gs_keyPath := gs_keyPathEf3 + PathKey('');
		filecopy(ExpandConstant('{log}'), LogFile(''), false);
		if RegQueryStringValue(gi_regRoot, gs_keyPath, gs_keyNameVersion, ls_version) then begin
			if not (ls_version = '{#ef3Version}') then begin
				if MsgBox(ExpandConstant('{cm:VersionUeberschreibenFrage_1}') + ls_version + ExpandConstant('{cm:VersionUeberschreibenFrage_2}') +
				gs_path + ExpandConstant('{cm:VersionUeberschreibenFrage_3}') + '{#ef3Version}' + ExpandConstant('{cm:VersionUeberschreibenFrage_4}'),
				mbConfirmation, MB_YESNO) = idNo then
					ExitProcess(0);
			end
			else
				if MsgBox(ExpandConstant('{cm:BereitsInstalliert_1}') + '{#ef3Version}' + ExpandConstant('{cm:BereitsInstalliert_2}'), mbConfirmation, MB_YESNO) = idNo then begin
					ExitProcess(0);
        end
      end
    end
end;
// ---------------------------------------------------------------------
