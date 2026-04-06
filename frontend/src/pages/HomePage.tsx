import React, { useState, useEffect } from "react";
import { createPlayer } from "../api/userApi";
import { getLobbies, createLobby } from "../api/lobbyApi";
import PlayerNameForm from "./components/PlayerNameForm";
import LobbyList from "./components/LobbyList";
import type { Lobby } from "../types/game.types";

const HomePage = () => {
  const [playerName, setPlayerName] = useState(
    () => localStorage.getItem("wh_player_name") ?? "",
  );
  const [lobbies, setLobbies] = useState<Lobby[]>([]);
  const [playerToken, setPlayerToken] = useState(
    () => localStorage.getItem("wh_token") ?? "",
  );

  useEffect(() => {
    const fetchLobbies = async () => {
      const lobbies = await getLobbies();
      setLobbies(lobbies);
    };
    fetchLobbies();
  }, []);

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
    await createLobby(playerToken);
  };

  const joinLobby = async (lobbyId: string) => {
    console.log(`Joining lobby ${lobbyId} with token ${playerToken}`);
  };

  return (
    <div>
      <h1>Whist Online</h1>
      {playerToken ? (
        <LobbyList
          playerName={playerName}
          lobbies={lobbies}
          joinLobby={joinLobby}
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
