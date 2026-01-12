namespace Exercice2.Tests;

using Exercice2;

public sealed class MazeParsingTests
{
    private const string SampleMaze = """
D..#.
##...
.#.#.
...#.
####S
""";

    [Fact]
    public void Constructor_ParsesStartAndExit_AndInterpretsWallsAndEmptyCells()
    {
        var maze = new Maze(SampleMaze);

        Assert.Equal((0, 0), maze.Start);
        Assert.Equal((4, 4), maze.Exit);

        // '#' => mur
        Assert.True((bool)maze.Grid[0][3]);
        Assert.True((bool)maze.Grid[1][0]);
        Assert.True((bool)maze.Grid[4][0]);

        // '.' / 'D' / 'S' => case vide
        Assert.False((bool)maze.Grid[0][0]);
        Assert.False((bool)maze.Grid[0][1]);
        Assert.False((bool)maze.Grid[0][2]);
        Assert.False((bool)maze.Grid[4][4]);
    }

    [Fact]
    public void Constructor_InitializesDistances_SameSizeAsGrid_AndAllZeros()
    {
        var maze = new Maze(SampleMaze);

        Assert.Equal((int)maze.Grid.Length, (int)maze.Distances.Length);

        for (var y = 0; y < maze.Grid.Length; y++)
        {
            Assert.Equal((int)maze.Grid[y].Length, (int)maze.Distances[y].Length);

            for (var x = 0; x < maze.Grid[y].Length; x++)
            {
                Assert.Equal(0, maze.Distances[y][x]);
            }
        }
    }
}

public sealed class MazeNeighboursTests
{
    private const string NeighbourMaze = """
.....
.....
..D#.
.....
....S
""";

    [Fact]
    public void GetNeighbours_ReturnsAll4_WhenAllAdjacentCellsAreAvailable()
    {
        var maze = new Maze(NeighbourMaze);

        var neighbours = maze.GetNeighbours(1, 1);

        Assert.Equal(4, neighbours.Count);
        Assert.Contains((1, 0), neighbours);
        Assert.Contains((1, 2), neighbours);
        Assert.Contains((0, 1), neighbours);
        Assert.Contains((2, 1), neighbours);
    }

    [Fact]
    public void GetNeighbours_ExcludesOutsideLeftBoundary()
    {
        var maze = new Maze(NeighbourMaze);

        var neighbours = maze.GetNeighbours(0, 1);

        Assert.DoesNotContain((-1, 1), neighbours);
        Assert.Contains((0, 0), neighbours);
        Assert.Contains((0, 2), neighbours);
        Assert.Contains((1, 1), neighbours);
    }

    [Fact]
    public void GetNeighbours_ExcludesOutsideRightBoundary()
    {
        var maze = new Maze(NeighbourMaze);

        var neighbours = maze.GetNeighbours(4, 1);

        Assert.DoesNotContain((5, 1), neighbours);
        Assert.Contains((4, 0), neighbours);
        Assert.Contains((4, 2), neighbours);
        Assert.Contains((3, 1), neighbours);
    }

    [Fact]
    public void GetNeighbours_ExcludesOutsideTopBoundary()
    {
        var maze = new Maze(NeighbourMaze);

        var neighbours = maze.GetNeighbours(1, 0);

        Assert.DoesNotContain((1, -1), neighbours);
        Assert.Contains((1, 1), neighbours);
        Assert.Contains((0, 0), neighbours);
        Assert.Contains((2, 0), neighbours);
    }

    [Fact]
    public void GetNeighbours_ExcludesOutsideBottomBoundary()
    {
        var maze = new Maze(NeighbourMaze);

        var neighbours = maze.GetNeighbours(1, 4);
        
        Assert.DoesNotContain((1, 5), neighbours);
        Assert.Contains((1, 3), neighbours);
        Assert.Contains((0, 4), neighbours);
        Assert.Contains((2, 4), neighbours);
    }

    [Fact]
    public void GetNeighbours_ExcludesWalls()
    {
        var maze = new Maze(NeighbourMaze);

        var neighbours = maze.GetNeighbours(2, 2);

        Assert.DoesNotContain((3, 2), neighbours);
    }

    [Fact]
    public void GetNeighbours_ExcludesStartCell()
    {
        var maze = new Maze(NeighbourMaze);

        var neighbours = maze.GetNeighbours(2, 1);

        Assert.DoesNotContain((2, 2), neighbours);
    }
}

public sealed class MazeFillTests
{
    [Fact]
    public void Constructor_InitializesQueueWithStartDistance0()
    {
        const string mazeText = """
D.
.S
""";

        var maze = new Maze(mazeText);

        Assert.Single(maze.ToVisit);
        Assert.Equal((maze.Start.x, maze.Start.y, 0), maze.ToVisit.Peek());
    }

    [Fact]
    public void Fill_ReturnsFalseUntilExitThenTrue()
    {
        const string mazeText = """
D.
.S
""";

        var maze = new Maze(mazeText);

        // Start
        Assert.False(maze.Fill());
        // (1,0)
        Assert.False(maze.Fill());
        // (0,1)
        Assert.False(maze.Fill());
        // Exit (1,1)
        Assert.True(maze.Fill());
    }

    [Fact]
    public void Fill_IgnoresCellAlreadyHavingDistance_AndDoesNotEnqueueNeighbours()
    {
        const string mazeText = """
D..
...
..S
""";

        var maze = new Maze(mazeText);

        maze.Distances[0][1] = 42;

        maze.ToVisit.Clear();
        maze.ToVisit.Enqueue((1, 0, 1));

        Assert.False(maze.Fill());

        Assert.Empty(maze.ToVisit);
        Assert.Equal(42, maze.Distances[0][1]);
    }

    [Fact]
    public void Fill_EnqueuesNeighbours_WithDistancePlusOne()
    {
        const string mazeText = """
...
.D.
..S
""";

        var maze = new Maze(mazeText);

        maze.ToVisit.Clear();
        maze.ToVisit.Enqueue((1, 1, 0));

        Assert.False(maze.Fill());

        var items = maze.ToVisit.ToList();
        Assert.Equal(4, items.Count);

        Assert.Contains((1, 0, 1), items);
        Assert.Contains((1, 2, 1), items);
        Assert.Contains((0, 1, 1), items);
        Assert.Contains((2, 1, 1), items);
    }
}

public sealed class MazeDistanceTests
{
    [Fact]
    public void GetDistance_Returns2_ForSimple2x2Maze()
    {
        // Chemin minimal: (0,0) -> (1,0) -> (1,1) = 2 déplacements
        const string mazeText = """
D.
.S
""";

        var maze = new Maze(mazeText);

        Assert.Equal(2, maze.GetDistance());
    }

    [Fact]
    public void GetDistance_Returns8_ForSampleMaze()
    {
        const string sampleMaze = """
D..#.
##...
.#.#.
...#.
####S
""";

        var maze = new Maze(sampleMaze);

        Assert.Equal(8, maze.GetDistance());
    }
}

public sealed class MazeShortestPathTests
{
    [Fact]
    public void GetShortestPath_ReturnsExpectedPath_ForSimple2x2Maze()
    {
        const string mazeText = """
D.
.S
""";

        var maze = new Maze(mazeText);
        Assert.Equal(2, maze.GetDistance());

        var path = maze.GetShortestPath();

        Assert.Equal(3, path.Count);
        Assert.Equal((0, 0), path[0]);
        Assert.Equal((1, 0), path[1]);
        Assert.Equal((1, 1), path[2]);
    }

    [Fact]
    public void GetShortestPath_IsValidAndMinimal_ForSampleMaze()
    {
        const string sampleMaze = """
D..#.
##...
.#.#.
...#.
####S
""";

        var maze = new Maze(sampleMaze);
        var distance = maze.GetDistance();
        Assert.Equal(8, distance);

        var path = maze.GetShortestPath();

        Assert.NotEmpty(path);
        Assert.Equal(maze.Start, path[0]);
        Assert.Equal(maze.Exit, path[^1]);
        Assert.Equal(distance + 1, path.Count);

        for (var i = 1; i < path.Count; i++)
        {
            var (x1, y1) = path[i - 1];
            var (x2, y2) = path[i];

            var manhattan = Math.Abs(x1 - x2) + Math.Abs(y1 - y2);
            Assert.Equal(1, manhattan);
            Assert.False(maze.Grid[y2][x2]);
        }

        for (var i = 0; i < path.Count; i++)
        {
            var (x, y) = path[i];
            Assert.Equal(i, maze.Distances[y][x]);
        }
    }
}
