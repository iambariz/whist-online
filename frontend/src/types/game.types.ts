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
  status: string;
  currentRound: number;
  totalRounds: number;
  trumpSuit: string;
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
