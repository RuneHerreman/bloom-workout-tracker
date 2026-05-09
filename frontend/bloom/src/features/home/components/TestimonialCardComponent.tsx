interface Props{
    name: string;
    experience: string;
    rating: number;
    review: string;
}

function TestimonialCardComponent({ name, experience, rating, review }: Props) {
    return (
        <article>
            <div>
                <h3>{ name }</h3>
                <p>{ experience }</p>
            </div>
            <p>{review}</p>
            <div>
                {Array.from({ length: rating }, (_, i) => (
                    <img key={i} className={"bloom-bullet"} src="/media/bloom_bullet.svg" alt=""/>
                ))}
                {Array.from({ length: 5 - rating }, (_, i) => (
                    <img key={i} className={"bloom-bullet bloom-bullet-grey"} src="/media/bloom_bullet.svg" alt=""/>
                ))}
            </div>
        </article>
    );
}

export default TestimonialCardComponent;
