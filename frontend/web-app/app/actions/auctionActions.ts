"use server"

import { Auction, PagedResult } from "@/types";

export async function getData(query:string): Promise<PagedResult<Auction>>{
    const res = await fetch(`${process.env["services__gatewayservice__http__0"]}/search${query}`, {
        next: {revalidate: 60}
    })

    if (!res.ok) throw new Error("Failed to fetch data");

    return res.json();
}