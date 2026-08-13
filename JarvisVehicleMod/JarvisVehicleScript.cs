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
            public static Keys KeySummon = Keys.NumPad1;
            public static Keys KeyAutopilot = Keys.NumPad2;
            public static Keys KeyEMP = Keys.NumPad3;
            public static Keys KeyDefense = Keys.NumPad4;

            public static float SummonArrivalRadius = 5.0f;
            public static float EMPRadius = 30.0f;
            public static int EMPDurationMs = 10000;
            public static float DefenseScanRadius = 50.0f;

            public static float AutopilotSpeed = 30.0f;
            public static int NormalDrivingStyle = 786603;
            public static int RushedDrivingStyle = 1074528293;

            public static VehicleHash DefaultVehicleHash = VehicleHash.T20;
        }

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

        private Dictionary<Vehicle, int> _empDisabledVehicles = new Dictionary<Vehicle, int>();
        private bool _initialized = false;

        public JarvisVehicleScript()
        {
            Tick += OnTick;
            KeyDown += OnKeyDown;
            Aborted += OnAborted;
        }

        private void Initialize()
        {
            if (_initialized) return;

            if (Game.Player != null && Game.Player.Character != null)
            {
                GTA.UI.Notification.Show("~b~JARVIS System online.~w~ Bereit fuer Eingaben (Numpad 1-4).");
                _initialized = true;
            }
        }

        private void OnTick(object sender, EventArgs e)
        {
            Initialize();

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
                GTA.UI.Notification.Show($"~r~JARVIS Tick Error:~w~ {ex.Message}");
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

        private void SummonVehicle()
        {
            try
            {
                Ped playerPed = Game.Player.Character;

                if (playerPed.IsInVehicle())
                {
                    GTA.UI.Notification.Show("JARVIS: Sie sind bereits in einem Fahrzeug.");
                    return;
                }

                if (_jarvisVehicle == null || !_jarvisVehicle.Exists())
                {
                    SpawnJarvisVehicle();
                }

                if (_jarvisVehicle != null && _jarvisVehicle.Exists())
                {
                    Ped driver = GetOrSpawnDriver();
                    if (driver == null)
                    {
                        GTA.UI.Notification.Show("~r~JARVIS Fehler:~w~ Konnte keinen Fahrer generieren.");
                        return;
                    }

                    Vector3 playerPos = playerPed.Position;
                    Function.Call(Hash.TASK_VEHICLE_DRIVE_TO_COORD_LONGRANGE, driver, _jarvisVehicle, playerPos.X, playerPos.Y, playerPos.Z, Config.AutopilotSpeed, Config.NormalDrivingStyle, Config.SummonArrivalRadius);

                    _currentState = VehicleState.DrivingToPlayer;
                    GTA.UI.Notification.Show("JARVIS: Ich bin unterwegs zu Ihrer Position.");
                    PlayAudioFeedback("Beep_Green", "DLC_HEIST_HACKING_SNAKE_SOUNDS");
                }
            }
            catch (Exception ex)
            {
                GTA.UI.Notification.Show($"~r~Summon Error:~w~ {ex.Message}");
            }
        }

        private void ToggleAutopilot()
        {
            try
            {
                Ped playerPed = Game.Player.Character;

                if (!playerPed.IsInVehicle() || playerPed.CurrentVehicle != _jarvisVehicle)
                {
                    GTA.UI.Notification.Show("JARVIS: Autopilot ist nur im JARVIS-Fahrzeug verfuegbar.");
                    return;
                }

                if (_currentState == VehicleState.Autopilot)
                {
                    playerPed.Task.ClearAll();
                    _currentState = VehicleState.Idle;
                    GTA.UI.Notification.Show("JARVIS: Autopilot deaktiviert. Sie haben die Kontrolle.");
                    PlayAudioFeedback("Beep_Red", "DLC_HEIST_HACKING_SNAKE_SOUNDS");
                }
                else
                {
                    Vector3 waypoint = World.WaypointPosition;
                    if (waypoint != Vector3.Zero)
                    {
                        Function.Call(Hash.TASK_VEHICLE_DRIVE_TO_COORD_LONGRANGE, playerPed, _jarvisVehicle, waypoint.X, waypoint.Y, waypoint.Z, Config.AutopilotSpeed, Config.NormalDrivingStyle, 5.0f);
                        GTA.UI.Notification.Show("JARVIS: Autopilot aktiviert. Navigiere zum Zielpunkt.");
                    }
                    else
                    {
                        Function.Call(Hash.TASK_VEHICLE_DRIVE_WANDER, playerPed, _jarvisVehicle, Config.AutopilotSpeed, Config.RushedDrivingStyle);
                        GTA.UI.Notification.Show("JARVIS: Fluchtmodus aktiviert. Ausweichmanoever eingeleitet.");
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

        private void TriggerEmpBlast()
        {
            try
            {
                if (_jarvisVehicle == null || !_jarvisVehicle.Exists())
                {
                    GTA.UI.Notification.Show("JARVIS: Kein Fahrzeug in Reichweite fuer EMP.");
                    return;
                }

                GTA.UI.Notification.Show("JARVIS: EMP-Schockwelle initiiert.");
                PlayAudioFeedback("Beep_Red", "DLC_HEIST_HACKING_SNAKE_SOUNDS");
                Vector3 pos = _jarvisVehicle.Position;

                // Explode for visual/audio
                Function.Call(Hash.ADD_EXPLOSION, pos.X, pos.Y, pos.Z, 13, 0f, true, false, 0f); // 13 = WaterHydrant

                Vehicle[] vehicles = World.GetNearbyVehicles(pos, Config.EMPRadius);
                foreach (Vehicle veh in vehicles)
                {
                    if (veh != _jarvisVehicle && veh != Game.Player.Character.CurrentVehicle)
                    {
                        veh.EngineHealth = -1f;
                        veh.IsEngineRunning = false;

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
                GTA.UI.Notification.Show("JARVIS: Verteidigungsmodus aktiviert. Scanne Umgebung.");
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
                GTA.UI.Notification.Show("JARVIS: Verteidigungsmodus deaktiviert.");
                PlayAudioFeedback("Beep_Red", "DLC_HEIST_HACKING_SNAKE_SOUNDS");
            }
        }

        private void PlayAudioFeedback(string sound, string soundSet)
        {
            Function.Call(Hash.PLAY_SOUND_FRONTEND, -1, sound, soundSet, false);
        }

        private void ExecuteDefenseProtocol()
        {
            try
            {
                if (_jarvisVehicle == null || !_jarvisVehicle.Exists()) return;

                Ped playerPed = Game.Player.Character;

                if (_currentState == VehicleState.RammingTarget)
                {
                    if (_currentTarget == null || !_currentTarget.Exists() || _currentTarget.IsDead || _currentTarget.Position.DistanceTo(playerPed.Position) > Config.DefenseScanRadius * 1.5f)
                    {
                        _currentState = VehicleState.Idle;
                        _currentTarget = null;
                        GTA.UI.Notification.Show("JARVIS: Ziel eliminiert oder ausser Reichweite. Kehre zurueck.");
                        SummonVehicle();
                    }
                    return;
                }

                Ped[] peds = World.GetNearbyPeds(playerPed.Position, Config.DefenseScanRadius);
                foreach (Ped ped in peds)
                {
                    if (ped != playerPed && (ped.IsInCombatAgainst(playerPed) || ped.IsShooting))
                    {
                        _currentTarget = ped;
                        _currentState = VehicleState.RammingTarget;

                        Ped driver = GetOrSpawnDriver();
                        if (driver != null)
                        {
                            Function.Call(Hash.TASK_VEHICLE_MISSION_PED_TARGET, driver, _jarvisVehicle, ped, 8, Config.AutopilotSpeed, Config.RushedDrivingStyle, 0f, 0f, true);
                            GTA.UI.Notification.Show("JARVIS: Bedrohung erkannt. Leite Ramm-Gegenmassnahmen ein.");
                        }
                        break;
                    }
                }
            }
            catch
            {
                // Silent
            }
        }

        private void HandleState()
        {
            if (_currentState == VehicleState.DrivingToPlayer && _jarvisVehicle != null && _jarvisVehicle.Exists())
            {
                Ped playerPed = Game.Player.Character;
                if (playerPed != null && _jarvisVehicle.Position.DistanceTo(playerPed.Position) <= Config.SummonArrivalRadius)
                {
                    if (_jarvisVehicle.Driver != null && _jarvisVehicle.Driver != playerPed)
                    {
                        _jarvisVehicle.Driver.Task.ClearAll();
                        _jarvisVehicle.Driver.Delete(); // Fahrer entfernen, damit der Spieler einsteigen kann
                    }

                    _jarvisVehicle.SoundHorn(500);
                    Function.Call(Hash.SET_VEHICLE_DOORS_LOCKED, _jarvisVehicle, 1); // 1 = Unlocked

                    _currentState = VehicleState.Idle;
                    GTA.UI.Notification.Show("JARVIS: Ich bin an Ihrer Position angekommen.");
                }
            }
        }

        private void HandleEmpTimers()
        {
            List<Vehicle> toRemove = new List<Vehicle>();
            foreach (var kvp in _empDisabledVehicles)
            {
                if (Game.GameTime > kvp.Value)
                {
                    if (kvp.Key != null && kvp.Key.Exists())
                    {
                        kvp.Key.EngineHealth = 1000f;
                    }
                    toRemove.Add(kvp.Key);
                }
            }

            foreach (var veh in toRemove)
            {
                _empDisabledVehicles.Remove(veh);
            }
        }

        private void SpawnJarvisVehicle()
        {
            try
            {
                Model model = new Model(Config.DefaultVehicleHash);
                model.Request(10000);

                if (!model.IsInCdImage || !model.IsValid)
                {
                    GTA.UI.Notification.Show("~r~JARVIS Fehler:~w~ Fahrzeugmodell ungueltig.");
                    return;
                }

                Ped playerPed = Game.Player.Character;
                Vector3 spawnPos = playerPed.Position + playerPed.ForwardVector * 15.0f; // Naeher spawnen (15m vor Spieler)

                _jarvisVehicle = World.CreateVehicle(model, spawnPos);

                if (_jarvisVehicle != null && _jarvisVehicle.Exists())
                {
                    Blip blip = _jarvisVehicle.AddBlip();
                    blip.Sprite = BlipSprite.PersonalVehicleCar;
                    blip.Color = BlipColor.White;
                    blip.Name = "JARVIS";

                    Function.Call(Hash.SET_VEHICLE_COLOURS, _jarvisVehicle, 12, 12);
                    _jarvisVehicle.IsPersistent = true;
                    _jarvisVehicle.EnginePowerMultiplier = 2.0f;
                }
                else
                {
                    GTA.UI.Notification.Show("~r~JARVIS Fehler:~w~ Konnte Fahrzeug nicht spawnen.");
                }
            }
            catch (Exception ex)
            {
                GTA.UI.Notification.Show($"~r~Spawn Error:~w~ {ex.Message}");
            }
        }

        private Ped GetOrSpawnDriver()
        {
            try
            {
                Ped playerPed = Game.Player.Character;
                if (_jarvisVehicle.Driver != null && _jarvisVehicle.Driver.Exists() && _jarvisVehicle.Driver != playerPed)
                {
                    return _jarvisVehicle.Driver;
                }

                Model pedModel = PedHash.StrPunk01GMY;
                pedModel.Request(10000);

                Ped driver = _jarvisVehicle.CreatePedOnSeat(VehicleSeat.Driver, pedModel);
                if (driver != null && driver.Exists())
                {
                    driver.IsVisible = false;
                    driver.CanBeDraggedOutOfVehicle = false;
                    driver.BlockPermanentEvents = true;
                    return driver;
                }
                return null;
            }
            catch (Exception ex)
            {
                GTA.UI.Notification.Show($"~r~Driver Error:~w~ {ex.Message}");
                return null;
            }
        }

        private void OnAborted(object sender, EventArgs e)
        {
            if (_jarvisVehicle != null && _jarvisVehicle.Exists())
            {
                _jarvisVehicle.MarkAsNoLongerNeeded();
                if (_jarvisVehicle.Driver != null && _jarvisVehicle.Driver.Exists() && _jarvisVehicle.Driver != Game.Player.Character)
                {
                    _jarvisVehicle.Driver.Delete();
                }
            }
        }
    }
}