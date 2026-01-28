import TestimonialCardComponent from "./TestimonialCardComponent.tsx";

function FooterComponent() {

    const data = [
        {
            name: "Emma Vandenberghe",
            experience: "2 years",
            rating: 4,
            review: "Bloom replaced 3 apps for me. Templates save hours weekly, and the per-set RIR logging is insanely detailed. Progress charts showed my bench plateau, fixed it in 3 weeks."
        },
        {
            name: "Ilias Claeys",
            experience: "1 year",
            rating: 5,
            review: "Finally a tracker that handles both squats and 10K runs without switching apps. Free? Sold. The volume analytics are better than my paid coach"
        },
        {
            name: "Noah Vandewalle Chang",
            experience: "6 months",
            rating: 4,
            review: "Cardio distance and strength sets in one place is a game changer. Wish it had photo body progress, but logging is flawless for CrossFit WODs"
        },
        {
            name: "Leonard Minjauw",
            experience: "1 year",
            rating: 5,
            review: "Logged 500+ sessions. 1RM estimates match my actual maxes perfectly. No ads, no subscriptions, cleaner than Strong or Hevy."
        },
        {
            name: "Thibo Verhoye",
            experience: "9 months",
            rating: 5,
            review: "Home gym + trail running covered perfectly. RIR tracking keeps me from overtraining. Graphs spotted my weak rear delts, now fixed."
        },
        {
            name: "Mathieu Pinsart",
            experience: "3 months",
            rating: 4,
            review: "Push/pull/legs templates are dead simple to reuse. Wish mobile had barcode scanner for equipment, but volume tracking is elite."
        }
    ]

    return (
        <section id="testimonials" className="general-main-section">
            <h2>Don't take our word for it...</h2>
            <p>Hear from some of our long-time users</p>
            <div id="testimonial-container">
                {
                    data.map((testimonial, index) => (
                        <TestimonialCardComponent
                            key={index}
                            name={testimonial.name}
                            experience={testimonial.experience}
                            rating={testimonial.rating}
                            review={testimonial.review}
                        />
                    ))
                }
            </div>
        </section>
    );
}

export default FooterComponent;