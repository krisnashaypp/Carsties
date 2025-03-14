"use server"
import { fetchWrapper } from "@/lib/fetchWrapper";
import { Auction, PagedResult } from "@/types";
import {FieldValues} from "react-hook-form";
import {revalidatePath} from "next/cache";

export async function getData(query:string): Promise<PagedResult<Auction>>{
    return await fetchWrapper.get(`/search${query}`, {
        next: {revalidate: 60},
    })
}

export async function updateAuctionTest(){
    return await fetchWrapper.put(`/auctions/afbee524-5972-4075-8800-7d1f9d7b0a0c`, {})
}

export async function createAuction(data: FieldValues){
    return await fetchWrapper.post("/auctions", data)
}

export async function getDetailedViewData(id:string): Promise<Auction>{
    return await fetchWrapper.get(`/auctions/${id}`, {
        next: {revalidate: 60},
    })
}

export async function updateAuction(id: string, data: FieldValues){
    const res = await fetchWrapper.put(`/auctions/${id}`, data)
    revalidatePath(`/auctions/details/${id}`)
    return res;
}

export async function deleteAuction(id:string){
    const res =  await fetchWrapper.del(`/auctions/${id}`)
    revalidatePath('/', 'layout')
    return res;
}