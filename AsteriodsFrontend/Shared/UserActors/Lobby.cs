using Akka.Actor;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Shared;
using Shared.Metrics;

namespace Actors.UserActors
{
    /*add game timer
      update user states and if all are updated send to signalR
      list of current lazers that are fired.
    */
    public enum GameState { Joining, Playing, Over }
    public enum ShipMoves { Forward, Backward, Left, Right }
    public class LobbyActor : ReceiveActor
    {
        private Timer timer;
        private Timer powerTimer;
        private Timer asteroidTimer;
        private int powerupTic = 0;
        private int seed = Guid.NewGuid().GetHashCode();
        //if poweruptic is 0 then powerup can be made else none can be created.

        private readonly HttpClient _httpClient;
        private readonly ILogger<LobbyActor> logger;

        public GameLobby CurrentLobby { get; set; } = new GameLobby();
        public LobbyActor(ILogger<LobbyActor> logger)
        {

            Receive<Lobby>((lobby) =>
            {
                Console.WriteLine("made it to the lobby actor");
                logger.LogInformation("creating new lobby in the actor");
                CurrentLobby.CurrentState = GameState.Joining;
                CurrentLobby.HeadPlayer = lobby.HeadPlayer;
                CurrentLobby.Id = lobby.Id;

                var findUser = CurrentLobby.Players.Find(x => x.Username == lobby.HeadPlayer.Username && x.hubConnection == lobby.HeadPlayer.hubConnection);
                if (findUser == null)
                {
                    CurrentLobby.Players.Add(CurrentLobby.HeadPlayer);
                }

                Console.WriteLine($"Created a new state");

                Sender.Tell(CurrentLobby);
            });
            Receive<StopActorMessage>((pill) =>
            {
                Self.Tell(PoisonPill.Instance);
            });
            Receive<MoveEvent>((moveEvent) =>
            {
                // handle movement of ship
                switch(moveEvent.ShipMoves)
                {
                    case ShipMoves.Forward:
                        moveEvent.user.Ship.moveForward();
                        break;
                    case ShipMoves.Backward:
                        moveEvent.user.Ship.moveBackward();
                        break;
                    case ShipMoves.Left:
                        moveEvent.user.Ship.moveLeft();
                        break;
                    case ShipMoves.Right:
                        moveEvent.user.Ship.moveRight();
                        break;
                }
                Ship ship = moveEvent.user.Ship;
                AddPointsToShip(1,ship);
                Sender.Tell(CurrentLobby);

            });
            Receive<Lazer>((lazer) =>
            {
                CurrentLobby.Lazers.Add(lazer);
            });
            Receive<ChangeGameState>(async state =>
            {
                if (CurrentLobby.HeadPlayer.Username == state.user)
                {
                    logger.LogInformation($"Changing {state.lobbyId} state");
                    //CurrentLobby = state.lobby;
                    CurrentLobby.CurrentState = state.lobbyState;

                    AddPowerUp(seed);

                    asteroidTimer = new Timer(async (_) =>
                    {
                        await AddAsteroid();
                    }, null, TimeSpan.Zero, TimeSpan.FromSeconds(3));
                    timer = new Timer(async (_) =>
                    {
                        await MoveAsteroids(CurrentLobby.HeadPlayer.Ship);
                        await MoveLazers();
                        if(CurrentLobby.PowerUps.Count != 0)
                        {
                            await MovePowerups(CurrentLobby.HeadPlayer.Ship);
                            Sender.Tell(CurrentLobby.CurrentState);
                        }
                    }, null, TimeSpan.Zero, TimeSpan.FromSeconds(0.05));

                }
                if(CurrentLobby.CurrentState == GameState.Over)
                {
                    //add game points to users points
                    CurrentLobby.HeadPlayer.Points += CurrentLobby.HeadPlayer.Ship.Points;
                    timer.Dispose();
                }
            });

            Receive<AddUserToLobby>(state =>
            {
                DefineMetrics.UserCount.Add(1);
                // Check if the user is already in the lobby
                if (CurrentLobby.Players.Any(u => u.Username == state.username))
                {
                    Sender.Tell(CurrentLobby.CurrentState); // Reply with the current player count
                }
                else
                {
                    // Add the user to the lobby
                    CurrentLobby.Players.Add(new User { Username = state.username });
                    // Reply with the updated lobby state
                    Sender.Tell(CurrentLobby.CurrentState);
                }
            });
            Receive<User>(user =>
            {
                
            });

            Receive<DecreaseUserHealth>(userHealth =>
            {
                var user = CurrentLobby.Players.Find(u => u.Username == userHealth.User.Username);
                if (user != null)
                {
                    // Decrease the user's health
                    user.Ship.Health -= userHealth.Damage;

                    // If the user's health is zero or less, remove them from the lobby
                    if (user.Ship.Health <= 0)
                    {
                        CurrentLobby.Players.Remove(user);
                        user.Ship.isDead = true;
                        CurrentLobby.DeadPlayers.Add(user);
                        if (CurrentLobby.Players.Count == 0)
                        {
                            CurrentLobby.CurrentState = GameState.Over;
                        }
                    }
                    Sender.Tell(CurrentLobby);
                }

            });
            this.logger = logger;
        }

        public async Task AddAsteroid()// could input how many should be made and loop over.
        {
            int seed = Guid.NewGuid().GetHashCode();
            Asteroid NewAsteroid = new Asteroid();
            NewAsteroid.RandomCreation(seed);
            CurrentLobby.Asteroids.Add(NewAsteroid);
        }
        public async Task AddLazer(int x, int y, int angle)
        {
            //check how many lazers each user has and that it doesn't go over 7
            Lazer NewLazer = new Lazer
            {
                x= x, y = y, Angle = angle
            };
            CurrentLobby.Lazers.Add(NewLazer);
            
        }
        public async Task AddPowerUp(int seed)
        {
            PowerUp powerup = new PowerUp();
            powerup.PowerUpCreation(seed);
            CurrentLobby.PowerUps.Add(powerup);
        }
        public async Task MoveAsteroids(Ship state)
        {
            for(int i = CurrentLobby.Asteroids.Count -1; i >= 0; i--)
            {
                var asteroid = CurrentLobby.Asteroids[i];
                asteroid.Move();
                if(asteroid.Health <= 0)
                {
                    CurrentLobby.Asteroids.Remove(asteroid);
                }
                if (state.CheckBox(asteroid.X, asteroid.Y))
                {
                    state.Damage(10);
                }
                //check each ship to see if its been hit by an asteroid.
            }
        }
        public async Task MoveLazers()
        {
            for(int i = CurrentLobby.Lazers.Count -1; i >= 0; i--)
            {
                var lazer = CurrentLobby.Lazers[i];
                var isNotOnBoard = lazer.Move();
                if(isNotOnBoard)
                {
                    CurrentLobby.Lazers.Remove(lazer);
                }
                bool colisionDetected = false;
                for(int k = CurrentLobby.Asteroids.Count - 1; k >= 0; k--)
                {
                    var asteroid = CurrentLobby.Asteroids[k];
                    if(asteroid.CheckBox(lazer.x, lazer.y))
                    {
                        asteroid.Damage();
                        colisionDetected = true;
                        break;
                    }
                }
                if(colisionDetected)
                { CurrentLobby.Lazers.Remove(lazer); }
            }
        }
        public async Task MovePowerups(Ship state)
        {
            for(int i = CurrentLobby.PowerUps.Count -1;i >= 0; i--)
            {
                var powerUp = CurrentLobby.PowerUps[i];
                powerUp.MoveLeft();
                if (!powerUp.CheckBoundaries())
                {
                    CurrentLobby.PowerUps.Remove(powerUp);
                    PowerUp power = new PowerUp();
                    power.PowerUpCreation(seed);
                    CurrentLobby.PowerUps.Add(power);
                    continue;
                }
                if (state.CheckBox(powerUp.X, powerUp.Y))
                {
                    state.TogglePowerup(true);
                    StartPowerupTimer(state);
                    CurrentLobby.PowerUps.Remove(powerUp);
                    
                }

            }
        }
        private void StartPowerupTimer(Ship state)
        {
            powerupTic = 0;
            powerTimer = new Timer(async (_) =>
            {
                if (!state.HasPowerup) 
                {
                    powerTimer.Dispose();
                }
                else
                {
                    await ApplyPower(state);
                }
            }, null, TimeSpan.Zero, TimeSpan.FromSeconds(1));
        }
        private async Task ApplyPower(Ship state)
        {
            if (powerupTic != 20)
            {

                state.TogglePowerup(true);
                powerupTic += 1;
            }
            else
            {
                state.TogglePowerup(false);
                powerTimer.Dispose();
            }
        }
        private async Task AddPointsToShip(int points,Ship state)
        {
            state.Points += points;
        }
        public void TalkToGateway(Lobby lobby)
        {
            try
            {
                var serializedLobby = JsonConvert.SerializeObject(lobby);


                var kp = new KeyValue
                {
                    key = lobby.Id.ToString(),
                    value = serializedLobby
                };
            }
            catch { }
        }

        public static Props Props() =>
            Akka.Actor.Props.Create(() => new LobbyActor(new LoggerFactory().CreateLogger<LobbyActor>()));
    }
}
