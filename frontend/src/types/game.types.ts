export const GameStatus = {
  Waiting: "Waiting",
  Bidding: "Bidding",
  Playing: "Playing",
  Scoring: "Scoring",
  Finished: "Finished",
} as const;

export const TrumpSuits = {
  Clubs: "Clubs",
  Diamonds: "Diamonds",
  Hearts: "Hearts",
  Spades: "Spades",
};

export type TrumpSuit = (typeof TrumpSuits)[keyof typeof TrumpSuits];
export type GameStatus = (typeof GameStatus)[keyof typeof GameStatus];

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
  status: GameStatus;
  currentRound: number;
  totalRounds: number;
  trumpSuit: TrumpSuit | null;
  currentPlayerIndex: number;
  dealerIndex: number;
  players: PlayerSummary[];
  myHand: Card[];
}

export interface Lobby {
  id: string;
  name: string;
  maxPlayers: number;
  status: string;
  players: PlayerSummary[];
}

export interface Player {
  id: string
  name: string
  token: string
  gameId?: string | null
}

