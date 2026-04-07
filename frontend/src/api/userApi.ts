import axios from "axios";
import type { Player } from "../types/game.types";

const BASE_URL = import.meta.env.VITE_API_URL;

export const createPlayer = async (name: string): Promise<Player> => {
  const res = await axios.post(`${BASE_URL}/players`, { name });
  return { ...res.data.player, token: res.data.token };
}
