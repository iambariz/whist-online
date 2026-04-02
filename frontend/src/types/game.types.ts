export interface Card {
  suit: string;
  rank: string;
}

export interface PlayerSummary {
  id: string;
  name: string;
  seatIndex: number;
  cardCount: number;
  score: number;
}

export interface GameState {
  gameId: string;
  status: "Waiting" | "Bidding" | "Playing" | "Finished";
  currentRound: number;
  totalRounds: number;
  trumpSuit: "Clubs" | "Diamonds" | "Hearts" | "Spades" | null;
  currentPlayerIndex: number;
  dealerIndex: number;
  players: PlayerSummary[];
  myHand: Card[];
}

export interface Lobby {
  id: string;
  status: string;
  players: PlayerSummary[];
}

export interface Player {
  id: string
  name: string
  token: string
}

