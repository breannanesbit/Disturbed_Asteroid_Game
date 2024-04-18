using Akka.Dispatch.SysMsg;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Shared;

public class Ship
{
    public int x { get; set; } = 100;
    public int y { get; set; } = 100;
    public int BoundaryLeft { get; } = 0;
    public int BoundaryTop { get; } = 0;
    public int BoundaryRight { get; } = 700;
    public int BoundaryBottom { get; } = 700;
    public int speed { get; } = 10;
    public int Health { get; set; } = 100;
    public int Angle { get; set; } = 0;
    public int TurnStep { get; set; } = 10;
    public int HitBox { get; set; } = 25;
    public bool isDead { get; set; } = false;
    private double radiansPerDegree = Math.PI / 180;
    public string ShipColor { get; set; } = "white";
    public int ShipImage { get; set; } = 0;


    public void Damage(int hit)
    {
        Health -= hit;
        if (Health <= 0)
        {
            Health = 0;
            isDead = true;
        }
    }
    public bool CheckBox(int x, int y)
    {
        return x >= this.x - 25 && x <= this.x + 25 && y >= this.y - 25 && y <= this.y + 25;
    }
    public bool CheckBoundaries()
    {
        return x >= BoundaryLeft && x <= BoundaryRight && y <= BoundaryBottom && y >= BoundaryTop;
    }
    public void moveForward()
    {
        double angleInRadians = Angle * radiansPerDegree;
        var originx = x;
        var originy = y;
            int deltaX = (int)Math.Round(speed * Math.Sin(angleInRadians));
            int deltaY = (int)Math.Round(speed * Math.Cos(angleInRadians));
            x += deltaX;
            y -= deltaY;
            if (!CheckBoundaries())
            {
                x = originx;
                y = originy;
            }
    }
    public void moveBackward()
    {
        double angleInRadians = Angle * radiansPerDegree;
        var originx = x;
        var originy = y;
            int backwardDeltaX = -(int)Math.Round(speed * Math.Sin(angleInRadians));
            int backwardDeltaY = -(int)Math.Round(speed * Math.Cos(angleInRadians));
            x += backwardDeltaX;
            y -= backwardDeltaY;
        if (!CheckBoundaries())
        {
            x = originx;
            y = originy;
        }
    }

    public void moveRight()
    {
        Angle += TurnStep;
    }

    public void moveLeft()
    {
        Angle -= TurnStep;

    }

}
