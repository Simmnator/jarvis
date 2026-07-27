# Autohomora Mod für Hogwarts Legacy

Diese Mod überspringt automatisch das Alohomora-Rätsel-Minispiel.

## Installation

Um diese Lua-Mod zu verwenden, benötigst du **UE4SS** (Unreal Engine 4 Scripting System).

### Schritt 1: UE4SS herunterladen und installieren
1. Lade dir die neueste Version von **UE4SS-Xinput** herunter: [UE4SS GitHub Releases](https://github.com/UE4SS-RE/RE-UE4SS/releases) (Lade die Datei herunter, die z.B. `UE4SS_vX.X.X.zip` heißt - achte darauf, dass sie Xinput unterstützt oder befolge die Anweisungen im GitHub, falls Xinput separat geladen werden muss).
2. Entpacke den gesamten Inhalt der heruntergeladenen ZIP-Datei in folgenden Ordner deines Spiels:
   `...\Hogwarts Legacy\Phoenix\Binaries\Win64\`
   *(Falls du Steam nutzt: `C:\Program Files (x86)\Steam\steamapps\common\Hogwarts Legacy\Phoenix\Binaries\Win64\`)*

### Schritt 2: Mod installieren
1. Erstelle in dem Ordner `...\Hogwarts Legacy\Phoenix\Binaries\Win64\Mods\` einen neuen Ordner namens `Autohomora`.
2. Kopiere die Datei `main.lua` aus diesem Verzeichnis in den neu erstellten `Autohomora` Ordner.
   Pfad sollte dann so aussehen: `...\Hogwarts Legacy\Phoenix\Binaries\Win64\Mods\Autohomora\Scripts\main.lua` (Erstelle den `Scripts` Ordner, falls noch nicht vorhanden)
   bzw. je nach UE4SS Version einfach `...\Mods\Autohomora\main.lua` und in der `mods.txt` aktivieren. Beachte die Dokumentation von UE4SS.

Alternativ kannst du auch den Code einfach zu einer existierenden Mod hinzufügen oder in die `mods.txt` der UE4SS-Installation eintragen.

Wenn alles korrekt installiert ist, startet das Skript automatisch mit dem Spiel und überspringt die Alohomora-Rätsel sofort.