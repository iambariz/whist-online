import axios from "axios"
import type { GameState } from "../types/game.types"

const BASE_URL = import.meta.env.VITE_API_URL

export const getGameState = async (gameId: string, token: string):            
  Promise<GameState> => {                                                       
    const res = await axios.get(`${BASE_URL}/games/${gameId}`, {                
      headers: { Authorization: `Bearer ${token}` }                             
    })            
    return res.data
  }