using Akka.TestKit.Xunit2;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AstoridsTest;

public class AsteroidTests : TestKit
{
    //damage
    [Fact]
    public void Asteroid_Can_Move_Forward()
    {
        var y = 100 - 5;
        var x = 100;
        var asteroid = new Asteroid() { Y = 100, X=100, Movement = "Up" };
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
        var x = 100 -5;
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
        var asteroid = new Asteroid() { Y = 100, X = 100, Movement ="Right" };
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
    public void Asertoid_Can_Not_Go_Out_Of_Bounds()
    {
        var asteroid = new Asteroid() { Y = 694, X = 694,spawnX=100,spawnY=100, Movement="Down" };
        asteroid.Move();
        Assert.Equal(100, asteroid.Y);
        Assert.Equal(100, asteroid.X);
    }
}
