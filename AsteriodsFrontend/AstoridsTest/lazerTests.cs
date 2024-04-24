using Actors.UserActors;
using Actors;
using Akka.Actor;
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
    [Fact]
    public void Lazer_move_out_of_bounds()
    {
        int x = 699;
        int y = 1 - 20;
        var lazer = new Lazer() { x = 100, y = 100, Angle = 0 };
        lazer.Move();
        Assert.True(lazer.CheckBoundaries());
    }
    [Fact]
    public void SendLazerToActors()
    {
        using var system = ActorSystem.Create("MyTestSystem");

        var signalRActor = system.ActorOf(Props.Create<SignalRActor>(), "SignalRActor");
        var newLobbySupervisor = system.ActorOf(Props.Create<LobbySupervisor>(signalRActor), "NewLobbySupervisor");

        var username = "TomRiddle";

        var newlobby = new NewLobbyObject { username = username, hubConnection = "hubConnection" };

        newLobbySupervisor.Tell(newlobby);

        var user = new User() { Username = username };
        var response = ExpectMsg<GameLobby>(TimeSpan.FromSeconds(5));

        var message = new Lazer()
        {
            lobbyId = response.Id,
            user = user,
            Angle = 0,
            x= 100,
            y = 100,
        };

        newLobbySupervisor.Tell(message);
        //needs to update the GameLobby;
        var response2 = ExpectMsg<GameLobby>(TimeSpan.FromSeconds(5));

        Assert.Equal(1,response2.Lazers.Count());
    }
}
