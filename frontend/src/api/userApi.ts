import client from "./client";
import type { Player } from "../types/game.types";

export const createPlayer = async (name: string): Promise<Player> => {
  const res = await client.post("/players", { name });
  return { ...res.data.player, token: res.data.token };
};
