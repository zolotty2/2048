using System;
using System.Collections.Generic;

namespace _2048 {
    public class Game2048
{
    private int[,] grid;
    private int size;
    private Random random;
    public int Score { get; private set; }
    public bool GameOver { get; private set; }
    public bool Won { get; private set; }

    // Для отслеживания анимаций
    public List<Animation> Animations { get; private set; }
        public bool FirstWinAchieved { get; internal set; }

        public Game2048(int gridSize = 4)
    {
        size = gridSize;
        grid = new int[size, size];
        random = new Random();
        Score = 0;
        GameOver = false;
        Won = false;
        Animations = new List<Animation>();
        AddRandomTile();
        AddRandomTile();
    }

    public int[,] GetGrid()
    {
        return (int[,])grid.Clone();
    }

    public void Move(Direction direction)
    {
        if (GameOver || Won) return;

        int[,] previousGrid = (int[,])grid.Clone();
        bool moved = false;

        // Очищаем предыдущие анимации
        Animations.Clear();

        switch (direction)
        {
            case Direction.Up:
                moved = MoveUp();
                break;
            case Direction.Down:
                moved = MoveDown();
                break;
            case Direction.Left:
                moved = MoveLeft();
                break;
            case Direction.Right:
                moved = MoveRight();
                break;
        }

        if (moved)
        {
            AddRandomTile();
            CheckGameStatus();
        }
    }

    private bool MoveUp()
    {
        bool moved = false;
        for (int col = 0; col < size; col++)
        {
            for (int row = 1; row < size; row++)
            {
                if (grid[row, col] != 0)
                {
                    int currentRow = row;
                    int startRow = row;

                    while (currentRow > 0 && grid[currentRow - 1, col] == 0)
                    {
                        grid[currentRow - 1, col] = grid[currentRow, col];
                        grid[currentRow, col] = 0;
                        currentRow--;
                        moved = true;
                    }

                    if (currentRow > 0 && grid[currentRow - 1, col] == grid[currentRow, col])
                    {
                        // Анимация слияния
                        Animations.Add(new Animation(
                            AnimationType.Merge,
                            new TilePosition(row, col),
                            new TilePosition(currentRow - 1, col),
                            grid[currentRow, col] * 2
                        ));

                        grid[currentRow - 1, col] *= 2;
                        Score += grid[currentRow - 1, col];
                        grid[currentRow, col] = 0;
                        moved = true;
                    }
                    else if (startRow != currentRow)
                    {
                        // Анимация перемещения
                        Animations.Add(new Animation(
                            AnimationType.Move,
                            new TilePosition(startRow, col),
                            new TilePosition(currentRow, col),
                            grid[currentRow, col]
                        ));
                    }
                }
            }
        }
        return moved;
    }

    private bool MoveDown()
    {
        bool moved = false;
        for (int col = 0; col < size; col++)
        {
            for (int row = size - 2; row >= 0; row--)
            {
                if (grid[row, col] != 0)
                {
                    int currentRow = row;
                    int startRow = row;

                    while (currentRow < size - 1 && grid[currentRow + 1, col] == 0)
                    {
                        grid[currentRow + 1, col] = grid[currentRow, col];
                        grid[currentRow, col] = 0;
                        currentRow++;
                        moved = true;
                    }

                    if (currentRow < size - 1 && grid[currentRow + 1, col] == grid[currentRow, col])
                    {
                        // Анимация слияния
                        Animations.Add(new Animation(
                            AnimationType.Merge,
                            new TilePosition(row, col),
                            new TilePosition(currentRow + 1, col),
                            grid[currentRow, col] * 2
                        ));

                        grid[currentRow + 1, col] *= 2;
                        Score += grid[currentRow + 1, col];
                        grid[currentRow, col] = 0;
                        moved = true;
                    }
                    else if (startRow != currentRow)
                    {
                        // Анимация перемещения
                        Animations.Add(new Animation(
                            AnimationType.Move,
                            new TilePosition(startRow, col),
                            new TilePosition(currentRow, col),
                            grid[currentRow, col]
                        ));
                    }
                }
            }
        }
        return moved;
    }

    private bool MoveLeft()
    {
        bool moved = false;
        for (int row = 0; row < size; row++)
        {
            for (int col = 1; col < size; col++)
            {
                if (grid[row, col] != 0)
                {
                    int currentCol = col;
                    int startCol = col;

                    while (currentCol > 0 && grid[row, currentCol - 1] == 0)
                    {
                        grid[row, currentCol - 1] = grid[row, currentCol];
                        grid[row, currentCol] = 0;
                        currentCol--;
                        moved = true;
                    }

                    if (currentCol > 0 && grid[row, currentCol - 1] == grid[row, currentCol])
                    {
                        // Анимация слияния
                        Animations.Add(new Animation(
                            AnimationType.Merge,
                            new TilePosition(row, col),
                            new TilePosition(row, currentCol - 1),
                            grid[row, currentCol] * 2
                        ));

                        grid[row, currentCol - 1] *= 2;
                        Score += grid[row, currentCol - 1];
                        grid[row, currentCol] = 0;
                        moved = true;
                    }
                    else if (startCol != currentCol)
                    {
                        // Анимация перемещения
                        Animations.Add(new Animation(
                            AnimationType.Move,
                            new TilePosition(row, startCol),
                            new TilePosition(row, currentCol),
                            grid[row, currentCol]
                        ));
                    }
                }
            }
        }
        return moved;
    }

    private bool MoveRight()
    {
        bool moved = false;
        for (int row = 0; row < size; row++)
        {
            for (int col = size - 2; col >= 0; col--)
            {
                if (grid[row, col] != 0)
                {
                    int currentCol = col;
                    int startCol = col;

                    while (currentCol < size - 1 && grid[row, currentCol + 1] == 0)
                    {
                        grid[row, currentCol + 1] = grid[row, currentCol];
                        grid[row, currentCol] = 0;
                        currentCol++;
                        moved = true;
                    }

                    if (currentCol < size - 1 && grid[row, currentCol + 1] == grid[row, currentCol])
                    {
                        // Анимация слияния
                        Animations.Add(new Animation(
                            AnimationType.Merge,
                            new TilePosition(row, col),
                            new TilePosition(row, currentCol + 1),
                            grid[row, currentCol] * 2
                        ));

                        grid[row, currentCol + 1] *= 2;
                        Score += grid[row, currentCol + 1];
                        grid[row, currentCol] = 0;
                        moved = true;
                    }
                    else if (startCol != currentCol)
                    {
                        // Анимация перемещения
                        Animations.Add(new Animation(
                            AnimationType.Move,
                            new TilePosition(row, startCol),
                            new TilePosition(row, currentCol),
                            grid[row, currentCol]
                        ));
                    }
                }
            }
        }
        return moved;
    }

    private void AddRandomTile()
    {
        var emptyCells = new List<(int, int)>();
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                if (grid[i, j] == 0)
                {
                    emptyCells.Add((i, j));
                }
            }
        }

        if (emptyCells.Count > 0)
        {
            var (row, col) = emptyCells[random.Next(emptyCells.Count)];
            grid[row, col] = random.Next(10) < 9 ? 2 : 4;

            // Анимация появления новой плитки
            Animations.Add(new Animation(
                AnimationType.Appear,
                new TilePosition(row, col),
                new TilePosition(row, col),
                grid[row, col]
            ));
        }
    }

    private void CheckGameStatus()
    {
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                if (grid[i, j] == 2048 && !Won) // Добавляем проверку !Won чтобы засчитывать только первую победу за игру
                {
                    Won = true;
                    GameStatsManager.RecordWin(); // Записываем победу
                    return;
                }
            }
        }

        // Проверка победы
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                if (grid[i, j] == 2048)
                {
                    Won = true;
                    return;
                }
            }
        }

        // Проверка наличия пустых клеток
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                if (grid[i, j] == 0) return;
            }
        }

        // Проверка возможных ходов
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                int current = grid[i, j];
                if ((i < size - 1 && grid[i + 1, j] == current) ||
                    (j < size - 1 && grid[i, j + 1] == current))
                {
                    return;
                }
            }
        }

        GameOver = true;
    }

    public void Restart()
    {
        grid = new int[size, size];
        Score = 0;
        GameOver = false;
        Won = false;
        Animations.Clear();
        AddRandomTile();
        AddRandomTile();
    }
}

public enum Direction
{
    Up,
    Down,
    Left,
    Right
}

public enum AnimationType
{
    Move,
    Merge,
    Appear
}

public class Animation
{
    public AnimationType Type { get; set; }
    public TilePosition From { get; set; }
    public TilePosition To { get; set; }
    public int Value { get; set; }
    public float Progress { get; set; }

    public Animation(AnimationType type, TilePosition from, TilePosition to, int value)
    {
        Type = type;
        From = from;
        To = to;
        Value = value;
        Progress = 0f;
    }
}

public class TilePosition
{
    public int Row { get; set; }
    public int Col { get; set; }

    public TilePosition(int row, int col)
    {
        Row = row;
        Col = col;
    }
}
}
