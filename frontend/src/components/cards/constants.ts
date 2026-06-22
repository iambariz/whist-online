import type { CSSProperties } from "react";
import type { Suit, Rank } from "../../types/game.types";

export const SUIT_SYMBOLS: Record<Suit, string> = {
  Clubs: "♣",
  Diamonds: "♦",
  Hearts: "♥",
  Spades: "♠",
};

export const RANK_LABELS: Record<Rank, string> = {
  Two: "2", Three: "3", Four: "4", Five: "5", Six: "6", Seven: "7",
  Eight: "8", Nine: "9", Ten: "10", Jack: "J", Queen: "Q", King: "K", Ace: "A",
};

export const CARD_FONT = "'Patrick Hand', cursive";

export const CARD_FRAME =
  "relative w-24 h-32 bg-white border-[3px] border-black rounded-lg shadow-[2px_3px_0_0_rgba(0,0,0,0.85)] select-none";

export const CARD_BACK_PATTERN: CSSProperties = {
  backgroundColor: "#fff",
  backgroundImage:
    "repeating-linear-gradient(45deg, #b91c1c 0 2px, transparent 2px 9px), repeating-linear-gradient(-45deg, #b91c1c 0 2px, transparent 2px 9px)",
};
