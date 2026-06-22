import { useEffect, useState, useCallback } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { getGameState, startGame } from "../api/gameApi";
import { leaveLobby } from "../api/lobbyApi";
import { GameStatus } from "../types/game.types";
import type { GameState } from "../types/game.types";

const POLL_INTERVAL_MS = 2000;

const LobbyPage = () => {
  const { id } = useParams();
  const navigate = useNavigate();
  const [game, setGame] = useState<GameState | null>(null);

  const myPlayerId = localStorage.getItem("wh_player_id");

  const refresh = useCallback(async () => {
    if (!id) return;
    try {
      const state = await getGameState(id);
      if (state.status !== GameStatus.Waiting) {
        navigate(`/game/${id}`);
        return;
      }
      setGame(state);
    } catch {
      navigate("/");
    }
  }, [id, navigate]);

  useEffect(() => {
    refresh();
    const timer = setInterval(refresh, POLL_INTERVAL_MS);
    return () => clearInterval(timer);
  }, [refresh]);

  const handleStart = async () => {
    if (!id) return;
    await startGame(id);
    navigate(`/game/${id}`);
  };

  const handleLeave = async () => {
    if (!id) return;
    await leaveLobby(id);
    navigate("/");
  };

  if (!game) return <p className="text-center mt-16">Loading lobby…</p>;

  const isHost = game.hostPlayerId === myPlayerId;
  const enoughPlayers = game.players.length >= game.minPlayers;
  const sortedPlayers = [...game.players].sort(
    (a, b) => a.seatIndex - b.seatIndex,
  );

  return (
    <div className="flex flex-col items-center gap-6 mt-16 px-4">
      <h2 className="text-2xl font-semibold">Waiting Room</h2>
      <p className="text-gray-400">
        {game.players.length}/{game.maxPlayers} players
      </p>

      <div className="w-full max-w-lg">
        <h3 className="text-left text-lg font-medium mb-2">Players</h3>
        <div className="flex flex-col gap-2">
          {sortedPlayers.map((player) => (
            <div
              key={player.id}
              className="flex justify-between items-center border rounded px-4 py-3"
            >
              <span>
                {player.name}
                {player.id === game.hostPlayerId && (
                  <span className="ml-2 text-sm text-purple-400">(host)</span>
                )}
                {player.id === myPlayerId && (
                  <span className="ml-2 text-sm text-gray-400">(you)</span>
                )}
              </span>
              <span className="text-gray-400 text-sm">
                Seat {player.seatIndex}
              </span>
            </div>
          ))}
        </div>
      </div>

      <div className="flex gap-4">
        {isHost && (
          <button
            onClick={handleStart}
            disabled={!enoughPlayers}
            className="bg-purple-600 text-white px-6 py-2 rounded hover:bg-purple-700 cursor-pointer disabled:opacity-40 disabled:cursor-not-allowed"
          >
            Start Game
          </button>
        )}
        <button
          onClick={handleLeave}
          className="bg-gray-600 text-white px-6 py-2 rounded hover:bg-gray-700 cursor-pointer"
        >
          Leave
        </button>
      </div>

      {isHost && !enoughPlayers && (
        <p className="text-gray-400 text-sm">
          Need at least {game.minPlayers} players to start.
        </p>
      )}
    </div>
  );
};

export default LobbyPage;
