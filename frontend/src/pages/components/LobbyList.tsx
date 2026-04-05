import type { Lobby } from "../../types/game.types";

interface LobbyListProps {
  playerName: string;
  lobbies: Lobby[];
  joinLobby: (lobbyId: string) => void;
  createLobby: () => void;
}

const LobbyList = ({ playerName, lobbies, joinLobby, createLobby }: LobbyListProps) => {
  return (
    <div>
      <h2>Welcome, {playerName}!</h2>
      <button onClick={createLobby}>Create Lobby</button>
      <h3>Lobbies:</h3>
      <ul>
        {lobbies.map((lobby) => (
          <li key={lobby.id}>
            {lobby.name} — {lobby.players.length}/{lobby.maxPlayers} players
            <button onClick={() => joinLobby(lobby.id)}>Join</button>
          </li>
        ))}
      </ul>
    </div>
  );
};

export default LobbyList;
