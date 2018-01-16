using UnityEngine;
using UnityEngine.Networking;
using System;
using UnityEngine.Serialization;

namespace Smooth
{
    /// <summary>
    /// Sync a transform or rigidbody over the network. Includes interpolation and extrapolation.
    /// </summary>
    public class SmoothSync : NetworkBehaviour
    {
        #region Configuration

        /// <summary>How far in the past non-owner's should be.</summary>
        /// <remarks>
        /// Increasing this will make interpolation more likely to be used, 
        /// which means the synced position will be more likely to be an actual position that the owner was at (but more in the past of course).
        /// 
        /// Decreasing this will make extrapolation more likely to be used, 
        /// this will increase reponsiveness but the position will be less correct to where the player was actually at.
        /// The position is being predicted into the future, so it is hopefully closer to the owner's actual position on 
        /// their system before you even receive the message as to where they literally were.
        /// Measured in seconds.
        /// </remarks>
        [Tooltip("Increasing will make interpolation more likely to be used, decreasing will make extrapolation more likely to be used. In seconds.")]
        public float interpolationBackTime = .1f;

        /// <summary>How much time into the 'future' a non-owner is allowed to extrapolate.</summary>
        /// <remarks>
        /// Extrapolating too far tends to cause erratic and non-realistic movement but a little bit of extrapolation is 
        /// better than none because it keeps things working semi-right during latency spikes.
        /// Measured in seconds.
        /// Must be syncing velocity in order to utilize extrapolation.
        /// </remarks>
        [Tooltip("How much time into the 'future' a non-owner is allowed to extrapolate. In seconds.")]
        public float extrapolationTimeLimit = .3f;
        /// <summary>How much distance into the 'future' a non-owner is allowed to extrapolate.</summary>
        /// <remarks>
        /// Extrapolating too far tends to cause erratic and non-realistic movement but a little bit of extrapolation is 
        /// better than none because it keeps things working semi-right during latency spikes.
        /// Measured in distance units.
        /// Must be syncing velocity in order to utilize extrapolation.
        /// </remarks>
        [Tooltip("How much distance into the 'future' a non-owner is allowed to extrapolate. In distance units.")]
        public float extrapolationDistanceLimit = .3f;

        /// <summary>A synced object's position is only sent if its position is off from the last sent position 
        /// by more than the threshold. 
        /// Measured in distance units.</summary>
        [Tooltip("A synced object's position is only sent if it is off from the last sent position by more than the threshold. In distance units.")]
        public float sendMovementThreshold = .001f;

        /// <summary>A synced object's rotation is only sent if its rotation is off from the last sent rotation 
        /// by more than the threshold.
        /// Measured in degrees.</summary>
        [Tooltip("A synced object's rotation is only sent if it is off from the last sent rotation by more than the threshold. In degrees.")]
        public float sendRotationThreshold = .001f;

        /// <summary>A synced object's velocity is only sent if its velocity is off from the last sent velocity
        /// by more than the threshold. 
        /// Measured in velocity units.</summary>
        [Tooltip("A synced object's velocity is only sent if it is off from the last sent velocity by more than the threshold. In velocity units.")]
        public float sendVelocityThreshold = .001f;

        /// <summary>A synced object's angular velocity is only sent if its angular velocity is off from the last sent angular velocity
        /// by more than the threshold. 
        /// Measured in radians per second.</summary>
        [Tooltip("A synced object's angular velocity is only sent if it is off from the last sent angular velocity by more than the threshold. In radians per second.")]
        public float sendAngularVelocityThreshold = .001f;

        /// <summary>A synced object's position is only updated if it is off from the target position 
        /// by more than the threshold. Usually keep this at 0 or really low but it's useful if you are 
        /// extrapolating into the future and want to stop instantly and not have it backtrack to where it was.
        /// Measured in distance units.</summary>
        [Tooltip("A synced object's position is only updated if it is off from the target position by more than the threshold. In distance units.")]
        [FormerlySerializedAs("movementThreshold")] // So you don't lose your settings when I change variable names.
        [SerializeField]
        public float receivedMovementThreshold = .001f;

        /// <summary>A synced object's rotation is only updated if it is off from the target rotation 
        /// by more than the threshold. Usually keep this at 0 or really low but it's useful if you are 
        /// extrapolating into the future and want to stop instantly and not have it backtrack to where it was.
        /// Measured in degrees.</summary>
        [Tooltip("A synced object's rotation is only updated if it is off from the target rotation by more than the threshold. In degrees.")]
        [SerializeField]
        public float receivedRotationThreshold = .001f;

        /// <summary>If a synced object's position is more than positionSnapThreshold units from the target position it will jump to the target position immediately instead of lerping.
        /// Measured in distance units.</summary>
        [Tooltip("If a synced object's position is more than snapThreshold units from the target position it will jump to the target position immediately instead of lerping. In distance units.")]
        public float positionSnapThreshold = 5;

        /// <summary>If a synced object's rotation is more than rotationSnapThreshold from the target rotation it will jump to the target position immediately instead of lerping.
        /// Measured in degrees.</summary>
        [Tooltip("If a synced object's position is more than snapThreshold units from the target position it will jump to the target position immediately instead of lerping. In degrees.")]
        public float rotationSnapThreshold = 60;

        /// <summary>How fast to lerp to the target state. Value 0 to 1.</summary>
        /// <remarks>
        /// Lower values mean smoother but maybe sluggish movement. 
        /// Higher values mean more responsive but maybe jerky or stuttery movement.
        /// </remarks>
        [Range(0, 1)]
        [Tooltip("How fast to lerp to the new target state.")]
        public float lerpSpeed = .2f;

        /// <summary>Position sync mode</summary>
        /// <remarks>
        /// Fine tune how position is synced. 
        /// For 3D use SyncMode.XYZ
        /// For 2D use SyncMode.XY
        /// For objects that don't move use SyncMode.NONE
        /// </remarks>
        [Tooltip("Fine tune how position is synced.")]
        public SyncMode syncPosition = SyncMode.XYZ;

        /// <summary>Rotation sync mode</summary>
        /// <remarks>
        /// Fine tune how rotation is synced. 
        /// For 2D use RotationSyncMode.XYZ
        /// For 2D use RotationSyncMode.Z
        /// For objects that do not rotate use RotationSyncMode.NONE
        /// </remarks>
        [Tooltip("Fine tune how rotation is synced.")]
        public SyncMode syncRotation = SyncMode.XYZ;

        /// <summary>Velocity sync mode</summary>
        /// <remarks>
        /// Fine tune how velocity is synced. 
        /// For 3D use SyncMode.XYZ
        /// For 2D use SyncMode.XY
        /// For non-physics objects use SyncMode.NONE
        /// </remarks>
        [Tooltip("Fine tune how velocity is synced.")]
        public SyncMode syncVelocity = SyncMode.XYZ;

        /// <summary>Angular velocity sync mode</summary>
        /// <remarks>
        /// Fine tune how angular velocity is synced. 
        /// For 3D use RotationSyncMode.XYZ
        /// For 2D use RotationSyncMode.Z
        /// For non-physics objects use RotationSyncMode.NONE
        /// </remarks>
        [Tooltip("Fine tune how angular velocity is synced.")]
        public SyncMode syncAngularVelocity = SyncMode.XYZ;

        /// <summary>Compress position floats when sending over the network.</summary>
        /// <remarks>
        /// Convert position floats sent over the network to Halfs, which use half as much bandwidth but are also half as precise.
        /// </remarks>
        [Tooltip("Compress floats to save bandwidth.")]
        public bool isPositionCompressed = true;
        /// <summary>Compress rotation floats when sending over the network.</summary>
        /// <remarks>
        /// Convert rotation floats sent over the network to Halfs, which use half as much bandwidth but are also half as precise.
        /// </remarks>
        [Tooltip("Compress floats to save bandwidth.")]
        public bool isRotationCompressed = true;
        /// <summary>Compress velocity floats when sending over the network.</summary>
        /// <remarks>
        /// Convert velocity floats sent over the network to Halfs, which use half as much bandwidth but are also half as precise.
        /// </remarks>
        [Tooltip("Compress floats to save bandwidth.")]
        public bool isVelocityCompressed = true;
        /// <summary>Compress angular velocity floats when sending over the network.</summary>
        /// <remarks>
        /// Convert angular velocity floats sent over the network to Halfs, which use half as much bandwidth but are also half as precise.
        /// </remarks>
        [Tooltip("Compress floats to save bandwidth.")]
        public bool isAngularVelocityCompressed = true;

        /// <summary>How many times per second to send network updates</summary>
        /// <remarks>
        /// Keep in mind actual send rate may be limited by the NetworkManager configuration.
        /// </remarks>
        [Tooltip("How many times per second to send network updates")]
        public float sendRate = 30;

        /// <summary>The channel to send network updates on.</summary>
        [Tooltip("The channel to send network updates on.")]
        public int networkChannel = Channels.DefaultUnreliable;

        /// <summary>Child object to sync</summary>
        /// <remarks>
        /// Leave blank if you want to sync this object. 
        /// In order to sync a child object, you must add two instances of SmoothSync to the parent. 
        /// Set childObjectToSync on one of them to point to the child you want to sync and leave it blank on the other to sync the parent.
        /// You cannot sync children without syncing the parent.
        /// </remarks>
        [Tooltip("Set this to sync a child object, leave blank to sync this object. Must have one blank to sync the parent in order to sync children.")]
        public GameObject childObjectToSync;
        /// <summary>Does this game object have a child object to sync?</summary>
        /// <remarks>
        /// Is much less resource intensive to check a boolean than if a Gameobject exists.
        /// </remarks>
        [NonSerialized]
        public bool hasChildObject = false;

        #endregion Configuration

        #region Runtime data

        /// <summary>Non-owners keep a list of recent states received over the network for interpolating</summary>
        [NonSerialized]
        public State[] stateBuffer = new State[10]; // TODO: Users should be able to set the length of this. Also a circular buffer would be more efficient but it probably doesn't matter.

        /// <summary>The number of states in the stateBuffer</summary>
        [NonSerialized]
        public int stateCount;

        /// <summary>Store a reference to the rigidbody so that we only have to call GetComponent() once.</summary>
        /// <remarks>Will automatically use Rigidbody or Rigidbody2D depending on what is on the game object.</remarks>
        [NonSerialized]
        public Rigidbody rb;
        /// <summary>Does this game object have a Rigidbody component?</summary>
        /// <remarks>
        /// Is much less resource intensive to check a boolean than if a component exists.
        /// </remarks>
        [NonSerialized]
        public bool hasRigdibody = false;
        /// <summary>Store a reference to the 2D rigidbody so that we only have to call GetComponent() once.</summary>
        [NonSerialized]
        public Rigidbody2D rb2D;
        /// <summary>Does this game object have a Rigidbody2D component?</summary>
        /// <remarks>
        /// Is much less resource intensive to check a boolean than if a component exists.
        /// </remarks>
        [NonSerialized]
        public bool hasRigidbody2D = false;

        /// <summary>
        /// Used via stopLerping() and restartLerping() to 'teleport' a synced object without unwanted lerping.
        /// Useful for player spawning and whatnot.
        /// </summary>
        bool skipLerp = false;
        /// <summary>
        /// Used via stopLerping() and restartLerping() to 'teleport' a synced object without unwanted lerping.
        /// Useful for player spawning and whatnot.
        /// </summary>
        bool dontLerp = false;
        /// <summary>Last time the object was teleported.</summary>
        [NonSerialized]
        public float lastTeleportOwnerTime;

        /// <summary>Last time owner sent state.</summary>
        [NonSerialized]
        public float lastTimeStateWasSent;

        /// <summary>Last time state was received on non-owner.</summary>
        [NonSerialized]
        public float lastTimeStateWasReceived;

        /// <summary>Position owner was at when the last position state was sent.</summary>
        [NonSerialized]
        public Vector3 lastPositionWhenStateWasSent;

        /// <summary>Rotation owner was at when the last rotation state was sent.</summary>
        [NonSerialized]
        public Quaternion lastRotationWhenStateWasSent = Quaternion.identity;

        /// <summary>Velocity owner was at when the last velocity state was sent.</summary>
        [NonSerialized]
        public Vector3 lastVelocityWhenStateWasSent;

        /// <summary>Angular velocity owner was at when the last angular velociy state was sent.</summary>
        [NonSerialized]
        public Vector3 lastAngularVelocityWhenStateWasSent;

        /// <summary>Cached network ID since we use it a lot.</summary>
        NetworkIdentity netID;

        /// <summary>Gets assigned to the real object to sync. Either this object or a child object.</summary>
        [NonSerialized]
        public GameObject realObjectToSync;
        /// <summary>Index to know which object to sync.</summary>
        [NonSerialized]
        public int syncIndex = 0;
        /// <summary>Reference to child objects so you can compare to syncIndex.</summary>
        [NonSerialized]
        public SmoothSync[] childObjectSmoothSyncs = new SmoothSync[0];

        /// <summary>State when extrapolation ended.</summary>
        State extrapolationEndState;
        /// <summary>Time when extrapolation ended.</summary>
        float extrapolationStopTime;

        #endregion Runtime data

        #region Unity methods

        /// <summary>Cache references to components.</summary>
        void Awake()
        {
            netID = GetComponent<NetworkIdentity>();
            rb = GetComponent<Rigidbody>();
            rb2D = GetComponent<Rigidbody2D>();
            if (rb && childObjectToSync == null)
            {
                hasRigdibody = true;
            }
            if (rb2D && childObjectToSync == null)
            {
                hasRigidbody2D = true;
                // If 2D rigidbody, it only has a velocity of X, Y and an angular veloctiy of Z. So force it if you want any syncing.
                if (syncVelocity != SyncMode.NONE) syncVelocity = SyncMode.XY;
                if (syncAngularVelocity != SyncMode.NONE) syncAngularVelocity = SyncMode.Z;
                
            }
            // If no rigidbodies or is child object, there is no rigidbody supplied velocity, so don't sync it.
            if ((!rb && !rb2D) || childObjectToSync)
            {
                syncVelocity = SyncMode.NONE;
                syncAngularVelocity = SyncMode.NONE;
            }
            // If you want to sync a child object, assign it
            if (childObjectToSync)
            {
                realObjectToSync = childObjectToSync;
                hasChildObject = true;

                // Throw error if no SmoothSync script is handling the parent object
                bool foundAParent = false;
                childObjectSmoothSyncs = GetComponents<SmoothSync>();
                for (int i = 0; i < childObjectSmoothSyncs.Length; i++)
                {
                    if (!childObjectSmoothSyncs[i].childObjectToSync)
                    {
                        foundAParent = true;
                    }
                }
                if (!foundAParent)
                {
                    Debug.LogError("Must have one SmoothSync script with unassigned childObjectToSync to sync the parent object");
                }
            }
            // If you want to sync this object, assign it
            // and then assign indexes to know which objects to sync to what.
            // Unity guarantees same order in GetComponents<>() so indexes are already synced across the network
            else
            {
                realObjectToSync = this.gameObject;

                int indexToGive = 0;
                childObjectSmoothSyncs = GetComponents<SmoothSync>();
                for (int i = 0; i < childObjectSmoothSyncs.Length; i++)
                {
                    childObjectSmoothSyncs[i].syncIndex = indexToGive;
                    indexToGive++;
                }
            }
        }

        /// <summary>Get position of object based on if child or not</summary>
        public Vector3 getPosition()
        {
            if (hasChildObject)
            {
                return realObjectToSync.transform.localPosition;
            }
            else
            {
                return realObjectToSync.transform.position;
            }
        }
        /// <summary>Get rotation of object based on if child or not</summary>
        public Quaternion getRotation()
        {
            if (hasChildObject)
            {
                return realObjectToSync.transform.localRotation;
            }
            else
            {
                return realObjectToSync.transform.rotation;
            }
        }
        /// <summary>Set position of object based on if child or not</summary>
        public void setPosition(Vector3 position, bool isTeleporting)
        {
            if (hasChildObject)
            {
                realObjectToSync.transform.localPosition = position;
            }
            else
            {
                if (hasRigdibody && !isTeleporting)
                {
                    rb.MovePosition(position);
                }
                if (hasRigidbody2D && !isTeleporting)
                {
                    rb2D.MovePosition(position);
                }
                else
                {
                    realObjectToSync.transform.position = position;
                }
            }
        }
        /// <summary>Set rotation of object based on if child or not</summary>
        public void setRotation(Quaternion rotation, bool isTeleporting)
        {
            if (hasChildObject)
            {
                realObjectToSync.transform.localRotation = rotation;
            }
            else
            {
                if (hasRigdibody && !isTeleporting)
                {
                    rb.MoveRotation(rotation);
                }
                if (hasRigidbody2D && !isTeleporting)
                {
                    rb2D.MoveRotation(rotation.eulerAngles.z);
                }
                else
                {
                    realObjectToSync.transform.rotation = rotation;
                }
            }
        }

        /// <summary>Set interpolated / extrapolated position on non-owners.</summary>
        void FixedUpdate()
        {
            if (hasAuthority) return;

            setInterpolationPosition();
        }

        #endregion

        #region Internal stuff

        /// <summary>Use the state buffer to set interpolated/extrapolated position on non-owners.</summary>
        void setInterpolationPosition()
        {
            if (stateCount == 0) return;

            State targetState;
            bool triedToExtrapolateTooFar = false;
            
            if (dontLerp)
            {
                targetState = new State(this);
            }
            else
            {
                // The target playback time
                float interpolationTime = approximateNetworkTimeOnOwner - interpolationBackTime * 1000;

                // Use interpolation if the target playback time is present in the buffer
                if (stateCount > 1 && stateBuffer[0].ownerTimestamp > interpolationTime)
                {
                    interpolate(interpolationTime, out targetState);
                }
                // The newest state is too old, we'll have to use extrapolation
                else
                {
                    bool success = extrapolate(interpolationTime, out targetState);
                    triedToExtrapolateTooFar = !success;
                }
            }

            // This all has to do with teleporting
            float actualLerpSpeed = lerpSpeed;
            // Clamp value between 0 and 1, because 1 is instant lerping and 0 is no lerping.
            Mathf.Clamp01(actualLerpSpeed);

            if (skipLerp)
            {
                actualLerpSpeed = 1;
                skipLerp = false;
                dontLerp = false;
            }
            else if (dontLerp)
            {
                stateCount = 0;
                actualLerpSpeed = 1;
            }

            // Finally actually set position and velocity (as long as we didn't try and extrapolate too far)
            if (!triedToExtrapolateTooFar || (!hasRigdibody && !hasRigidbody2D))
            {
                bool changedDistanceEnough = false;
                float distance = Vector3.Distance(getPosition(), targetState.position);
                if (distance > receivedMovementThreshold)
                {
                    changedDistanceEnough = true;
                }
                bool changedAnglesEnough = false;
                float angleDifference = Quaternion.Angle(getRotation(), targetState.rotation);
                if (angleDifference > receivedRotationThreshold)
                {
                    changedAnglesEnough = true;
                }
                if (hasRigdibody && !rb.isKinematic)
                {
                    if (changedDistanceEnough)
                    {
                        Vector3 newVelocity = rb.velocity;
                        if (isSyncingXVelocity)
                        {
                            newVelocity.x = targetState.velocity.x;
                        }
                        if (isSyncingYVelocity)
                        {
                            newVelocity.y = targetState.velocity.y;
                        }
                        if (isSyncingZVelocity)
                        {
                            newVelocity.z = targetState.velocity.z;
                        }
                        rb.velocity = Vector3.Lerp(rb.velocity, newVelocity, actualLerpSpeed);
                    }
                    else
                    {
                        rb.velocity = Vector3.zero;
                        rb.angularVelocity = Vector3.zero;
                    }
                    if (changedAnglesEnough)
                    {
                        Vector3 newAngularVelocity = rb.angularVelocity;
                        if (isSyncingXAngularVelocity)
                        {
                            newAngularVelocity.x = targetState.angularVelocity.x;
                        }
                        if (isSyncingYAngularVelocity)
                        {
                            newAngularVelocity.y = targetState.angularVelocity.y;
                        }
                        if (isSyncingZAngularVelocity)
                        {
                            newAngularVelocity.z = targetState.angularVelocity.z;
                        }
                        rb.angularVelocity = Vector3.Lerp(rb.angularVelocity, newAngularVelocity, actualLerpSpeed);
                    }
                    else
                    {
                        rb.angularVelocity = Vector3.zero;
                    }
                }
                else if (hasRigidbody2D && !rb2D.isKinematic)
                {
                    if (syncVelocity == SyncMode.XY)
                    {
                        if (changedDistanceEnough)
                        {
                            rb2D.velocity = Vector2.Lerp(rb2D.velocity, targetState.velocity, actualLerpSpeed);
                        }
                        else
                        {
                            rb2D.velocity = Vector2.zero;
                        }                        
                    }
                    if (syncAngularVelocity == SyncMode.Z)
                    {
                        if (changedAnglesEnough)
                        {
                            rb2D.angularVelocity = Mathf.Lerp(rb2D.angularVelocity, targetState.angularVelocity.z, actualLerpSpeed);
                        }
                        else
                        {
                            rb2D.angularVelocity = 0;
                        }
                    }
                }
                if (syncPosition != SyncMode.NONE)
                {
                    if (changedDistanceEnough)
                    {
                        float positionLerpSpeed = actualLerpSpeed;
                        bool shouldTeleport = false;
                        if (distance > positionSnapThreshold)
                        {
                            positionLerpSpeed = 1;
                            shouldTeleport = true;
                        }
                        Vector3 newPosition = getPosition();
                        if (isSyncingXPosition)
                        {
                            newPosition.x = targetState.position.x;
                        }
                        if (isSyncingYPosition)
                        {
                            newPosition.y = targetState.position.y;
                        }
                        if (isSyncingZPosition)
                        {
                            newPosition.z = targetState.position.z;
                        }
                        setPosition(Vector3.Lerp(getPosition(), newPosition, positionLerpSpeed), shouldTeleport);
                    }
                }
                if (syncRotation != SyncMode.NONE)
                {
                    if (changedAnglesEnough) 
                    {
                        float positionLerpSpeed = actualLerpSpeed;
                        bool shouldTeleport = false;
                        if (angleDifference > rotationSnapThreshold)
                        {
                            positionLerpSpeed = 1;
                            shouldTeleport = true;
                        }
                        Vector3 newRotation = getRotation().eulerAngles;
                        if (isSyncingXRotation)
                        {
                            newRotation.x = targetState.rotation.eulerAngles.x;
                        }
                        if (isSyncingYRotation)
                        {
                            newRotation.y = targetState.rotation.eulerAngles.y;
                        }
                        if (isSyncingZRotation)
                        {
                            newRotation.z = targetState.rotation.eulerAngles.z;
                        }
                        Quaternion newQuaternion = Quaternion.Euler(newRotation);
                        setRotation(Quaternion.Lerp(getRotation(), newQuaternion, positionLerpSpeed), shouldTeleport);
                    }
                }
            }
            else
            {
                // Don't let position get too far off
                if (Vector3.Distance(stateBuffer[0].position, realObjectToSync.transform.position) >= extrapolationDistanceLimit)
                {
                    if (hasRigdibody)
                    {
                        rb.velocity = Vector3.zero;
                        rb.angularVelocity = Vector3.zero;
                    }
                    if (hasRigidbody2D)
                    {
                        rb2D.velocity = Vector2.zero;
                        rb2D.angularVelocity = 0;
                    }
                }
            }
        }

        /// <summary>
        /// Interpolate between two States from the stateBuffer in order calculate the targetState.
        /// </summary>
        /// <param name="interpolationTime">The target time</param>
        void interpolate(float interpolationTime, out State targetState)
        {
            // Go through buffer and find correct state to play back
            int stateIndex = 0;
            for (; stateIndex < stateCount; stateIndex++)
            {
                if (stateBuffer[stateIndex].ownerTimestamp <= interpolationTime) break;
            }
            
            if (stateIndex == stateCount)
            {
                //Debug.LogError("Ran out of states in SmoothSync state buffer for object: " + gameObject.name);
                stateIndex--;
            }

            // The state one slot newer than the best playback state
            State end = stateBuffer[Mathf.Max(stateIndex - 1, 0)];
            // The best playback state
            State start = stateBuffer[stateIndex];

            // Calculate how far between the two states we should be
            float t = (interpolationTime - start.ownerTimestamp) / (end.ownerTimestamp - start.ownerTimestamp);

            // Interpolate between the states to get the target state
            targetState = State.Lerp(start, end, t);
        }

        /// <summary>
        /// Attempt to extrapolate from the newest state in the buffer
        /// </summary>
        /// <param name="interpolationTime">The target time</param>
        /// <returns>true on success, false if interpolationTime is more than extrapolationLength in the future</returns>
        bool extrapolate(float interpolationTime, out State targetState) // TODO: Wouldn't it make sense to at least extrapolate up to extrapolation limit even when it's trying to extrapolate too far?
        {
            // Start from the latest state
            targetState = new State(stateBuffer[0]);

            // See how far we will need to extrapolate
            float extrapolationLength = (interpolationTime - targetState.ownerTimestamp) / 1000.0f;

            // If latest received velocity is close to zero, don't extrapolate. This is so we don't
            // try to extrapolate through the ground while at rest.
            if (syncVelocity == SyncMode.NONE || targetState.velocity.magnitude < sendVelocityThreshold)
            {
                return true;
            }

            if (((hasRigdibody && !rb.isKinematic) || (hasRigidbody2D && !rb2D.isKinematic)))
            {
                float simulatedTime = 0;
                while (simulatedTime < extrapolationLength)
                {
                    // Don't extrapolate for more than extrapolationTimeLimit or things get crazy
                    if (simulatedTime > extrapolationTimeLimit)
                    {
                        if (extrapolationStopTime < lastTimeStateWasReceived)
                        {
                            extrapolationEndState = targetState;
                        }
                        extrapolationStopTime = Time.realtimeSinceStartup;
                        targetState = extrapolationEndState;
                        return false;
                    }

                    float timeDif = Mathf.Min(Time.fixedDeltaTime, extrapolationLength - simulatedTime);

                    // Velocity
                    targetState.position += targetState.velocity * timeDif;

                    // Gravity
                    if (hasRigdibody && rb.useGravity)
                    {
                        targetState.velocity += Physics.gravity * timeDif;
                    }
                    else if (hasRigidbody2D)
                    {
                        targetState.velocity += Physics.gravity * rb2D.gravityScale * timeDif;
                    }

                    // Drag
                    if (hasRigdibody)
                    {
                        targetState.velocity -= targetState.velocity * timeDif * rb.drag;
                    }
                    else if (hasRigidbody2D)
                    {
                        targetState.velocity -= targetState.velocity * timeDif * rb2D.drag;
                    }
                    
                    // Angular velocity
                    float axisLength = timeDif * targetState.angularVelocity.magnitude * Mathf.Rad2Deg;
                    Quaternion angularRotation = Quaternion.AngleAxis(axisLength, targetState.angularVelocity);
                    targetState.rotation = angularRotation * targetState.rotation;

                    // TODO: Angular drag?!

                    // Don't extrapolate for more than extrapolationDistanceLimit or things get crazy
                    if (Vector3.Distance(stateBuffer[0].position, targetState.position) >= extrapolationDistanceLimit)
                    {
                        extrapolationEndState = targetState;
                        extrapolationStopTime = Time.realtimeSinceStartup;
                        targetState = extrapolationEndState;
                        return false;
                    }

                    simulatedTime += Time.fixedDeltaTime;
                }
            }

            return true;
        }

        #endregion Internal stuff

        #region Public interface

        /// <summary>Add an incoming state to the stateBuffer on non-owners.</summary>
        public void addState(State state)
        {
            if (stateCount > 1 && state.ownerTimestamp < stateBuffer[0].ownerTimestamp)
            {
                // This state arrived out of order and we already have a newer state.
                // TODO: It is theoretically possible to add this state at the proper place in the buffer
                // but I think that would cause erratic behaviour
                Debug.LogWarning("Received state out of order for: " + realObjectToSync.name);
                return;
            }

            lastTimeStateWasReceived = Time.realtimeSinceStartup;

            // Shift the buffer, deleting the oldest state
            for (int i = stateBuffer.Length - 1; i >= 1; i--)
            {
                stateBuffer[i] = stateBuffer[i - 1];
            }

            // Add the new state at the front of the buffer
            stateBuffer[0] = state;

            // Keep track of how many states are in the buffer
            stateCount = Mathf.Min(stateCount + 1, stateBuffer.Length);
        }

        /// <summary>Stop update the state of non-owners so that the object can be teleported.</summary>
        public void stopLerping()
        {
            dontLerp = true;
        }

        /// <summary>Resuming updating the state of non-owners after teleport.</summary>
        public void restartLerping()
        {
            if (!dontLerp) return;

            skipLerp = true;
        }

        public void clearBuffer()
        {
            stateCount = 0;
        }

        public void teleport(int networkTimestamp, Vector3 pos, Quaternion rot)
        {
            lastTeleportOwnerTime = networkTimestamp;
            setPosition(pos, true);
            setRotation(rot, true);
            clearBuffer();
            stopLerping();
        }

        #endregion Public interface

        #region Networking

        /// <summary>Register network message handlers on server</summary>
        public override void OnStartServer()
        {
            if (GetComponent<NetworkIdentity>().localPlayerAuthority)
            {
                if (!NetworkServer.handlers.ContainsKey(MsgType.SmoothSyncFromOwnerToServer))
                {
                    NetworkServer.RegisterHandler(MsgType.SmoothSyncFromOwnerToServer, HandleSyncFromOwnerToServer);
                }

                if (NetworkManager.singleton.client != null)
                {
                    if (!NetworkManager.singleton.client.handlers.ContainsKey(MsgType.SmoothSyncFromServerToNonOwners))
                    {
                        NetworkManager.singleton.client.RegisterHandler(MsgType.SmoothSyncFromServerToNonOwners, HandleSyncFromServerToNonOwners);
                    }
                }
            }
        }

        /// <summary>Register network message handlers on clients</summary>
        public override void OnStartClient()
        {
            if (!NetworkServer.active)
            {
                if (!NetworkManager.singleton.client.handlers.ContainsKey(MsgType.SmoothSyncFromServerToNonOwners))
                {
                    NetworkManager.singleton.client.RegisterHandler(MsgType.SmoothSyncFromServerToNonOwners, HandleSyncFromServerToNonOwners);
                }
            }
        }

        /// <summary>Send the owner's state over the network every 1 / sendRate seconds</summary>
        void Update()
        {
            // We only want to send from owners who are ready
            if (!hasAuthority || (!NetworkServer.active && !ClientScene.ready)) return;

            // If hasn't been long enough since the last send return and don't send out.
            if (Time.realtimeSinceStartup - lastTimeStateWasSent < GetNetworkSendInterval()) return;

            if (!shouldSendPosition() && !shouldSendRotation() && !shouldSendVelocity() && !shouldSendAngularVelocity()) return;

            lastTimeStateWasSent = Time.realtimeSinceStartup;

            // Get the current state of the object and send it out
            NetworkState state = new NetworkState(this);

            if (NetworkServer.active)
            {
                // If owner is the host then send the state to everyone else
                SendStateToNonOwners(state);

                // If sending certain things, set latest position, rotation, velocity, angular velocity accordingly
                // Do it here instead of serialize for the server since it's going to be sending it out to each client
                bool sendPosition = shouldSendPosition();
                if (sendPosition) lastPositionWhenStateWasSent = getPosition();
                bool sendVelocity = shouldSendVelocity();
                if (hasRigdibody)
                {
                    if (sendVelocity) lastVelocityWhenStateWasSent = rb.velocity;
                }
                else if (hasRigidbody2D)
                {
                    if (sendVelocity) lastVelocityWhenStateWasSent = rb2D.velocity;
                }
                bool sendAngularVelocity = shouldSendAngularVelocity();
                if (hasRigdibody)
                {
                    if (sendAngularVelocity) lastAngularVelocityWhenStateWasSent = rb.angularVelocity;
                }
                else if (hasRigidbody2D)
                {
                    if (sendAngularVelocity) lastAngularVelocityWhenStateWasSent = new Vector3(0,0, rb2D.angularVelocity);
                }
                bool sendRotation = shouldSendRotation();
                if (sendRotation) lastRotationWhenStateWasSent = getRotation();
            }
            else
            {
                // If owner is not the host then send the state to the host so they can send it to everyone else
                NetworkManager.singleton.client.connection.SendByChannel(MsgType.SmoothSyncFromOwnerToServer, state, networkChannel);
            }
        }

        /// <summary>
        /// Check if position has changed enough
        /// </summary>
        /// <remarks>
        /// Returns true if distance between position and last sent position is greater than the movement threshold.
        /// </remarks>
        public bool shouldSendPosition()
        {
            if (Vector3.Distance(lastPositionWhenStateWasSent, getPosition()) >
                sendMovementThreshold)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// Check if velocity has changed enough
        /// </summary>
        /// <remarks>
        /// Returns true if difference between velocity and last sent velocity is greater than the velocity threshold.
        /// </remarks>
        public bool shouldSendVelocity()
        {
            if (hasRigdibody)
            {
                if (Vector3.Distance(lastVelocityWhenStateWasSent, rb.velocity) >
                    sendVelocityThreshold)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (hasRigidbody2D)
            {
                if (Vector2.Distance(lastVelocityWhenStateWasSent, rb2D.velocity) >
                    sendVelocityThreshold)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// Check if rotation has changed enough
        /// </summary>
        /// <remarks>
        /// Returns true if angle between rotation and last sent rotation is greater than the rotation threshold.
        /// </remarks>
        public bool shouldSendRotation()
        {
            float rotationDistance = Quaternion.Angle(lastRotationWhenStateWasSent, getRotation());
            if (rotationDistance > sendRotationThreshold)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// Check if angular velocity has changed enough
        /// </summary>
        /// <remarks>
        /// Returns true if difference between angular velocity and last sent velocity is greater than the velocity threshold.
        /// </remarks>
        public bool shouldSendAngularVelocity()
        {
            if (hasRigdibody)
            {
                if (Vector3.Distance(lastAngularVelocityWhenStateWasSent, rb.angularVelocity) >
                    sendAngularVelocityThreshold)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (hasRigidbody2D)
            {
                if (Mathf.Abs(lastAngularVelocityWhenStateWasSent.z - rb2D.angularVelocity) >
                    sendAngularVelocityThreshold)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        #region Sync Properties
        /// <summary>
        /// Determine if should be syncing.
        /// </summary>
        public bool isSyncingXPosition
        {
            get
            {
                return syncPosition == SyncMode.XYZ ||
                     syncPosition == SyncMode.XY ||
                     syncPosition == SyncMode.XZ ||
                     syncPosition == SyncMode.X;
            }
        }
        /// <summary>
        /// Determine if should be syncing.
        /// </summary>
        public bool isSyncingYPosition
        {
            get
            {
                return syncPosition == SyncMode.XYZ ||
                     syncPosition == SyncMode.XY ||
                     syncPosition == SyncMode.YZ ||
                     syncPosition == SyncMode.Y;
            }
        }
        /// <summary>
        /// Determine if should be syncing.
        /// </summary>
        public bool isSyncingZPosition
        {
            get
            {
                return syncPosition == SyncMode.XYZ ||
                     syncPosition == SyncMode.XZ ||
                     syncPosition == SyncMode.YZ ||
                     syncPosition == SyncMode.Z;
            }
        }
        /// <summary>
        /// Determine if should be syncing.
        /// </summary>
        public bool isSyncingXRotation
        {
            get
            {
                return syncRotation == SyncMode.XYZ ||
                     syncRotation == SyncMode.XY ||
                     syncRotation == SyncMode.XZ ||
                     syncRotation == SyncMode.X;
            }
        }
        /// <summary>
        /// Determine if should be syncing.
        /// </summary>
        public bool isSyncingYRotation
        {
            get
            {
                return syncRotation == SyncMode.XYZ ||
                     syncRotation == SyncMode.XY ||
                     syncRotation == SyncMode.YZ ||
                     syncRotation == SyncMode.Y;
            }
        }
        /// <summary>
        /// Determine if should be syncing.
        /// </summary>
        public bool isSyncingZRotation
        {
            get
            {
                return syncRotation == SyncMode.XYZ ||
                     syncRotation == SyncMode.XZ ||
                     syncRotation == SyncMode.YZ ||
                     syncRotation == SyncMode.Z;
            }
        }
        /// <summary>
        /// Determine if should be syncing.
        /// </summary>
        public bool isSyncingXVelocity
        {
            get
            {
                return syncVelocity == SyncMode.XYZ ||
                     syncVelocity == SyncMode.XY ||
                     syncVelocity == SyncMode.XZ ||
                     syncVelocity == SyncMode.X;
            }
        }
        /// <summary>
        /// Determine if should be syncing.
        /// </summary>
        public bool isSyncingYVelocity
        {
            get
            {
                return syncVelocity == SyncMode.XYZ ||
                     syncVelocity == SyncMode.XY ||
                     syncVelocity == SyncMode.YZ ||
                     syncVelocity == SyncMode.Y;
            }
        }
        /// <summary>
        /// Determine if should be syncing.
        /// </summary>
        public bool isSyncingZVelocity
        {
            get
            {
                return syncVelocity == SyncMode.XYZ ||
                     syncVelocity == SyncMode.XZ ||
                     syncVelocity == SyncMode.YZ ||
                     syncVelocity == SyncMode.Z;
            }
        }
        /// <summary>
        /// Determine if should be syncing.
        /// </summary>
        public bool isSyncingXAngularVelocity
        {
            get
            {
                return syncAngularVelocity == SyncMode.XYZ ||
                     syncAngularVelocity == SyncMode.XY ||
                     syncAngularVelocity == SyncMode.XZ ||
                     syncAngularVelocity == SyncMode.X;
            }
        }
        /// <summary>
        /// Determine if should be syncing.
        /// </summary>
        public bool isSyncingYAngularVelocity
        {
            get
            {
                return syncAngularVelocity == SyncMode.XYZ ||
                     syncAngularVelocity == SyncMode.XY ||
                     syncAngularVelocity == SyncMode.YZ ||
                     syncAngularVelocity == SyncMode.Y;
            }
        }
        /// <summary>
        /// Determine if should be syncing.
        /// </summary>
        public bool isSyncingZAngularVelocity
        {
            get
            {
                return syncAngularVelocity == SyncMode.XYZ ||
                     syncAngularVelocity == SyncMode.XZ ||
                     syncAngularVelocity == SyncMode.YZ ||
                     syncAngularVelocity == SyncMode.Z;
            }
        }
        #endregion

        /// <summary>Called on the host to send the owner's state to non-owners.</summary>
        /// <remarks>
        /// The host does not send to itself nor does it send an owner's own state back to the owner.
        /// </remarks>
        /// <param name="state">The owner's state at the time the message was sent</param>
        [Server]
        void SendStateToNonOwners(MessageBase state)
        {
            // Skip sending the Command to ourselves and immediately send to all non-owners
            for (int i = 0; i < NetworkServer.connections.Count; i++)
            {
                NetworkConnection conn = NetworkServer.connections[i];
                
                // Skip sending to clientAuthorityOwner since owners don't need their own state back
                // Also skip sending to localClient (hostId == -1) since the state was already recorded 
                if (conn != null && conn != netID.clientAuthorityOwner && conn.hostId != -1 && conn.isReady)
                {
                    if (isObservedByConnection(conn) == false) continue;
                    // Send the message, this calls HandleRigidbodySyncFromServerToNonOwners on the receiving clients
                    conn.SendByChannel(MsgType.SmoothSyncFromServerToNonOwners, state, networkChannel);
                }
            }
        }

        /// <summary>The server checks if it should send based on Network Proximity Checker.</summary>
        /// <remarks>
        /// Checks who it should send update information to. Will send to everyone unless something like a
        /// Network Proximity Checker is limiting it.
        /// </remarks>
        bool isObservedByConnection(NetworkConnection conn)
        {
            for (int i = 0; i < netID.observers.Count; i++)
            {
                if (netID.observers[i] == conn)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>Receive incoming state on non-owners.</summary>
        /// <remarks>
        /// This static method receives incoming state messages for all SmoothSync objects and uses
        /// the netID included in the message to find the target game object.
        /// Calls NonOwnerReceiveState() on the target SmoothSync object.
        /// </remarks>
        static void HandleSyncFromServerToNonOwners(NetworkMessage msg)
        {
            NetworkState networkState = msg.ReadMessage<NetworkState>();

            if (networkState != null && !networkState.smoothSync.hasAuthority)
            {
                networkState.smoothSync.adjustOwnerTime(networkState.state.ownerTimestamp);
                if (networkState.state.ownerTimestamp > networkState.smoothSync.lastTeleportOwnerTime)
                {
                    networkState.smoothSync.restartLerping();
                    networkState.smoothSync.addState(networkState.state);
                }
            }
        }

        /// <summary>Receive owner's state on the host and send it back out to all non-owners</summary>
        /// <remarks>
        /// This static method receives incoming state messages for all SmoothSync objects and uses
        /// the netID included in the message to find the target game object.
        /// Calls addState() and SendStateToNonOwners() on the target SmoothSync object.
        /// </remarks>
        static void HandleSyncFromOwnerToServer(NetworkMessage msg)
        {
            NetworkState networkState = msg.ReadMessage<NetworkState>();

            networkState.smoothSync.adjustOwnerTime(networkState.state.ownerTimestamp);
            networkState.smoothSync.SendStateToNonOwners(networkState);
            if (networkState.state.ownerTimestamp > networkState.smoothSync.lastTeleportOwnerTime)
            {
                networkState.smoothSync.restartLerping();
                networkState.smoothSync.addState(networkState.state);
            }
        }

        public override float GetNetworkSendInterval()
        {
            return 1 / sendRate;
        }

        public override int GetNetworkChannel()
        {
            return networkChannel;
        }

        #region Time stuff

        /// <summary>
        /// The last owner time received over the network
        /// </summary>
        int _ownerTime;

        /// <summary>
        /// The realTimeSinceStartup when we received the last owner time.
        /// </summary>
        float lastTimeOwnerTimeWasSet;

        /// <summary>
        /// The current estimated time on the owner.
        /// </summary>
        /// <remarks>
        /// Time comes from the owner in every sync message
        /// When it is received we set _ownerTime and lastTimeOwnerTimeWasSet
        /// Then when we want to know what time it is we add time elapsed to the last _ownerTime we received
        /// </remarks>
        public int approximateNetworkTimeOnOwner
        {
            get
            {
                return _ownerTime + (int)((Time.realtimeSinceStartup - lastTimeOwnerTimeWasSet) * 1000);
            }
            set
            {
                _ownerTime = value;
                lastTimeOwnerTimeWasSet = Time.realtimeSinceStartup;
            }
        }

        /// <summary>
        /// Adjust owner time based on latest timestamp
        /// </summary>
        void adjustOwnerTime(int ownerTimestamp) // TODO: I'd love to see a graph of owner time
        {
            int newTime = ownerTimestamp;

            int maxTimeChange = 50;
            int timeChangeMagnitude = Mathf.Abs(approximateNetworkTimeOnOwner - newTime);
            if (approximateNetworkTimeOnOwner == 0 || timeChangeMagnitude < maxTimeChange || timeChangeMagnitude > maxTimeChange * 10)
            {
                approximateNetworkTimeOnOwner = newTime;
            }
            else
            {
                if (approximateNetworkTimeOnOwner < newTime)
                {
                    approximateNetworkTimeOnOwner += maxTimeChange;
                }
                else
                {
                    approximateNetworkTimeOnOwner -= maxTimeChange;
                }
            }
        }

        #endregion

        #endregion Networking
    }
}