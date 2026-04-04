interface LobbyListProps {
  playerName: string;
  lobbies: any[];
  joinLobby: (lobbyId: string) => void;
}

const LobbyList = ({ playerName, lobbies, joinLobby }: LobbyListProps) => {
  return (
    <div>
      <h2>Welcome, {playerName}!</h2>
      <h3>Lobbies:</h3>
      <ul>
        {lobbies.map((lobby) => (
          <li key={lobby.id}>
            {lobby.name} - 
            <button onClick={() => joinLobby(lobby.id)}>Join</button>
          </li>
        ))}
      </ul>
    </div>
  );
};

export default LobbyList;
