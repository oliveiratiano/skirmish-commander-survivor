GPU-Instanced materials go here.

When creating materials in Unity:
1. Use "Sprites/Default" shader (or a custom instanced sprite shader)
2. Check "Enable GPU Instancing" on each material
3. One shared material per unit type — assigned via UnitData or prefab

For MVP primitives, materials are created at runtime via GameManager.CreatePrimitive().
GPU Instancing will be enabled once final materials are set up.
