# Unity project context

Analyzed 2026-09-22 at commit 45109c3. Project root: current jaeminrun workspace.

- Unity 6000.6.0f1; 2D sprites/physics. URP installed; Input System PlayerInput message callbacks drive RunnerMovement and Attack.
- Startup/build scene: Assets/Scenes/전투1.unity. No networking or separate bootstrap found.
- First-party runtime: Assets/scripts, default Assembly-CSharp. Editor tooling: Assets/scripts/Editor. Test Framework 1.8.0 installed; no existing first-party test assemblies.
- Architecture: small MonoBehaviours, Inspector configuration, coroutines for timing. No namespaces, four-space indentation, Korean comments.
- PLAYER root owns movement/physics/attacks. Player child owns BouncyRun2D. Minion bounce must be on a child, since it writes local position.
- EntityHealth owns enemy health. Projectile applies popcorn damage; BossMinion receives one damage per popcorn and lethal slam damage. RunnerMovement owns slam state and landing radius.
- BossPatern1 owns pattern sequence and cleanup; FloatingObject pauses its positional animation only during the swing pattern while its component remains enabled. The original empty BossPatern class had no scene usage; filename and class are now aligned without changing meta GUID.

## Player damage

- Physics 2D ignores collisions between the PLAYER layer (3) and enemy layer (9). This project setting is intentional and remains unchanged.
- BossProjectile, BossMinion and the boss swing use direct Collider2D shape-distance overlap checks, which do not depend on the layer collision matrix.
- PLAYER has EntityHealth with 5 max health. Enemy contact attacks deal 1 damage by default. EntityHealth flashes its child SpriteRenderer red for the configured hit-effect duration.
- Unity Pipeline MCP connected to this project. Status, scene and console work. Dynamic eval intermittently fails with Roslyn encoding errors even for ASCII input; compiled editor menus are the fallback.
- Baseline console: Unity AI relay process-exit error unrelated to gameplay. Active scene had unsaved edits: preserve via live scene backup before setup.
- Sources: manifest.json, ProjectVersion.txt, EditorBuildSettings.asset, GraphicsSettings.asset, Physics2DSettings.asset, battle hierarchy, Entity and Player scripts.
- Release target remains unspecified. Player health uses EntityHealth; battle results are described below.

## Validation status (2026-09-11)

- Unity compilation completed without compiler errors after runtime, editor and grounding changes.
- Confirmed serialized scene references to player, camera, cookie prefab and minion prefab.
- Confirmed minion prefab maxHealth=5, trigger collider, speed=2, ground layer=64; player impact layer includes enemy layer.
- Confirmed .meta files for all created Unity assets.
- Game-view captures were inspected, but the automated Play Mode run did not produce its completion report. Play Mode repeatedly returned to stopped state; runtime assertions must not be reported as passed.
- Pre-existing Unity AI subscription/relay logs are unrelated to this feature.

## Battle results (2026-09-13)

- BattleEndController listens to EntityHealth.Died/Damaged, freezes combat, fades out using unscaled time and loads the shared 전투 끝 scene. Both battle and result scenes are enabled in Build Settings.
- ScoreManager is a session-only DontDestroyOnLoad singleton holding an immutable BattleResult. GameOverView displays the result and retries the original battle scene. New battles reset the previous result.
- Result UI is authored in the scene with uGUI and the existing SeoulNamsanB font. Configuration, score rules and other-boss reuse: `Docs/AI/BattleResults.md`.
- Unity compilation and automated Play Mode victory/retry/defeat checks passed, including overkill, duplicate completion, singleton persistence and time restoration. Result screenshot visually inspected. No Player build performed.

## Battle 2 escape (2026-09-13)

- Battle 2 uses the existing long static terrain, camera/confiner, ship and character art. EscapeStageController, HutCover and BossSlamHunter implement artwork-aligned concealment behind houses, a periodic rumble/rise/reveal with a local boss scan and lethal exposure, a fast fist accent only when exposed, and ship boarding/takeoff.
- BattleEndController supports opt-in objective victory; battle 1's boss-death victory remains the default. Battle 2 is enabled in Build Settings for retry.
- Setup, tuning, backup and validation instructions: `Docs/AI/EscapeStage.md`.

## Battle 3 survival (2026-09-22)

- `Battle3GameController` owns the flight-survival loop, projectile spawning, failure result, and objective-clear sequence.
- Surviving for 45 seconds stops hazards, moves the Earth in from the right, flies the player toward it, and records an objective victory before loading the shared result scene.
- `Battle3FlightController` owns input-driven vertical flight. Battle 3 remains a small MonoBehaviour/Inspector-configured scene in the default Assembly-CSharp assembly.
