using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Lunarbin.Valheim.CrossServerPortals
{
    [BepInPlugin("lunarbin.games.valheim", "Valheim Cross Server Portals Continued", BuildInfo.Version)]
    public class CrossServerPortals : BaseUnityPlugin
    {
        private static CrossServerPortals instance = null!;
        private static TeleportInfo? teleportInfo;
        private static bool teleportingToServer;
        private static bool hasJoinedServer;
        private static float portalExitDistance;

        private static List<ZDO> knownPortals = new();

        public new static readonly ManualLogSource Logger = BepInEx.Logging.Logger.CreateLogSource("CrossServerPortals");
        
        private readonly Harmony _harmony = new Harmony("lunarbin.games.valheim");


        private void Awake()
        {
            instance = this;
            _harmony.PatchAll();
            CSPConfig.Init(Config);
            Config.SettingChanged += OnConfigChanged;
            StartCoroutine(RunOncePerMinute());
        }

        private static void OnConfigChanged(object sender, SettingChangedEventArgs e)
        {
            if (e.ChangedSetting.Definition.Key == CSPConfig.recolorPortalEffects.Definition.Key
                || e.ChangedSetting.Definition.Key == CSPConfig.recolorPortalGlyphs.Definition.Key
                || e.ChangedSetting.Definition.Key == CSPConfig.customPortalGlyphColor.Definition.Key
                || e.ChangedSetting.Definition.Key == CSPConfig.customPortalEffectColor.Definition.Key)
            {
                ModifyPortalColors.OnColorSettingsChanged();
            }
            
        }

        private static IEnumerator RunOncePerMinute()
        {
            while (true)
            {
                yield return new WaitForSecondsRealtime(60);
                
                ModifyPortalColors.GarbageCollect();
            }
            // ReSharper disable once IteratorNeverReturns
        }

        // Patch TeleportWorld.Teleport.
        // If the portal tag matches the PortalTagRegex, use the custom Teleport method instead.
        [HarmonyPatch(typeof(TeleportWorld), nameof(TeleportWorld.Teleport))]
        internal static class PatchTeleport
        {
            private static bool Prefix(TeleportWorld __instance, ref Player player)
            {
                string portalTag = __instance.GetText();

                if (TeleportInfo.PortalTagIsTeleportInfo(portalTag))
                {
                    if (CSPConfig.promptBeforeTeleport.Value)
                    {
                        UnifiedPopup.Push(new YesNoPopup("Cross Server Teleport", $"Do you want to teleport to {portalTag}?",
                            delegate
                            {
                                TeleportToServer(portalTag, __instance);
                                UnifiedPopup.Pop();
                            }, delegate
                            {
                                Player.m_localPlayer.Message(MessageHud.MessageType.Center, $"Cancelled Cross Server Teleport");
                                UnifiedPopup.Pop();
                            }));
                    }
                    else
                    {
                        
                        TeleportToServer(portalTag, __instance);
                    }
                    
                    return false;
                }

                return true;
            }
        }

        // TeleportToServer simply logs the player out.
        // The connection will be handled on the main menu.
        public static void TeleportToServer(string tag, TeleportWorld portal)
        {
            teleportInfo = TeleportInfo.ParsePortalTag(tag);
            if (teleportInfo != null)
            {
                PreserveStatusEffects.SaveStatusEffects(Player.m_localPlayer);

                Game.instance.IncrementPlayerStat(PlayerStatType.PortalsUsed);

                teleportingToServer = true;
                hasJoinedServer = false;
                Player.m_localPlayer.Message(MessageHud.MessageType.Center, $"Teleporting to {tag}");
                instance.StartCoroutine(LogoutThroughPortal(portal));
                return;
            }

            Player.m_localPlayer.Message(MessageHud.MessageType.Center, $"Invalid portal tag: {tag}");
        }

        private static IEnumerator LogoutThroughPortal(TeleportWorld portal)
        {
            yield return new WaitForSecondsRealtime(1f);
            if (Player.m_localPlayer != null && portal != null)
            {
                Player player = Player.m_localPlayer;
                MovePlayerToPortalExit(ref player, ref portal);
            }
            Game.instance.Logout();
        }

        [HarmonyPatch(typeof(Player), "ShowTeleportAnimation")]
        internal static class PatchShowTeleportAnimation
        {
            private static bool Prefix(Player __instance, ref bool __result)
            {
                if (teleportingToServer)
                {
                    __result = true;
                    return false;
                }

                return true;
            }
        }
        
        [HarmonyPatch(typeof(Player), "IsTeleporting")]
        internal static class PatchIsTeleporting
        {
            private static bool Prefix(Player __instance, ref bool __result)
            {
                if (teleportingToServer)
                {
                    __result = true;
                    return false;
                }

                return true;
            }
        }
        
        [HarmonyPatch(typeof(Player), "CanMove")]
        internal static class PatchPlayerCanMove
        {
            private static bool Prefix(Player __instance, ref bool __result)
            {
                if (teleportingToServer)
                {
                    __result = false;
                    return false;
                }

                return true;
            }
        }

        // Move the player to the portal exit position.
        // This is called prior to logging the player out to prevent
        // the player from logging back in inside the portal.
        public static void MovePlayerToPortalExit(ref Player player, ref TeleportWorld portal)
        {
            var position = portal.transform.position;
            var rotation = portal.transform.rotation;
            var offsetDir = rotation * Vector3.forward;
            var newPos = position + offsetDir * portal.m_exitDistance + Vector3.up;
            player.transform.position = newPos;
            player.transform.rotation = rotation;
            // Set the portal exit distance to double the normal
            portalExitDistance = portal.m_exitDistance * 2;
        }

        // Patch TeleportWorld.Interact
        // This is to increase character limit for portal tags.
        [HarmonyPatch(typeof(TeleportWorld), "Interact")]
        internal class PatchTeleportWorldInteract
        {
            private static bool Prefix(ref Humanoid human, ref bool hold, ref bool __result, TeleportWorld __instance)
            {
                if (hold)
                {
                    __result = false;
                    return false;
                }

                if (!PrivateArea.CheckAccess(__instance.transform.position))
                {
                    human.Message(MessageHud.MessageType.Center, "$piece_noaccess");
                    __result = true;
                    return false;
                }


                string portalName = __instance.GetText();
                if (portalName.Contains("|"))
                {
                    if (CSPConfig.requireAdminToRename.Value)
                    {
                        if (Admin.IsAdmin)
                        {
                            // nothing
                        }
                        else
                        {
                            human.Message(MessageHud.MessageType.Center, "$piece_noaccess");

                            return false;
                        }
                    }
                }

                TextInput.instance.RequestText(__instance, "$piece_portal_tag", 50);

                __result = true;
                return false;
            }
        }


        // Patch FejdStartup.Start
        // This hooks in to connect to a server if the player has used
        // a cross-server portal to logout from a server.
        [HarmonyPatch(typeof(FejdStartup), "Start")]
        internal static class PatchFejdStartupStart
        {
            private static void Postfix()
            {
                if (teleportInfo != null && teleportingToServer && !hasJoinedServer)
                {
                    TeleportInfo destination = teleportInfo.Value;
                    if (destination.Type == TeleportInfo.PortalType.World)
                    {
                        string world = destination.Address;
                        if (world != "")
                        {
                            List<World> worlds = SaveSystem.GetWorldList();
                            foreach (var w in worlds)
                            {
                                if (w.m_name == world)
                                {
                                    PlayerPrefs.SetString("world", world);

                                    FejdStartup.instance.OnStartGame();
                                    FejdStartup.instance.OnWorldStart();
                                    return;
                                }
                            }
                            UnifiedPopup.Push(new WarningPopup("World Not Found", $"World {world} does not exist.",
                                delegate
                                {
                                    UnifiedPopup.Pop();
                                }));
                        }
                    }
                    else
                    {
                        //ServerJoinDataDedicated joinDataDedicated = new ServerJoinDataDedicated(ServerToJoin?.Address, (ushort)(ServerToJoin?.Port));
                        ServerJoinData joinData =
                            new ServerJoinData(new ServerJoinDataDedicated(destination.Address, destination.Port));

                        FejdStartup.instance.SetServerToJoin(joinData);
                        FejdStartup.instance.JoinServer();
                        hasJoinedServer = true; // Set that we have joined a server so we dont get stuck in reconnect loop.
                    }
                    //ZSteamMatchmaking.instance.QueueServerJoin($"{ServerToJoin?.Address}:{ServerToJoin?.Port}");
                }
            }
        }


        // Patch FejdStartup.ShowCharacterSelection
        // This confirms the Character Selector with the current character
        // if the player has used a cross-server portal to logout from a server.
        [HarmonyPatch(typeof(FejdStartup), "ShowCharacterSelection")]
        internal class PatchFejdStartupShowCharacterSelection
        {
            private static void Postfix()
            {
                if (teleportInfo != null && teleportingToServer && !hasJoinedServer)
                {
                    hasJoinedServer = true;
                    FejdStartup.instance.OnCharacterStart();
                }
            }
        }

        // Patch Game.SpawnPlayer
        // This teleports the player to the selected portal tag when they login
        // instead of their logged-out location.
        [HarmonyPatch(typeof(Game), "SpawnPlayer")]
        internal class PatchGameSpawnPlayer
        {
            private static void Prefix(ref Vector3 spawnPoint)
            {
                if (teleportingToServer && teleportInfo != null && teleportInfo?.TargetTag != "")
                {
                    List<ZDO> zdos = ZDOMan.instance.GetPortalList();
                    Vector3? targetPos = null;
                    foreach (var portal in zdos)
                    {
                        if (portal == null) continue;
                        string tag = portal.GetString(ZDOVars.s_tag);
                        // If the tag is a direct match OR has a SourceTag that is
                        // then change the SpawnPoint to the portal exit location.
                        if (tag == teleportInfo?.TargetTag
                            || TeleportInfo.ParsePortalTag(tag)?.SourceTag == teleportInfo?.TargetTag)
                        {
                            var position = portal.GetPosition();
                            var rotation = portal.GetRotation();
                            var offsetDir = rotation * Vector3.forward;
                            spawnPoint = position + offsetDir * portalExitDistance + Vector3.up;
                            targetPos = spawnPoint;
                            break;
                        }
                    }
                    instance.StartCoroutine(FinishCrossServerTeleport(2f, targetPos));
                    return;
                }
                // Null the ServerToJoin since we only just connected.
                
                teleportingToServer = false;
                teleportInfo = null;
            }
        }

        private static IEnumerator FinishCrossServerTeleport(float seconds, Vector3? position)
        {
            yield return new WaitForSecondsRealtime(seconds);

            if (position.HasValue && Player.m_localPlayer != null)
            {
                Player.m_localPlayer.transform.position = position.Value;
            }

            teleportingToServer = false;
            teleportInfo = null;
        }

        // Patch Game.Start to track portals when the game starts.
        [HarmonyPatch(typeof(Game), "Start")]
        internal class PatchGameStart
        {
            private static void Postfix()
            {
                knownPortals.Clear();
                Game.instance.StartCoroutine(FetchPortals());
            }

            // Every 10 seconds, refresh the list of known portals.
            private static IEnumerator FetchPortals()
            {
                while (ZNet.instance == null || ZDOMan.instance == null)
                {
                    yield return null;
                }

                // Portal force-sending is server bookkeeping; clients do not need this loop.
                if (!ZNet.instance.IsServer()) yield break;

                while (true)
                {
                    if (ZDOMan.instance == null || ZNet.instance == null || !ZNet.instance.IsServer())
                    {
                        yield break;
                    }

                    List<ZDO> portals = ZDOMan.instance.GetPortalList();
                    HashSet<ZDOID> knownPortalIds = new HashSet<ZDOID>(knownPortals
                        .Where(portal => portal != null)
                        .Select(portal => portal.m_uid));

                    // Send the portals to the connected client.
                    foreach (ZDO zdo in portals)
                    {
                        if (zdo != null && !knownPortalIds.Contains(zdo.m_uid))
                        {
                            ZDOMan.instance.ForceSendZDO(zdo.m_uid);
                        }
                    }

                    knownPortals = portals;

                    yield return new WaitForSeconds(10f);
                }
            }
        }

        // When a player connects to the server, send them the list of all portals.
        [HarmonyPatch(typeof(ZDOMan), "AddPeer")]
        internal class PatchZDOManAddPeer
        {
            private static void Postfix(ZDOMan __instance, ZNetPeer netPeer)
            {
                if (ZNet.instance.IsServer())
                {
                    foreach (ZDO zdo in knownPortals)
                    {
                        __instance.ForceSendZDO(netPeer.m_uid, zdo.m_uid);
                    }
                }
            }
        }
    }
}
