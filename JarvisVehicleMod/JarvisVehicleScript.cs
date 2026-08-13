using System;
using System.Collections.Generic;
using System.Windows.Forms;
using GTA;
using GTA.Math;
using GTA.Native;

namespace JarvisVehicleMod
{
    public class JarvisVehicleScript : Script
    {
        // ---------------------------------------------------------
        // 1. CONFIGURATION (Keybinds & Parameter)
        // ---------------------------------------------------------
        public static class Config
        {
            // Tastenbelegung
            public static Keys KeySummon = Keys.NumPad1;
            public static Keys KeyAutopilot = Keys.NumPad2;
            public static Keys KeyEMP = Keys.NumPad3;
            public static Keys KeyDefense = Keys.NumPad4;

            // Radien & Timer
            public static float SummonArrivalRadius = 5.0f;
            public static float EMPRadius = 30.0f;
            public static int EMPDurationMs = 10000;
            public static float DefenseScanRadius = 50.0f;

            // Fahr-Einstellungen
            public static float AutopilotSpeed = 30.0f; // m/s (entspricht ca. 108 km/h)
            public static int NormalDrivingStyle = 786603; // Ruhiger Fahrstil, beachtet Ampeln
            public static int RushedDrivingStyle = 1074528293; // Aggressiv, ignoriert Ampeln und weicht aus

            // Standard Fahrzeug (z. B. T20, Pegassi)
            public static VehicleHash DefaultVehicleHash = VehicleHash.T20;
        }

        // ---------------------------------------------------------
        // 2. STATE MACHINE
        // ---------------------------------------------------------
        public enum VehicleState
        {
            Idle,
            DrivingToPlayer,
            Autopilot,
            RammingTarget
        }

        private Vehicle _jarvisVehicle = null;
        private VehicleState _currentState = VehicleState.Idle;
        private bool _defenseModeActive = false;
        private Ped _currentTarget = null;

        // Timer für EMP-deaktivierte Fahrzeuge
        private Dictionary<Vehicle, int> _empDisabledVehicles = new Dictionary<Vehicle, int>();

        public JarvisVehicleScript()
        {
            Tick += OnTick;
            KeyDown += OnKeyDown;
            Aborted += OnAborted;

            JarvisUI.ShowMessage("JARVIS System online. Bereit für Eingaben, Sir.");
        }

        // ---------------------------------------------------------
        // 3. MAIN LOOP & EVENTS
        // ---------------------------------------------------------
        private void OnTick(object sender, EventArgs e)
        {
            try
            {
                HandleState();
                HandleEmpTimers();

                if (_defenseModeActive)
                {
                    ExecuteDefenseProtocol();
                }
            }
            catch (Exception ex)
            {
                GTA.UI.Notification.Show($"~r~JARVIS System Error:~w~ {ex.Message}");
            }
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Config.KeySummon)
            {
                SummonVehicle();
            }
            else if (e.KeyCode == Config.KeyAutopilot)
            {
                ToggleAutopilot();
            }
            else if (e.KeyCode == Config.KeyEMP)
            {
                TriggerEmpBlast();
            }
            else if (e.KeyCode == Config.KeyDefense)
            {
                ToggleDefenseMode();
            }
        }

        // ---------------------------------------------------------
        // 4. CORE FEATURES
        // ---------------------------------------------------------

        /// <summary>
        /// A. Autonomes Herbeirufen (Call Vehicle)
        /// </summary>
        private void SummonVehicle()
        {
            try
            {
                if (Game.Player.Character.IsInVehicle())
                {
                    JarvisUI.ShowMessage("JARVIS: Sie sind bereits in einem Fahrzeug, Sir.");
                    return;
                }

                if (_jarvisVehicle == null || !_jarvisVehicle.Exists())
                {
                    SpawnJarvisVehicle();
                }

                if (_jarvisVehicle != null)
                {
                    Ped driver = GetOrSpawnDriver();

                    // Nutze TASK_VEHICLE_DRIVE_TO_COORD_LONGRANGE für autonomes, intelligentes Fahren
                    Vector3 playerPos = Game.Player.Character.Position;
                    Function.Call(Hash.TASK_VEHICLE_DRIVE_TO_COORD_LONGRANGE, driver, _jarvisVehicle, playerPos.X, playerPos.Y, playerPos.Z, Config.AutopilotSpeed, Config.NormalDrivingStyle, Config.SummonArrivalRadius);

                    _currentState = VehicleState.DrivingToPlayer;
                    JarvisUI.ShowMessage("JARVIS: Ich bin unterwegs zu Ihrer Position, Sir.");
                    PlayAudioFeedback("Beep_Green", "DLC_HEIST_HACKING_SNAKE_SOUNDS");
                }
            }
            catch (Exception ex)
            {
                GTA.UI.Notification.Show($"~r~Summon Error:~w~ {ex.Message}");
            }
        }

        /// <summary>
        /// B. Autopilot & Flucht-Modus (Autopilot)
        /// </summary>
        private void ToggleAutopilot()
        {
            try
            {
                if (!Game.Player.Character.IsInVehicle() || Game.Player.Character.CurrentVehicle != _jarvisVehicle)
                {
                    JarvisUI.ShowMessage("JARVIS: Autopilot ist nur im JARVIS-Fahrzeug verfügbar.");
                    return;
                }

                if (_currentState == VehicleState.Autopilot)
                {
                    // Deaktivieren: Spieler erhält die volle Kontrolle zurück
                    Game.Player.Character.Task.ClearAll();
                    _currentState = VehicleState.Idle;
                    JarvisUI.ShowMessage("JARVIS: Autopilot deaktiviert. Sie haben die Kontrolle.");
                    PlayAudioFeedback("Beep_Red", "DLC_HEIST_HACKING_SNAKE_SOUNDS");
                }
                else
                {
                    // Aktivieren
                    Vector3 waypoint = World.WaypointPosition;
                    if (waypoint != Vector3.Zero)
                    {
                        // Modus 1: Waypoint-Fahrt
                        Function.Call(Hash.TASK_VEHICLE_DRIVE_TO_COORD_LONGRANGE, Game.Player.Character, _jarvisVehicle, waypoint.X, waypoint.Y, waypoint.Z, Config.AutopilotSpeed, Config.NormalDrivingStyle, 5.0f);
                        JarvisUI.ShowMessage("JARVIS: Autopilot aktiviert. Navigiere zum Zielpunkt.");
                    }
                    else
                    {
                        // Modus 2: Flucht/Wander (Spieler-Task wandern lassen)
                        Function.Call(Hash.TASK_VEHICLE_DRIVE_WANDER, Game.Player.Character, _jarvisVehicle, Config.AutopilotSpeed, Config.RushedDrivingStyle);
                        JarvisUI.ShowMessage("JARVIS: Fluchtmodus aktiviert. Ausweichmanöver eingeleitet.");
                    }

                    _currentState = VehicleState.Autopilot;
                    PlayAudioFeedback("Beep_Green", "DLC_HEIST_HACKING_SNAKE_SOUNDS");
                }
            }
            catch (Exception ex)
            {
                GTA.UI.Notification.Show($"~r~Autopilot Error:~w~ {ex.Message}");
            }
        }

        /// <summary>
        /// D. EMP-Schockwelle (EMP Blast)
        /// </summary>
        private void TriggerEmpBlast()
        {
            try
            {
                if (_jarvisVehicle == null || !_jarvisVehicle.Exists()) return;

                JarvisUI.ShowMessage("JARVIS: EMP-Schockwelle initiiert.");
                PlayAudioFeedback("Beep_Red", "DLC_HEIST_HACKING_SNAKE_SOUNDS");
                Vector3 pos = _jarvisVehicle.Position;

                // Visueller Effekt laden
                Function.Call(Hash.REQUEST_NAMED_PTFX_ASSET, "core");
                int timeout = 0;
                while (!Function.Call<bool>(Hash.HAS_NAMED_PTFX_ASSET_LOADED, "core") && timeout < 50)
                {
                    Script.Wait(10);
                    timeout++;
                }

                // Elektrische Funken abspielen
                Function.Call((Hash)0x6C38AF3693A69A91, "core"); // _USE_PARTICLE_FX_ASSET_NEXT_CALL
                Function.Call(Hash.START_PARTICLE_FX_NON_LOOPED_AT_COORD, "ent_dst_elec_crackle", pos.X, pos.Y, pos.Z, 0f, 0f, 0f, 4.0f, false, false, false);

                // Schockwellen-Audio abspielen (Wasserhydrant knallt schön ohne physischen Schaden anzurichten)
                // WaterHydrant ist 13 in V3, wir nehmen einfach 13 als Int.
                Function.Call(Hash.ADD_EXPLOSION, pos.X, pos.Y, pos.Z, 13, 0f, true, false, 0f);

                // Fremde Fahrzeuge im Radius lahmlegen
                Vehicle[] vehicles = World.GetNearbyVehicles(pos, Config.EMPRadius);
                foreach (Vehicle veh in vehicles)
                {
                    if (veh != _jarvisVehicle && veh != Game.Player.Character.CurrentVehicle)
                    {
                        veh.EngineHealth = -1f; // Motor stirbt temporär ab (verhindert aber sofortige Explosion)
                        veh.IsEngineRunning = false;
                        veh.AreLightsOn = false;

                        _empDisabledVehicles[veh] = Game.GameTime + Config.EMPDurationMs;
                    }
                }
            }
            catch (Exception ex)
            {
                GTA.UI.Notification.Show($"~r~EMP Error:~w~ {ex.Message}");
            }
        }

        private void ToggleDefenseMode()
        {
            _defenseModeActive = !_defenseModeActive;
            if (_defenseModeActive)
            {
                JarvisUI.ShowMessage("JARVIS: Verteidigungsmodus aktiviert. Scanne Umgebung.");
                PlayAudioFeedback("Beep_Green", "DLC_HEIST_HACKING_SNAKE_SOUNDS");
            }
            else
            {
                _currentState = VehicleState.Idle;
                if (_jarvisVehicle != null && _jarvisVehicle.Exists() && _jarvisVehicle.Driver != null && _jarvisVehicle.Driver != Game.Player.Character)
                {
                    _jarvisVehicle.Driver.Task.ClearAll();
                }
                _currentTarget = null;
                JarvisUI.ShowMessage("JARVIS: Verteidigungsmodus deaktiviert.");
                PlayAudioFeedback("Beep_Red", "DLC_HEIST_HACKING_SNAKE_SOUNDS");
            }
        }

        /// <summary>
        /// C. Verteidigungs- & Ramm-Modus (Aggressive Defense)
        /// </summary>
        private void ExecuteDefenseProtocol()
        {
            try
            {
                if (_jarvisVehicle == null || !_jarvisVehicle.Exists()) return;

                // Status prüfen
                if (_currentState == VehicleState.RammingTarget)
                {
                    // Abbruchbedingung: Ziel ist tot oder zu weit weg
                    if (_currentTarget == null || !_currentTarget.Exists() || _currentTarget.IsDead || _currentTarget.Position.DistanceTo(Game.Player.Character.Position) > Config.DefenseScanRadius * 1.5f)
                    {
                        _currentState = VehicleState.Idle;
                        _currentTarget = null;
                        JarvisUI.ShowMessage("JARVIS: Ziel eliminiert oder außer Reichweite. Kehre zurück.");
                        SummonVehicle(); // Auto kommt nach getaner Arbeit zurück zum Spieler
                    }
                    return;
                }

                // Scannen nach Feinden
                Ped[] peds = World.GetNearbyPeds(Game.Player.Character.Position, Config.DefenseScanRadius);
                foreach (Ped ped in peds)
                {
                    if (ped != Game.Player.Character && (ped.IsInCombatAgainst(Game.Player.Character) || ped.IsShooting))
                    {
                        _currentTarget = ped;
                        _currentState = VehicleState.RammingTarget;

                        Ped driver = GetOrSpawnDriver();

                        // 8 = VehicleMissionType.Ram
                        Function.Call(Hash.TASK_VEHICLE_MISSION_PED_TARGET, driver, _jarvisVehicle, ped, 8, Config.AutopilotSpeed, Config.RushedDrivingStyle, 0f, 0f, true);

                        JarvisUI.ShowMessage("JARVIS: Bedrohung erkannt. Leite Ramm-Gegenmaßnahmen ein.");
                        PlayAudioFeedback("Beep_Green", "DLC_HEIST_HACKING_SNAKE_SOUNDS");
                        break;
                    }
                }
            }
            catch
            {
                // Silent catch im Scan-Loop für die Performance
            }
        }

        // ---------------------------------------------------------
        // 5. HELPER METHODS
        // ---------------------------------------------------------

        private void HandleState()
        {
            if (_currentState == VehicleState.DrivingToPlayer && _jarvisVehicle != null && _jarvisVehicle.Exists())
            {
                // Prüfen ob Auto beim Spieler angekommen ist
                if (_jarvisVehicle.Position.DistanceTo(Game.Player.Character.Position) <= Config.SummonArrivalRadius)
                {
                    if (_jarvisVehicle.Driver != null) _jarvisVehicle.Driver.Task.ClearAll(); // Stoppen
                    _jarvisVehicle.SoundHorn(500); // Kurz hupen
                    Function.Call(Hash.SET_VEHICLE_INDICATOR_LIGHTS, _jarvisVehicle, 0, true);
                    Function.Call(Hash.SET_VEHICLE_INDICATOR_LIGHTS, _jarvisVehicle, 1, true);
                    Function.Call(Hash.SET_VEHICLE_DOORS_LOCKED, _jarvisVehicle, 1); // 1 = Unlocked

                    _currentState = VehicleState.Idle;
                    JarvisUI.ShowMessage("JARVIS: Ich bin an Ihrer Position angekommen, Sir.");

                    // Warnblinker nach 3 Sekunden ausschalten
                    Script.Wait(3000);
                    if (_jarvisVehicle != null && _jarvisVehicle.Exists())
                    {
                        Function.Call(Hash.SET_VEHICLE_INDICATOR_LIGHTS, _jarvisVehicle, 0, false);
                        Function.Call(Hash.SET_VEHICLE_INDICATOR_LIGHTS, _jarvisVehicle, 1, false);
                    }
                }
            }
        }

        private void HandleEmpTimers()
        {
            List<Vehicle> toRemove = new List<Vehicle>();
            foreach (var kvp in _empDisabledVehicles)
            {
                // Timer abgelaufen?
                if (Game.GameTime > kvp.Value)
                {
                    if (kvp.Key != null && kvp.Key.Exists())
                    {
                        kvp.Key.EngineHealth = 1000f; // Motorreparatur
                    }
                    toRemove.Add(kvp.Key);
                }
            }

            // Timer bereinigen
            foreach (var veh in toRemove)
            {
                _empDisabledVehicles.Remove(veh);
            }
        }

        private void SpawnJarvisVehicle()
        {
            Model model = new Model(Config.DefaultVehicleHash);
            model.Request(10000);

            // Spawnt das Auto in ~80m Entfernung (außerhalb des direkten Blickfelds)
            Vector3 spawnPos = World.GetNextPositionOnStreet(Game.Player.Character.Position.Around(80f));
            _jarvisVehicle = World.CreateVehicle(model, spawnPos);

            if (_jarvisVehicle != null)
            {
                Blip blip = _jarvisVehicle.AddBlip();
                blip.Sprite = BlipSprite.PersonalVehicleCar;
                blip.Color = BlipColor.White; // Black is not defined in all SHVDN versions
                blip.Name = "JARVIS";

                Function.Call(Hash.SET_VEHICLE_COLOURS, _jarvisVehicle, 12, 12); // 12 is matte black
                _jarvisVehicle.IsPersistent = true;
                _jarvisVehicle.EnginePowerMultiplier = 2.0f; // Leistungsschub
            }
        }

        private Ped GetOrSpawnDriver()
        {
            if (_jarvisVehicle.Driver != null && _jarvisVehicle.Driver.Exists() && _jarvisVehicle.Driver != Game.Player.Character)
            {
                return _jarvisVehicle.Driver;
            }

            Model pedModel = PedHash.StrPunk01GMY; // Generisches Modell
            pedModel.Request(10000);
            Ped driver = _jarvisVehicle.CreatePedOnSeat(VehicleSeat.Driver, pedModel);

            driver.IsVisible = false; // Unsichtbar für den Eindruck eines "Ghost-Drivers"
            driver.CanBeDraggedOutOfVehicle = false;
            driver.BlockPermanentEvents = true; // Ignoriert Schüsse, flüchtet nicht

            return driver;
        }

        private void PlayAudioFeedback(string sound, string soundSet)
        {
            // Native Audio Feedback von GTA V
            Function.Call(Hash.PLAY_SOUND_FRONTEND, -1, sound, soundSet, false);
        }

        private void OnAborted(object sender, EventArgs e)
        {
            // Aufräumarbeiten beim Reload/Beenden
            if (_jarvisVehicle != null && _jarvisVehicle.Exists())
            {
                _jarvisVehicle.MarkAsNoLongerNeeded();
                if (_jarvisVehicle.Driver != null && _jarvisVehicle.Driver.Exists())
                {
                    _jarvisVehicle.Driver.Delete();
                }
            }
        }
    }

    // ---------------------------------------------------------
    // 6. UI HELPER CLASS
    // ---------------------------------------------------------
    public static class JarvisUI
    {
        public static void ShowMessage(string message)
        {
            // Zeigt Subtitles im GTA V Stil (unten im Bildschirm)
            GTA.UI.Screen.ShowSubtitle($"~b~{message}", 4000);
        }
    }
}