using Exercice2;

static string GetMazeText(string[] args)
{
    if (args.Length > 0)
    {
        var path = args[0];
        if (!File.Exists(path))
            throw new FileNotFoundException($"Fichier introuvable: {path}", path);

        return File.ReadAllText(path);
    }

    return """
D..#.
##...
.#.#.
...#.
####S
""";
}

static void PrintPath(IList<(int x, int y)> path)
{
    for (var i = 0; i < path.Count; i++)
    {
        var (x, y) = path[i];
        Console.Write($"({x},{y})");
        if (i < path.Count - 1)
            Console.Write(" -> ");
    }

    Console.WriteLine();
}

try
{
    var mazeText = GetMazeText(args);
    var maze = new Maze(mazeText);

    var distance = maze.GetDistance();

    Console.WriteLine("Labyrinthe:");
    Console.WriteLine(mazeText.TrimEnd());
    Console.WriteLine();

    if (distance < 0)
    {
        Console.WriteLine("Aucun chemin trouvé entre D et S.");
        Environment.ExitCode = 1;
        return;
    }

    Console.WriteLine($"Distance minimale: {distance}");

    var path = maze.GetShortestPath();
    Console.WriteLine($"Chemin (longueur {path.Count}):");
    PrintPath(path);
}
catch (Exception ex)
{
    Console.Error.WriteLine(ex.Message);
    Environment.ExitCode = 1;
}
