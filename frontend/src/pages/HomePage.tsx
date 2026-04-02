 const HomePage = () => {
    return (
      <div>
        <h1>Whist Online</h1>

        <input type="text" placeholder="Your name" />
        <button>Create Lobby</button>

        <h2>Open Lobbies</h2>
        <ul>
          {/* lobby list will go here */}
        </ul>
      </div>
    )
  }


export default HomePage;