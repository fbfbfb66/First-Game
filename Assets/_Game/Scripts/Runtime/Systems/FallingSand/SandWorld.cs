using UnityEngine;

public class SandWorld : MonoBehaviour
{
    [SerializeField] private SandGridRenderer gridRenderer;
    [SerializeField] private int width = 64;
    [SerializeField] private int height = 96;
    [SerializeField] private float cellSize = 0.1f;
    [SerializeField, Min(1f)] private float simulationStepsPerSecond = 15f;
    [SerializeField] private int simulationSeed = 12345;
    [SerializeField, Range(0f, 1f)] private float edgeBurstChance = 0.43f;
    [SerializeField, Range(0f, 1f)] private float interiorCrackChance = 0.095f;
    [SerializeField, Min(0)] private int impactBurstExtraDistance = 2;
    private float simulationTimeAccumulator;
    private bool impactDetected;

    private SandGrid grid;
    private SandSimulation simulation;

    private void Awake()
    {
        grid = new SandGrid(width, height);
        simulation = new SandSimulation(grid, simulationSeed);
        gridRenderer.Initialize(grid, cellSize);

        for (int y = 160; y < 176; y++)
        {
            for (int x = 56; x < 72; x++)
            {
                byte materialId = (byte)Random.Range(1, 5);
                if (grid.TrySetMaterial(x, y, materialId) == false) continue;
            }
        }
        gridRenderer.Render();
    }

    private void Update()
    {
        simulationTimeAccumulator += Time.deltaTime;
        float duration = 1f / simulationStepsPerSecond;
        bool gridChanged = false;
        if (simulationTimeAccumulator >= duration)
        {
            gridChanged = simulation.Step();
            if (impactDetected == false && DoesBottomRowContainSand())
            {
                impactDetected = true;
                int movedParticleCount = simulation.ApplyImpactBurst(edgeBurstChance, interiorCrackChance, impactBurstExtraDistance);
                if (movedParticleCount > 0) gridChanged = true;
            }
            simulationTimeAccumulator -= duration;
        }
        if (gridChanged) gridRenderer.Render();
    }

    private bool DoesBottomRowContainSand()
    {
        for (int x = 0; x < width; x++)
        {
            if (grid.TryGetMaterial(x, 0, out var materialId) == false) continue;
            if (materialId != 0) return true;
        }
        return false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        float worldWidth = width * cellSize;
        float worldHeight = height * cellSize;
        float centerX = transform.position.x + worldWidth / 2f;
        float centerY = transform.position.y + worldHeight / 2f;
        Gizmos.DrawWireCube(new Vector3(centerX, centerY, 0f), new Vector3(worldWidth, worldHeight, 0f));
    }
}
