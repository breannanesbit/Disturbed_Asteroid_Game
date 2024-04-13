using Akka.TestKit.Xunit2;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AstoridsTest;

public class lazerTests : TestKit
{
    [Fact]
    public void Lazer_move_forward()
    {
        int x = 100;
        int y = 100 -20;
        var lazer = new Lazer() { x = 100, y = 100, Angle= 0 };
        lazer.Move();
        Assert.Equal(x, lazer.x);
        Assert.Equal(y, lazer.y);
    }
}
