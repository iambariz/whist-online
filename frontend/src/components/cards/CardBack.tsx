import { CARD_FRAME, CARD_FONT, CARD_BACK_PATTERN } from "./constants";

const CardBack = () => (
  <div className={`${CARD_FRAME} p-1.5`}>
    <div
      className="absolute inset-1.5 rounded border-2 border-red-700"
      style={CARD_BACK_PATTERN}
    />
    <div
      className="absolute inset-0 flex items-center justify-center text-2xl text-red-700"
      style={{ fontFamily: CARD_FONT }}
    >
      <span className="bg-white px-1 rounded">♠</span>
    </div>
  </div>
);

export default CardBack;
