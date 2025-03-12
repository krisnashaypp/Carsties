
type Props = {
    title: string
    subtitle: string
    center?: boolean
}
export default function Heading({ title, subtitel, center }: Props) {
    return (<div className={center ? 'text-center' : 'text-start'}>
        <div className="text-2xl font-bold">
            {title}
        </div>
        <div className="font-lgiht text-neutral-500 mt-2">
            {subtitel}
        </div>
    </div>)
}