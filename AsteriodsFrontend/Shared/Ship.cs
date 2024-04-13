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
    public int Health { get; set; }

    public void moveForward()
    {
        if (y - speed >= BoundaryTop)
        {
            y -= speed;
        }
    }
    public void moveBackward()
    {
        if (y + speed <= BoundaryBottom)
        {
            y += speed;
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
