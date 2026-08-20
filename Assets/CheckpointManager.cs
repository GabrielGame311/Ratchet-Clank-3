using UnityEngine;

public static class CheckpointManager
{
    // Statiska variabler sparar värdet i RAM-minnet mellan scenladdningar
    public static bool HasCheckpoint = false;
    public static Vector3 LastCheckpointPos;
    public static Quaternion LastCheckpointRot;

    public static void SetCheckpoint(Vector3 pos, Quaternion rot)
    {
        LastCheckpointPos = pos;
        LastCheckpointRot = rot;
        HasCheckpoint = true;
    }

    public static void ResetCheckpoint()
    {
        HasCheckpoint = false;
    }
}