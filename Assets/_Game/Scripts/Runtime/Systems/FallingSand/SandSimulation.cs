using System;

public class SandSimulation
{
    private readonly SandGrid grid;
    private readonly Random random;
    private static readonly int[] BurstDirectionX = { 0, -1, 1, -1, 1, -1, 1 };
    private static readonly int[] BurstDirectionY = { 1, 1, 1, 0, 0, -1, -1 };
    public SandSimulation(SandGrid grid, int randomSeed = 0)
    {
        if (grid == null) throw new ArgumentNullException(nameof(grid));
        this.grid = grid;
        random = new Random(randomSeed);
    }

    public int ApplyImpactBurst(float edgeBurstChance, float interiorCrackChance, int maxExtraDistance)
    {
        bool[] occupiedAtImpact = new bool[grid.Width * grid.Height];
        for (int y = 0; y < grid.Height; y++)
        {
            for (int x = 0; x < grid.Width; x++)
            {
                if (grid.TryGetMaterial(x, y, out byte materialId) == false) continue;
                if (materialId == 0) continue;
                int index = x + y * grid.Width;
                occupiedAtImpact[index] = true;
            }
        }
        int interiorMovedCount = MoveBurstGroup(occupiedAtImpact, false, interiorCrackChance, maxExtraDistance);
        int edgeMovedCount = MoveBurstGroup(occupiedAtImpact, true, edgeBurstChance, maxExtraDistance);

        return interiorMovedCount + edgeMovedCount;
    }

    private int MoveBurstGroup(bool[] occupiedAtImpact, bool moveEdgeParticles, float burstChance, int maxExtraDistance)
    {
        int result = 0;
        for (int i = 0; i < occupiedAtImpact.Length; i++)
        {
            if (occupiedAtImpact[i] == false) continue;
            int x = i % grid.Width;
            int y = i / grid.Width;
            bool isEdgeParticle = HasEmptyBurstNeighbor(x, y, occupiedAtImpact);
            if (isEdgeParticle != moveEdgeParticles) continue;
            if (random.NextDouble() >= burstChance) continue;
            int dirX = 0;
            int dirY = 0;
            if (isEdgeParticle) GetExposedBurstDirection(x, y, occupiedAtImpact, out dirX, out dirY);
            else GetRandomBurstDirection(out dirX, out dirY);
            if (TryFindBurstTarget(x, y, dirX, dirY, maxExtraDistance, out int toX, out int toY) && TryMove(x, y, toX, toY))
            {
                result++;
            }
        }
        return result;
    }

    private bool TryFindBurstTarget(int fromX, int fromY, int dirX, int dirY, int maxExtraDistance, out int toX, out int toY)
    {
        toX = 0; toY = 0;
        int currentX = fromX + dirX;
        int currentY = fromY + dirY;

        while (grid.TryGetMaterial(currentX, currentY, out var materialId) && materialId != 0)
        {
            currentX = currentX + dirX;
            currentY = currentY + dirY;
        }
        if (grid.IsInside(currentX, currentY) == false) return false;

        int extraDistance = random.Next(0, maxExtraDistance + 1);

        for (int i = 0; i < extraDistance; i++)
        {
            int nextX = currentX + dirX;
            int nextY = currentY + dirY;
            if (grid.TryGetMaterial(nextX, nextY, out var materiaId) == false || materiaId != 0) break;
            currentX = nextX; currentY = nextY;
        }

        toX = currentX;
        toY = currentY;
        return true;
    }

    private void GetExposedBurstDirection(int x, int y, bool[] occupiedAtImpact, out int directionX, out int directionY)
    {
        directionX = 0;
        directionY = 0;
        int startIndex = random.Next(0, BurstDirectionX.Length);
        for (int offset = 0; offset < BurstDirectionX.Length; offset++)
        {
            int index = (startIndex + offset) % BurstDirectionX.Length;
            int neighborX = x + BurstDirectionX[index];
            int neighborY = y + BurstDirectionY[index];
            if (grid.IsInside(neighborX, neighborY) == false) continue;
            int neighborIndex = neighborX + neighborY * grid.Width;
            if (occupiedAtImpact[neighborIndex] == false)
            {
                directionX = BurstDirectionX[index];
                directionY = BurstDirectionY[index];
                return;
            }
        }
    }

    private void GetRandomBurstDirection(out int directionX, out int directionY)
    {
        int index = random.Next(0, BurstDirectionX.Length);
        directionX = BurstDirectionX[index];
        directionY = BurstDirectionY[index];
    }

    private bool HasEmptyBurstNeighbor(int x, int y, bool[] occupiedAtImpact)
    {
        for (int i = 0; i < BurstDirectionX.Length; i++)
        {
            int neighborX = x + BurstDirectionX[i];
            int neighborY = y + BurstDirectionY[i];
            if (grid.IsInside(neighborX, neighborY) == false) continue;
            int index = neighborX + neighborY * grid.Width;
            if (occupiedAtImpact[index] == false) return true;
        }
        return false;
    }

    public bool Step()
    {
        bool movedAnyParticle = false;

        for (int y = 1; y < grid.Height; y++)
        {
            for (int x = 0; x < grid.Width; x++)
            {
                if (grid.TryGetMaterial(x, y, out byte materialId) == false || materialId == 0) continue;

                bool succeeded = TryMove(x, y, x, y - 1);
                if (succeeded == false)
                {
                    int firstDirection = random.Next(0, 2) == 0 ? -1 : 1;
                    int secondDirection = -firstDirection;
                    succeeded = TryMove(x, y, x + firstDirection, y - 1);
                    if (succeeded == false) succeeded = TryMove(x, y, x + secondDirection, y - 1);
                }

                if (succeeded)
                    movedAnyParticle = true;
            }
        }
        return movedAnyParticle;
    }

    private bool TryMove(int fromX, int fromY, int toX, int toY)
    {
        if (grid.TryGetMaterial(fromX, fromY, out byte materialId) == false) return false;
        if (materialId == 0) return false;
        if (grid.TryGetMaterial(toX, toY, out byte material) == false) return false;
        if (material != 0) return false;
        if (grid.TrySetMaterial(toX, toY, materialId) == false) return false;
        if (grid.TrySetMaterial(fromX, fromY, 0) == false) return false;
        return true;
    }
}
