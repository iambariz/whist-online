import { useEffect, useState } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { getLobby } from "../api/lobbyApi";
import { startGame } from "../api/gameApi";
import type { Lobby } from "../types/game.types";

const LobbyPage = () => {
  const { id } = useParams();
  const navigate = useNavigate();
  const [lobby, setLobby] = useState<Lobby | null>(null);
  const [loading, setLoading] = useState(true);

  const myPlayerId = localStorage.getItem("wh_player_id");

  useEffect(() => {
    if (!id) return;
    let active = true;
    let timer: ReturnType<typeof setInterval>;

    const load = async () => {
      try {
        const data = await getLobby(id);
        if (!active) return;
        setLobby(data);
        setLoading(false);
      } catch {
        clearInterval(timer);
      }
    };

    load();
    timer = setInterval(load, 3000);
    return () => {
      active = false;
      clearInterval(timer);
    };
  }, [id]);

  const handleStart = async () => {
    if (!id) return;
    const game = await startGame(id);
    navigate(`/game/${game.gameId}`);
  };

  if (loading) {
    return <p className="text-center mt-16">Loading lobby…</p>;
  }

  if (!lobby) {
    return <p className="text-center mt-16">Lobby not found.</p>;
  }

  return (
    <div className="flex flex-col items-center gap-6 mt-16 px-4">
      <h2 className="text-2xl font-semibold">
        {lobby.name ? lobby.name : "Unnamed Lobby"}
      </h2>
      <p className="text-gray-400">
        {lobby.players.length}/{lobby.maxPlayers} players
      </p>

      <div className="w-full max-w-lg">
        <h3 className="text-left text-lg font-medium mb-2">Players</h3>
        <div className="flex flex-col gap-2">
          {lobby.players.map((player) => (
            <div
              key={player.id}
              className="flex justify-between items-center border rounded px-4 py-3"
            >
              <span>
                {player.name}
                {player.id === myPlayerId ? " (you)" : ""}
              </span>
              <span className="text-gray-400 text-sm">
                Seat {player.seatIndex}
              </span>
            </div>
          ))}
        </div>
      </div>

      <button
        onClick={handleStart}
        className="bg-purple-600 text-white px-6 py-2 rounded hover:bg-purple-700 w-64 cursor-pointer"
      >
        Start Game
      </button>
    </div>
  );
};

export default LobbyPage;
