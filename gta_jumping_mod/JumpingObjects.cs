using System;
using System.Collections.Generic;
using System.Linq;
using GTA;
using GTA.Math;
using GTA.Native;

public class JumpingObjects : Script
{
    private const float DetectionRadius = 25f; // Radius in dem Objekte erkannt werden
    private const float JumpForce = 20f; // Kraft des Wegspringens
    private const float UpwardForce = 15f; // Kraft nach oben

    private Dictionary<int, long> _processedEntities = new Dictionary<int, long>();
    private const long CooldownMs = 2000; // Cooldown, damit dasselbe Objekt nicht spammt

    private long _lastCleanupTime = 0;
    private const long CleanupIntervalMs = 60000; // Aufräumen alle 60 Sekunden

    public JumpingObjects()
    {
        Tick += OnTick;
        GTA.UI.Notification.Show("~g~Jumping Objects Mod~w~ wurde erfolgreich geladen!");
    }

    private void OnTick(object sender, EventArgs e)
    {
        Ped playerPed = Game.Player.Character;

        // Nur ausführen, wenn der Spieler existiert und in einem Fahrzeug sitzt
        if (playerPed == null || !playerPed.IsInVehicle()) return;

        Vehicle playerVehicle = playerPed.CurrentVehicle;
        Vector3 playerPos = playerVehicle.Position;
        Vector3 playerForward = playerVehicle.ForwardVector;

        // Hole Props, Fahrzeuge und Fußgänger separat (für SHVDN v3 Kompatibilität)
        var props = World.GetNearbyProps(playerPos, DetectionRadius);
        var vehicles = World.GetNearbyVehicles(playerPed, DetectionRadius);
        var peds = World.GetNearbyPeds(playerPed, DetectionRadius);

        // Verbinde alle zu einer Liste
        List<Entity> nearbyEntities = new List<Entity>();
        nearbyEntities.AddRange(props);
        nearbyEntities.AddRange(vehicles);
        nearbyEntities.AddRange(peds);

        long currentTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        // Memory Cleanup
        if (currentTime - _lastCleanupTime > CleanupIntervalMs)
        {
            var keysToRemove = _processedEntities.Where(kvp => currentTime - kvp.Value > CooldownMs).Select(kvp => kvp.Key).ToList();
            foreach (var key in keysToRemove)
            {
                _processedEntities.Remove(key);
            }
            _lastCleanupTime = currentTime;
        }

        foreach (Entity entity in nearbyEntities)
        {
            // Ignoriere den Spieler und sein eigenes Fahrzeug
            if (entity == playerPed || entity == playerVehicle) continue;

            // Überprüfe, ob das Entity existiert
            if (!entity.Exists()) continue;

            // Cooldown check
            if (_processedEntities.ContainsKey(entity.Handle))
            {
                if (currentTime - _processedEntities[entity.Handle] < CooldownMs)
                {
                    continue; // Noch im Cooldown
                }
            }

            // Berechne die Richtung vom Spieler zum Objekt
            Vector3 directionToEntity = (entity.Position - playerPos).Normalized;

            // Überprüfe, ob das Objekt vor dem Spieler ist (Dot-Produkt > 0)
            float dotProduct = Vector3.Dot(playerForward, directionToEntity);

            // Nur wenn das Objekt einigermaßen vor uns ist, springt es weg
            if (dotProduct > 0.3f)
            {
                // Mache das Objekt dynamisch/physisch manipulierbar
                // Besonders wichtig für viele Props (Mülleimer, Laternen, Schilder)
                if (entity is Prop prop)
                {
                    // Versuche festgefrorene Props zu lösen
                    Function.Call(Hash.FREEZE_ENTITY_POSITION, entity.Handle, false);
                    entity.IsPersistent = true;
                }

                // Berechne die Vektorrichtung zum Wegstoßen
                // Wir stoßen es zur Seite (Cross Product mit UP) und nach oben
                Vector3 rightVector = Vector3.Cross(playerForward, Vector3.WorldUp).Normalized;

                // Bestimme, auf welcher Seite das Objekt ist
                float rightDot = Vector3.Dot(rightVector, directionToEntity);

                // Wenn rightDot > 0, ist es rechts, sonst links.
                Vector3 jumpDirection = (rightDot > 0 ? rightVector : -rightVector) * JumpForce;
                jumpDirection += Vector3.WorldUp * UpwardForce; // Nach oben hüpfen

                // Setze die Velocity (Geschwindigkeit/Impuls)
                entity.Velocity = jumpDirection;

                // Füge zum Cooldown hinzu
                _processedEntities[entity.Handle] = currentTime;
            }
        }
    }
}
