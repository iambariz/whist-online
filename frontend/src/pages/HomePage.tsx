import React, { useState, useEffect } from "react";
import { createPlayer } from "../api/userApi";
import PlayerNameForm from "./components/PlayerNameForm";
import LobbyList from "./components/LobbyList";

const HomePage = () => {
  const [playerName, setPlayerName] = useState(
    () => localStorage.getItem("wh_player_name") ?? "",
  );
  const [lobbies, setLobbies] = useState([]);
  const [playerToken, setPlayerToken] = useState(
    () => localStorage.getItem("wh_token") ?? "",
  );

  useEffect(() => {}, []);

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

  const joinLobby = async (lobbyId: string) => {
    console.log(`Joining lobby ${lobbyId} with token ${playerToken}`);
    // After joining, you might want to fetch the updated list of lobbies or navigate to the lobby page
  };

  return (
    <div>
      <h1>Whist Online</h1>
      {playerToken ? (
        <LobbyList
          playerName={playerName}
          lobbies={lobbies}
          joinLobby={joinLobby}
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
