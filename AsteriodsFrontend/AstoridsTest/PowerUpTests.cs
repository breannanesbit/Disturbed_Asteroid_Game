using Actors.UserActors;
using Actors;
using Akka.Actor;
using Akka.TestKit.Xunit2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared;

namespace AstoridsTest;

public class PowerUpTests : TestKit
{
    [Fact]
    public void PowerUpMoves()
    {
        var powerup = new PowerUp();
        powerup.MoveLeft();
        int X = 690 - 1;
        int Y = 100;
        
        Assert.Equal(X, powerup.X);
        Assert.Equal(Y, powerup.Y);
    }
    [Fact]
    public void PowerUpCreation() 
    {
        int seed = Guid.NewGuid().GetHashCode();
        PowerUp powerup = new PowerUp();
        powerup.PowerUpCreation(seed);
        int x = 690;
        int y = 100;
        Assert.NotEqual(x, powerup.X);
        Assert.NotEqual(y, powerup.Y);

    }
    [Fact]
    public void PowerUpMovesAndGetsPickedUp()
    {
        var powerup = new PowerUp {X = 101, Y = 100};
        var ship = new Ship();
        powerup.MoveLeft();

        if(ship.CheckBox(powerup.X, powerup.Y))
        {
            ship.TogglePowerup(true);
        }
        Assert.True(ship.HasPowerup);
        Assert.Equal(110, ship.Health);
        Assert.Equal(ship.ShipColor, "powerBlue");
    }
    
}
