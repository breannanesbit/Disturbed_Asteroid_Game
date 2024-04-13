using Akka.Dispatch.SysMsg;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    public int HitBox { get; set; } = 25;
    public bool isDead { get; set; } = false;
    private double radiansPerDegree = Math.PI / 180;


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
    public void moveForward()
    {
        double angleInRadians = Angle * radiansPerDegree;

        if (y-speed >= BoundaryTop)
        {
            int deltaX = (int)Math.Round(speed * Math.Sin(angleInRadians));
            int deltaY = (int)Math.Round(speed * Math.Cos(angleInRadians));
            x += deltaX;
            y -= deltaY;
        }
    }
    public void moveBackward()
    {
        double angleInRadians = Angle * radiansPerDegree;

        if (y+speed <= BoundaryBottom)
        {
            int backwardDeltaX = -(int)Math.Round(speed * Math.Sin(angleInRadians));
            int backwardDeltaY = -(int)Math.Round(speed * Math.Cos(angleInRadians));
            x += backwardDeltaX;
            y -= backwardDeltaY;
        }
    }

    public void moveRight()
    {
        if (x + speed <= BoundaryRight)
        {
            x += speed;
        }
    }

    public void moveLeft()
    {
        if (x - speed >= BoundaryLeft)
        {
            x -= speed;
        }
    }

}
