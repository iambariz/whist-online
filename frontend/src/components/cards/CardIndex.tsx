// The rank-over-suit index shown in a corner (e.g. "9♣"). `flipped` rotates it
// 180° for the mirrored bottom-right corner.
interface CardIndexProps {
  label: string;
  symbol: string;
  flipped?: boolean;
}

const CardIndex = ({ label, symbol, flipped = false }: CardIndexProps) => (
  <div className={`leading-none text-center ${flipped ? "rotate-180" : ""}`}>
    <div className="text-2xl">{label}</div>
    <div className="text-xl -mt-1">{symbol}</div>
  </div>
);

export default CardIndex;
