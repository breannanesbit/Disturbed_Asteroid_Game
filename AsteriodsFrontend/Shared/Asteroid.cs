using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared;

public class Asteroid
{

    public int X { get; set; } = 0;
    public int Y { get; set; } = 0;
    public int spawnX { get; set; } = 0;
    public int spawnY { get; set; } = 0;
    public int Health { get; set; } = 100;
    public string ImagePath { get; set; } = "";
    public int Speed { get; set; } = 5;
    //public bool Diagonal { get; set; }
    public string Movement { get; set; } = "";
    public int BoundaryLeft { get; } = 0;
    public int BoundaryTop { get; } = 0;
    public int BoundaryRight { get; } = 695;
    public int BoundaryBottom { get; } = 695;
    public int HitBox { get; set; } = 25;

    public void RandomCreation(int seed)
    {
        Random rand = new Random(seed);
        List<string> possibleMovements = new List<string> { "Diagonal", "Down", "Up", "Left", "Right" };
        string randomMovement = possibleMovements[rand.Next(possibleMovements.Count)];

        switch(randomMovement)
        {
            case "Diagonal":
                ImagePath = "asteroid-1.svg";
                X = BoundaryRight;
                Y = rand.Next(BoundaryTop, BoundaryBottom/2);
                break;
            case "Down":
                ImagePath = "asteroid-3.svg";
                X = rand.Next(BoundaryLeft, BoundaryRight);
                Y = BoundaryTop;
                break;
            case "Up":
                ImagePath = "asteroid-2.svg";
                X = rand.Next(BoundaryLeft, BoundaryRight);
                Y = BoundaryBottom;
                break;
            case "Left":
                ImagePath = "asteroid-2.svg";
                X = BoundaryRight;
                Y = rand.Next(BoundaryTop, BoundaryBottom);
                break;
            case "Right":
                ImagePath = "asteroid-2.svg";
                X = BoundaryLeft;
                Y = rand.Next(BoundaryTop, BoundaryBottom);
                break;
            default:
                break;
        }
        spawnX = X;
        spawnY = Y;
        Movement = randomMovement;
    }
    public void Damage()
    {
        Health -= 10;
    }
    public bool CheckBox(int x, int y)
    {
        return x >= this.X - 25 && x <= this.X + 25 && y >= this.Y - 25 && y <= this.Y + 25;
    }
    public void Move()
    {
        switch (Movement)
        {
            case "Diagonal":
                MoveDiagonal();
                break;
            case "Down":
                MoveDown();
                break;
            case "Up":
                MoveUp();
                break;
            case "Left":
                MoveLeft();
                break;
            case "Right":
                MoveRight();
                break;
        }
        if (!CheckBoundaries())
        {
            X = spawnX;
            Y = spawnY;
        }
    }


    public void MoveDown()
    {
        Y += Speed;
        //if the asteroid goes in border it needs to disapear or bounce off.
    }

    public void MoveDiagonal()
    {
        X -= Speed;
        Y += Speed;
    }

    public void MoveUp()
    {
        Y -= Speed;
    }

    public void MoveLeft()
    {
        X -= Speed;
    }

    public void MoveRight()
    {
        X += Speed;
    }

    public bool CheckBoundaries()
    {
        return X >= BoundaryLeft && X <= BoundaryRight && Y <= BoundaryBottom && Y >= BoundaryTop;
    }
}
