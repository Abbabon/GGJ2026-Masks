using Fusion;
using UnityEngine;

namespace Network
{
    /// <summary>
    /// Holds a single [Networked] position for shared transform sync (e.g. Human character).
    /// Requires NetworkObject on the same GameObject. Used by Player.cs to push/read position without referencing PlayerController.
    /// </summary>
    public class ReplicatedPosition : NetworkBehaviour
    {
        [Networked] public Vector3 Position { get; set; }

        public override void Spawned()
        {
            if (HasStateAuthority)
                Position = transform.position;
        }
    }
}
