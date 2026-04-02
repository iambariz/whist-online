import axios from "axios";
import type { GameState } from "../types/game.types";

const BASE_URL = import.meta.env.VITE_API_URL;

export const getGameState = async (
  gameId: string,
  token: string,
): Promise<GameState> => {
  const res = await axios.get(`${BASE_URL}/games/${gameId}`, {
    headers: { Authorization: `Bearer ${token}` },
  });
  return res.data;
};

export const startGame = async (
  gameId: string,
  token: string,
): Promise<GameState> => {
  const res = await axios.post(
    `${BASE_URL}/games/${gameId}/start`,
    {},
    {
      headers: { Authorization: `Bearer ${token}` },
    },
  );
  return res.data;
};

export const submitBid = async (
  gameId: string,
  token: string,
  bid: number,
): Promise<GameState> => {
  const res = await axios.post(
    `${BASE_URL}/games/${gameId}/bid`,
    { bid },
    {
      headers: { Authorization: `Bearer ${token}` },
    },
  );
  return res.data;
};

export const playCard = async (
  gameId: string,
  suit: string,
  rank: string,
  token: string,
): Promise<GameState> => {
  const res = await axios.post(
    `${BASE_URL}/games/${gameId}/play`,
    { suit, rank },
    {
      headers: { Authorization: `Bearer ${token}` },
    },
  );
  return res.data;
};

export const getScoreBoard = async (
  gameId: string,
  token: string,
): Promise<GameState> => {
  const res = await axios.get(`${BASE_URL}/games/${gameId}/scoreboard`, {
    headers: { Authorization: `Bearer ${token}` },
  });
  return res.data;
};