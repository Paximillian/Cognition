using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System;

namespace Smooth
{
    /// <summary>
    /// The state of an object: position, rotation, velocity, angular velocity.
    /// </summary>
    public class State
    {
        public bool syncPosition = true;
        public bool syncRotation = true;
        /// <summary>
        /// The network timestamp on the owner when the state was sent
        /// </summary>
        public int ownerTimestamp;
        /// <summary>
        /// The position of the owner when the state was sent
        /// </summary>
        public Vector3 position;
        /// <summary>
        /// The rotation of the owner when the state was sent
        /// </summary>
        public Quaternion rotation;
        /// <summary>
        /// The velocity of the owner when the state was sent
        /// </summary>
        public Vector3 velocity;
        /// <summary>
        /// The angularVelocity of owner when the state was sent
        /// </summary>
        public Vector3 angularVelocity;

        /// <summary>
        /// Default constructor. Does nothing.
        /// </summary>
        public State() { }

        /// <summary>
        /// Copy an existing state
        /// </summary>
        public State(State state)
        { 
            syncPosition = state.syncPosition;
            syncRotation = state.syncRotation;
            ownerTimestamp = state.ownerTimestamp;
            position = state.position;
            rotation = state.rotation;
            velocity = state.velocity;
            angularVelocity = state.angularVelocity;
        }

        /// <summary>
        /// Create a State from a GameObject.
        /// </summary>
        /// <remarks>
        /// This should only be called on owners when creating the States to be passed over the network.
        /// </remarks>
        /// <param name="smoothSyncScript"></param>
        public State(SmoothSync smoothSyncScript)
        {
            ownerTimestamp = NetworkTransport.GetNetworkTimestamp();
            position = smoothSyncScript.getPosition();
            rotation = smoothSyncScript.getRotation();

            if (smoothSyncScript.hasRigdibody)
            {
                velocity = smoothSyncScript.rb.velocity;
                angularVelocity = smoothSyncScript.rb.angularVelocity;
            }
            else if (smoothSyncScript.hasRigidbody2D)
            {
                velocity = smoothSyncScript.rb2D.velocity;
                angularVelocity = new Vector3(0, 0, smoothSyncScript.rb2D.angularVelocity);
            }
            else
            {
                velocity = Vector3.zero;
                angularVelocity = Vector3.zero;
            }
        }

        /// <summary>
        /// Convenient way to Lerp between states
        /// </summary>
        /// <param name="start">Start state</param>
        /// <param name="end">End state</param>
        /// <param name="t">Time</param>
        /// <returns></returns>
        public static State Lerp(State start, State end, float t)
        {
            State state = new State();

            state.position = Vector3.Lerp(start.position, end.position, t);            
            state.rotation = Quaternion.Lerp(start.rotation, end.rotation, t);
            state.velocity = Vector3.Lerp(start.velocity, end.velocity, t);
            state.angularVelocity = Vector3.Lerp(start.angularVelocity, end.angularVelocity, t);
            
            state.ownerTimestamp = (int)Mathf.Lerp(start.ownerTimestamp, end.ownerTimestamp, t);

            return state;
        }
    }

    /// <summary>
    /// Wraps the State in the NetworkMessage so we can send it over the network.
    /// </summary>
    /// <remarks>
    /// This only sends / receives the parts of the State that are enabled on the SmoothSync component.
    /// </remarks>
    public class NetworkState : MessageBase
    {
        /// <summary>
        /// The smooth sync object associated with this state.
        /// </summary>
        public SmoothSync smoothSync;
        /// <summary>
        /// The State that will be sent over the network
        /// </summary>
        public State state = new State();

        /// <summary>
        /// Default contstructor, does nothing.
        /// </summary>
        public NetworkState() { }

        /// <summary>
        /// Create a NetworkState from a SmoothSync object
        /// </summary>
        /// <param name="smoothSyncScript">The SmoothSync object</param>
        public NetworkState(SmoothSync smoothSyncScript)
        {
            this.smoothSync = smoothSyncScript;
            state = new State(smoothSyncScript);
        }
        /// <summary>
        /// Info to know what to sync.
        /// </summary>
        public enum SyncInfo
        {
            NONE, POSITION, ROTATION, BOTH
        }
        /// <summary>
        /// Serialize the message over the network
        /// </summary>
        /// <remarks>
        /// Only sends what it needs. Optionally compresses floats depending on the settings on the SmoothSync object.
        /// </remarks>
        /// <param name="writer">The NetworkWriter to write to</param>
        override public void Serialize(NetworkWriter writer)
        {
            // If haven't changed enough since last sent update, we don't want to send out or assign things
            bool sendPosition = smoothSync.shouldSendPosition();
            bool sendVelocity = smoothSync.shouldSendVelocity();
            bool sendAngularVelocity = smoothSync.shouldSendAngularVelocity();
            bool sendRotation = smoothSync.shouldSendRotation();

            // only set last sync states on clients here because server needs to send multiple serializes
            if (!NetworkServer.active)
            {
                if (sendPosition) smoothSync.lastPositionWhenStateWasSent = state.position;
                if (sendVelocity) smoothSync.lastVelocityWhenStateWasSent = state.velocity;
                if (sendAngularVelocity) smoothSync.lastAngularVelocityWhenStateWasSent = state.angularVelocity;
                if (sendRotation) smoothSync.lastRotationWhenStateWasSent = state.rotation;
            }

            writer.Write(encodeSyncInformation(sendPosition, sendRotation, sendVelocity, sendAngularVelocity));
            writer.Write(smoothSync.netId);
            writer.WritePackedUInt32((uint)smoothSync.syncIndex);
            writer.WritePackedUInt32((uint)state.ownerTimestamp);

            // write position
            if (sendPosition)
            { 
                if (smoothSync.isPositionCompressed)
                {
                    if (smoothSync.isSyncingXPosition)
                    {
                        writer.Write(HalfHelper.Compress(state.position.x));
                    }
                    if (smoothSync.isSyncingYPosition)
                    {
                        writer.Write(HalfHelper.Compress(state.position.y));
                    }
                    if (smoothSync.isSyncingZPosition)
                    {
                        writer.Write(HalfHelper.Compress(state.position.z));
                    }
                }
                else
                {
                    if (smoothSync.isSyncingXPosition)
                    {
                        writer.Write(state.position.x);
                    }
                    if (smoothSync.isSyncingYPosition)
                    {
                        writer.Write(state.position.y);
                    }
                    if (smoothSync.isSyncingZPosition)
                    {
                        writer.Write(state.position.z);
                    }
                }
            }
            // write velocity
            if (sendVelocity)
            {
                if (smoothSync.isVelocityCompressed)
                {
                    if (smoothSync.isSyncingXVelocity)
                    {
                        writer.Write(HalfHelper.Compress(state.velocity.x));
                    }
                    if (smoothSync.isSyncingYVelocity)
                    {
                        writer.Write(HalfHelper.Compress(state.velocity.y));
                    }
                    if (smoothSync.isSyncingZVelocity)
                    {
                        writer.Write(HalfHelper.Compress(state.velocity.z));
                    }
                }
                else
                {
                    if (smoothSync.isSyncingXVelocity)
                    {
                        writer.Write(state.velocity.x);
                    }
                    if (smoothSync.isSyncingYVelocity)
                    {
                        writer.Write(state.velocity.y);
                    }
                    if (smoothSync.isSyncingZVelocity)
                    {
                        writer.Write(state.velocity.z);
                    }
                }
            }
            // write rotation
            if (sendRotation)
            {
                Vector3 rot = state.rotation.eulerAngles;
                if (smoothSync.isRotationCompressed)
                {
                    if (smoothSync.isSyncingXRotation)
                    {
                        writer.Write(HalfHelper.Compress(rot.x));
                    }
                    if (smoothSync.isSyncingYRotation)
                    {
                        writer.Write(HalfHelper.Compress(rot.y));
                    }
                    if (smoothSync.isSyncingZRotation)
                    {
                        writer.Write(HalfHelper.Compress(rot.z));
                    }
                }
                else
                {
                    if (smoothSync.isSyncingXRotation)
                    {
                        writer.Write(rot.x);
                    }
                    if (smoothSync.isSyncingYRotation)
                    {
                        writer.Write(rot.y);
                    }
                    if (smoothSync.isSyncingZRotation)
                    {
                        writer.Write(rot.z);
                    }
                }
            }
            // write angular velocity
            if (sendAngularVelocity)
            {
                if (smoothSync.isAngularVelocityCompressed)
                {
                    if (smoothSync.isSyncingXAngularVelocity)
                    {
                        writer.Write(HalfHelper.Compress(state.angularVelocity.x));
                    }
                    if (smoothSync.isSyncingYAngularVelocity)
                    {
                        writer.Write(HalfHelper.Compress(state.angularVelocity.y));
                    }
                    if (smoothSync.isSyncingZAngularVelocity)
                    {
                        writer.Write(HalfHelper.Compress(state.angularVelocity.z));
                    }
                }
                else
                {
                    if (smoothSync.isSyncingXAngularVelocity)
                    {
                        writer.Write(state.angularVelocity.x);
                    }
                    if (smoothSync.isSyncingYAngularVelocity)
                    {
                        writer.Write(state.angularVelocity.y);
                    }
                    if (smoothSync.isSyncingZAngularVelocity)
                    {
                        writer.Write(state.angularVelocity.z);
                    }
                }
            }
        }

        /// <summary>
        /// Deserialize a message from the network
        /// </summary>
        /// <remarks>
        /// Only receives what it needs. Optionally decompresses floats depending on the settings on the SmoothSync object.
        /// </remarks>
        /// <param name="writer">The Networkreader to read from </param>
        override public void Deserialize(NetworkReader reader)
        {
            byte syncInfoByte = reader.ReadByte();
            bool syncPosition = shouldSyncPosition(syncInfoByte);
            bool syncRotation = shouldSyncRotation(syncInfoByte);
            bool syncVelocity = shouldSyncVelocity(syncInfoByte);
            bool syncAngularVelocity = shouldSyncAngularVelocity(syncInfoByte);

            NetworkInstanceId netID = reader.ReadNetworkId();
            int syncIndex = (int)reader.ReadPackedUInt32();
            state.ownerTimestamp = (int)reader.ReadPackedUInt32();
            GameObject ob = null;

            if (NetworkServer.active)
            {
                ob = NetworkServer.FindLocalObject(netID);
            }
            else
            {
                ob = ClientScene.FindLocalObject(netID);
            }

            if (!ob)
            {
                Debug.LogWarning("Could not find target for network state message.");
                return;
            }

            // Doesn't matter which SmoothSync is returned since they all have the same list.
            smoothSync = ob.GetComponent<SmoothSync>();
            // Find the correct object to sync according to syncIndex
            for (int i = 0; i < smoothSync.childObjectSmoothSyncs.Length; i++)
            {
                if (smoothSync.childObjectSmoothSyncs[i].syncIndex == syncIndex)
                {
                    smoothSync = smoothSync.childObjectSmoothSyncs[i];
                }
            }

            if (!smoothSync)
            {
                Debug.LogWarning("Could not find target for network state message.");
                return;
            }

            // read position
            if (syncPosition)
            {
                if (smoothSync.isPositionCompressed)
                {
                    if (smoothSync.isSyncingXPosition)
                    {
                        state.position.x = HalfHelper.Decompress(reader.ReadUInt16());
                    }
                    if (smoothSync.isSyncingYPosition)
                    {
                        state.position.y = HalfHelper.Decompress(reader.ReadUInt16());
                    }
                    if (smoothSync.isSyncingZPosition)
                    {
                        state.position.z = HalfHelper.Decompress(reader.ReadUInt16());
                    }
                }
                else
                {
                    if (smoothSync.isSyncingXPosition)
                    {
                        state.position.x = reader.ReadSingle();
                    }
                    if (smoothSync.isSyncingYPosition)
                    {
                        state.position.y = reader.ReadSingle();
                    }
                    if (smoothSync.isSyncingZPosition)
                    {
                        state.position.z = reader.ReadSingle();
                    }
                }
            }
            else
            {
                if (smoothSync.stateCount > 0)
                {
                    state.position = smoothSync.stateBuffer[0].position;
                }
                else
                {
                    state.position = smoothSync.getPosition();
                }
            }
            // read velocity
            if (syncVelocity)
            {
                if (smoothSync.isVelocityCompressed)
                {
                    if (smoothSync.isSyncingXVelocity)
                    {
                        state.velocity.x = HalfHelper.Decompress(reader.ReadUInt16());
                    }
                    if (smoothSync.isSyncingYVelocity)
                    {
                        state.velocity.y = HalfHelper.Decompress(reader.ReadUInt16());
                    }
                    if (smoothSync.isSyncingZVelocity)
                    {
                        state.velocity.z = HalfHelper.Decompress(reader.ReadUInt16());
                    }
                }
                else
                {
                    if (smoothSync.isSyncingXVelocity)
                    {
                        state.velocity.x = reader.ReadSingle();
                    }
                    if (smoothSync.isSyncingYVelocity)
                    {
                        state.velocity.y = reader.ReadSingle();
                    }
                    if (smoothSync.isSyncingZVelocity)
                    {
                        state.velocity.z = reader.ReadSingle();
                    }
                }
            }
            else
            {
                state.velocity = Vector3.zero;
            }

            // read rotation
            if (syncRotation)
            {
                Vector3 rot = new Vector3();
                if (smoothSync.isRotationCompressed)
                {
                    if (smoothSync.isSyncingXRotation)
                    {
                        rot.x = HalfHelper.Decompress(reader.ReadUInt16());
                    }
                    if (smoothSync.isSyncingYRotation)
                    {
                        rot.y = HalfHelper.Decompress(reader.ReadUInt16());
                    }
                    if (smoothSync.isSyncingZRotation)
                    {
                        rot.z = HalfHelper.Decompress(reader.ReadUInt16());
                    }
                    state.rotation = Quaternion.Euler(rot);
                }
                else
                {
                    if (smoothSync.isSyncingXRotation)
                    {
                        rot.x = reader.ReadSingle();
                    }
                    if (smoothSync.isSyncingYRotation)
                    {
                        rot.y = reader.ReadSingle();
                    }
                    if (smoothSync.isSyncingZRotation)
                    {
                        rot.z = reader.ReadSingle();
                    }
                    state.rotation = Quaternion.Euler(rot);
                }
            }
            else
            {
                if (smoothSync.stateCount > 0)
                {
                    state.rotation = smoothSync.stateBuffer[0].rotation;
                }
                else
                {
                    state.rotation = smoothSync.getRotation();
                }
            }
            // read anguluar velocity
            if (syncAngularVelocity)
            {
                if (smoothSync.isAngularVelocityCompressed)
                {
                    if (smoothSync.isSyncingXAngularVelocity)
                    {
                        state.angularVelocity.x = HalfHelper.Decompress(reader.ReadUInt16());
                    }
                    if (smoothSync.isSyncingYAngularVelocity)
                    {
                        state.angularVelocity.y = HalfHelper.Decompress(reader.ReadUInt16());
                    }
                    if (smoothSync.isSyncingZAngularVelocity)
                    {
                        state.angularVelocity.z = HalfHelper.Decompress(reader.ReadUInt16());
                    }
                }
                else
                {
                    if (smoothSync.isSyncingXAngularVelocity)
                    {
                        state.angularVelocity.x = reader.ReadSingle();
                    }
                    if (smoothSync.isSyncingYAngularVelocity)
                    {
                        state.angularVelocity.y = reader.ReadSingle();
                    }
                    if (smoothSync.isSyncingZAngularVelocity)
                    {
                        state.angularVelocity.z = reader.ReadSingle();
                    }
                }
            }
            else
            {
                state.angularVelocity = Vector3.zero;
            }
        }
        /// <summary>
        /// Hardcoded information to determine position syncing.
        /// </summary>
        byte positionMask = 1;        // 0000_0001
        /// <summary>
        /// Hardcoded information to determine rotation syncing.
        /// </summary>
        byte rotationMask = 2;        // 0000_0010
        /// <summary>
        /// Hardcoded information to determine velocity syncing.
        /// </summary>
        byte velocityMask = 4;        // 0000_0100
        /// <summary>
        /// Hardcoded information to determine angular velocity syncing.
        /// </summary>
        byte angularVelocityMask = 8; // 0000_1000
        /// <summary>
        /// Encode sync info based on what we want to send.
        /// </summary>
        byte encodeSyncInformation(bool sendPosition, bool sendRotation, bool sendVelocity, bool sendAngularVelocity)
        {
            byte encoded = 0;

            if (sendPosition)
            {
                encoded = (byte)(encoded | positionMask);
            }
            if (sendRotation)
            {
                encoded = (byte)(encoded | rotationMask);
            }
            if (sendVelocity)
            {
                encoded = (byte)(encoded | velocityMask);
            }
            if (sendAngularVelocity)
            {
                encoded = (byte)(encoded | angularVelocityMask);
            }
            return encoded;
        }
        /// <summary>
        /// Decode sync info to see if we want to sync position.
        /// </summary>
        bool shouldSyncPosition (byte syncInformation)
        {
            if ((syncInformation & positionMask) == positionMask)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// Decode sync info to see if we want to sync rotation.
        /// </summary>
        bool shouldSyncRotation(byte syncInformation)
        {
            if ((syncInformation & rotationMask) == rotationMask)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// Decode sync info to see if we want to sync velocity.
        /// </summary>
        bool shouldSyncVelocity(byte syncInformation)
        {
            if ((syncInformation & velocityMask) == velocityMask)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// Decode sync info to see if we want to sync angular velocity.
        /// </summary>
        bool shouldSyncAngularVelocity(byte syncInformation)
        {
            if ((syncInformation & angularVelocityMask) == angularVelocityMask)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// Assign sync info based on what we were sent.
        /// </summary>
        SyncInfo assignSyncInfo(int syncPositionRotationInfo)
        {
            if (syncPositionRotationInfo == 3)
            {
                return SyncInfo.BOTH;
            }
            if (syncPositionRotationInfo == 2)
            {
                return SyncInfo.ROTATION;
            }
            if (syncPositionRotationInfo == 1)
            {
                return SyncInfo.POSITION;
            }
            return SyncInfo.NONE;
        }
    }
}