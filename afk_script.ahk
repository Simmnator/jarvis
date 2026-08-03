#Persistent
SetTimer, AfkAction, 600000 ; 600000 Millisekunden = 10 Minuten

return

AfkAction:
    ; Drückt die Taste 'w' und hält sie gedrückt
    Send, {w down}

    ; Wartet für 3 Sekunden (3000 Millisekunden)
    Sleep, 3000

    ; Lässt die Taste 'w' los
    Send, {w up}

    ; Kurze Pause (500 ms) zur Sicherheit, damit die Eingabe sauber registriert wird
    Sleep, 500

    ; Sendet den Text 'esx r' und drückt Enter
    Send, esx r{Enter}
return

; Notfall-Ausgang: Drücke die Escape-Taste, um das Skript zu beenden
Esc::ExitApp
