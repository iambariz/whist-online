import type { Lobby } from "../../types/game.types";

interface LobbyListProps {
  playerName: string;
  lobbies: Lobby[];
  joinLobby: (lobbyId: string) => void;
  createLobby: () => void;
}

const LobbyList = ({ playerName, lobbies, joinLobby, createLobby }: LobbyListProps) => {
  return (
    <div className="flex flex-col items-center gap-6 mt-16 px-4">
      <h2 className="text-2xl font-semibold">
        Welcome, {playerName ? playerName : "Player"}!
      </h2>
      <button
        onClick={createLobby}
        className="bg-purple-600 text-white px-6 py-2 rounded hover:bg-purple-700 w-64 cursor-pointer"
      >
        Create Lobby
      </button>
      <div className="w-full max-w-lg">
        <h3 className="text-left text-lg font-medium mb-2">Open Lobbies</h3>
        {lobbies.length === 0 ? (
          <p className="text-gray-400">No open lobbies. Create one!</p>
        ) : (
          <div className="flex flex-col gap-2">
            {lobbies.map((lobby) => (
              <div
                key={lobby.id}
                className="flex justify-between items-center border rounded px-4 py-3"
              >
                <span>
                  {lobby.name ? lobby.name : "Unnamed Lobby"} -{" "}
                  {lobby.players.length}/{lobby.maxPlayers} players
                </span>
                <button
                  onClick={() => joinLobby(lobby.id)}
                  className="bg-purple-600 text-white px-4 py-1 rounded hover:bg-purple-700 text-sm cursor-pointer"
                >
                  Join
                </button>
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  );
};

export default LobbyList;
