using GameNetcodeStuff;
using HarmonyLib;

using System;
using System.Reflection;
using UnityEngine;

namespace MonitorEnhance;

internal static class MapCameraExtension
{
    private static PlayerControllerB LOCAL_PLAYER => GameNetworkManager.Instance?.localPlayerController;
    public static FieldInfo updateMapCameraCoroutine = typeof(ManualCameraRenderer).GetField("updateMapCameraCoroutine", BindingFlags.NonPublic | BindingFlags.Instance);
    public static MethodInfo updateMapTarget = typeof(ManualCameraRenderer).GetMethod("updateMapTarget", BindingFlags.NonPublic | BindingFlags.Instance);

    public static TransformAndName GetCurrentRadarTarget(this ManualCameraRenderer renderer)
    {
        TransformAndName t = null;
        int i = renderer.targetTransformIndex;

        if (i < 0) return null;
        t = renderer.radarTargets[i];

        return t;
    }

    public static int GetNextPlayerIdx(this ManualCameraRenderer renderer)
    {
        bool isNonPlayer = renderer.GetCurrentRadarTarget().isNonPlayer;
        for (int i = isNonPlayer ? 0 : (renderer.targetTransformIndex < renderer.radarTargets.Count - 1 ? renderer.targetTransformIndex + 1 : 0); i < renderer.radarTargets.Count; i++)
        {
            var trans = renderer.radarTargets[i];
            if (trans?.transform.gameObject.activeSelf == true && trans.transform.gameObject.GetComponent<PlayerControllerB>()?.isPlayerControlled == true)
                return i;
        }
        return renderer.targetTransformIndex;
    }

    public static int GetNextRadarIdx(this ManualCameraRenderer renderer)
    {
        bool isNonPlayer = renderer.GetCurrentRadarTarget().isNonPlayer;
        for (int i = isNonPlayer ? (renderer.targetTransformIndex < renderer.radarTargets.Count - 1 ? renderer.targetTransformIndex + 1 : 0) : 0; i < renderer.radarTargets.Count; i++)
        {
            var trans = renderer.radarTargets[i];
            if (trans?.transform.gameObject.activeSelf == true && trans?.isNonPlayer == true)
                return i;
        }
        return renderer.targetTransformIndex;
    }

    public static void SwitchMonitorPlayer(this ManualCameraRenderer renderer)
    {
        renderer.SwitchRadarTargetAndSync(renderer.GetNextPlayerIdx());
    }

    public static void SwitchMonitorRadar(this ManualCameraRenderer renderer)
    {
        renderer.SwitchRadarTargetAndSync(renderer.GetNextRadarIdx());
    }

    public static void SwitchRadarTargetByNum(this ManualCameraRenderer renderer, int target)
    {
        int index = 0, count = 0;
        if (target == 0)
        {
            while (index < renderer.radarTargets.Count)
            {
                PlayerControllerB player = renderer.radarTargets[index].transform.GetComponent<PlayerControllerB>();
                if (player.Equals(LOCAL_PLAYER))
                {
                    renderer.SwitchRadarTargetAndSync(index);
                    return;
                }
                index++;
            }
            return;
        }

        while (index < renderer.radarTargets.Count)
        {
            TransformAndName trans = renderer.radarTargets[index];
            PlayerControllerB player = trans.transform.GetComponent<PlayerControllerB>();
            if (trans?.transform.gameObject.activeSelf == true &&
                player?.isPlayerControlled == true && !player.Equals(LOCAL_PLAYER))
            {
                if (++count == target)
                {
                    renderer.SwitchRadarTargetAndSync(index);
                    break;
                }
                else if (count > target) return;
            }
            index++;
        }
    }
}
