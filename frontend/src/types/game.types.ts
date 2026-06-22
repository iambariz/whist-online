export const GameStatus = {
  Waiting: "Waiting",
  Bidding: "Bidding",
  Playing: "Playing",
  Scoring: "Scoring",
  Finished: "Finished",
} as const;

export const Suits = {
  Clubs: "Clubs",
  Diamonds: "Diamonds",
  Hearts: "Hearts",
  Spades: "Spades",
} as const;

export const Ranks = {
  Two: "Two",
  Three: "Three",
  Four: "Four",
  Five: "Five",
  Six: "Six",
  Seven: "Seven",
  Eight: "Eight",
  Nine: "Nine",
  Ten: "Ten",
  Jack: "Jack",
  Queen: "Queen",
  King: "King",
  Ace: "Ace",
} as const;

export type Suit = (typeof Suits)[keyof typeof Suits];
export type Rank = (typeof Ranks)[keyof typeof Ranks];
export type GameStatus = (typeof GameStatus)[keyof typeof GameStatus];

export interface Card {
  suit: Suit;
  rank: Rank;
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
  hostPlayerId: string | null;
  status: GameStatus;
  currentRound: number;
  totalRounds: number;
  trumpSuit: Suit | null;
  currentPlayerIndex: number;
  dealerIndex: number;
  minPlayers: number;
  maxPlayers: number;
  players: PlayerSummary[];
  myHand: Card[];
}

export interface Lobby {
  id: string;
  name: string;
  maxPlayers: number;
  status: GameStatus;
  players: PlayerSummary[];
}

export interface Player {
  id: string
  name: string
  token: string
  gameId?: string | null
}

