import client from "./client";
import type { GameState, Lobby } from "../types/game.types";

export const getLobbies = async (): Promise<Lobby[]> => {
  const res = await client.get("/lobbies");
  return res.data;
};

export const createLobby = async (): Promise<Lobby> => {
  const res = await client.post("/lobbies", {});
  return res.data;
};

export const joinLobby = async (lobbyId: string): Promise<GameState> => {
  const res = await client.post(`/lobbies/${lobbyId}/join`, {});
  return res.data;
};

export const leaveLobby = async (lobbyId: string): Promise<void> => {
  await client.post(`/lobbies/${lobbyId}/leave`, {});
};

// Dev-only: backend endpoint is gated to the Development environment.
export const addDummyPlayer = async (lobbyId: string): Promise<void> => {
  await client.post(`/lobbies/${lobbyId}/add-dummy`, {});
};
