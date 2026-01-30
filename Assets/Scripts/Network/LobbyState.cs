using Fusion;
using UnityEngine;

namespace Network
{
    /// <summary>
    /// Shared lobby state for character selection. One instance per session.
    /// In Shared Mode, first peer to need it spawns it and has State Authority; others set choices via RPCs.
    /// Requires NetworkObject on the same GameObject.
    /// </summary>
    public class LobbyState : NetworkBehaviour
    {
        [Networked] public PlayerRef GodPlayer { get; set; }
        [Networked] public PlayerRef HumanPlayer { get; set; }
        [Networked] public NetworkBool GameStarted { get; set; }

        public bool BothSelected => !GodPlayer.IsNone && !HumanPlayer.IsNone;

        /// <summary>One role per player: clears this player from the other slot, then assigns the chosen slot.</summary>
        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void RPC_SelectRole(PlayerRef player, bool wantGod)
        {
            if (wantGod)
            {
                if (HumanPlayer == player) HumanPlayer = default;
                GodPlayer = player;
            }
            else
            {
                if (GodPlayer == player) GodPlayer = default;
                HumanPlayer = player;
            }
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void RPC_SetGod(PlayerRef player)
        {
            if (GodPlayer.IsNone)
                GodPlayer = player;
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void RPC_SetHuman(PlayerRef player)
        {
            if (HumanPlayer.IsNone)
                HumanPlayer = player;
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void RPC_StartGame()
        {
            if (BothSelected)
                GameStarted = true;
        }
    }
}
