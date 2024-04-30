using Akka.TestKit.Xunit2;
using Shared;

namespace AstoridsTest;

public class AsteroidTests : TestKit
{
    //damage
    [Fact]
    public void Asteroid_Can_Move_Forward()
    {
        var y = 100 - 5;
        var x = 100;
        var asteroid = new Asteroid() { Y = 100, X = 100, Movement = "Up" };
        asteroid.Move();
        Assert.Equal(y, asteroid.Y);
        Assert.Equal(x, asteroid.X);
    }
    [Fact]
    public void Asteroid_Can_Move_Backwards()
    {
        var y = 100 + 5;
        var x = 100;
        var asteroid = new Asteroid() { Y = 100, X = 100, Movement = "Down" };
        asteroid.Move();
        Assert.Equal(y, asteroid.Y);
        Assert.Equal(x, asteroid.X);
    }
    [Fact]
    public void Asteroid_Can_Move_Left()
    {
        var y = 100;
        var x = 100 - 5;
        var asteroid = new Asteroid() { Y = 100, X = 100, Movement = "Left" };
        asteroid.Move();
        Assert.Equal(y, asteroid.Y);
        Assert.Equal(x, asteroid.X);
    }
    [Fact]
    public void Asteroid_Can_Move_Right()
    {
        var y = 100;
        var x = 100 + 5;
        var asteroid = new Asteroid() { Y = 100, X = 100, Movement = "Right" };
        asteroid.Move();
        Assert.Equal(y, asteroid.Y);
        Assert.Equal(x, asteroid.X);
    }
    [Fact]
    public void Asteroid_Can_Move_Right()
    {
        var y = 100;
        var x = 100 + 5;
        var asteroid = new Asteroid() { Y = 100, X = 100, Movement = "Right" };
        asteroid.Move();
        Assert.Equal(y, asteroid.Y);
        Assert.Equal(x, asteroid.X);
    }
    [Fact]
    public void Asteroid_Can_Move_Right2()
    {
        var y = 100;
        var x = 100 + 5;
        var asteroid = new Asteroid() { Y = 100, X = 100, Movement = "Right" };
        asteroid.Move();
        Assert.Equal(y, asteroid.Y);
        Assert.Equal(x, asteroid.X);
    }
    [Fact]
    public void Asteroid_Can_Move_Right3()
    {
        var y = 100;
        var x = 100 + 5;
        var asteroid = new Asteroid() { Y = 100, X = 100, Movement = "Right" };
        asteroid.Move();
        Assert.Equal(y, asteroid.Y);
        Assert.Equal(x, asteroid.X);
    }
    [Fact]
    public void Asteroid_Can_Move_Right4()
    {
        var y = 100;
        var x = 100 + 5;
        var asteroid = new Asteroid() { Y = 100, X = 100, Movement = "Right" };
        asteroid.Move();
        Assert.Equal(y, asteroid.Y);
        Assert.Equal(x, asteroid.X);
    }
    [Fact]
    public void Asteroid_Can_Move_Righ5t()
    {
        var y = 100;
        var x = 100 + 5;
        var asteroid = new Asteroid() { Y = 100, X = 100, Movement = "Right" };
        asteroid.Move();
        Assert.Equal(y, asteroid.Y);
        Assert.Equal(x, asteroid.X);
    }
    [Fact]
    public void Asteroid_Can_Move_Diagonal()
    {
        var y = 100 + 5;
        var x = 100 - 5;
        var asteroid = new Asteroid() { Y = 100, X = 100, Movement = "Diagonal" };
        asteroid.Move();
        Assert.Equal(y, asteroid.Y);
        Assert.Equal(x, asteroid.X);
    }

    [Fact]
    public void Asteroid_Can_Move_Diagonal2()
    {
        var y = 100 + 5;
        var x = 100 - 5;
        var asteroid = new Asteroid() { Y = 100, X = 100, Movement = "Diagonal" };
        asteroid.Move();
        Assert.Equal(y, asteroid.Y);
        Assert.Equal(x, asteroid.X);
    }
    [Fact]
    public void Asertoid_Can_Not_Go_Out_Of_Bounds()
    {
        var asteroid = new Asteroid() { Y = 694, X = 694, spawnX = 100, spawnY = 100, Movement = "Down" };
        asteroid.Move();
        Assert.Equal(100, asteroid.Y);
        Assert.Equal(100, asteroid.X);
    }
    [Fact]
    public void Asertoid_Damages_Ship_on_Colision()
    {
        var ship = new Ship() { x = 100, y = 100 };
        var asteroid = new Asteroid() { Y = 694, X = 694, spawnX = 100, spawnY = 100, Movement = "Down" };
        asteroid.Move();
        if (ship.CheckBox(asteroid.X, asteroid.Y))
        {
            ship.Damage(10);
        }
        Assert.Equal(90, ship.Health);
    }
}
