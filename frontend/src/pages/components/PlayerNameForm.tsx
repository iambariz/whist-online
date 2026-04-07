interface PlayerNameFormProps {
  playerName: string;
  setPlayerName: (name: string) => void;
  onSubmit: (event: React.SyntheticEvent<HTMLFormElement>) => void;
}

const PlayerNameForm = ({ playerName, setPlayerName, onSubmit }: PlayerNameFormProps) => {
  return (
    <div className="flex flex-col items-center gap-4 mt-32">
      <h2 className="text-2xl font-semibold">Enter your name to play</h2>
      <form onSubmit={onSubmit} className="flex flex-col items-center gap-3">
        <input
          type="text"
          placeholder="Your name"
          value={playerName}
          onChange={(e) => setPlayerName(e.target.value)}
          className="border border-gray-300 rounded px-4 py-2 text-lg w-64 focus:outline-none focus:ring-2 focus:ring-purple-500"
        />
        <button
          type="submit"
          className="bg-purple-600 text-white px-6 py-2 rounded hover:bg-purple-700 w-64 cursor-pointer"
        >
          Let's play
        </button>
      </form>
    </div>
  );
};

export default PlayerNameForm;
