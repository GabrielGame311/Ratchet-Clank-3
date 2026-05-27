#if UNITY_EDITOR
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Marionette.Editor
{
    public static class Marionette
    {
        private const int PROTOCOL_VERSION = 6;
        private const int PORT = 11183;
        private const string PLUGIN_ID = "UNITY";

        private class MarionettePluginException : Exception
        {
            public MarionettePluginException()
            {
            }

            public MarionettePluginException(string message) : base(message)
            {
            }

            public MarionettePluginException(string message, Exception inner) : base(message, inner)
            {
            }
        }

        private class UnreachableException : MarionettePluginException
        {
            public UnreachableException()
            {
            }

            public UnreachableException(string message) : base(message)
            {
            }

            public UnreachableException(string message, Exception inner) : base(message, inner)
            {
            }
        }

        private class ConnectionLostException : MarionettePluginException
        {
            public ConnectionLostException()
            {
            }

            public ConnectionLostException(string message) : base(message)
            {
            }

            public ConnectionLostException(string message, Exception inner) : base(message, inner)
            {
            }
        }

        /// <summary>
        ///     Handles reading and writing primitives from a socket.
        /// </summary>
        private sealed class DccStream
        {
            public Socket Socket { get; set; }
            private byte[] buffer = new byte[1024];

            public bool ReceiveBool()
            {
                ReceiveIntoBuffer(1);
                if (buffer[0] != 0 && buffer[0] != 1) throw new MarionettePluginException("Invalid bool value.");
                return buffer[0] != 0;
            }

            public byte ReceiveByte()
            {
                ReceiveIntoBuffer(1);
                return buffer[0];
            }

            public unsafe double ReceiveDouble()
            {
                ReceiveIntoBuffer(8);
                fixed (byte* bytes = buffer)
                {
                    return *(double*)&bytes[0];
                }
            }

            public Vector3 ReceiveVector3() => new(ReceiveFloat(), ReceiveFloat(), ReceiveFloat());

            public unsafe float ReceiveFloat()
            {
                ReceiveIntoBuffer(4);
                fixed (byte* bytes = buffer)
                {
                    return *(float*)&bytes[0];
                }
            }

            public unsafe int ReceiveInt()
            {
                ReceiveIntoBuffer(4);
                fixed (byte* bytes = buffer)
                {
                    return *(int*)&bytes[0];
                }
            }

            public void ReceiveIntoBuffer(int length)
            {
                var readTotal = 0;
                while (buffer.Length < length) buffer = new byte[buffer.Length * 2];
                while (readTotal < length)
                {
                    var read = Socket.Receive(buffer, readTotal, length - readTotal, SocketFlags.None);
                    if (read == 0) throw new ConnectionLostException();
                    readTotal += read;
                }
            }

            public unsafe long ReceiveLong()
            {
                ReceiveIntoBuffer(8);
                fixed (byte* bytes = buffer)
                {
                    return *(long*)&bytes[0];
                }
            }

            public unsafe short ReceiveShort()
            {
                ReceiveIntoBuffer(2);
                fixed (byte* bytes = buffer)
                {
                    return *(short*)&bytes[0];
                }
            }

            public string ReceiveString()
            {
                var count = ReceiveInt();
                ReceiveIntoBuffer(count);
                return Encoding.UTF8.GetString(buffer, 0, count);
            }

            public void SendBool(bool value)
            {
                while (buffer.Length < 1) buffer = new byte[buffer.Length * 2];
                buffer[0] = value ? (byte)1 : (byte)0;
                SendFromBuffer(Socket, buffer, 1);
            }

            public void SendByte(byte value)
            {
                while (buffer.Length < 1) buffer = new byte[buffer.Length * 2];
                buffer[0] = value;
                SendFromBuffer(Socket, buffer, 1);
            }

            public unsafe void SendDouble(double value)
            {
                while (buffer.Length < 8) buffer = new byte[buffer.Length * 2];
                fixed (byte* bytes = buffer)
                {
                    *(double*)&bytes[0] = value;
                }

                SendFromBuffer(Socket, buffer, 8);
            }

            public unsafe void SendFloat(float value)
            {
                while (buffer.Length < 4) buffer = new byte[buffer.Length * 2];
                fixed (byte* bytes = buffer)
                {
                    *(float*)&bytes[0] = value;
                }

                SendFromBuffer(Socket, buffer, 4);
            }

            public void SendVector3(Vector3 value)
            {
                SendFloat(value.x);
                SendFloat(value.y);
                SendFloat(value.z);
            }

            public unsafe void SendInt(int value)
            {
                while (buffer.Length < 4) buffer = new byte[buffer.Length * 2];
                fixed (byte* bytes = buffer)
                {
                    *(int*)&bytes[0] = value;
                }

                SendFromBuffer(Socket, buffer, 4);
            }

            public void SendString(string value)
            {
                var byteCount = Encoding.UTF8.GetByteCount(value);
                SendInt(byteCount);
                while (buffer.Length < byteCount) buffer = new byte[buffer.Length * 2];
                if (value.Length > 0)
                {
                    Encoding.UTF8.GetBytes(value, 0, value.Length, buffer, 0);
                    SendFromBuffer(Socket, buffer, byteCount);
                }
            }

            public void SetTimeout(int timeout)
            {
                Socket.SendTimeout = timeout;
                Socket.ReceiveTimeout = timeout;
            }

            public void Shutdown() => Socket.Shutdown(SocketShutdown.Both);

            private static void SendFromBuffer(Socket socket, byte[] buffer, int length)
            {
                var sentTotal = 0;
                while (sentTotal < length)
                {
                    var sent = socket.Send(buffer, sentTotal, length - sentTotal, SocketFlags.None);
                    sentTotal += sent;
                }
            }
        }

        private enum CoordinateSystem
        {
            RIGHT_X = 0x001, /* 0b000000001 */
            RIGHT_Y = 0x002, /* 0b000000010 */
            RIGHT_Z = 0x004, /* 0b000000100 */
            RIGHT_NEG = 0x008, /* 0b000001000 */

            // Up axis (bits 4-6) + sign (bit 7)
            UP_X = 0x10, /* 0b000010000 */
            UP_Y = 0x020, /* 0b000100000 */
            UP_Z = 0x040, /* 0b001000000 */
            UP_NEG = 0x080, /* 0b010000000 */

            // Forward sign (bit 8) - axis is implicit (the remaining one)
            FORWARD_NEG = 0x100, /* 0b100000000 */

            // Right, Up, Forward
            Unity = RIGHT_X | UP_Y, // +X, +Y, +Z
        }

        private enum RotationUnit
        {
            Degrees = 0,
            Radians = 1,
            Unity = Degrees,
        }

        private enum RotationOrder
        {
            /// <summary>
            ///     Extrinsic rotation, with rotations applied in ZXY order.
            ///     Alternatively, intrinsic rotation with rotations applied in YXZ order.
            /// </summary>
            ZXY = 0,

            /// <summary>
            ///     Extrinsic rotation, with rotations applied in YXZ order.
            ///     Alternatively, intrinsic rotation with rotations applied in ZXY order.
            /// </summary>
            YXZ = 1,

            /// <summary>
            ///     Extrinsic rotation, with rotations applied in XYZ order.
            ///     Alternatively, intrinsic rotation with rotations applied in ZYX order.
            /// </summary>
            XYZ = 2,

            /// <summary>
            ///     Extrinsic rotation, with rotations applied in YZX order.
            ///     Alternatively, intrinsic rotation with rotations applied in XZY order.
            /// </summary>
            YZX = 3,

            /// <summary>
            ///     Extrinsic rotation, with rotations applied in ZYX order.
            ///     Alternatively, intrinsic rotation with rotations applied in XYZ order.
            /// </summary>
            ZYX = 4,

            /// <summary>
            ///     Extrinsic rotation, with rotations applied in XZY order.
            ///     Alternatively, intrinsic rotation with rotations applied in YZX order.
            /// </summary>
            XZY = 5,
            Unity = ZXY,
        }

        private enum PositionUnit
        {
            Meter = 0,
            Centimeter = 1,
            Unity = Meter,
        }

        private enum TangentMode
        {
            Auto = 0,
            Stepped = 1,
            CustomTangents = 2,
        }

        private enum DccBoneName
        {
            Hips = HumanBodyBones.Hips,
            LeftUpperLeg = HumanBodyBones.LeftUpperLeg,
            RightUpperLeg = HumanBodyBones.RightUpperLeg,
            LeftLowerLeg = HumanBodyBones.LeftLowerLeg,
            RightLowerLeg = HumanBodyBones.RightLowerLeg,
            LeftFoot = HumanBodyBones.LeftFoot,
            RightFoot = HumanBodyBones.RightFoot,
            Spine = HumanBodyBones.Spine,
            Chest = HumanBodyBones.Chest,
            Neck = HumanBodyBones.Neck,
            Head = HumanBodyBones.Head,
            LeftShoulder = HumanBodyBones.LeftShoulder,
            RightShoulder = HumanBodyBones.RightShoulder,
            LeftUpperArm = HumanBodyBones.LeftUpperArm,
            RightUpperArm = HumanBodyBones.RightUpperArm,
            LeftLowerArm = HumanBodyBones.LeftLowerArm,
            RightLowerArm = HumanBodyBones.RightLowerArm,
            LeftHand = HumanBodyBones.LeftHand,
            RightHand = HumanBodyBones.RightHand,
            LeftToes = HumanBodyBones.LeftToes,
            RightToes = HumanBodyBones.RightToes,
            LeftEye = HumanBodyBones.LeftEye,
            RightEye = HumanBodyBones.RightEye,
            Jaw = HumanBodyBones.Jaw,
            LeftThumbProximal = HumanBodyBones.LeftThumbProximal,
            LeftThumbIntermediate = HumanBodyBones.LeftThumbIntermediate,
            LeftThumbDistal = HumanBodyBones.LeftThumbDistal,
            LeftIndexProximal = HumanBodyBones.LeftIndexProximal,
            LeftIndexIntermediate = HumanBodyBones.LeftIndexIntermediate,
            LeftIndexDistal = HumanBodyBones.LeftIndexDistal,
            LeftMiddleProximal = HumanBodyBones.LeftMiddleProximal,
            LeftMiddleIntermediate = HumanBodyBones.LeftMiddleIntermediate,
            LeftMiddleDistal = HumanBodyBones.LeftMiddleDistal,
            LeftRingProximal = HumanBodyBones.LeftRingProximal,
            LeftRingIntermediate = HumanBodyBones.LeftRingIntermediate,
            LeftRingDistal = HumanBodyBones.LeftRingDistal,
            LeftLittleProximal = HumanBodyBones.LeftLittleProximal,
            LeftLittleIntermediate = HumanBodyBones.LeftLittleIntermediate,
            LeftLittleDistal = HumanBodyBones.LeftLittleDistal,
            RightThumbProximal = HumanBodyBones.RightThumbProximal,
            RightThumbIntermediate = HumanBodyBones.RightThumbIntermediate,
            RightThumbDistal = HumanBodyBones.RightThumbDistal,
            RightIndexProximal = HumanBodyBones.RightIndexProximal,
            RightIndexIntermediate = HumanBodyBones.RightIndexIntermediate,
            RightIndexDistal = HumanBodyBones.RightIndexDistal,
            RightMiddleProximal = HumanBodyBones.RightMiddleProximal,
            RightMiddleIntermediate = HumanBodyBones.RightMiddleIntermediate,
            RightMiddleDistal = HumanBodyBones.RightMiddleDistal,
            RightRingProximal = HumanBodyBones.RightRingProximal,
            RightRingIntermediate = HumanBodyBones.RightRingIntermediate,
            RightRingDistal = HumanBodyBones.RightRingDistal,
            RightLittleProximal = HumanBodyBones.RightLittleProximal,
            RightLittleIntermediate = HumanBodyBones.RightLittleIntermediate,
            RightLittleDistal = HumanBodyBones.RightLittleDistal,
            UpperChest = HumanBodyBones.UpperChest,
            LeftArmBendGoal,
            RightArmBendGoal,
            LeftLegBendGoal,
            RightLegBendGoal,
            LastBone,
        }

        private enum MessageType
        {
            ConvertWorldPoseToLocalPose = 1, // Send to dcc (salient pose)
            SetLocalKeyframes = 4, // Send to dcc
            SetFrameRate = 5, // Send to dcc
            WriteWorldPose = 10, // Preview
            CreateHumanoidAnimationClip = 2,
            Close = 6,
            Heartbeat = 12,
        }

        private abstract class Message
        {
        }

        private sealed class PoseMessage : Message
        {
            public Pose[] Pose { get; } = new Pose[(int)DccBoneName.LastBone];
            public bool Convert { get; set; }
            public bool RestoreOriginalPose { get; set; }
        }

        private sealed class HumanAnimationMessage : Message
        {
            public string Name { get; set; }
            public List<(string name, Keyframe[] keys)> Curves { get; set; }
        }

        private sealed class AnimationMessage : Message
        {
            public Keyframe[][][][] Keys { get; set; } // (joint, type, axis, frame)
            public string Name { get; set; }
            public TangentMode TangentMode { get; set; }
        }

        private sealed class Client : IDisposable
        {
            public Task fgTask;
            private readonly CancellationTokenSource fgCancellationSource = new();
            public readonly ConcurrentQueue<Message> sharedInboundQueue = new();
            public readonly BlockingCollection<Message> sharedOutboundQueue = new();
            public volatile bool sharedIsConnected = false;
            public HumanDescription HumanDescription { get; }

            public Client(Animator animator)
            {
                HumanDescription = animator.avatar.humanDescription;
                var humanBones = HumanDescription.human.ToArray();
                var worldSkeletonBones = TransformSkeletonBonesToWorldSpace(animator, HumanDescription.skeleton);
                var cancellation = fgCancellationSource.Token;
                fgTask = Task.Run(() =>
                {
                    DccStream stream = new();
                    while (true)
                    {
                        using var socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
                        try
                        {
                            socket.SendTimeout = 15000;
                            socket.ReceiveTimeout = 15000;
                            var connectTask = socket.ConnectAsync(IPAddress.Loopback, PORT);
                            while (!connectTask.IsCompleted)
                            {
                                cancellation.ThrowIfCancellationRequested();
                            }

                            if (connectTask.IsCompletedSuccessfully)
                            {
                                cancellation.ThrowIfCancellationRequested();
                                stream.Socket = socket;
                                var protocolVersion = stream.ReceiveInt();
                                if (protocolVersion != PROTOCOL_VERSION)
                                {
                                    throw new MarionettePluginException(
                                        $"Unsupported Marionette version. Please make sure that the plugin version and the Marionette application version match. Plugin protocol {PROTOCOL_VERSION} doesn't match Marionette protocol {protocolVersion}.");
                                }

                                stream.SendInt(PROTOCOL_VERSION);
                                stream.SendString(PLUGIN_ID);
                                stream.SendBool(false); // TODO: Implement mesh sending?
                                stream.SendInt((int)CoordinateSystem.Unity);
                                stream.SendInt((int)PositionUnit.Unity);
                                stream.SendInt((int)RotationUnit.Unity);
                                stream.SendInt(humanBones.Length);
                                for (var i = 0; i < humanBones.Length; i++)
                                {
                                    var dccName =
                                        Enum.Parse<DccBoneName>(humanBones[i].humanName.Replace(" ", ""));
                                    var boneName = humanBones[i].boneName;
                                    var bone = worldSkeletonBones[boneName];
                                    stream.SendString(dccName.ToString());
                                    stream.SendString(boneName);
                                    stream.SendFloat(bone.position.x);
                                    stream.SendFloat(bone.position.y);
                                    stream.SendFloat(bone.position.z);
                                    stream.SendString("ZXY");
                                    stream.SendFloat(bone.rotation.eulerAngles.x);
                                    stream.SendFloat(bone.rotation.eulerAngles.y);
                                    stream.SendFloat(bone.rotation.eulerAngles.z);
                                }

                                if (stream.ReceiveString() != "HANDSHAKE COMPLETED")
                                    throw new MarionettePluginException($"Handshake failed.");
                                stream.SendString("ROGER");
                                sharedIsConnected = true;

                                var framerate = 30;
                                while (true)
                                {
                                    cancellation.ThrowIfCancellationRequested();
                                    var type = (MessageType)stream.ReceiveInt();
                                    PoseMessage poseMessage;
                                    AnimationMessage animationMessage;
                                    switch (type)
                                    {
                                        case MessageType.ConvertWorldPoseToLocalPose:
                                            var frameCount = stream.ReceiveInt();
                                            var restoreOriginalPose = stream.ReceiveBool();
                                            for (var i = 0; i < frameCount; i++)
                                            {
                                                cancellation.ThrowIfCancellationRequested();
                                                poseMessage = new PoseMessage
                                                    { Convert = true, RestoreOriginalPose = restoreOriginalPose };
                                                for (var j = 0; j < humanBones.Length; j++)
                                                {
                                                    poseMessage.Pose[j] = new Pose(
                                                        stream.ReceiveVector3(),
                                                        Quaternion.Euler(stream.ReceiveVector3())
                                                    );
                                                }

                                                sharedInboundQueue.Enqueue(poseMessage);
                                            }

                                            for (var i = 0; i < frameCount; i++)
                                            {
                                                cancellation.ThrowIfCancellationRequested();
                                                poseMessage = (PoseMessage)sharedOutboundQueue.Take(cancellation);
                                                for (var j = 0; j < humanBones.Length; j++)
                                                {
                                                    stream.SendVector3(poseMessage.Pose[j].position);
                                                    stream.SendVector3(poseMessage.Pose[j].rotation.eulerAngles);
                                                }
                                            }

                                            break;
                                        case MessageType.CreateHumanoidAnimationClip:
                                            var animationName = stream.ReceiveString();
                                            var curveCount = stream.ReceiveInt();
                                            var curves = new List<(string name, Keyframe[] keys)>();
                                            for (var i = 0; i < curveCount; i++)
                                            {
                                                var name = stream.ReceiveString();
                                                var keyCount = stream.ReceiveInt();
                                                var keys = new Keyframe[keyCount];
                                                for (var j = 0; j < keyCount; j++)
                                                {
                                                    keys[j] = new Keyframe
                                                    {
                                                        time = stream.ReceiveFloat(),
                                                        weightedMode = (WeightedMode)stream.ReceiveInt(),
                                                        inTangent = stream.ReceiveFloat(),
                                                        inWeight = stream.ReceiveFloat(),
                                                        value = stream.ReceiveFloat(),
                                                        outTangent = stream.ReceiveFloat(),
                                                        outWeight = stream.ReceiveFloat(),
                                                    };
                                                }

                                                curves.Add((name, keys));
                                            }

                                            sharedInboundQueue.Enqueue(new HumanAnimationMessage
                                                { Name = animationName, Curves = curves });
                                            break;
                                        case MessageType.SetLocalKeyframes:
                                            // (joint, type, axis, frame)
                                            animationMessage = new AnimationMessage
                                            {
                                                Keys = new Keyframe[humanBones.Length][][][],
                                                Name = stream.ReceiveString(),
                                                TangentMode = (TangentMode)stream.ReceiveInt(),
                                            };
                                            for (var joint = 0; joint < humanBones.Length; joint++)
                                            {
                                                var keyframeCount = stream.ReceiveInt();

                                                animationMessage.Keys[joint] = new Keyframe[2][][];
                                                for (var keyType = 0; keyType < 2; keyType++)
                                                {
                                                    animationMessage.Keys[joint][keyType] = new Keyframe[3][];
                                                    for (var axis = 0; axis < 3; axis++)
                                                    {
                                                        animationMessage.Keys[joint][keyType][axis] =
                                                            new Keyframe[keyframeCount];
                                                    }
                                                }

                                                for (var keyIndex = 0; keyIndex < keyframeCount; keyIndex++)
                                                for (var keyType = 0; keyType < 2; keyType++)
                                                for (var axis = 0; axis < 3; axis++)
                                                {
                                                    var key = new Keyframe();
                                                    key.time = stream.ReceiveFloat();
                                                    key.value = stream.ReceiveFloat();
                                                    key.inTangent = stream.ReceiveFloat();
                                                    key.outTangent = stream.ReceiveFloat();
                                                    key.inWeight = stream.ReceiveFloat();
                                                    key.outWeight = stream.ReceiveFloat();
                                                    animationMessage.Keys[joint][keyType][axis][keyIndex] = key;
                                                }
                                            }

                                            sharedInboundQueue.Enqueue(animationMessage);
                                            break;
                                        case MessageType.SetFrameRate:
                                            framerate = stream.ReceiveInt();
                                            break;
                                        case MessageType.Close:
                                            throw new ConnectionLostException();
                                        case MessageType.WriteWorldPose:
                                            var canBeDiscarded = stream.ReceiveBool();
                                            poseMessage = new PoseMessage();
                                            for (var j = 0; j < humanBones.Length; j++)
                                            {
                                                poseMessage.Pose[j] = new Pose(
                                                    stream.ReceiveVector3(),
                                                    Quaternion.Euler(stream.ReceiveVector3())
                                                );
                                            }

                                            if (sharedInboundQueue.Count < 2 || !canBeDiscarded)
                                            {
                                                sharedInboundQueue.Enqueue(poseMessage);
                                            }

                                            break;
                                        case MessageType.Heartbeat:
                                            stream.SendBool(true);
                                            break;
                                        default:
                                            throw new MarionettePluginException("Unknown message type.");
                                    }
                                }
                            }
                            else
                            {
                                Debug.LogException(connectTask.Exception);
                            }
                        }
                        catch (ConnectionLostException)
                        {
                        }
                        catch (OperationCanceledException)
                        {
                            throw;
                        }
                        catch (Exception e)
                        {
                            Debug.LogException(e);
                        }
                        finally
                        {
                            socket.Close();
                        }

                        sharedIsConnected = false;
                        Thread.Sleep(1000);
                    }
                }, fgCancellationSource.Token);
            }

            public void Cancel()
            {
                fgCancellationSource?.Cancel();
            }

            public void CancelAndWait(int timeout)
            {
                fgCancellationSource?.Cancel();
                try
                {
                    fgTask.Wait(timeout);
                }
                catch (AggregateException e)
                {
                    if (e.InnerException is TaskCanceledException)
                    {
                        // Ignored.
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            public void Dispose()
            {
                fgCancellationSource?.Dispose();
                sharedOutboundQueue?.Dispose();
            }

            private static Dictionary<string, Pose> TransformSkeletonBonesToWorldSpace(
                Animator animator,
                SkeletonBone[] skeleton
            )
            {
                var worldTransforms = new Dictionary<string, Pose>();
                var boneNameToIndex = new Dictionary<string, int>(skeleton.Length);
                var childIndices = new List<int>[skeleton.Length];
                // Root bone is always at index 0.
                for (var i = 0; i < skeleton.Length; i++)
                {
                    boneNameToIndex[skeleton[i].name] = i;
                    childIndices[i] = new List<int>();
                }

                for (var i = 1; i < skeleton.Length; i++)
                {
                    var bone = FindBoneInChildren(animator.transform, skeleton[i].name);
                    if (bone == null)
                    {
                        throw new MarionettePluginException($"Failed to find bone \"{skeleton[i].name}\".");
                    }

                    var parent = bone.parent;
                    if (parent == null)
                    {
                        throw new MarionettePluginException(
                            $"Failed to find parent of non-root bone \"{skeleton[i].name}\".");
                    }

                    if (parent == animator.transform)
                    {
                        childIndices[0].Add(i);
                    }
                    else
                    {
                        if (boneNameToIndex.TryGetValue(parent.name, out var index))
                        {
                            childIndices[index].Add(i);
                        }
                        else
                        {
                            throw new MarionettePluginException($"Found unexpected bone \"{parent.name}\".");
                        }
                    }

                    continue;

                    static Transform FindBoneInChildren(Transform parent, string name)
                    {
                        for (var j = 0; j < parent.childCount; j++)
                        {
                            var child = FindBone(parent.GetChild(j), name);
                            if (child != null) return child;
                        }

                        return null;
                    }

                    static Transform FindBone(Transform parent, string name)
                    {
                        if (parent.name == name) return parent;
                        for (var j = 0; j < parent.childCount; j++)
                        {
                            var child = FindBone(parent.GetChild(j), name);
                            if (child != null) return child;
                        }

                        return null;
                    }
                }

                Process(0, Matrix4x4.identity);
                return worldTransforms;

                void Process(int boneIndex, Matrix4x4 parentWorldMatrix)
                {
                    var bone = skeleton[boneIndex];
                    var localMatrix = Matrix4x4.TRS(
                        bone.position,
                        bone.rotation,
                        bone.scale
                    );
                    var worldMatrix = parentWorldMatrix * localMatrix;
                    worldTransforms[bone.name] = new Pose(worldMatrix.GetPosition(), worldMatrix.rotation);
                    foreach (var childIndex in childIndices[boneIndex])
                    {
                        Process(childIndex, worldMatrix);
                    }
                }
            }
        }

	private enum ConnectionStatus {
	    NotConnected,
	    Connecting,
	    Connected,
	    Disconnecting,
	}

        private sealed class Window : EditorWindow
        {
            private Animator animator;
            private readonly Stopwatch stopwatch = new();
            private bool wantsToConnect;
            private Client client;
            private static readonly Assembly assembly = typeof(EditorWindow).Assembly;
            private static readonly Type gameViewType = assembly.GetType("UnityEditor.GameView");
	    private Avatar avatarAtConnect;
	    private ConnectionStatus previousConnectionStatus = ConnectionStatus.NotConnected;

            private static readonly DccBoneName[] boneOrder = new[]
            {
                DccBoneName.LeftThumbDistal,
                DccBoneName.LeftThumbIntermediate,
                DccBoneName.LeftThumbProximal,

                DccBoneName.LeftIndexDistal,
                DccBoneName.LeftIndexIntermediate,
                DccBoneName.LeftIndexProximal,

                DccBoneName.LeftMiddleDistal,
                DccBoneName.LeftMiddleIntermediate,
                DccBoneName.LeftMiddleProximal,

                DccBoneName.LeftRingDistal,
                DccBoneName.LeftRingIntermediate,
                DccBoneName.LeftRingProximal,

                DccBoneName.LeftLittleDistal,
                DccBoneName.LeftLittleIntermediate,
                DccBoneName.LeftLittleProximal,

                DccBoneName.RightThumbDistal,
                DccBoneName.RightThumbIntermediate,
                DccBoneName.RightThumbProximal,

                DccBoneName.RightIndexDistal,
                DccBoneName.RightIndexIntermediate,
                DccBoneName.RightIndexProximal,

                DccBoneName.RightMiddleDistal,
                DccBoneName.RightMiddleIntermediate,
                DccBoneName.RightMiddleProximal,

                DccBoneName.RightRingDistal,
                DccBoneName.RightRingIntermediate,
                DccBoneName.RightRingProximal,

                DccBoneName.RightLittleDistal,
                DccBoneName.RightLittleIntermediate,
                DccBoneName.RightLittleProximal,

                DccBoneName.LeftArmBendGoal,
                DccBoneName.LeftHand,
                DccBoneName.LeftLowerArm,
                DccBoneName.LeftUpperArm,
                DccBoneName.LeftShoulder,

                DccBoneName.RightArmBendGoal,
                DccBoneName.RightHand,
                DccBoneName.RightLowerArm,
                DccBoneName.RightUpperArm,
                DccBoneName.RightShoulder,

                DccBoneName.LeftEye,
                DccBoneName.RightEye,

                DccBoneName.LeftLegBendGoal,
                DccBoneName.LeftToes,
                DccBoneName.LeftFoot,
                DccBoneName.LeftLowerLeg,
                DccBoneName.LeftUpperLeg,

                DccBoneName.RightLegBendGoal,
                DccBoneName.RightToes,
                DccBoneName.RightFoot,
                DccBoneName.RightLowerLeg,
                DccBoneName.RightUpperLeg,

                DccBoneName.Head,
                DccBoneName.Neck,
                DccBoneName.UpperChest,
                DccBoneName.Chest,
                DccBoneName.Spine,
                DccBoneName.Hips,
            }.Reverse().ToArray();
            private static string[] humanBoneNames;
	    private GUIStyle helpBoxStyle; 

            [MenuItem("Window/Marionette")]
            private static void ShowWindow()
            {
                GetWindow(typeof(Window), false, "Marionette");
            }

            private void OnEnable()
            {
                humanBoneNames = HumanTrait.BoneName;
                minSize = new Vector2(200, 75);
                maxSize = new Vector2(500, 300);
		helpBoxStyle = new GUIStyle(EditorStyles.helpBox)
		{
		    padding = new RectOffset(8, 8, 8, 8),
		    alignment = TextAnchor.MiddleLeft
		};
            }

            private void OnDisable()
            {
                if (client != null)
                {
                    try
                    {
                        // We wait a bit to allow the thread chance to quit gracefully, as the domain reload will otherwise call thread abort.
                        client.CancelAndWait(1000);
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }

                    client.Dispose();
                    client = null;
                }

                wantsToConnect = false;
            }

            /// (type, axis)
            private static readonly string[][] propertyNames =
            {
                new[] { "localPosition.x", "localPosition.y", "localPosition.z" },
                new[] { "localEulerAngles.x", "localEulerAngles.y", "localEulerAngles.z" },
            };

            private void Update()
            {
		if (GetConnectionStatus() != previousConnectionStatus) {
		    Repaint();
		}
		previousConnectionStatus = GetConnectionStatus();

                var shouldRepaint = false;
		if (client != null) {
		    if (!animator || !animator.avatar) {
			Debug.LogError("MARIONETTE: Animator or avatar has been destroyed.");
			Disconnect();
			Repaint();
			return;
		    }
		    if (animator.avatar != avatarAtConnect) {
			Debug.LogError("MARIONETTE: Avatar changed.");
			Disconnect();
			Repaint();
			return;
		    }
		}
                if (client is { sharedInboundQueue: { IsEmpty: false } })
                {
                    stopwatch.Restart();
		    while (stopwatch.ElapsedMilliseconds < 33 && client.sharedInboundQueue.TryDequeue(out var message))
                    {
                        if (message is PoseMessage poseMessage)
                        {
			    if (EditorUtility.IsPersistent(animator)) {
				poseMessage.RestoreOriginalPose = true;
			    }
                            foreach (var boneName in boneOrder)
                            {
                                if ((int)boneName < (int)HumanBodyBones.LastBone)
                                {
                                    var originalOrder = -1;
                                    var bones = client.HumanDescription.human;
                                    for (var j = 0; j < bones.Length; j++)
                                    {
                                        if (bones[j].humanName == humanBoneNames[(int)boneName])
                                        {
                                            originalOrder = j;
                                            break;
                                        }
                                    }

                                    if (originalOrder == -1) continue;
                                    var bone = animator.GetBoneTransform((HumanBodyBones)boneName);
				    if (bone == null) {
					Debug.LogError($"MARIONETTE: Failed to get bone {(HumanBodyBones)boneName}. Has the bone game object been deleted?");
					Disconnect();
					Repaint();
					return;
				    }
                                    var originalPose = new Pose(bone.position, bone.rotation);
                                    bone.position = poseMessage.Pose[originalOrder].position;
                                    bone.rotation = poseMessage.Pose[originalOrder].rotation;
                                    if (poseMessage.Convert)
                                    {
                                        poseMessage.Pose[originalOrder].position = bone.localPosition;
                                        poseMessage.Pose[originalOrder].rotation = bone.localRotation;
                                    }

                                    if (poseMessage.RestoreOriginalPose)
                                    {
                                        bone.position = originalPose.position;
                                        bone.rotation = originalPose.rotation;
                                    }
                                    else
                                    {
                                        EditorUtility.SetDirty(bone);
                                    }
                                }
                                else
                                {
                                    // We currently do not provide IK support in the Unity DCC plugin.
                                }
                            }

                            if (!poseMessage.RestoreOriginalPose)
                            {
                                shouldRepaint = true;
                            }

                            if (poseMessage.Convert)
                            {
                                client.sharedOutboundQueue.Add(message);
                            }
                        }
                        else if (message is AnimationMessage animationMessage)
                        {
                            var path = new List<string>();
                            var clip = new AnimationClip();
                            clip.name = $"Marionette Animation {GUID.Generate()}";
                            for (var joint = 0; joint < client.HumanDescription.human.Length; joint++)
                            {
                                var humanName = (HumanBodyBones)Array.FindIndex(
                                    humanBoneNames, n => n == client.HumanDescription.human[joint].humanName
                                );
                                var bone = animator.GetBoneTransform(humanName);
				if (bone == null) {
				    Debug.LogError($"MARIONETTE: Failed to get bone {humanName}.");
				    Disconnect();
				    Repaint();
				    return;
				}
                                path.Clear();
                                while (bone != animator.transform)
                                {
                                    path.Insert(0, bone.name);
                                    bone = bone.parent;
                                }

                                var fullPath = string.Join("/", path);

                                for (var keyType = 0; keyType < 2; keyType++)
                                for (var axis = 0; axis < 3; axis++)
                                {
                                    clip.SetCurve(fullPath, typeof(Transform), propertyNames[keyType][axis],
                                        new AnimationCurve
                                        {
                                            keys = animationMessage.Keys[joint][keyType][axis],
                                        });
                                }
                            }

                            for (var keyType = 0; keyType < 2; keyType++)
                            for (var axis = 0; axis < 3; axis++)
                            {
                                clip.SetCurve("", typeof(Transform), propertyNames[keyType][axis],
                                    new AnimationCurve());
                            }

                            Directory.CreateDirectory(Path.Combine("Assets", "MarionetteAnimations"));
                            var animPath = Path.Combine("Assets", "MarionetteAnimations", clip.name + ".anim");
                            AssetDatabase.CreateAsset(clip, animPath);
                            Debug.Log($"MARIONETTE: Saved animation to \"{animPath}\".", clip);
                        }
                        else if (message is HumanAnimationMessage humanMessage)
                        {
                            var clip = new AnimationClip();
                            for (var i = 0; i < humanMessage.Curves.Count; i++)
                            {
                                clip.SetCurve("", typeof(Animator), humanMessage.Curves[i].name,
                                    new AnimationCurve
                                    {
                                        keys = humanMessage.Curves[i].keys,
                                    }
                                );
                            }
                            Directory.CreateDirectory(Path.Combine("Assets", "MarionetteAnimations"));
			    clip.name = humanMessage.Name;
                            string animPath = Path.Combine("Assets", "MarionetteAnimations", clip.name + ".anim");
			    var j = 0;
			    while (File.Exists(animPath)) {
				j++;
				clip.name = $"{humanMessage.Name}.{j:D3}";
				animPath = Path.Combine("Assets", "MarionetteAnimations", clip.name + ".anim");
			    }
                            AssetDatabase.CreateAsset(clip, animPath);
                            Debug.Log($"MARIONETTE: Saved animation to \"{animPath}\".", clip);
                        }
                    }
                }

                if (shouldRepaint)
                {
                    // Force the game view to update, even if the unity editor isn't focused.
                    foreach (EditorWindow w in Resources.FindObjectsOfTypeAll(gameViewType)) w.Repaint();
		    // Force the scene view to update, even if the unity editor isn't focused.
                    foreach (EditorWindow w in Resources.FindObjectsOfTypeAll<SceneView>()) w.Repaint();
                }
            }

            private void OnGUI()
            {
		var connectionStatus = GetConnectionStatus();
		GUI.enabled = !wantsToConnect;
		if (wantsToConnect) {
                    EditorGUILayout.ObjectField("Animator", animator, typeof(Animator), true);
		} else {
                    animator = (Animator)EditorGUILayout.ObjectField("Animator", animator, typeof(Animator), true);
		}
		GUI.enabled = true;
		var hasError = false;
		if (!animator)
		{
		    DrawCustomHelpBox("console.erroricon.sml", "Please assign an animator.");
		    hasError = true;
		}
		else if (!animator.avatar || !animator.avatar.isValid || !animator.avatar.isHuman)
		{
		    DrawCustomHelpBox("console.erroricon.sml", "Animator does not have a valid humanoid avatar.");
		    hasError = true;
		}
		else if (EditorUtility.IsPersistent(animator))
		{
		    DrawCustomHelpBox("console.warnicon.sml", "Animator is not part of a scene. Live preview won't work."); 
		}
		else {
		    DrawCustomHelpBox(
			connectionStatus switch {
			    ConnectionStatus.Connected => "TestPassed",
			    ConnectionStatus.Connecting => "WaitSpin00",
			    ConnectionStatus.Disconnecting => "WaitSpin00",
			    ConnectionStatus.NotConnected => "TestFailed",
			    _ => throw new UnreachableException(),
			},
			connectionStatus switch {
			    ConnectionStatus.Connected => "Connected.",
			    ConnectionStatus.Connecting => "Connecting...",
			    ConnectionStatus.Disconnecting => "Disconnecting...",
			    ConnectionStatus.NotConnected => "Not Connected.",
			    _ => throw new UnreachableException(),
			}
		    );
		}
		GUI.enabled = wantsToConnect ? true : !hasError;
		if (GUILayout.Button(wantsToConnect ? "Disconnect" : "Connect"))
		{
		    if (wantsToConnect)
		    {
			Disconnect();
		    }
		    else
		    {
			avatarAtConnect = animator.avatar;
                        wantsToConnect = true;
                        foreach (var s in animator.GetComponentsInChildren<SkinnedMeshRenderer>())
                        {
                            // Force the skinned mesh renderer to update, even if the unity editor isn't focused.
                            s.forceMatrixRecalculationPerRender = true;
                        }

                        client = new Client(animator);
		    }
		}
		GUI.enabled = true;
            }

	    private void Disconnect() {
		wantsToConnect = false;
		client.Cancel();
		client.Dispose();
		client = null;
	    }

	    private void DrawCustomHelpBox(string iconName, string message)
	    {
		EditorGUILayout.BeginHorizontal(helpBoxStyle);
		GUILayout.Label(EditorGUIUtility.IconContent(iconName).image, GUILayout.Width(20), GUILayout.Height(20));
		GUILayout.Space(4);
		GUILayout.Label(message, EditorStyles.wordWrappedLabel);
		EditorGUILayout.EndHorizontal();
	    }

	    private ConnectionStatus GetConnectionStatus() {
                var isConnected = client is { sharedIsConnected: true };
		if (wantsToConnect && isConnected)
		{
		    return ConnectionStatus.Connected;
		}
		else if (wantsToConnect && !isConnected)
		{
		    return ConnectionStatus.Connecting;
		}
		else if (!wantsToConnect && isConnected)
		{
		    return ConnectionStatus.Disconnecting;
		}
		else if (!wantsToConnect && !isConnected)
		{
		    return ConnectionStatus.NotConnected;
		}
		throw new UnreachableException();
	    }
        }
    }
}
#endif
