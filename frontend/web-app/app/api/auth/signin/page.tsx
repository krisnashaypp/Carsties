import EmptyFilter from "@/app/components/EmptyFilter";

export default function SignIn({searchParams}: {searchParams: {callbackUrl: string}}){
    
    return (<EmptyFilter
        title="You need to belogged in to do that"
        subtitle="Please click below to login"
        showLogin
        callbackUrl={searchParams.callbackUrl}
    />)
}