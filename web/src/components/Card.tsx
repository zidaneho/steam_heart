interface CardProps {
    name : string;
    price : string;
    
}
function Card(props : CardProps) {
    return <div className = "flex flex-col">
        <p>{props.name}</p>
        <p>{props.price}</p>
    </div>
}
export default Card;