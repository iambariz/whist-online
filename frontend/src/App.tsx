import { BrowserRouter, Routes, Route } from "react-router-dom";
import HomePage from "./pages/HomePage";
import LobbyPage from "./pages/LobbyPage";
import GamePage from "./pages/GamePage";
import CardTestPage from "./pages/CardTestPage";
import { ErrorProvider } from "./context/ErrorContext";
import ErrorBanner from "./components/ErrorBanner";

import "./App.css";

function App() {
  return (
    <ErrorProvider>
      <ErrorBanner />
      <BrowserRouter>
        <Routes>
          <Route path="/" element={<HomePage />} />
          <Route path="/lobby/:id" element={<LobbyPage />} />
          <Route path="/game/:id" element={<GamePage />} />
          <Route path="/cards" element={<CardTestPage />} />
        </Routes>
      </BrowserRouter>
    </ErrorProvider>
  );
}

export default App;
