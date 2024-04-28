using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared;
public class PowerUp
{
    public int PowerupType { get; set; } = 0;
    public int X { get; set; } = 690;
    public int Y { get; set; } = 100;
    public int BoundaryLeft { get; } = 0;
    public int BoundaryTop { get; } = 0;
    public int BoundaryRight { get; } = 695;
    public int BoundaryBottom { get; } = 695;
    public int HitBox { get; set; } = 30;
    public string Image { get; set; } = "gear.svg";
    public int Speed { get; set; } = 1;

    public void MoveLeft()
    {
        X -= Speed;
    }
    public bool CheckBoundaries()
    {
        return X >= BoundaryLeft && X <= BoundaryRight && Y <= BoundaryBottom && Y >= BoundaryTop;
    }
    public void PowerUpCreation(int seed)
    {
        Random rand = new Random(seed);
        PowerupType = rand.Next(3);
        X = BoundaryRight;
        Y = rand.Next(BoundaryTop, BoundaryBottom);
        switch (PowerupType)
        {
            case 0:
                Image = "gear.svg";
                break;
            case 1:
                Image = "target.svg";
                //dual
                //color change
                break;
            case 2:
                Image = "target.svg";
                //tripple
                //color change
                break;
            default:
                break;
        }
    }
}
