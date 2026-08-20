using UnityEngine;

public static class TempCheckpoint
{
    public static bool HasCheckpoint = false;
    public static Vector3 CheckpointPos;
    public static Quaternion CheckpointRot;

    public static void SetCheckpoint(Vector3 pos, Quaternion rot)
    {
        CheckpointPos = pos;
        CheckpointRot = rot;
        HasCheckpoint = true;
    }

    public static void Reset()
    {
        HasCheckpoint = false;
        CheckpointPos = Vector3.zero;
        CheckpointRot = Quaternion.identity;
    }
}