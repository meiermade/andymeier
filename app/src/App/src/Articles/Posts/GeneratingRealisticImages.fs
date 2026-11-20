module App.Articles.Posts.GeneratingRealisticImages

open App.Articles
open App.Articles.Shared
open FSharp.ViewEngine
open System
open type Html

let private permalink = "generating-realistic-images-with-blender-and-openai"

let private imageUrl file =
    $"https://assets.meiermade.com/andymeier/articles/{permalink}/{file}.webp"

let private finalImage = "final-concept-b716c1f201ce"

let private metadata: ArticleMetadata =
    {
        permalink = permalink
        title = "Generating realistic images with Blender and OpenAI"
        summary =
            "A floating barbell in the Athleneum gym showed me what to model in Blender and what to leave to OpenAI."
        cover = imageUrl finalImage
        tags = [| "Blender"; "OpenAI"; "Athleneum" |]
        createdAt = DateTimeOffset(2026, 11, 19, 0, 0, 0, TimeSpan.Zero)
    }

let private paragraph (value: string) = p { text value }

let private link href (label: string) = a {
    _href href
    text label
}

let private figureImage url (width: int) (height: int) description (caption: HtmlElement) = figure {
    _class "not-prose my-8 min-w-0"

    a {
        _href url
        _class "block rounded-md focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-emerald-600"

        img {
            _src url
            _alt description
            _width (string width)
            _height (string height)
            _loading "lazy"
            _class "h-auto w-full rounded-md"
        }
    }

    figcaption {
        _class "mt-2 text-sm leading-6 text-gray-600 dark:text-gray-400"
        caption
    }
}

let private image file width height description (caption: string) =
    figureImage (imageUrl file) width height description (text caption)

let private frame file = image file 1536 1024

let private lindbergh =
    figureImage
        (imageUrl "lindbergh-weight-room-d708713d16f6")
        1536
        1152
        "Three-quarter photograph of green double-sided racks at Lindbergh High School, with racked barbells, benches, and plates on outward-facing pegs."
        (fragment {
            "Lindbergh High School’s weight room. Photo: "

            a {
                _href "https://www.legendfitness.com/showcase/lindbergh-high-school/"
                _class "underline underline-offset-2"
                "Legend Fitness"
            }

            ". This is the exact view I used as a reference."
        })

let private sections =
    [
        ArticlePage.section
            "overview"
            "Overview"
            [
                p {
                    "I wanted a realistic image of the gym I’m planning for "
                    link "https://athleneum.com/" "Athleneum"
                    ". The early AI renders looked convincing at a glance, but the double-sided weight racks didn’t hold up. Uprights disappeared, benches faced the wrong way, and one barbell looked like it was floating. Even photos of real racks didn’t reliably fix it."
                }
                paragraph
                    "What worked was building the whole gym in Blender first: the racks, cardio equipment, windows, mirrors, and people, all in one scene. Then I sent the Blender render to OpenAI and asked it to make the same view look like a photograph. Getting there meant trying two different ways of combining 3D and AI before I found one that worked."
            ]
        ArticlePage.section
            "generating-directly"
            "Generating the gym directly"
            [
                paragraph
                    "The original concept had what I wanted on the floor: racks on the left, turf down the middle, dumbbells and kettlebells on the right, and cardio by the back windows. It also had a double-height ceiling that might be hard to find in a real building. I wanted to keep the daylight and make the gym work on one floor."
                paragraph
                    "I also wanted back-to-back lifting stations like the ones I remembered from the Rhodes weight room. Two people should be able to use the same rack from opposite sides, each with a bench and a racked barbell. The first batch of lower-ceiling concepts gave me a good sense of the room, but I couldn’t tell how both sides of the racks would actually work."
                div {
                    _class "grid gap-x-5 sm:grid-cols-2"

                    frame
                        "original-concept-1c276c529a42"
                        "Original concept with tall windows, racks along the left wall, turf in the middle, and free weights on the right."
                        "The original concept. I wanted to keep the daylight and the basic layout."

                    frame
                        "early-gym-study-58f6b2cb1755"
                        "Early lower-ceiling gym study with a row of black racks on wood platforms."
                        "An early lower-ceiling study. The racks looked substantial, but the two lifting stations weren’t clear."
                }
            ]
        ArticlePage.section
            "real-rack-photos"
            "Trying again with real rack photos"
            [
                paragraph
                    "I started with photos from Rhodes and equipment makers, then looked for a side or three-quarter view of a real installation. The photo of Lindbergh High School’s weight room was the one I kept coming back to. I could follow each bar to its hooks and see how the plates hang off the sides, leaving the floor between the two lifting positions open."
                lindbergh
                paragraph
                    "I used that photo to guide edits to the gym render. The racks started to look more like the equipment I wanted, but the same problems kept coming back. A revision fixed a treadmill’s running deck and still left the far end of a barbell hanging in midair. In later passes an upright looked like it ran through a bench, and some benches were either missing or facing the same direction."
                div {
                    _class "max-w-lg"

                    image
                        "rack-flaws-0f962f9f30d8"
                        730
                        834
                        "Red arrows point to overhead rails with missing uprights and the unsupported far end of a barbell."
                        "My markup on a revision that already used real rack photos. Follow the far end of the foreground bar: there’s nothing underneath it."
                }
            ]
        ArticlePage.section
            "researching-a-better-way"
            "Researching a better way"
            [
                paragraph
                    "After enough rounds of circling the same mistakes, I asked whether I should make a CAD model first. I looked at other image models too, but the missing hook wasn’t about the overall quality of the picture. It was about whether one part attached to another, and whether that relationship survived four times down the row."
                p {
                    "I settled on "
                    link "https://www.blender.org/" "Blender"
                    " because I needed a dimensioned scene I could inspect and render, not a full set of architectural plans. I used the "
                    link "https://www.legendfitness.com/product/pro-series-double-sided-half/" "Legend Fitness #3227"
                    " as a starting point. A Python script built one double-sided rack with its uprights, benches, bars, and J-cups (the hooks the bars rest in). Once that assembly was right, I could repeat it without having to redraw any of the hardware."
                }
            ]
        ArticlePage.section
            "compositing-the-racks"
            "Putting Blender racks into the generated gym"
            [
                paragraph
                    "I already liked the room in the generated image, so I tried keeping it. I used OpenAI to erase the old rack row, then composited the rack equipment rendered in Blender into the cleared space. When I compared the model with Lindbergh again, I added outward-facing plate pegs halfway between the two lifting faces."
                image
                    "blender-rack-73bbe0264bc8"
                    1100
                    825
                    "Isolated Blender rack with two separate barbell positions, benches, and plate storage on outward-facing pegs."
                    "One rack in Blender, after adding the plate pegs. I could inspect both sides without the rest of the gym in the way."
                paragraph
                    "The equipment now held together on its own. But lining it up with the old image was a separate problem. In a later composite a bench landed on the turf, and a bike’s upright crossed in front of a seated person. The pieces made sense in Blender and didn’t quite fit together in the picture."
                frame
                    "rack-composite-aa00a8f033d7"
                    "Modeled gym equipment composited into an earlier AI image, with a bench extending onto the turf and a bike crossing in front of a seated person."
                    "The rack-and-cardio composite. Look at the edge of the turf and the seated person on the right."
            ]
        ArticlePage.section
            "build-the-scene"
            "Building the whole gym and patching in details"
            [
                paragraph
                    "Instead of lining up the equipment with an existing picture, I built the room around it. The overhead plan let me count four double-sided racks, eight lifting benches, five treadmills, and five bikes, and check that there was room to walk to the cardio wall. The mirrors, windows, turf, and two simple figures went into the same scene, on the same floor."
                div {
                    _class "max-w-lg"

                    image
                        "blender-plan-11d10ef65dbd"
                        1050
                        1260
                        "Overhead Blender view with four double-sided racks, a turf strip, free weights, five treadmills, and five bikes."
                        "From overhead I could verify the layout without benches or cardio equipment hiding behind the racks."
                }
                paragraph
                    "The first camera view showed too much empty floor. I moved the camera closer rather than moving the racks. The people still looked like mannequins, but I knew where they stood and how tall they were next to the equipment."
                frame
                    "blender-scene-445407226fdb"
                    "Raw Blender render of the full gym with simplified people, racks, mirrors, windows, and green turf."
                    "The full Blender scene, before any AI finishing."
                paragraph
                    "My first attempt to finish it kept the Blender render and patched in more realistic people, views through the windows, and floor texture. The patches looked fine in isolation. Together, it still looked like a Blender render with pictures of people pasted on top."
            ]
        ArticlePage.section
            "edit-the-whole-frame"
            "Giving OpenAI the whole scene"
            [
                p {
                    "I didn’t want better cutouts. I wanted the scene itself to look like a photograph. So I sent the whole render to the "
                    link "https://developers.openai.com/api/docs/guides/image-generation" "OpenAI Images Edits API"
                    " without a mask, along with two reference images. I gave each image a different job:"
                }
                ol {
                    li { "The Blender frame set the camera, layout, scale, and placement of every object and person." }
                    li { "The Lindbergh photo guided the rack hardware, not the colors or layout of the gym." }
                    li { "An earlier Athleneum render served as a reference for daylight and materials, not composition." }
                }
                paragraph "Here’s how the prompt started:"
                blockquote {
                    paragraph
                        "Re-render image 1 as ONE cohesive, truly photoreal editorial architecture photograph of the SAME Manhattan clubhouse gym, not a pasted or isolated detail edit. Image 1 is a complete approved Blender scene and the authoritative geometric blueprint for every surface, person, and object."
                }
                paragraph
                    "I spelled out what should stay put: the eye-level camera and crop, all four rack frames, a bench on each side, two hooks per bar, and a clear path to cardio. Rendering the whole image in one pass made a bigger difference than polishing any one detail. The light crossing the turf, the shadows under the equipment, and the people all looked like they belonged in the same room."
                div {
                    _class "grid gap-x-5 sm:grid-cols-2"

                    frame
                        "patched-render-f65faf75daad"
                        "Blender gym with photographic people and window views composited into the render."
                        "Patched render. The people and window views changed, but the lighting still reads as Blender."

                    frame
                        "full-frame-edit-330d0d159a09"
                        "Full-frame edit with warm daylight and a stray figure visible in the right mirror."
                        "Full-frame edit. Look at the daylight crossing the floor and the shadows beneath the people."
                }
                paragraph
                    "I checked the result against the Blender frame. The right mirror still had a figure that looked like a third person by the windows. I removed it in one last edit and swapped the wood ceiling for matte warm gray. I still count the cardio machines in Blender, not from this angle: the rack row hides part of the treadmill lineup."
                frame
                    finalImage
                    "Final Athleneum gym concept with a warm-gray ceiling, black beams, two people, and sunlight across the turf."
                    "The final concept image, with a quieter ceiling and no stray figure in the mirror."
                paragraph
                    "The Lindbergh photo was helpful both times, but it couldn’t tell the model where every part of my gym belonged. The complete Blender scene could. Next time I need something to look real and fit together, I’ll model the things I need to count, measure, or position, then let the image model work on the whole frame."
            ]
    ]

let article =
    Article.create metadata (ArticlePage.primary metadata [] sections)
