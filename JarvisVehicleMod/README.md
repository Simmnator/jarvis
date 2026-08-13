# JARVIS Vehicle Mod für GTA V

Dieser Mod bringt ein hochentwickeltes K.I.T.T. / "JARVIS" Fahrzeug-System nach Los Santos. Das Script wurde in C# für ScriptHookVDotNet geschrieben.

## Features
* **Autonomes Herbeirufen:** Rufe dein Fahrzeug per Knopfdruck zu dir (fährt autonom zu deiner Position).
* **Autopilot & Fluchtmodus:** Lass dich zu einem Wegpunkt fahren oder aktiviere den aggressiven Fluchtmodus, um Feinden zu entkommen.
* **EMP-Schockwelle:** Lege Fahrzeuge in einem Radius von 30 Metern für 10 Sekunden lahm (Motor aus, Licht aus).
* **Verteidigungs- & Ramm-Modus:** Das Fahrzeug scannt die Umgebung auf Feinde und schaltet sie durch Ramm-Manöver aus.

## Voraussetzungen
* [Grand Theft Auto V](https://store.steampowered.com/app/271590/Grand_Theft_Auto_V/)
* [ScriptHookV](http://www.dev-c.com/gtav/scripthookv/)
* [ScriptHookVDotNet v3](https://github.com/scripthookvdotnet/scripthookvdotnet/releases)

## Installation
1. Stelle sicher, dass **ScriptHookV** und **ScriptHookVDotNet v3** in deinem GTA V Hauptverzeichnis installiert sind.
2. Kopiere die Datei **`JarvisVehicleScript.cs`** in den **`scripts`** Ordner in deinem GTA V Verzeichnis (z. B. `C:\Program Files (x86)\Steam\steamapps\common\Grand Theft Auto V\scripts`). Wenn der Ordner nicht existiert, erstelle ihn.
3. Starte das Spiel.

## Standard Steuerung (Keybinds)
* `Numpad 1`: **Fahrzeug rufen** - Das JARVIS-Fahrzeug wird gespawnt und fährt autonom zu deiner aktuellen Position.
* `Numpad 2`: **Autopilot umschalten** - (Nur verfügbar, wenn du im JARVIS-Fahrzeug sitzt). Fährt zum auf der Karte markierten Wegpunkt. Wenn kein Wegpunkt markiert ist, wird der Ausweich-/Fluchtmodus aktiviert.
* `Numpad 3`: **EMP-Schockwelle** - Löst eine Schockwelle aus, die Fremdfahrzeuge im Umkreis von 30m für 10 Sekunden lahmlegt.
* `Numpad 4`: **Verteidigungsmodus umschalten** - Aktiviert/Deaktiviert das automatische Scannen nach Feinden. Erkannte Feinde werden gerammt.

## Fehlerbehebung
* Wenn der Mod nicht lädt, stelle sicher, dass du im Spiel die `Einfügen` (Insert) Taste drückst, um die ScriptHook-Konsole zu öffnen und überprüfe, ob Fehler angezeigt werden.
* Du kannst das Script jederzeit mit dem Befehl `Reload()` in der Konsole neu laden.
* Stelle sicher, dass du in den Windows-Eigenschaften der `.cs`-Datei nicht blockiert bist.