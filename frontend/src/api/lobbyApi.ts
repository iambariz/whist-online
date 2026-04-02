import axios from "axios";
import type { GameState } from "../types/game.types";

const BASE_URL = import.meta.env.VITE_API_URL;

export const getLobbies = async (token: string): Promise<GameState[]> => {
  const res = await axios.get(`${BASE_URL}/lobbies`, {
    headers: { Authorization: `Bearer ${token}` },
  });
  return res.data;
}

export const createLobby = async (token: string): Promise<GameState> => {
  const res = await axios.post(`${BASE_URL}/lobbies`, {}, {
    headers: { Authorization: `Bearer ${token}` },
  });
  return res.data;
}

export const joinLobby = async (lobbyId: string, token: string): Promise<GameState> => {
  const res = await axios.post(`${BASE_URL}/lobbies/${lobbyId}/join`, {}, {
    headers: { Authorization: `Bearer ${token}` },
  });
  return res.data;
}
