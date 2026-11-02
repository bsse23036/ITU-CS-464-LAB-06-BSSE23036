# CS464 Lab 06

**Name:** Mustafa Fawwaz | **Roll Number:** BSSE23036

## System Architecture & Implementation

The architecture separates responsibilities across dedicated singletons, component systems, and execution loops to maintain high performance and clean modularity:

- **Input Reading & Game Feel (`Update`):** Reads WASD and mouse input, tracks Coyote Time and Jump Buffering windows, and updates level countdown timers.
- **Physics & Velocity (`FixedUpdate`):** Calculates camera-relative directional vectors and applies velocity directly to the `Rigidbody`. Custom fall and low-jump gravity multipliers generate crisp, snappy jump curves.
- **Camera Tracking (`LateUpdate`):** Resolves horizontal and vertical mouse orbit offsets around the player target, keeping camera movement smooth and jitter-free.
- **Stealth AI & Line-of-Sight (`EnemyPatrol.cs`):** Combines distance checking, forward FOV angle cone checks, and physical Raycasting against an `Obstacle Mask` layer to trigger active player chases.
- **Health & Damage Processing (`Health.cs` / `Hazard.cs`):** Manages player health, post-hit invulnerability frames, and hazard trigger/collision damage.
- **State & UI Persistence (`GameManager.cs` / `UIManager.cs`):** Utilizes a persistent singleton pattern to sync coin counts, remaining level time, and heart displays across scene loads while automatically resetting per-level progress.

## Tuning Table

Below are the exact parameters configured across the system C# scripts to achieve balanced, responsive gameplay:

| Parameter                                    | Selected Value | Design Justification                                                                                  |
| :------------------------------------------- | :------------- | :---------------------------------------------------------------------------------------------------- |
| **Movement Speed (`moveSpeed`)**             | `8.0`          | Ensures swift horizontal traversal across level geometries without sacrificing platforming precision. |
| **Jump Force (`jumpForce`)**                 | `7.0`          | Calibrated to reliably clear standard hazard obstacles and elevated greybox ledges.                   |
| **Coyote Time (`coyoteTime`)**               | `0.15`         | Grants a $150\text{ms}$ grace period to execute jumps after running off platform edges.               |
| **Jump Buffer (`jumpBufferTime`)**           | `0.15`         | Stores jump inputs made $150\text{ms}$ before landing, ensuring seamless jump chaining.               |
| **Fall Multiplier (`fallMultiplier`)**       | `2.5`          | Increases downward gravity while falling to eliminate floatiness and provide a firm landing feel.     |
| **Detection Radius (`detectionRadius`)**     | `10.0`         | Defines the maximum visual alert distance for patrolling guard entities.                              |
| **Field of View Angle (`fieldOfViewAngle`)** | `90.0`         | Sets a clear $90^\circ$ forward vision cone for stealth evasion behind cover obstacles.               |
| **Invulnerability Window (`iFrames`)**       | `1.0`          | Prevents rapid multi-hit damage stacks upon touching hazards or guard collision bounds.               |

## Video Showcase

**Video ink:** https://drive.google.com/file/d/1mwIhpGv4z7-mspFuZmxbuOs2j05wWgkL/view?usp=sharing
