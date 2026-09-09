# AI Agents Guidelines

Welcome to this project. All AI agents, coding assistants, and automated tools working on this repository must strictly adhere to the project's specific conventions and instructions.

### 1. Unity Project Scope

- Treat this repository as a Unity project first.
- Follow Unity-first workflows for gameplay, scenes, prefabs, assets, and C# scripts.
- Prefer Unity MCP tooling for validation, inspection, and editor-safe automation when useful.
- Never use dot net commands to validate. Use Unity MCP instead.
- Do not hand-author or manually edit `.meta` files.
- Unity-generated `.meta` files are expected and valid when Unity creates them (for example, after adding new scripts/assets).

### 2. Input System Standard (Mandatory)

- Always use the `Input System Package (New)`.
- Do not introduce or rely on the legacy Input Manager (`UnityEngine.Input` old system) for new work.
- If touching input-related code, align it with the New Input System patterns already used in the project.

### 3. Self-Improvement Loop

- After ANY correction from the user: update `tasks/lessons.md` with the pattern
- Write rules for yourself that prevent the same mistake
- Ruthlessly iterate on these lessons until mistake rate drops
- Review lessons at session start for relevant project

### 4. Verification Before Done

- Never mark a task complete without proving it works
- Diff behavior between main and your changes when relevant
- Ask yourself: "Would a staff engineer approve this?"
- Run tests, check logs, demonstrate correctness
- Use unity mcp if needed to verify changes

### 5. Demand Elegance (Balanced)

- For non-trivial changes: pause and ask "is there a more elegant way?"
- If a fix feels hacky: "Knowing everything I know now, implement the elegant solution"
- Skip this for simple, obvious fixes - don't over-engineer
- Challenge your own work before presenting it

### 6. Autonomous Bug Fixing

- When given a bug report: just fix it. Don't ask for hand-holding
- Point at logs, errors, failing tests - then resolve them
- Zero context switching required from the user
- Go fix failing CI tests without being told how

### 7. Subagent Strategy

- Spawn subagents liberally to keep main context window clean
- Offload research, exploration, and parallel analysis to subagents
- For complex problems, throw more compute at it via subagents
- One task per subagent for focused execution

### 8. Project-Specific Architecture (BeastClad)

- **Component-Based & Data-Driven**: Strictly use Unity's ScriptableObjects for all monster data, stats, and elemental types. Avoid hardcoding values.
- **The "Infuse" Mechanic**: Monsters in this game DO NOT fight independently on the field. They transform into the player's armor/weapons (e.g., Head=HP, Arms=Attack). All combat code must reflect this modular equipment/infuse system.
- **Non-Linear 2D Sandbox**: The game is a top-down 2D RPG supporting multiple player paths (Arena Fighter, Adventurer, Illegal Black Market). Code should be highly modular so new systems can be integrated step-by-step without breaking the core loop.
