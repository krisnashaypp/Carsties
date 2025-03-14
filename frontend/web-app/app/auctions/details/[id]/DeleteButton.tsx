"use client"

import {useState} from "react";
import {Button} from "flowbite-react";
import {useRouter} from "next/navigation";
import { deleteAuction } from "@/app/actions/auctionActions";
import toast from "react-hot-toast";

export default function DeleteButton({ id }: { id: string }) {
    const [loading, setLoading] = useState(false);
    const router = useRouter();
    
    
    const doDelete = async () => {
        setLoading(true);
        try{
            await deleteAuction(id)
            router.push("/")
        }
        catch(error: any){
            toast.error(error.status + " " + error.message);
        }
        finally {
            setLoading(false);
        }
        
    }
    
    return (<Button color="failure" isProcessing={loading} onClick={doDelete}>
        Delete Auction
    </Button>)
}