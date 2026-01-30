# NPC Prefab Setup (Human SENDs / God GETs)

NPCs use the same pattern as the Human player: **Human** has state authority and **SENDs** positions; **God** **GETs** positions from the network. No PlayerController — uses **ReplicatedPosition** only.

## What to add to the NPC prefab

1. **Network Object** (Photon Fusion)  
   - Add Component → Photon Fusion → Network Object.

2. **Replicated Position** (script)  
   - Add Component → search **Replicated Position** (`Assets/Scripts/Network/ReplicatedPosition.cs`).  
   - Holds the `[Networked]` position so Human can write and God can read.

3. **Npc** (script)  
   - Your existing Npc script; it now contains the send/get logic.

4. **Mover** (optional)  
   - For movement; Npc drives it when this client has authority.

5. **Rigidbody 2D** (optional)  
   - If you use Mover; Npc uses it for position sync when present.

6. **Box Collider 2D** (optional)  
   - For collisions.

## Spawning so the Human has authority

NPCs must be **spawned by the Human client** so the Human has state authority and can SEND positions. If you use **scene** NPCs with Network Object, whoever loads first may have authority (so God might send instead of Human).

**Recommended:** spawn NPCs when the game starts:

1. Add the **NPC prefab** to Fusion’s **Network Project Config** (same as the player prefab).
2. On **GameLauncher** (in the scene or prefab):
   - **Npc Prefab**: assign your NPC prefab (with Network Object + Replicated Position + Npc).
   - **Npc Spawn Count**: e.g. `3` (number of NPCs to spawn).
   - **Npc Spawn Offset**: e.g. `(2, 0)` so each NPC is offset from the previous.
3. When the Human clicks Start, the Human spawns the player and then spawns `Npc Spawn Count` NPCs; the Human has state authority over them and SENDs their positions. The God client GETs and applies those positions.

If you use **scene** NPCs (no spawning), ensure the **Human** is the one with state authority over them (e.g. only the Human client places or “claims” them), or they will not follow the Human-send / God-get behaviour.
