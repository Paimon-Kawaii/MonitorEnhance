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
    private TransformAndName _selectedTarget;
    private const float _isCloseMax = 2f;
    private bool _isLookingAtMonitor = false;
    private bool _isTargetSelected = false;
    private float _tpCoolDown = 0f;
    
    private Bounds GetMonitorScreenBounds()
    {
        return Plugin.CREATE_BOUNDS.Invoke(this.gameObject);
    }

    private bool GetMonitorCameraRay(out Ray camRay)
    {
        PlayerControllerB ply = LOCAL_PLAYER;
        if (ply is not null && ply.isInHangarShipRoom)
        {
            Ray lookRay = new(ply.gameplayCamera.transform.position, ply.gameplayCamera.transform.forward);
            Bounds bounds = this.GetMonitorScreenBounds();

            if (bounds.IntersectRay(lookRay, out float distance) && distance <= ply.grabDistance)
            {
                camRay = MAP_RENDERER.cam.ViewportPointToRay(this.GetMonitorCoordinates(bounds, lookRay.GetPoint(distance)));
                return true;
            }
        }

        camRay = default;
        return false;
    }

    private bool IsLookingAtMonitor()
    {
        PlayerControllerB player = LOCAL_PLAYER;
        if (player is not null && player.isInHangarShipRoom)
        {
            Ray lookRay = new(player.gameplayCamera.transform.position, player.gameplayCamera.transform.forward);
            Bounds bounds = this.GetMonitorScreenBounds();

            if (bounds.IntersectRay(lookRay, out float distance) && distance <= player.grabDistance * 1.5f) 
                return true;
        }

        return false;
    }

    private Vector3 GetMonitorCoordinates(Bounds bounds, Vector3 point) => new(
            1f - 1f / Math.Abs(bounds.max.z - bounds.min.z) * (point.z - bounds.min.z),
            1f / Math.Abs(bounds.max.y - bounds.min.y) * (point.y - bounds.min.y),
            0
        );

    internal void OnSelectTarget()
    {
        if (!(Plugin.IsActive && _isLookingAtMonitor)) return;

        TransformAndName target = RADAR_TARGET;
        if (target is null) return;

        _selectedTarget = target;
        _isTargetSelected = true;
    }

    internal void OnPlayerInteraction(bool isSecondary)
    {   
        if (!(Plugin.IsActive && _isLookingAtMonitor)) return;

        PlayerControllerB player = LOCAL_PLAYER;
        if (player is null) return;

        if (_isTargetSelected)
        {
            TransformAndName target = _selectedTarget;
            if (target.isNonPlayer)
                this.TriggerRadar(target.transform.GetComponent<RadarBoosterItem>(), isSecondary);
            else if (!isSecondary) this.Teleport();
            else this.TriggerTrap();
        }
        else if (isSecondary) this.TriggerTrap();

        #region 原逻辑
        //if (player != null && IsLookingAtMonitor(out Bounds bounds, out Ray lookRay, out Ray camRay))
        //{
        //    foreach (Collider collider in Physics.OverlapCapsule(camRay.GetPoint(0), camRay.GetPoint(10), _isCloseMax))
        //    {
        //        if (!isPlayer && collider.GetComponent<TerminalAccessibleObject>() is TerminalAccessibleObject trap)
        //        {
        //            // Clicked on BigDoor, Land mine, Turret
        //            trap.CallFunctionFromTerminal();
        //            return;
        //        }
        //        else if (collider.GetComponent<RadarBoosterItem>() is RadarBoosterItem rItem)
        //        {
        //            // Clicked on Radar booster
        //            TriggerRadar(rItem, isPlayer);
        //            return;
        //        }
        //        else if (collider.GetComponent<PlayerControllerB>() is PlayerControllerB tgtPlayer)
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

        TransformAndName target = _selectedTarget;
        Plugin.LOGGER.LogInfo($"> 传送: {target.name}");
        if (target is null) return;

        float cooldownTime = (float)typeof(ShipTeleporter).GetField("cooldownTime", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(shipTeleporter);
        if (cooldownTime > 5.0f)
            return;

        shipTeleporter.PressTeleportButtonOnLocalClient();
    }

    private void TriggerTrap()
    {
        if (!(Plugin.IsActive && _isLookingAtMonitor)) return;

        if (!this.GetMonitorCameraRay(out Ray camRay)) return;
        foreach (Collider collider in Physics.OverlapCapsule(camRay.GetPoint(0), camRay.GetPoint(10), _isCloseMax))
            if (collider.GetComponent<TerminalAccessibleObject>() is TerminalAccessibleObject trap)
                trap.CallFunctionFromTerminal(); // Clicked on BigDoor, Land mine, Turret

        #region 旧逻辑
        TransformAndName target = _selectedTarget;
        if (target is null) return;
        PlayerControllerB player = target.transform.GetComponent<PlayerControllerB>();
        if (player is null) return;

        Vector2 pVec = new(target.transform.position.x, target.transform.position.y);
        TerminalAccessibleObject[] traps = FindObjectsOfType<TerminalAccessibleObject>();
        foreach (TerminalAccessibleObject trap in traps)
        {
            if (trap.isBigDoor) continue;
            Vector2 tVec = new(trap.transform.position.x, trap.transform.position.y);
            float distance = (pVec - tVec).magnitude;
            Plugin.LOGGER.LogInfo($"> [{trap.objectCode}] 与 {player.name} 距离: {distance}");
            if (distance <= player.grabDistance * 4f)
                trap.CallFunctionFromTerminal();
        }
        #endregion
    }

    internal void ShipDoor()
    {
        if (!(Plugin.IsActive && _isLookingAtMonitor)) return;

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
        if (!(Plugin.IsActive && _isLookingAtMonitor)) return;

        PlayerControllerB player = LOCAL_PLAYER;
        Vector3 vec = this.gameObject.transform.position - player.transform.position;
        float distance = Math.Abs(vec.x) + Math.Abs(vec.y) + Math.Abs(vec.z);
        if (distance < 10f)
        {
            if (isPlayer) MAP_RENDERER.SwitchMonitorPlayer();
            else MAP_RENDERER.SwitchMonitorRadar();
        }
    }

    internal void OnPlayerQuickSwitchByNum()
    {
        if (!(Plugin.IsActive && _isLookingAtMonitor)) return;

        if (InputUtil.INPUT_QUICKSWITCH_1.IsPressed())
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
    }

    private void OnDisable()
    {
        InputUtil.SCREEN_SCRIPT = null;
    }

    private void Update()
    {
        _tpCoolDown -= Time.deltaTime;
        if (_tpCoolDown < 0) _tpCoolDown = 0;

        PlayerControllerB player = LOCAL_PLAYER;
        player.isGrabbingObjectAnimation = false;
        if (!Plugin.IsActive) return;

        _isLookingAtMonitor = this.IsLookingAtMonitor();
        if (_isLookingAtMonitor)
        {
            player.isGrabbingObjectAnimation = true; // Blocks the default code from overwriting it again

            #region 原版准星开关
            //if (ConfigUtil.CONFIG_SHOW_POINTER.Value)
            //{
            //    // Display Pointer
            //    player.cursorIcon.enabled = true;
            //    player.cursorIcon.sprite = ConfigUtil.HOVER_ICON;
            //}
            #endregion

            player.cursorIcon.enabled = true;
            player.cursorIcon.sprite = ConfigUtil.HOVER_ICON;

            if (_isTargetSelected && RADAR_TARGET != _selectedTarget)
                _isTargetSelected = false;

            if(!ConfigUtil.SELECT_REQUIRE)
            {
                _selectedTarget = RADAR_TARGET;
                _isTargetSelected = true;
            }

            if (ConfigUtil.CONFIG_SHOW_TOOLTIP.Value)
            {
                // Display Tooltips
                TransformAndName target = _selectedTarget;
                if (!_isTargetSelected) target = RADAR_TARGET;

                player.cursorTip.text = string.Empty;
                player.cursorTip.text += $"[ {InputUtil.GetButtonDescription(InputUtil.INPUT_DOOR_SWITCH)} ] "
                    + (StartOfRound.Instance.hangarDoorsClosed ? LocalizationManager.GetString("Open") : LocalizationManager.GetString("Close"))
                    + LocalizationManager.GetString("ShipDoor") + "\n";

                player.cursorTip.text +=
                    _isTargetSelected ? ($"[ {InputUtil.GetButtonDescription(InputUtil.INPUT_PRIMARY)} ] "
                        + (target.isNonPlayer ? LocalizationManager.GetString("PingRadar") : LocalizationManager.GetString("TPPlayer")) + "\n"
                        + $"[ {InputUtil.GetButtonDescription(InputUtil.INPUT_SECONDARY)} ] "
                        + (target.isNonPlayer ? LocalizationManager.GetString("FlashRadar") : LocalizationManager.GetString("TriggerTrap")) + "\n")
                    : ($"[ {InputUtil.GetButtonDescription(InputUtil.INPUT_SELECT_TARGET)} ] " + LocalizationManager.GetString("SelectTarget") + "\n"
                        + $"[ {InputUtil.GetButtonDescription(InputUtil.INPUT_SECONDARY)} ] " + LocalizationManager.GetString("TriggerTrap") + "\n");

                player.cursorTip.text += $"[ {InputUtil.GetButtonDescription(InputUtil.INPUT_QUICKSWITCH)} & 0-9 ] " + LocalizationManager.GetString("SwitchPlayer") + "\n";

                player.cursorTip.text += $"[ {InputUtil.GetButtonDescription(InputUtil.INPUT_ALT_QUICKSWITCH)} ] " + LocalizationManager.GetString("SwitchRadar") + "\n";
            }
        }
    }
}