# God is a DJ - Game Design Document

## Game Concept
Asymmetric multiplayer game where one GOD player hunts heretic players who are hiding among NPCs.

## Platform Requirements
- **Target Platform:** WebGL (must run in browser)
- **Engine:** Unity
- **Networking:** Must support WebGL builds (Photon Fusion or Mirror + SimpleWebTransport)

## Player Roles

### GOD Player (1 player)
**Objective:** Find and eliminate all heretic players before they destroy the shrines

**Abilities:**
- View the world through a movable iris (circular viewport)
- Control iris target position with mouse - iris smoothly follows/drags behind the mouse cursor
- Action button to kill targets in iris (has cooldown timer)
- Can only see what is within the iris area

**Constraints:**
- Limited to iris vision only
- Action button (kill) has cooldown between uses
- Must identify heretics among identical-looking NPCs
- Loses the game if 3 non-heretics (NPCs) are killed

### Heretic Players (2+ players)
**Objective:** Destroy 2 out of 3 shrines before time runs out

**Abilities:**
- Move around the 2D world
- Run (faster movement)
- Dance action (blend in with NPCs)
- Limited circular vision radius around themselves

**Constraints:**
- Time limit to complete objective
- Running makes them easier to spot by GOD
- Can only see within their vision radius
- Must avoid being killed by GOD

## World & Environment

### The World
- 2D plane visible from top-down perspective
- Contains 3 shrines placed around the map
- Populated with wandering NPCs

### NPCs
- Wander around the world randomly
- Perform dance animation periodically
- Look identical to heretic players
- Serve as camouflage for heretics

### Shrines
- 3 total shrines exist in the world
- Heretics must destroy 2 out of 3 to win
- Destructible by heretic players

## Vision & Information Systems

### GOD Vision
- Circular iris viewport controlled by mouse
- Can only see what is inside the iris circle
- Uses mask layer to limit visible area

### Heretic Vision
- Zoomed-in camera view (does not see the whole world)
- Circular fog of war around each heretic player
- Limited radius from player position
- Uses mask layer to create visibility area
- Circle indicator showing the threat area of GOD's iris direction
- Arrow indicators pointing toward shrine locations
- **"GOD IS WATCHING" warning:** When heretic is inside GOD's iris, screen displays overwhelming visual warning that GOD is watching them

## Game Mechanics

### Movement
- Smooth 2D movement on a plane
- Heretics have two movement speeds:
  - Normal walk (safe, blends with NPCs)
  - Run (faster but risky, easier to spot)

### Actions
- **Action Button (context-sensitive):**
  - **GOD:** Click action button to kill target in iris (cooldown applies)
  - **Heretic:** Hold action button to perform dance animation
  - **Heretic (near shrine):** Hold action button near shrine to begin destruction timer - must continue holding (and dancing) until timer completes to destroy shrine
- **Shrine Destruction Alert:** When a shrine is destroyed, GOD receives "SHRINE DESTROYED!" warning message

### Win Conditions
- **Heretics Win:** 
  - Destroy 2 out of 3 shrines within time limit, OR
  - GOD kills 3 non-heretics (NPCs)
- **GOD Wins:** 
  - Kill all heretic players before they destroy 2 shrines, OR
  - Time runs out with fewer than 2 shrines destroyed

### Time Limit
- Match has a countdown timer
- Displayed to all players
- Heretics must complete objective before timer expires

## Multiplayer Requirements

### Player Count
- 1 GOD player (required)
- 2+ Heretic players (minimum 2 recommended)

### Network Sync Requirements
- GOD iris position (frequent updates)
- All player positions (smooth interpolation)
- Player animation states (idle/walk/run/dance)
- Kill events
- Shrine destruction events
- Timer synchronization
- Cooldown states

## UI Requirements

### GOD UI
- Iris viewport (circular mask)
- Action button (with cooldown indicator)
- Remaining heretics counter
- NPC kills counter (X/3 - game over at 3)
- Timer display
- "SHRINE DESTROYED!" warning message (appears when shrine is destroyed)

### Heretic UI
- Zoomed-in camera view
- Fog of war (circular visibility mask)
- Circle of threat showing GOD's iris direction/area
- Arrow indicators pointing toward shrine locations
- Shrines destroyed counter (X/2)
- Shrine destruction progress bar (when holding action button near shrine)
- Timer display
- Run/dance action indicators
- **"GOD IS WATCHING" warning:** Overwhelming visual alert when inside GOD's iris

### Shared UI
- Player count display
- Game start/end screens
- Win/loss notifications

## Technical Constraints
- Must be completable within game jam timeframe
- WebGL build requirement (browser-playable)
- Multiplayer functionality must work reliably
- Keep scope manageable for rapid development
