import React, { useState } from "react";
import { createPlayer } from "../api/userApi";

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

  return (
    <div>
      <h1>Whist Online</h1>
      <form onSubmit={registerPlayer}>
        <input
          type="text"
          placeholder="Your name"
          value={playerName}
          onChange={(e) => setPlayerName(e.target.value)}
        />
        <button type="submit">Register</button>
      </form>

      <h2>Open Lobbies</h2>
      <ul>{/* lobby list will go here */}</ul>
    </div>
  );
};

export default HomePage;
