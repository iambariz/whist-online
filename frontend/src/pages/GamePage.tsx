import { useEffect, useState } from "react";
import { useParams } from "react-router-dom";
import { getGameState } from "../api/gameApi";
import { createGameConnection } from "../api/gameHub";
import type { GameState } from "../types/game.types";

const GamePage = () => {
  const { id } = useParams();
  const [game, setGame] = useState<GameState | null>(null);

  useEffect(() => {
    if (!id) return;

    getGameState(id).then(setGame);

    const token = localStorage.getItem("wh_token");
    if (!token) {
      console.error("No token found in localStorage");
      return;
    }

    const connection = createGameConnection(id, token);
    connection.on("GameUpdated", () => getGameState(id).then(setGame));

    let cancelled = false;
    const startPromise = connection
      .start()
      .then(() => {
        if (!cancelled) connection.invoke("SubscribeToGame", id);
      })
      .catch((err) => {
        if (!cancelled) console.error("Hub connection failed:", err);
      });

    return () => {
      cancelled = true;
      startPromise.then(() => connection.stop());
    };
  }, [id]);

  return (
    <div className="game-page">
      <h1>Welcome to Game Page!</h1>
    </div>
  );
};

export default GamePage;
