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
        private int asteroidTic = 0;
        private int powerupTic = 0;
        private int seed = Guid.NewGuid().GetHashCode();
        private UpdateGame gameUpdate = new UpdateGame();
        private bool isLooping = false;
        //if poweruptic is 0 then powerup can be made else none can be created.

        private readonly HttpClient _httpClient;
        private readonly ILogger<LobbyActor> logger;
        private ICancelable gameLoopCancel;

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
                var player = CurrentLobby.Players.Find(p => p.Username == moveEvent.user.Username);
                if(player == null)
                {
                    player.Ship = ship;
                }
                Sender.Tell(CurrentLobby);

            });
            Receive<Lazer>((lazer) =>
            {
                CurrentLobby.Lazers.Add(lazer);
            });
            Receive<ShipUpdate>((ship) =>
            {
                var user = CurrentLobby.Players.Find(g => g.Username == ship.user.Username);
                user.Ship = ship.user.Ship;
                Console.WriteLine($"Ship color is {user.Ship.ShipColor} and image is {user.Ship.ShipImage}");
                Context.Parent.Tell(CurrentLobby);
            });
            Receive<ChangeGameState>(async state =>
            {
                Console.WriteLine($"Made it to the lobby");
                Console.WriteLine($"The User is {state.user}");
                if (CurrentLobby.HeadPlayer.Username == state.user)
                {
                    logger.LogInformation($"Changing {state.lobbyId} state to {state.lobbyState}");
                    //CurrentLobby = state.lobby;
                    CurrentLobby.CurrentState = state.lobbyState;

                    

                    /*timer = new Timer(async (_) =>
                    {
                        Console.WriteLine("Anotehr iteration of the game timer");
                        await UpdateGame();
                    }, null, TimeSpan.Zero, TimeSpan.FromSeconds(0.05));*/
                    await UpdateGame();
                    if(!isLooping && CurrentLobby.CurrentState == GameState.Playing) //so it only gets called once.
                    {
                        gameUpdate.lobby = CurrentLobby;
                        Self.Tell(gameUpdate);// call update game
                    }
                }
                if(CurrentLobby.CurrentState == GameState.Over)
                {
                    //add game points to users points
                    gameLoopCancel.Cancel();
                    CurrentLobby.HeadPlayer.Points += CurrentLobby.HeadPlayer.Ship.Points;
                    Context.Parent.Tell(CurrentLobby.CurrentState);
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

            Receive<UpdateGame>(game =>
            {
                isLooping = true;
                //every time the timespan goes off call updategame method
                gameLoopCancel = Context.System.Scheduler.ScheduleTellRepeatedlyCancelable(
                    TimeSpan.FromSeconds(0),
                    TimeSpan.FromSeconds(0.5),
                    Self,
                    new ChangeGameState {
                        user = CurrentLobby.HeadPlayer.Username,
                        lobbyState = CurrentLobby.CurrentState,
                        lobbyId = CurrentLobby.Id,
                        lobby = CurrentLobby
                    },
                    Self);
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
        public async Task UpdateGame()
        {
            if(powerupTic == 0 && CurrentLobby.PowerUps.Count == 0)
            {
                AddPowerUp(seed);
            }
            if (asteroidTic == 60)
            {
                Console.WriteLine("another iteration of the asteroid timer");
                await AddAsteroid();
                Console.WriteLine("Made it back to asteroids timer");
            }
            if (CurrentLobby != null && CurrentLobby.Asteroids.Count != 0)
            {
                Console.WriteLine("Moving asteroids");
                await MoveAsteroids(CurrentLobby.HeadPlayer.Ship);
                Console.WriteLine("Moved asteroids");
            }
            if (CurrentLobby.Lazers.Count != 0)
            {
                Console.WriteLine("Moving Lazers");
                await MoveLazers();
            }
            if (CurrentLobby.PowerUps.Count != 0)
            {
                Console.WriteLine("Moving Powerups");
                await MovePowerups(CurrentLobby.HeadPlayer.Ship);
                Console.WriteLine("Move powerups");
            }
            Console.WriteLine("Sending current state to sender");
            asteroidTic += 1;
            Context.Parent.Tell(CurrentLobby);
        }
        public async Task AddAsteroid()// could input how many should be made and loop over.
        {
            int seed = Guid.NewGuid().GetHashCode();
            Asteroid NewAsteroid = new Asteroid();
            NewAsteroid.RandomCreation(seed);
            Console.WriteLine("Adding a new Asteroid");
            CurrentLobby.Asteroids.Add(NewAsteroid);
            Console.WriteLine("Was able to add asteroid");
        }
        public async Task AddLazer(int x, int y, int angle)
        {
            //check how many lazers each user has and that it doesn't go over 7
            Lazer NewLazer = new Lazer
            {
                x= x, y = y, Angle = angle
            };
            Console.WriteLine("Adding a new Lazer");

            CurrentLobby.Lazers.Add(NewLazer);
            
        }
        public async Task AddPowerUp(int seed)
        {
            PowerUp powerup = new PowerUp();
            powerup.PowerUpCreation(seed);
            Console.WriteLine("Adding a new Powerup");

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
                AddPowerUp(seed);
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
