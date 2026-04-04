interface PlayerNameFormProps {
  playerName: string;
  setPlayerName: (name: string) => void;
  onSubmit: (event: React.SyntheticEvent<HTMLFormElement>) => void;
}

const PlayerNameForm = ({ playerName, setPlayerName, onSubmit }: PlayerNameFormProps) => {
  return (
    <form onSubmit={onSubmit}>
      <input
        type="text"
        placeholder="Your name"
        value={playerName}
        onChange={(e) => setPlayerName(e.target.value)}
      />
      <button type="submit">Register</button>
    </form>
  );
};

export default PlayerNameForm;
