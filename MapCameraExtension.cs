using GameNetcodeStuff;

namespace MonitorEnhance;

internal static class MapCameraExtension
{
    private static PlayerControllerB LOCAL_PLAYER => GameNetworkManager.Instance?.localPlayerController;

    public static TransformAndName GetCurrentRadarTarget(this ManualCameraRenderer renderer)
    {
        int i = renderer.targetTransformIndex;

        if (i < 0) return null;
        TransformAndName t = renderer.radarTargets[i];

        return t;
    }

    public static int GetNextPlayerIdx(this ManualCameraRenderer renderer)
    {
        bool isNonPlayer = renderer.GetCurrentRadarTarget().isNonPlayer;
        for (int i = isNonPlayer ? 0 : (renderer.targetTransformIndex < renderer.radarTargets.Count - 1 ? renderer.targetTransformIndex + 1 : 0); i <= renderer.radarTargets.Count; i++)
        {
            if (i == renderer.radarTargets.Count) i = 0;

            TransformAndName trans = renderer.radarTargets[i];
            if (trans?.transform.gameObject.activeSelf == true && trans?.isNonPlayer == false)
                return i;
        }

        return renderer.targetTransformIndex;
    }

    public static int GetNextRadarIdx(this ManualCameraRenderer renderer)
    {
        bool isNonPlayer = renderer.GetCurrentRadarTarget().isNonPlayer;
        for (int i = isNonPlayer ? (renderer.targetTransformIndex < renderer.radarTargets.Count - 1 ? renderer.targetTransformIndex + 1 : 0) : 0; i < renderer.radarTargets.Count; i++)
        {
            TransformAndName trans = renderer.radarTargets[i];
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
                trans?.isNonPlayer == false && !player.Equals(LOCAL_PLAYER))
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
