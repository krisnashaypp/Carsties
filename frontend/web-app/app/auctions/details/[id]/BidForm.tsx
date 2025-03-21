"use client"

import { placeBidForAuction } from "@/app/actions/auctionActions"
import { useBidStore } from "@/hooks/useBidStore"
import currencyFormatter from "@/lib/formatters"
import {FieldValues, useForm} from "react-hook-form"
import toast from "react-hot-toast";

type Props = {
    auctionId: string
    highBid: number
    
}
export default function BidForm({auctionId, highBid}: Props){
    const {register, handleSubmit, reset, formState: {errors}} = useForm();
    const addBid = useBidStore(state => state.addBid)
    
    const onSubmit = async (data: FieldValues) => {
        if(data.amount <= highBid){
            reset()
            return toast.error("Bid must be at least " + currencyFormatter.format(highBid+1))
        }
        
        try {
            const response = await placeBidForAuction(auctionId, +data.amount)
            if(response.error) throw response.error
            addBid(response)
            reset();
        } catch (e: any) {
            toast.error(e.message)
        }
    }
    
    return (<form onSubmit={handleSubmit(onSubmit)} className="flex items-center border-2 rounded-lg py-2">
        <input type="number" {...register("amount")} className="input-custom text-sm text-gray-600" placeholder={`Enter your bid (minimum bid is ${currencyFormatter.format(highBid+1)})`}/>
    </form>)
}