"use client"

import {useState} from "react";
import {updateAuctionTest} from "../actions/auctionActions";
import { Button } from "flowbite-react";


export default function AuthTest() {
    const [loading, setLoading] = useState(false);
    const [result, setResult] = useState<any>();

    const doUpdate = async () => {
        setResult(undefined)
        setLoading(true)

        try {
            const update = await updateAuctionTest()
            setResult(update)
        } catch (e) {
            setResult(e)
        } finally {
            setLoading(false)
        }
    }
    
    return (<div className="flex items-center gap-4">
        <Button outline isProcessing={loading} onClick={doUpdate}>
            Test auth
        </Button>
        <div>
            {JSON.stringify(result, null, 2)}
        </div>
    </div>)
}