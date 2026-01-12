namespace Exercice2;

public sealed class Maze
{
    public int[][] Distances { get; init; }
    public bool[][] Grid { get; init; }
    public Queue<(int x, int y, int distance)> ToVisit { get; init; }
    public (int x, int y) Start { get; init; }
    public (int x, int y) Exit { get; init; }

    public Maze(string maze)
    {
        if (maze is null) throw new ArgumentNullException(nameof(maze));

        var lines = maze.Replace("\r\n", "\n").Split('\n', StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length == 0) throw new ArgumentException("Le labyrinthe est vide", nameof(maze));

        var height = lines.Length;
        var width = lines[0].Length;
        if (width == 0) throw new ArgumentException("Le labyrinthe est vide", nameof(maze));

        for (var y = 0; y < height; y++)
        {
            if (lines[y].Length != width)
                throw new ArgumentException("Le labyrinthe doit être rectangulaire", nameof(maze));
        }

        var grid = new bool[height][];
        (int x, int y)? start = null;
        (int x, int y)? exit = null;

        for (var y = 0; y < height; y++)
        {
            grid[y] = new bool[width];
            for (var x = 0; x < width; x++)
            {
                var c = lines[y][x];
                switch (c)
                {
                    case '#':
                        grid[y][x] = true;
                        break;
                    case '.':
                        grid[y][x] = false;
                        break;
                    case 'D':
                        grid[y][x] = false;
                        start = (x, y);
                        break;
                    case 'S':
                        grid[y][x] = false;
                        exit = (x, y);
                        break;
                    default:
                        throw new ArgumentException($"Caractère invalide '{c}' dans le labyrinthe", nameof(maze));
                }
            }
        }

        if (start is null) throw new ArgumentException("Le labyrinthe doit contenir un départ 'D'", nameof(maze));
        if (exit is null) throw new ArgumentException("Le labyrinthe doit contenir une sortie 'S'", nameof(maze));

        Grid = grid;
        Start = start.Value;
        Exit = exit.Value;

        Distances = new int[height][];
        for (var y = 0; y < height; y++) Distances[y] = new int[width];

        ToVisit = new Queue<(int x, int y, int distance)>();
        ToVisit.Enqueue((Start.x, Start.y, 0));
    }

    public int GetDistance()
    {
        while (ToVisit.Count > 0)
        {
            if (Fill())
                return Distances[Exit.y][Exit.x];
        }

        return Distances[Exit.y][Exit.x];
    }

    public IList<(int, int)> GetNeighbours(int x, int y)
    {
        var result = new List<(int, int)>(capacity: 4);

        void TryAdd(int nx, int ny)
        {
            if (nx < 0 || ny < 0) return;
            if (ny >= Grid.Length) return;
            if (nx >= Grid[ny].Length) return;
            if (nx == Start.x && ny == Start.y) return;
            if (Grid[ny][nx]) return;
            result.Add((nx, ny));
        }

        TryAdd(x, y - 1);
        TryAdd(x, y + 1);
        TryAdd(x - 1, y);
        TryAdd(x + 1, y);

        return result;
    }

    public bool Fill()
    {
        if (ToVisit.Count == 0) return false;

        var (x, y, distance) = ToVisit.Dequeue();

        if (x == Exit.x && y == Exit.y)
        {
            Distances[y][x] = distance;
            return true;
        }

        if (Distances[y][x] != 0) return false;

        Distances[y][x] = distance;

        foreach (var (nx, ny) in GetNeighbours(x, y))
            ToVisit.Enqueue((nx, ny, distance + 1));

        return false;
    }

    public IList<(int, int)> GetShortestPath()
    {
        var distance = GetDistance();
        if (distance <= 0) return new List<(int, int)> { Start };

        var pathReversed = new List<(int, int)>(capacity: distance + 1);
        var current = Exit;
        pathReversed.Add(current);

        while (current != Start)
        {
            var (cx, cy) = current;
            var currentDistance = Distances[cy][cx];
            if (currentDistance == 0) break;

            (int x, int y)? next = null;

            var candidates = new (int x, int y)[]
            {
                (cx, cy - 1),
                (cx, cy + 1),
                (cx - 1, cy),
                (cx + 1, cy)
            };

            foreach (var (nx, ny) in candidates)
            {
                if (nx < 0 || ny < 0) continue;
                if (ny >= Grid.Length) continue;
                if (nx >= Grid[ny].Length) continue;
                if (Grid[ny][nx]) continue;

                if (nx == Start.x && ny == Start.y && currentDistance == 1)
                {
                    next = Start;
                    break;
                }

                if (Distances[ny][nx] == currentDistance - 1)
                {
                    next = (nx, ny);
                    break;
                }
            }

            if (next is null) break;

            current = next.Value;
            pathReversed.Add(current);
        }

        pathReversed.Reverse();
        return pathReversed;
    }
}
