import type { Suit, Rank } from "../../types/game.types";
import { SUIT_SYMBOLS, RANK_LABELS, CARD_FONT, CARD_FRAME } from "./constants";
import CardIndex from "./CardIndex";
import CardCenter from "./CardCenter";
import CardBack from "./CardBack";

type CardProps =
  | { faceDown: true; suit?: Suit; rank?: Rank }
  | { faceDown?: false; suit: Suit; rank: Rank; interactive?: boolean };

const Card = (props: CardProps) => {
  if (props.faceDown) return <CardBack />;

  const { suit, rank, interactive = false } = props;
  const symbol = SUIT_SYMBOLS[suit];
  const label = RANK_LABELS[rank];
  const isRed = suit === "Hearts" || suit === "Diamonds";
  const color = isRed ? "text-red-600" : "text-neutral-900";
  const cursor = interactive ? "cursor-pointer" : "";

  return (
    <div
      className={`${CARD_FRAME} ${color} ${cursor}`}
      style={{ fontFamily: CARD_FONT }}
    >
      <div className="absolute top-1 left-2">
        <CardIndex label={label} symbol={symbol} />
      </div>

      <div className="absolute inset-0 flex items-center justify-center">
        <CardCenter symbol={symbol} />
      </div>

      <div className="absolute bottom-1 right-2">
        <CardIndex label={label} symbol={symbol} flipped />
      </div>
    </div>
  );
};

export default Card;
