import client from "./client";
import type { GameState } from "../types/game.types";

export const getGameState = async (gameId: string): Promise<GameState> => {
  const res = await client.get(`/games/${gameId}`);
  return res.data;
};

export const startGame = async (gameId: string): Promise<GameState> => {
  const res = await client.post(`/games/${gameId}/start`, {});
  return res.data;
};

export const submitBid = async (gameId: string, bid: number): Promise<GameState> => {
  const res = await client.post(`/games/${gameId}/bid`, { bid });
  return res.data;
};

export const playCard = async (gameId: string, suit: string, rank: string): Promise<GameState> => {
  const res = await client.post(`/games/${gameId}/play`, { suit, rank });
  return res.data;
};

export const getScoreBoard = async (gameId: string): Promise<GameState> => {
  const res = await client.get(`/games/${gameId}/scoreboard`);
  return res.data;
};
