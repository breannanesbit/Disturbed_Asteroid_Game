using Actors.UserActors;

namespace Shared
{
    // Define a scoped service to hold the lobby data
    public class LobbyDataService
    {
        private Dictionary<Guid, GameLobby> lobbyDataDictionary = new Dictionary<Guid, GameLobby>();

        public void SetLobbyData(Guid lobbyId, GameLobby lobbyData)
        {
            lobbyDataDictionary[lobbyId] = lobbyData;
        }

        public GameLobby GetLobbyData(Guid lobbyId)
        {
            if (lobbyDataDictionary.ContainsKey(lobbyId))
            {
                return lobbyDataDictionary[lobbyId];
            }
            return null;
        }
    }

}
