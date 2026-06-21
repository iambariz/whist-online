import React, { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import { createPlayer, getMe } from "../api/userApi";
import { getLobbies, createLobby, joinLobby } from "../api/lobbyApi";
import { getGameState } from "../api/gameApi";
import PlayerNameForm from "./components/PlayerNameForm";
import LobbyList from "./components/LobbyList";
import { GameStatus } from "../types/game.types";
import type { Lobby } from "../types/game.types";

const HomePage = () => {
  const navigate = useNavigate();
  const [playerName, setPlayerName] = useState(
    () => localStorage.getItem("wh_player_name") ?? "",
  );
  const [lobbies, setLobbies] = useState<Lobby[]>([]);
  const [playerToken, setPlayerToken] = useState(
    () => localStorage.getItem("wh_token") ?? "",
  );
  const [loading, setLoading] = useState(() =>
    Boolean(localStorage.getItem("wh_token")),
  );

  useEffect(() => {
    const init = async () => {
      try {
        if (playerToken) {
          const me = await getMe();
          if (me.gameId) {
            const game = await getGameState(me.gameId);
            navigate(
              game.status === GameStatus.Waiting
                ? `/lobby/${me.gameId}`
                : `/game/${me.gameId}`,
            );
            return;
          }
        }
        setLobbies(await getLobbies());
        setLoading(false);
      } catch {
        setLoading(false);
      }
    };
    init();
  }, [playerToken, navigate]);

  const registerPlayer = async (
    event: React.SyntheticEvent<HTMLFormElement>,
  ) => {
    event.preventDefault();
    const player = await createPlayer(playerName);
    setPlayerToken(player.token);
    localStorage.setItem("wh_token", player.token);
    localStorage.setItem("wh_player_id", player.id);
    localStorage.setItem("wh_player_name", player.name);
  };

  const handleCreateLobby = async () => {
    const lobby = await createLobby();
    navigate(`/lobby/${lobby.id}`);
  };

  const handleJoinLobby = async (lobbyId: string) => {
    await joinLobby(lobbyId);
    navigate(`/lobby/${lobbyId}`);
  };

  if (loading) return <p>Loading…</p>;

  return (
    <div>
      <h1>Nomination Whist Online</h1>
      {playerToken ? (
        <LobbyList
          playerName={playerName}
          lobbies={lobbies}
          joinLobby={handleJoinLobby}
          createLobby={handleCreateLobby}
        />
      ) : (
        <PlayerNameForm
          playerName={playerName}
          setPlayerName={setPlayerName}
          onSubmit={registerPlayer}
        />
      )}
    </div>
  );
};

export default HomePage;
