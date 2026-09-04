using System;
using NUnit.Framework;

/// <summary>
/// SandSimulation 的 EditMode 测试，覆盖垂直重力、随机斜滑与边界行为。
/// </summary>
public class SandSimulationTests
{
    private SandGrid grid;
    private SandSimulation simulation;

    [SetUp]
    public void SetUp()
    {
        grid = new SandGrid(4, 4);
        simulation = new SandSimulation(grid);
    }

    [Test]
    public void Constructor_WithNullGrid_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new SandSimulation(null));
    }

    [Test]
    public void Step_OnEmptyGrid_ReturnsFalse()
    {
        Assert.IsFalse(simulation.Step());
    }

    [Test]
    public void Step_WithEmptyCellBelow_MovesParticleDownOneCell()
    {
        const byte RedSand = 2;
        grid.TrySetMaterial(2, 2, RedSand);

        Assert.IsTrue(simulation.Step());

        grid.TryGetMaterial(2, 2, out byte oldCell);
        grid.TryGetMaterial(2, 1, out byte newCell);
        Assert.AreEqual(0, oldCell);
        Assert.AreEqual(RedSand, newCell);
    }

    [Test]
    public void Step_WithParticleAboveFloor_MovesOnlyOneCellPerStep()
    {
        grid.TrySetMaterial(2, 3, 1);

        simulation.Step();

        grid.TryGetMaterial(2, 2, out byte oneCellBelow);
        grid.TryGetMaterial(2, 1, out byte twoCellsBelow);
        Assert.AreEqual(1, oneCellBelow);
        Assert.AreEqual(0, twoCellsBelow);
    }

    [Test]
    public void Step_WithParticleOnBottomRow_ReturnsFalseAndLeavesItInPlace()
    {
        grid.TrySetMaterial(2, 0, 1);

        Assert.IsFalse(simulation.Step());

        grid.TryGetMaterial(2, 0, out byte materialId);
        Assert.AreEqual(1, materialId);
    }

    [Test]
    public void Step_WithTwoStackedParticles_MovesEachParticleDownOnce()
    {
        grid.TrySetMaterial(1, 1, 1);
        grid.TrySetMaterial(1, 2, 2);

        Assert.IsTrue(simulation.Step());

        grid.TryGetMaterial(1, 0, out byte bottomMaterialId);
        grid.TryGetMaterial(1, 1, out byte topMaterialId);
        grid.TryGetMaterial(1, 2, out byte oldTopCell);
        Assert.AreEqual(1, bottomMaterialId);
        Assert.AreEqual(2, topMaterialId);
        Assert.AreEqual(0, oldTopCell);
    }

    [Test]
    public void Step_RepeatedUntilFloor_EventuallyReturnsFalse()
    {
        grid.TrySetMaterial(0, 2, 1);

        Assert.IsTrue(simulation.Step());
        Assert.IsTrue(simulation.Step());
        Assert.IsFalse(simulation.Step());

        grid.TryGetMaterial(0, 0, out byte materialId);
        Assert.AreEqual(1, materialId);
    }

    [Test]
    public void Step_WhenDownAndDiagonalsAreEmpty_PrefersStraightDown()
    {
        grid.TrySetMaterial(2, 2, 2);

        Assert.IsTrue(simulation.Step());

        grid.TryGetMaterial(2, 1, out byte downMaterialId);
        grid.TryGetMaterial(1, 1, out byte downLeftMaterialId);
        grid.TryGetMaterial(3, 1, out byte downRightMaterialId);
        Assert.AreEqual(2, downMaterialId);
        Assert.AreEqual(0, downLeftMaterialId);
        Assert.AreEqual(0, downRightMaterialId);
    }

    [Test]
    public void Step_WhenDownIsBlocked_MovesToExactlyOneOpenDiagonal()
    {
        grid.TrySetMaterial(2, 1, 2);
        grid.TrySetMaterial(2, 0, 1);

        Assert.IsTrue(simulation.Step());

        grid.TryGetMaterial(2, 1, out byte oldCell);
        grid.TryGetMaterial(1, 0, out byte downLeftMaterialId);
        grid.TryGetMaterial(3, 0, out byte downRightMaterialId);
        Assert.AreEqual(0, oldCell);
        Assert.AreEqual(1, (downLeftMaterialId == 2 ? 1 : 0) + (downRightMaterialId == 2 ? 1 : 0),
            "左右都开放时，颗粒必须随机选择且只占据一个斜向目标");
    }

    [Test]
    public void Step_WhenDownAndDownLeftAreBlocked_MovesDownRight()
    {
        grid.TrySetMaterial(2, 1, 3);
        grid.TrySetMaterial(2, 0, 1);
        grid.TrySetMaterial(1, 0, 1);

        Assert.IsTrue(simulation.Step());

        grid.TryGetMaterial(2, 1, out byte oldCell);
        grid.TryGetMaterial(3, 0, out byte downRightMaterialId);
        Assert.AreEqual(0, oldCell);
        Assert.AreEqual(3, downRightMaterialId);
    }

    [Test]
    public void Step_WhenAllThreeTargetsAreBlocked_ReturnsFalseAndDoesNotMove()
    {
        grid.TrySetMaterial(2, 1, 2);
        grid.TrySetMaterial(1, 0, 1);
        grid.TrySetMaterial(2, 0, 1);
        grid.TrySetMaterial(3, 0, 1);

        Assert.IsFalse(simulation.Step());

        grid.TryGetMaterial(2, 1, out byte materialId);
        Assert.AreEqual(2, materialId);
    }

    [Test]
    public void Step_AtLeftEdge_TreatsDownLeftOutsideGridAsBlockedAndMovesDownRight()
    {
        grid.TrySetMaterial(0, 1, 2);
        grid.TrySetMaterial(0, 0, 1);

        Assert.IsTrue(simulation.Step());

        grid.TryGetMaterial(1, 0, out byte downRightMaterialId);
        Assert.AreEqual(2, downRightMaterialId);
    }

    [Test]
    public void Step_AtRightEdge_WithOtherTargetsBlocked_DoesNotLeaveGrid()
    {
        grid.TrySetMaterial(3, 1, 2);
        grid.TrySetMaterial(2, 0, 1);
        grid.TrySetMaterial(3, 0, 1);

        Assert.IsFalse(simulation.Step());

        grid.TryGetMaterial(3, 1, out byte materialId);
        Assert.AreEqual(2, materialId);
    }

    [Test]
    public void Step_WithSameSeed_ProducesSameDiagonalChoice()
    {
        var firstGrid = new SandGrid(3, 2);
        var secondGrid = new SandGrid(3, 2);
        firstGrid.TrySetMaterial(1, 1, 2);
        firstGrid.TrySetMaterial(1, 0, 1);
        secondGrid.TrySetMaterial(1, 1, 2);
        secondGrid.TrySetMaterial(1, 0, 1);

        var firstSimulation = new SandSimulation(firstGrid, randomSeed: 2468);
        var secondSimulation = new SandSimulation(secondGrid, randomSeed: 2468);

        firstSimulation.Step();
        secondSimulation.Step();

        firstGrid.TryGetMaterial(0, 0, out byte firstLeft);
        firstGrid.TryGetMaterial(2, 0, out byte firstRight);
        secondGrid.TryGetMaterial(0, 0, out byte secondLeft);
        secondGrid.TryGetMaterial(2, 0, out byte secondRight);
        Assert.AreEqual(firstLeft, secondLeft);
        Assert.AreEqual(firstRight, secondRight);
    }

    [Test]
    public void ApplyImpactBurst_WithZeroChances_DoesNotMoveParticles()
    {
        var burstGrid = CreateFilledBlockGrid();
        var burstSimulation = new SandSimulation(burstGrid, randomSeed: 12345);

        int movedParticleCount = burstSimulation.ApplyImpactBurst(0f, 0f, 2);

        Assert.AreEqual(0, movedParticleCount);
        Assert.AreEqual(25, CountOccupiedCells(burstGrid));
        Assert.AreEqual(25, CountOccupiedCells(burstGrid, 5, 5, 5, 5));
    }

    [Test]
    public void ApplyImpactBurst_WithFullChances_MovesParticlesWithoutChangingTotalCount()
    {
        var burstGrid = CreateFilledBlockGrid();
        var burstSimulation = new SandSimulation(burstGrid, randomSeed: 12345);

        int movedParticleCount = burstSimulation.ApplyImpactBurst(1f, 1f, 2);

        Assert.Greater(movedParticleCount, 0);
        Assert.AreEqual(25, CountOccupiedCells(burstGrid),
            "爆散只能搬运颗粒，不能覆盖、生成或删除颗粒");
        Assert.Less(CountOccupiedCells(burstGrid, 5, 5, 5, 5), 25,
            "爆散后原沙块区域应该留下裂隙或边缘缺口");
    }

    [Test]
    public void ApplyImpactBurst_WithSameSeed_ProducesSameResult()
    {
        var firstGrid = CreateFilledBlockGrid();
        var secondGrid = CreateFilledBlockGrid();
        var firstSimulation = new SandSimulation(firstGrid, randomSeed: 2468);
        var secondSimulation = new SandSimulation(secondGrid, randomSeed: 2468);

        int firstMovedCount = firstSimulation.ApplyImpactBurst(0.43f, 0.095f, 2);
        int secondMovedCount = secondSimulation.ApplyImpactBurst(0.43f, 0.095f, 2);

        Assert.AreEqual(firstMovedCount, secondMovedCount);
        for (int y = 0; y < firstGrid.Height; y++)
        {
            for (int x = 0; x < firstGrid.Width; x++)
            {
                firstGrid.TryGetMaterial(x, y, out byte firstMaterialId);
                secondGrid.TryGetMaterial(x, y, out byte secondMaterialId);
                Assert.AreEqual(firstMaterialId, secondMaterialId,
                    $"相同 Seed 在 Cell ({x}, {y}) 产生了不同结果");
            }
        }
    }

    private static SandGrid CreateFilledBlockGrid()
    {
        var targetGrid = new SandGrid(15, 15);
        for (int y = 5; y < 10; y++)
        {
            for (int x = 5; x < 10; x++)
                targetGrid.TrySetMaterial(x, y, 2);
        }

        return targetGrid;
    }

    private static int CountOccupiedCells(SandGrid targetGrid, int startX = 0, int startY = 0,
        int areaWidth = -1, int areaHeight = -1)
    {
        int endX = areaWidth < 0 ? targetGrid.Width : startX + areaWidth;
        int endY = areaHeight < 0 ? targetGrid.Height : startY + areaHeight;
        int count = 0;

        for (int y = startY; y < endY; y++)
        {
            for (int x = startX; x < endX; x++)
            {
                if (targetGrid.TryGetMaterial(x, y, out byte materialId) && materialId != 0)
                    count++;
            }
        }

        return count;
    }
}
