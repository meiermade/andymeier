module App.Articles.Posts.WhyImBuildingAthleneum

open App.Articles
open App.Articles.Shared
open FSharp.ViewEngine
open System
open type Html

let private paragraph (value: string) = p { text value }

let private link href (label: string) = a {
    _href href
    text label
}

let private metadata: ArticleMetadata =
    {
        permalink = "why-im-building-athleneum"
        title = "Why I’m building Athleneum"
        summary =
            "From playing two sports and studying physics at Rhodes to working in New York: why I want work, training, meals and recovery to fit into the same day."
        cover = "https://athleneum.com/images/spaces-hero-0f3e793e.webp"
        tags = [| "Athleneum"; "Work"; "Training"; "Design" |]
        createdAt = DateTimeOffset(2026, 11, 19, 0, 0, 0, TimeSpan.Zero)
    }

let private introduction =
    [
        paragraph
            "In college, balancing two sports with a demanding academic workload required an efficient system. I’m building Athleneum to bring that same support to working life."
    ]

let private campus = p {
    link "https://www.rhodes.edu/campus-maps" "See the Rhodes College campus map"
    "."
}

let private chapters =
    [
        "A Division III student-athlete’s day",
        "2007–2011 · Rhodes College",
        [
            "I played football and baseball at Rhodes College in Memphis while studying physics and math. Balancing those commitments meant I needed an efficient system for studying and training."
            "I’d start with coffee and breakfast at the refectory, then walk next door to the library to work or head to the physics lab."
            "When it was time to train, I’d go to the athletic facility, collect my clean laundry loop and change at my personal locker. I’d get stretched in the training room, then go to practice or work out. Afterward, I could shower, drop off my used loop and get something to eat in the athletic dining area. Then I’d go back to the library to finish working."
            "Whether I was living on campus or biking in, I could spend the entire day there with just my backpack. Healthy meals and clean workout clothes were taken care of. I could walk between neighboring buildings and concentrate on studying and training, rather than cooking, doing gym laundry or carrying equipment around."
        ]
        "Separate spaces. Separate trips.",
        "2013–2016 · A Big Four consulting firm",
        [
            "My first job in New York was at a Big Four consulting firm. In the morning, I’d leave home with workout clothes and my own towel for Blink Fitness. It was inexpensive, but crowded. After training, I’d shower, put the sweaty clothes and towel back in my bag and bike to the office. Often, I was sweating again by the time I got there."
            "At work, I could use an app or an iPad on the floor to reserve a desk. I liked this kind of hoteling: a cubicle for concentration, an office for a call, or a meeting room when I needed to work with other people. I could choose a space for the work I was doing rather than stay at the same desk all day."
            "Sharing desks also made sense for a consulting firm where people traveled frequently; the company didn’t need a dedicated desk for every person. That part of the day worked well. The gym trip and the bag of laundry were still mine to manage."
        ]
        "An open office, then another ride.",
        "2016–2022 · Two fintech startups",
        [
            "After consulting, I worked at two fintech startups and joined the New York Athletic Club. I’d bike down to Flatiron and settle into an open-plan office. Then the calls would start around me. There weren’t enough enclosed meeting rooms or spaces for phone calls, so I’d put on noise-cancelling headphones and try to concentrate while people talked and moved around."
            "After work, I’d bike to the New York Athletic Club, collect my gym bag from my locker and train. Having somewhere to keep chalk, a lifting belt and swim goggles helped. Laundry was available, but it was priced per garment, like hotel laundry, and too expensive for my daily workout clothes. So after showering, I’d pack up my used kit and carry it home."
            "The athletic facilities were excellent. If I wanted to finish some work there, the library was good for concentration, but I couldn’t always count on getting a desk. I was still fitting work and training around separate spaces and trips."
        ]
        "Remote work, shared tables.",
        "From December 2022 · A Mexico-based fintech startup",
        [
            "I joined a Mexico-based fintech startup and Equinox. When I started working remotely in December 2022, I’d head to a coworking space, open my laptop at a long shared table and try to settle into the day. Someone would be facing me or sitting right beside me, and people took calls at the table. I was back in headphones for much of the day."
            "When I went to the nearby Equinox for a workout, I had to pack up and bring my gym bag and laundry again. The gym was close, but the work setup was distracting and I was still carrying my training routine around with me."
        ]
        "The closest I came to Rhodes.",
        "Later · Equinox Sports Club on the Upper West Side",
        [
            "Later, I joined Equinox Sports Club with an executive membership. I could arrive without lugging my gym bag and laundry back and forth: I had a permanent locker and a laundry service. I could get breakfast, work on my laptop, train, shower and get another meal without leaving the building. I often ate breakfast, lunch and dinner there."
            "Sometimes I’d listen to an all-hands call from a spin bike and get an hour of easy cardio. Moving between activities inside one building made a real difference. But when I returned to my laptop at a busy time, the cafe noise made concentration difficult. The cafe was useful for meals, but not a dependable quiet workspace."
            "Even meeting someone just for coffee meant registering them as a guest in the app, then asking them to open their email and sign up. I’d found meeting people easier at the New York Athletic Club: in the areas open to guests, visitors just gave their name and the name of the member they were meeting."
            "Swimming had another complication. If I wanted to reserve a swim there, I had to arrive early and put my name down. Only half a lane could be reserved, and I couldn’t book it before getting to the club. I had to make the trip before I knew whether I could swim."
        ]
        "A separate office still meant friction.",
        "Later · An office beside the club",
        [
            "Eventually, I rented a small office next door. I’d settle in there for focused work, then leave the building and go back to Equinox when I wanted coffee, a meal or a workout. Afterward, I’d head outside again to get back to my desk."
            "The office helped with concentration, although I could still hear my neighbors through the walls. I was now paying for two spaces, and every change of activity meant packing up or crossing between them. A short walk was better than a trip across the city, but it wasn’t the same as having everything in one building."
        ]
    ]

let private sections =
    [
        for index, (heading, period, paragraphs) in List.indexed chapters do
            ArticlePage.section
                ("chapter-" + string (index + 1))
                heading
                [
                    p {
                        _class "text-sm text-gray-600 dark:text-gray-400"
                        period
                    }
                    for value in paragraphs do
                        paragraph value
                    if index = 0 then
                        campus
                ]
        ArticlePage.section
            "workspaces"
            "Different spaces for different kinds of work."
            [
                p {
                    "In "
                    em { "Quiet" }
                    ", Susan Cain argues for workspaces where people can move between interaction and solitude. That was the choice I kept looking for: somewhere to talk, somewhere to concentrate, and the freedom to move between them."
                }
                p {
                    "Cal Newport’s "
                    em { "Deep Work" }
                    " introduced me to the Eudaimonia Machine, an architectural concept by David Dewane. Its sequence of five spaces supports different modes of thought, moving from interaction toward deeper concentration."
                }
                paragraph
                    "That idea shaped Athleneum’s workspaces. I’m adapting the principle rather than copying the five-room sequence: different kinds of work need different surroundings."
                dl {
                    for name, copy in
                        [
                            "Cafe",
                            "For coffee, food, conversation and casual work. Somewhere to talk through an idea without asking the whole room to be silent."
                            "Office",
                            "For quiet, individual work. Cubicles keep neighboring desks and passing movement out of view; calls belong in enclosed rooms nearby."
                            "Chambers",
                            "Private single-person rooms for a call or a stretch of deep work, without bringing that conversation onto the quiet floor."
                        ] do
                        dt { name }
                        dd { copy }
                }
                p {
                    "Read "

                    link
                        "https://blog.fentress.com/blog/the-eudaimonia-machine-a-space-concept-for-the-21st-century"
                        "Alan Ruby’s explanation of the Eudaimonia Machine at Fentress"

                    "."
                }
            ]
        ArticlePage.section
            "the-whole-day"
            "The whole day, under one roof."
            [
                paragraph
                    "I’m building Athleneum for people who want to stay active and keep doing the activities they love throughout adult life. Maintaining muscle, strength and fitness takes consistent work, especially as we get older. I want a system that makes that consistency easier."
                paragraph
                    "With work and training in one building, a workout doesn’t need to become a separate outing. Finish a focused block of work, change into clean kit, train, shower and get back to your day. Shorter sessions become practical when getting to the gym, packing equipment and dealing with laundry aren’t separate jobs."
                paragraph
                    "That makes it easier to spread training across the week. Five or more sessions could mean a mix of strength work, swimming and easy cardio, rather than squeezing everything into a few long gym visits. The point isn’t to train hard every day. It’s to make a routine you can sustain for years."
                paragraph
                    "Serious training, a pool and recovery belong alongside workspaces that respect both conversation and concentration. A permanent locker, with workout kit washed daily and returned to its own drawer. Practical meals and coffee. Somewhere to keep a bicycle and the gear that would otherwise travel back and forth. And a way to book a swim before leaving home."
                paragraph "That’s why Athleneum needs to exist."
                p {
                    "The site is live at "
                    link "https://athleneum.com/" "athleneum.com"
                    ". You can explore the concept and "
                    link "https://athleneum.com/waitlist" "join the waitlist"
                    "."
                }
            ]
    ]

let article =
    Article.create metadata (ArticlePage.primary metadata introduction sections)
