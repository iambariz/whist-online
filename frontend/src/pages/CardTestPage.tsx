import { Fragment } from "react";
import Card from "../components/cards";
import { Suits, Ranks } from "../types/game.types";

const CardTestPage = () => {
  return (
    <div className="p-6 flex flex-col gap-6 text-left">
      <h1 className="text-2xl font-semibold">Card preview</h1>
      <div className="flex flex-wrap gap-3">
        {Object.values(Suits).map((suit) => (
          <Fragment key={suit}>
            <div className="basis-full text-sm font-semibold text-neutral-500">
              {suit}
            </div>
            {Object.values(Ranks).map((rank) => (
              <Card key={`${suit}-${rank}`} suit={suit} rank={rank} />
            ))}
          </Fragment>
        ))}
      </div>

      <div className="flex flex-wrap gap-3">
        <div className="basis-full text-sm font-semibold text-neutral-500">
          Back
        </div>
        <Card faceDown />
        <Card faceDown />
        <Card faceDown />
      </div>
    </div>
  );
};

export default CardTestPage;
