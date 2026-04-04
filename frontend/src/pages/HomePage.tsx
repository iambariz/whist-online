import React, { useState } from "react";
import { createPlayer } from "../api/userApi";
import PlayerNameForm from "./components/PlayerNameForm";
import LobbyList from "./components/LobbyList";

const HomePage = () => {
  const [playerName, setPlayerName] = useState("");
  const [lobbies, setLobbies] = useState([]);
  const [playerToken, setPlayerToken] = useState("");

  const registerPlayer = async (
    event: React.SyntheticEvent<HTMLFormElement>,
  ) => {
    event.preventDefault();
    const player = await createPlayer(playerName);
    setPlayerToken(player.token);
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
