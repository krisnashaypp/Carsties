"use client"

import {Dropdown, DropdownDivider, DropdownItem} from "flowbite-react";
import Link from "next/link";
import {User} from "next-auth";
import {usePathname, useRouter} from "next/navigation";
import {HiCog, HiUser} from "react-icons/hi";
import {AiFillCar, AiFillTrophy, AiOutlineLogout} from "react-icons/ai";
import { signOut } from "next-auth/react";
import { useParamsStore } from "@/hooks/useParamsStore";

type Props = {
    user: User
}

export default function UserActions({user}: Props){
    const router = useRouter();
    const pathname = usePathname();
    const setParams = useParamsStore(state => state.setParams)
    
    const setWinner = () => {
        setParams({winner: user.username, seller: undefined})
        if (pathname !== "/") router.push("/")
    }

    const setSeller = () => {
        setParams({winner: undefined, seller: user.username})
        if (pathname !== "/") router.push("/")
    }
    
    return (<Dropdown inline label={`Welcome ${user.name}`}>
        <DropdownItem icon={HiUser} onClick={setSeller}>
            My Auctions
        </DropdownItem>
        <DropdownItem icon={AiFillTrophy} onClick={setWinner}>
            Auctions won
        </DropdownItem>
        <DropdownItem icon={AiFillCar}>
            <Link href="/auctions/create">
                Sell my car</Link>
        </DropdownItem>        
        <DropdownItem icon={HiCog}>
            <Link href="/session">
                Session (dev)</Link>
        </DropdownItem> 
        <DropdownDivider/>
        <DropdownItem icon={AiOutlineLogout} onClick={() => signOut({redirectTo: "/"})}>
            Sign out
        </DropdownItem>
    </Dropdown>)
}