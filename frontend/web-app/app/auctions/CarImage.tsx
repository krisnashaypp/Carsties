"use client"
import { Auction } from "@/types";
import Image from "next/image";
import { useState } from "react";

type Props = Pick<Auction, "imageUrl" | "make">
export default function CarImage({imageUrl, make}: Props) {
    const [isLoading, setIsLoading] = useState<boolean>(true)
    
    return <Image
        src={imageUrl}
        alt={make}
        fill
        className={`
            object-cover group-hover:opacity-75 duration-700 ease-in-out
            ${isLoading ? 'grayscale blur-2xl scale-110' : 'grayscale-0 blur-0 scale-100'}
        `}
        priority
        sizes='(max-width: 768px) 100vw, (max-width: 1200px) 50vw, 25vw'
        onLoad={() => setIsLoading(false)}
    />
}