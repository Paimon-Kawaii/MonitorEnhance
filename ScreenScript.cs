using System;
using System.Linq;
using GameNetcodeStuff;
using MonitorEnhance.Utils;
using UnityEngine;

namespace MonitorEnhance;

public class ScreenScript : MonoBehaviour
{
    private static PlayerControllerB LOCAL_PLAYER => GameNetworkManager.Instance?.localPlayerController;
    private static ManualCameraRenderer MAP_RENDERER => StartOfRound.Instance?.mapScreen;
    private static TransformAndName RADAR_TARGET => MAP_RENDERER.GetCurrentRadarTarget();
    private bool _lookingAtMonitor = false;
    private float _tpCoolDown = 0f;

    //private static GameObject ShipScreen => GameObject.Find("Environment/HangarShip/ShipModels2b/MonitorWall/Cube.001").GetComponent<MeshRenderer>().gameObject;

    private Bounds GetMonitorScreenBounds()
    {
        return Plugin.CREATE_BOUNDS.Invoke(gameObject);
    }

    //private Bounds GetShipScreenBounds()
    //{
    //    return Plugin.CREATE_BOUNDS.Invoke(ShipScreen);
    //}

    private bool IsLookingAtMonitor()
    {
        PlayerControllerB player = LOCAL_PLAYER;
        if (player is not null && player.isInHangarShipRoom)
        {
            Ray lookRay = new Ray(player.gameplayCamera.transform.position, player.gameplayCamera.transform.forward);
            Bounds bounds = GetMonitorScreenBounds();

            float distance;
            if (bounds.IntersectRay(lookRay, out distance) && distance <= player.grabDistance * 1.5f)
                return true;
            //bounds = GetShipScreenBounds();
            //if (bounds.IntersectRay(lookRay, out distance) && distance <= player.grabDistance * 1.5f)
            //    return true;
        }
        return false;
    }

    //private Vector3 GetMonitorCoordinates(Bounds bounds, Vector3 point)
    //{
    //    return new Vector3(
    //        1f - 1f / Math.Abs(bounds.max.z - bounds.min.z) * (point.z - bounds.min.z),
    //        1f / Math.Abs(bounds.max.y - bounds.min.y) * (point.y - bounds.min.y),
    //        0
    //    );
    //}

    internal void OnPlayerInteraction(bool isSecondary)
    {
        if (!(Plugin.IsActive && _lookingAtMonitor)) return;

        PlayerControllerB player = LOCAL_PLAYER;
        if (player is null) return;

        TransformAndName target = RADAR_TARGET;
        if (target is null) return;

        if (target.isNonPlayer)
            TriggerRadar(target.transform.GetComponent<RadarBoosterItem>(), isSecondary);
        else if (!isSecondary) Teleport();
        else TriggerTrap();

        #region 原逻辑
            //if (player != null && IsLookingAtMonitor(out Bounds bounds, out Ray lookRay, out Ray camRay))
            //{
            //    foreach (Collider x in Physics.OverlapCapsule(camRay.GetPoint(0), camRay.GetPoint(10), _isCloseMax))
            //    {
            //        if (!isPlayer && x.GetComponent<TerminalAccessibleObject>() is TerminalAccessibleObject tObject)
            //        {
            //            // Clicked on BigDoor, Land mine, Turret
            //            tObject.CallFunctionFromTerminal();
            //            return;
            //        }
            //        else if (x.GetComponent<RadarBoosterItem>() is RadarBoosterItem rItem)
            //        {
            //            // Clicked on Radar booster
            //            TriggerRadar(rItem, isPlayer);
            //            return;
            //        }
            //        else if (x.GetComponent<PlayerControllerB>() is PlayerControllerB tgtPlayer)
            //        {
            //            // Clicked on player or radar the player is holding
            //            if (!TriggerRadar(tgtPlayer.currentlyHeldObjectServer?.GetComponent<RadarBoosterItem>(), isPlayer) && !isPlayer)
            //            {
            //                List<TransformAndName> list = MAP_RENDERER.radarTargets;
            //                for (int i = 0; i < list.Count; i++)
            //                {
            //                    if (tgtPlayer.transform.Equals(list[i].transform))
            //                    {
            //                        MAP_RENDERER.SwitchRadarTargetAndSync(i);
            //                        return;
            //                    }
            //                }
            //            }
            //        }
            //    }
            //}
            #endregion
    }

    private void TriggerRadar(RadarBoosterItem rItem, bool isSecondary)
    {
        if (rItem is null) return;

        if (isSecondary) rItem.FlashAndSync();
        else rItem.PlayPingAudioAndSync();
    }

    private void Teleport(bool isInverse = false)
    {
        ShipTeleporter[] shipTeleporters = FindObjectsOfType<ShipTeleporter>();
        ShipTeleporter shipTeleporter = shipTeleporters.Where(x => x.isInverseTeleporter == isInverse).FirstOrDefault();
        if (shipTeleporter is null) return;

        TransformAndName target = RADAR_TARGET;
        Plugin.LOGGER.LogInfo($"> 传送: {target.name}");
        if (target is null) return;

        var cooldownTime = (float)typeof(ShipTeleporter).GetField("cooldownTime", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(shipTeleporter);
        if (cooldownTime > 5.0f)
            return;

        shipTeleporter.PressTeleportButtonOnLocalClient();
    }

    private void TriggerTrap()
    {
        if (!(Plugin.IsActive && _lookingAtMonitor)) return;

        TransformAndName target = RADAR_TARGET;
        if (target is null) return;
        if (target.isNonPlayer) return;

        PlayerControllerB player = target.transform.GetComponent<PlayerControllerB>();
        if (player is null) return;

        Vector2 pVec = new Vector2(target.transform.position.x, target.transform.position.y);
        TerminalAccessibleObject[] traps = FindObjectsOfType<TerminalAccessibleObject>();
        foreach (TerminalAccessibleObject trap in traps)
        {
            Vector2 tVec = new Vector2(trap.transform.position.x, trap.transform.position.y);
            float distance = (pVec - tVec).magnitude;
            Plugin.LOGGER.LogInfo($"> [{trap.objectCode}] 与 {player.name} 距离: {distance}");
            if (distance <= player.grabDistance * 4f)
                trap.CallFunctionFromTerminal();
        }
    }

#if DEBUG
    internal void EmergencyTeleport()
    {
        if (_tpCoolDown > 0) return;

        TransformAndName target = RADAR_TARGET;
        PlayerControllerB player = target?.transform.GetComponent<PlayerControllerB>();
        if (player is null) return;

        StartCoroutine(TeleportPlayerCoroutine((int)player.playerClientId, LOCAL_PLAYER.transform.position));

        _tpCoolDown = 20f;
#if DEBUG
            _tpCoolDown = 2f;
#endif
    }
#endif

    internal void ShipDoor()
    {
        if (!(Plugin.IsActive && _lookingAtMonitor)) return;

        GameObject shipdoor = FindObjectOfType<HangarShipDoor>().gameObject;
        string animation = "CloseDoor";
        if (StartOfRound.Instance.hangarDoorsClosed)
            animation = "OpenDoor";

        shipdoor.GetComponentsInChildren<AnimatedObjectTrigger>()
            .First(trigger => trigger.animationString == animation)?
            .GetComponentInParent<InteractTrigger>().onInteract.Invoke(GameNetworkManager.Instance?.localPlayerController);
    }

    internal void OnMonitorQuickSwitch(bool isPlayer)
    {
        if (!(Plugin.IsActive && _lookingAtMonitor)) return;

        PlayerControllerB player = LOCAL_PLAYER;
        if (player?.isInHangarShipRoom == true)
        {
            Vector3 vec = gameObject.transform.position - player.transform.position;
            float distance = Math.Abs(vec.x) + Math.Abs(vec.y) + Math.Abs(vec.z);
            if (distance < 10f)
            {
                if (isPlayer) MAP_RENDERER.SwitchMonitorPlayer();
                else MAP_RENDERER.SwitchMonitorRadar();
            }
        }
    }

    internal void OnPlayerQuickSwitchByNum()
    {
        if (!(Plugin.IsActive && _lookingAtMonitor)) return;

        if(InputUtil.INPUT_QUICKSWITCH_1.IsPressed())
            MAP_RENDERER.SwitchRadarTargetByNum(1);
        else if (InputUtil.INPUT_QUICKSWITCH_2.IsPressed())
            MAP_RENDERER.SwitchRadarTargetByNum(2);
        else if (InputUtil.INPUT_QUICKSWITCH_3.IsPressed())
            MAP_RENDERER.SwitchRadarTargetByNum(3);
        else if (InputUtil.INPUT_QUICKSWITCH_4.IsPressed())
            MAP_RENDERER.SwitchRadarTargetByNum(4);
        else if (InputUtil.INPUT_QUICKSWITCH_5.IsPressed())
            MAP_RENDERER.SwitchRadarTargetByNum(5);
        else if (InputUtil.INPUT_QUICKSWITCH_6.IsPressed())
            MAP_RENDERER.SwitchRadarTargetByNum(6);
        else if (InputUtil.INPUT_QUICKSWITCH_7.IsPressed())
            MAP_RENDERER.SwitchRadarTargetByNum(7);
        else if (InputUtil.INPUT_QUICKSWITCH_8.IsPressed())
            MAP_RENDERER.SwitchRadarTargetByNum(8);
        else if (InputUtil.INPUT_QUICKSWITCH_9.IsPressed())
            MAP_RENDERER.SwitchRadarTargetByNum(9);
        else if (InputUtil.INPUT_QUICKSWITCH_0.IsPressed())
            MAP_RENDERER.SwitchRadarTargetByNum(0);
    }

    private void OnEnable()
    {
        InputUtil.SCREEN_SCRIPT = this;

        PlayerControllerB player = LOCAL_PLAYER;
        if (player is null)
        {
            Plugin.LOGGER.LogWarning("Unable to activate monitor touchscreen. Reason: Failed to get local player.");
            return;
        }
        else if (InputUtil.INPUT_PRIMARY != null || InputUtil.INPUT_SECONDARY != null)
            return;
    }

    private void OnDisable()
    {
        InputUtil.SCREEN_SCRIPT = null;
    }

    private void Update()
    {
        _tpCoolDown -= Time.deltaTime;
        if(_tpCoolDown < 0) _tpCoolDown = 0;

        PlayerControllerB player = LOCAL_PLAYER;
        player.isGrabbingObjectAnimation = false;
        if (!Plugin.IsActive) return;

        _lookingAtMonitor = IsLookingAtMonitor();
        if (_lookingAtMonitor)
        {
            player.isGrabbingObjectAnimation = true; // Blocks the default code from overwriting it again
#if DEBUG
            if (ConfigUtil.CONFIG_SHOW_POINTER.Value)
            {
                // Display Pointer
                player.cursorIcon.enabled = true;
                player.cursorIcon.sprite = ConfigUtil.HOVER_ICON;
            }
#endif
            if (ConfigUtil.CONFIG_SHOW_TOOLTIP.Value)
            {
                // Display Tooltips
                TransformAndName target = RADAR_TARGET;
                if (target is null) return;

                player.cursorTip.text = string.Format(
                    $"[{InputUtil.GetButtonDescription(InputUtil.INPUT_DOOR_SWITCH)}] " +
                        (StartOfRound.Instance.hangarDoorsClosed ? LocalizationManager.GetString("Open") : LocalizationManager.GetString("Close")) +
                        " " + LocalizationManager.GetString("ShipDoor") + "\n" +

                    $"[{InputUtil.GetButtonDescription(InputUtil.INPUT_QUICKSWITCH)} & 0-9] "+ LocalizationManager.GetString("SwitchPlayer") + "\n" +

                    $"[{InputUtil.GetButtonDescription(InputUtil.INPUT_PRIMARY)}] " +
                        (target.isNonPlayer ? LocalizationManager.GetString("PingRadar") : LocalizationManager.GetString("TPPlayer")) + "\n" +


                    $"[{InputUtil.GetButtonDescription(InputUtil.INPUT_SECONDARY)}] " +
                        (target.isNonPlayer ? LocalizationManager.GetString("FlashRadar") : LocalizationManager.GetString("TriggerTrap")) + "\n" +

                    $"[{InputUtil.GetButtonDescription(InputUtil.INPUT_ALT_QUICKSWITCH)}] " + LocalizationManager.GetString("SwitchRadar") + "\n"
                );
            }
        }
    }
}