using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared;

public class Lazer
{
    public int x { get; set; } = 0;
    public int y { get; set; } = 0;
    public int BoundaryLeft { get; } = 0;
    public int BoundaryTop { get; } = 0;
    public int BoundaryRight { get; } = 700;
    public int BoundaryBottom { get; } = 700;
    public int Speed { get; } = 20;
    public int Damage { get; } = 50;
    public int Angle { get; set; } = 0;
    private double radiansPerDegree = Math.PI / 180;

    public bool Move()
    {
        double angleInRadians = Angle * radiansPerDegree;

        int deltaX = (int)Math.Round(Speed * Math.Sin(angleInRadians));
        int deltaY = (int)Math.Round(Speed * Math.Cos(angleInRadians));
        x += deltaX;
        y -= deltaY;

        if (!CheckBoundaries())
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    public bool CheckBoundaries()
    {
        return x >= BoundaryLeft && x <= BoundaryRight && y <= BoundaryBottom && y >= BoundaryTop;

    }
}