# GTA V - Jumping Objects Mod

## Beschreibung
Diese Mod lässt Objekte (Laternen, Mülleimer, andere Autos und Fußgänger) im Spiel buchstäblich zur Seite hüpfen, wenn du dich ihnen in einem Auto näherst. So wird der Weg für dich immer frei gemacht!

**Wichtiger technischer Hinweis:**
Fest in die Map integrierte, unzerstörbare Gebäude (wie große Häuser, Wolkenkratzer, der Boden selbst) können von der Spiel-Engine in GTA V nicht physisch bewegt werden. Die Mod funktioniert bei allen dynamischen "Props" (kleinere Objekte, Laternen, Schilder), Fahrzeugen und Fußgängern, die sich in deinem Weg befinden.

## Voraussetzungen
Um diese Mod in GTA V (Singleplayer) nutzen zu können, benötigst du Folgendes:
1. **ScriptHookV**: [Download hier](http://www.dev-c.com/gtav/scripthookv/)
2. **ScriptHookVDotNet**: [Download hier](https://github.com/scripthookvdotnet/scripthookvdotnet/releases)

## Installation
1. Lade dir ScriptHookV herunter und entpacke die Dateien `ScriptHookV.dll` und `dinput8.dll` in dein GTA V Hauptverzeichnis (dort, wo die `GTA5.exe` liegt).
2. Lade ScriptHookVDotNet herunter und kopiere die Dateien `ScriptHookVDotNet.asi`, `ScriptHookVDotNet2.dll` und `ScriptHookVDotNet3.dll` ebenfalls in das Hauptverzeichnis.
3. Erstelle im GTA V Hauptverzeichnis einen Ordner namens `scripts` (falls er noch nicht existiert).
4. Kopiere die Datei `JumpingObjects.cs` aus diesem Ordner in den `scripts` Ordner.

Das war's! Starte GTA V im Singleplayer. Sobald du in ein Auto steigst und fährst, werden Objekte in deinem Weg zur Seite springen.

## Funktionsweise
Das Skript scannt die Umgebung des Spielers. Sobald ein Entity (Prop, Fahrzeug, Ped) in Fahrtrichtung und innerhalb eines Radius von 25 Metern ist, bekommt es einen starken physikalischen Impuls zur Seite und nach oben.
