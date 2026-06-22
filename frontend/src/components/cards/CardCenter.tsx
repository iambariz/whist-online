// The big suit symbol in the middle of the card.
interface CardCenterProps {
  symbol: string;
}

const CardCenter = ({ symbol }: CardCenterProps) => (
  <div className="text-5xl">{symbol}</div>
);

export default CardCenter;
