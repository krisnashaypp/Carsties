import currencyFormatter from "@/lib/formatters"
import { Bid } from "@/types"
import { format } from "date-fns"

type Props = {
    bid: Bid
}

const getBidInfo = (bidStatus: string) => {
    let bgColor = ""
    let text = ""

    switch (bidStatus)
    {
        case "Accepted":
            bgColor = "bg-green-200";
            text = "Accepted";
            break;
        case "AcceptedBelowReserve":
            bgColor = "bg-amber-500";
            text = "Reserve not met"
            break;
        case "TooLow":
            bgColor = "bg-red-200";
            text = "Bid was too low";
            break;
        default:
            bgColor = "bg-red-200";
            text = "Bid placed after auction finished";
            break;
    }
    return {bgColor, text};
}
export default function BidItem({bid}: Props){
    
    const bidInfo = getBidInfo(bid.bidStatus);
    
    return (<div className={`
        border-gray-300 border-2 px-3 py-2 rounded-lg
        flex justify-between items-center mb-2
        ${bidInfo.bgColor}
    `}>
        <div className="flex flex-col">
            <span>Bidder: {bid.bidder}</span>
            <span className="text-gray-700 text-sm">Time: {format(new Date(bid.bidTime), "dd MMM yyyy h:mm a")}</span>
        </div>
        <div className="flex flex-col text-right">
            <div className="text-xl font-semibold">
                {currencyFormatter.format(bid.amount)}
            </div>
            <div className="flex flex-row items-center">
                <span>{bidInfo.text}</span>
            </div>
        </div>
    </div>)
}