"use client"

import {FieldValues, useForm} from "react-hook-form";
import {Button} from "flowbite-react";
import Input from "../components/Input";
import {useEffect} from "react";
import DateInput from "../components/DateInput";
import {createAuction, updateAuction} from "../actions/auctionActions";
import {usePathname, useRouter} from "next/navigation";
import toast from "react-hot-toast";
import {Auction} from "@/types";

type Props = {
    auction?: Auction
}
export default function AuctionForm({auction}: Props) {
    const router = useRouter();
    const pathname = usePathname();
    const {
        control,
        handleSubmit,
        setFocus,
        formState: {isSubmitting, isValid, isDirty, errors},
        reset
    } = useForm({mode: "onTouched"});

    useEffect(() => {
        if (auction) {
            const {make, model, color, mileage, year} = auction;
            reset({make, model, color, mileage, year})
        }
        setFocus("make")
    }, [setFocus])

    const onUpdate = async (data: FieldValues) => {
        if (auction) {
            const res = await updateAuction(auction.id, data)
            return
        }
    }
    const onCreate = async () => {

    }
    const onSubmit = async (data: FieldValues) => {
        try {
            const res = auction
                ? await updateAuction(auction.id, data)
                : await createAuction(data)

            if (res.error) {
                throw res.error
            }
            const id = auction
                ? auction.id
                : res.id

            router.push(`/auctions/details/${id}`)
        } catch (error: any) {
            toast.error(error.status + " " + error.message)
        }
    }

    return (<form className="flex flex-col mt-3" onSubmit={handleSubmit(onSubmit)}>
        <Input label="Make" name="make" control={control} rules={{required: "Make is required"}}/>
        <Input label="Model" name="model" control={control} rules={{required: "Model is required"}}/>
        <Input label="Color" name="color" control={control} rules={{required: "Model is required"}}/>

        <div className="grid grid-cols-2 gap-3">
            <Input label="Year" name="year" control={control} type="number" rules={{required: "Year is required"}}/>
            <Input label="Mileage" name="mileage" control={control} type="number"
                   rules={{required: "Mileage is required"}}/>

        </div>
        {pathname === "/auctions/create" && <>
            <Input label="Image URL" name="imageUrl" control={control} rules={{required: "Image URL is required"}}/>

            <div className="grid grid-cols-2 gap-3">
                <Input label="Reserve Price (enter 0 if no reserver)" name="reservePrice" control={control}
                       type="number"
                       rules={{required: "Reserve price is required"}}/>
                <DateInput
                    label="Auction end date/time"
                    name="auctionEnd"
                    dateFormat="dd MMM yyyy h:mm a"
                    showTimeSelect
                    control={control}
                    rules={{required: "Auction end date is required"}}/>

            </div>
        </>}


        <div className="flex justify-between">
            <Button outline color="gray">Cancel</Button>
            <Button
                isProcessing={isSubmitting}
                type="submit"
                disabled={!isValid}
                outline color="gray">Submit</Button>
        </div>
    </form>)
}