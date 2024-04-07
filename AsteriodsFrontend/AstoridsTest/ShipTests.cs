using Akka.TestKit.Xunit2;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AstoridsTest;

public class ShipTests : TestKit
{
    [Fact]
    public void Ship_can_Move_Forward()
    {
        var y = 100 - 10;
        var ship = new Ship();
        ship.moveForward();
        Assert.Equal(ship.y, y);
    }
    [Fact]
    public void Ship_can_Move_Backward()
    {
        var y = 100 + 10;
        var ship = new Ship();
        ship.moveBackward();
        Assert.Equal(ship.y, y);
    }
    [Fact]
    public void Ship_can_Move_Right()
    {
        var x = 100 + 10;
        var ship = new Ship();
        ship.moveRight();
        Assert.Equal(ship.x, x);
    }
    [Fact]
    public void Ship_can_Move_Left()
    {
        var x = 100 - 10;
        var ship = new Ship();
        ship.moveLeft();
        Assert.Equal(ship.x, x);
    }

    [Fact]
    public void Ship_cant_go_past_Top_Bounds()
    {
        var y = 5 - 10;
        var ship = new Ship();
        ship.y = 5;
        ship.moveForward();
        Assert.NotEqual(ship.y, y);
        Assert.Equal(ship.y, 5);
    }
    [Fact]
    public void Ship_cant_go_past_Bottom_Bounds()
    {
        var y = 695 + 10;
        var ship = new Ship();
        ship.y = 695;
        ship.moveBackward();
        Assert.NotEqual(ship.y, y);
        Assert.Equal(ship.y, 695);
    }
    [Fact]
    public void Ship_cant_go_past_Right_Bounds()
    {
        var x = 695 + 10;
        var ship = new Ship();
        ship.x = 695;
        ship.moveBackward();
        Assert.NotEqual(ship.x, x);
        Assert.Equal(ship.x, 695);
    }
    [Fact]
    public void Ship_cant_go_past_Left_Bounds()
    {
        var x = 5 - 10;
        var ship = new Ship();
        ship.x = 5;
        ship.moveBackward();
        Assert.NotEqual(ship.x, x);
        Assert.Equal(ship.x, 5);
    }
}
