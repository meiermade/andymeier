module App.Articles.Posts.Bpc157

open App.Articles
open App.Articles.Shared
open FSharp.ViewEngine
open System
open type Html

let private metadata =
    { permalink = "bpc-157-elbow-research"
      title = "BPC-157: Elbow Tendonitis Recovery"
      summary = "My elbow pain, recovery goals, materials, preparation, dosing, and observations using BPC-157."
      cover = "https://assets.meiermade.com/andymeier/articles/shared/gradient-purple-4776537cdf89.webp"
      tags = [| "Health"; "Peptides" |]
      createdAt = DateTimeOffset(2026, 11, 8, 0, 0, 0, TimeSpan.Zero) }

let private paragraph value = p { text value }

let private link href label = a {
    _href href
    text label
}

let private materials =
    [ "BPC-157", "NovaPrime Labs · 10 mg freeze-dried peptide in a 3 mL vial",
      "https://novaprimelabs.com/product/bpc-157-1/", "Product", 69.99M
      "Reconstitution solution", "NovaPrime Labs · 30 mL bottle; my label states 0.9% benzyl alcohol",
      "https://novaprimelabs.com/product/reconstitution-solution/", "Product", 19.99M
      "Alcohol prep pads", "MED PRIDE · 70% isopropyl alcohol · 200 count",
      "https://www.amazon.com/dp/B07F2MQ9NJ", "Product", 6.99M
      "Injection syringes", "EasyTouch U-100 · 0.3 mL · 31G · 5/16 inch (8 mm) · 200 count",
      "https://www.amazon.com/dp/B0HKQ2R7BC", "Product", 39.99M
      "Reconstitution syringes", "1 mL · 30G · 5/16 inch (8 mm) · box of 100; sold by TENKAI",
      "https://www.amazon.com/dp/B0CKYJFDQR", "Product", 20.00M
      "Sharps container", "Alcedo · 1 quart · single container",
      "https://www.amazon.com/dp/B0BFJW6X41", "Product", 6.95M
      "Spring clamps", "Amazon Basics · 14-piece nylon set for photography, woodworking, and crafts",
      "https://www.amazon.com/dp/B07ZTW34SC", "Product", 14.99M
      "Vial case", "Vamctry · nine-slot vial organizer",
      "https://www.amazon.com/dp/B0GYDKH4XD", "Product", 7.99M
      "Shipping", "NovaPrime Labs · standard US shipping",
      "", "", 19.95M ]

let private materialsTotal =
    materials |> List.sumBy (fun (_, _, _, _, cost) -> cost)

let private materialsTable = div {
    _class "overflow-x-auto"
    table {
        _class "min-w-[44rem]"
        thead {
            tr {
                th { _scope "col"; "Material" }
                th { _scope "col"; "Description" }
                th { _scope "col"; "Link" }
                th { _scope "col"; "Cost (USD)" }
            }
        }
        tbody {
            for name, description, href, label, cost in materials do
                tr {
                    th { _scope "row"; text name }
                    td { text description }
                    td {
                        if href <> "" then
                            link href label
                    }
                    td { text $"${cost:F2}" }
                }
        }
        tfoot {
            tr {
                th { _scope "row"; "Total" }
                td { "Before tax and discounts" }
                td { "" }
                td { strong { text $"${materialsTotal:F2}" } }
            }
        }
    }
}

let private sections =
    [ ArticlePage.section
          "overview"
          "Overview"
          [ paragraph
                "I have had pain around the inside/front of my left elbow and into my forearm for months, especially when lifting something palm-up. For the last nine months, I have regularly held my son in my left arm while feeding him. I suspect that sustained bent-elbow, palm-up position has been a major contributor. Spending most of the day typing may also add to the load."
            paragraph
                "Based on the location of the pain and how a resisted palm-up curl triggers it, distal biceps tendinopathy seems like the most likely explanation."
            paragraph
                "Stopping lifting and trying stretching have not resolved it. My goal is to get back to lifting heavy and doing activities without pain."
            figure {
                _class "mx-auto max-w-lg"
                img {
                    _src "/images/articles/bpc-157/distal-biceps-tendinopathy-9a79a62bf636.webp"
                    _alt "Anatomical illustration of the biceps muscle and distal tendon attaching to the radius, with the typical pain area highlighted at the front of the elbow."
                    _loading "lazy"
                    _class "aspect-[3/2] h-auto w-full rounded-lg"
                }
                figcaption {
                    _class "text-center"
                    "The distal biceps tendon and typical pain location (illustration)."
                }
            }
            p {
                text "BPC-157 is a synthetic peptide made from 15 amino acids. It has been studied for tissue repair, including tendon, ligament, and muscle injuries. Most of that research is in animals; "
                link "https://pubmed.ncbi.nlm.nih.gov/42885452/" "human evidence remains limited"
                text ". I am documenting my preparation, administration, and perceived results."
            } ]
      ArticlePage.section
          "materials"
          "Materials"
          [ p {
                text "A close friend who has used the supplier for a while recommended "
                link "https://novaprimelabs.com/" "NovaPrime Labs"
                text ". I ordered the peptide and reconstitution solution on October 31, 2026, and bought the remaining supplies on Amazon."
            }
            materialsTable
            paragraph
                "EasyTouch 1 mL syringes were sold out, so I ordered a different brand for reconstitution." ]
      ArticlePage.section
          "preparation"
          "Preparation: reconstituting"
          [ paragraph
                "Both vials had been in the refrigerator since delivery, so I first let them come to room temperature. I laid out alcohol swabs, 1 mL 30G syringes, the reconstitution-solution and BPC-157 vials, and the sharps container."
            paragraph
                "I removed the protective caps from both bottles and swabbed the rubber stoppers with alcohol. Using the 1 mL syringes, I transferred two full 1 mL portions of reconstitution solution into the 3 mL vial containing 10 mg of BPC-157: 2 mL of solution in total."
            paragraph
                "I mixed the solution gently to minimize foaming and potential peptide aggregation, using slow rolling rather than vigorous shaking. I rolled the vial between my palms and gently turned it over a couple of times. The powder was fully dissolved in about 5–10 minutes, with no white powder remaining visible."
            paragraph
                "This produced a nominal concentration of 5 mg/mL, or 5,000 mcg/mL, assuming approximately 2 mL of final solution." ]
      ArticlePage.section
          "dose-and-schedule"
          "Dosing and schedule"
          [ p {
                text "My starting schedule is "
                strong { "250 mcg once daily in the morning" }
                text ". With my 10 mg vial mixed with 2 mL of reconstitution solution, that corresponds to "
                strong { "5 units on a U-100 syringe" }
                text ":"
            }
            pre {
                code {
                    _class "language-none"
                    text "10 mg = 10,000 mcg\n10,000 mcg ÷ 2 mL = 5,000 mcg/mL\n250 mcg ÷ 5,000 mcg/mL = 0.05 mL\n0.05 mL × 100 U-100 units/mL = 5 units"
                }
            }
            paragraph
                "U-100 markings measure volume: 5 units is 0.05 mL on either a 0.3 mL or a 1 mL U-100 syringe. The syringe capacity changes how much it holds, not the volume represented by each unit. The 250 mcg conversion applies specifically to this concentration." ]
      ArticlePage.section
          "administration"
          "Administration"
          [ paragraph
                "For administration, I laid out alcohol swabs, a spring clamp, the reconstituted BPC-157 vial, a 0.3 mL U-100 syringe with a 31G, 8 mm needle, and the sharps container."
            paragraph
                "I removed the syringe caps and pulled the plunger back to 5 units to draw in air. I inserted the needle through the vial's rubber stopper and pushed the air into the vial. Then I inverted the vial and drew 5 units of solution. Replacing the withdrawn volume with air reduced the vacuum that otherwise pulled the plunger back toward the vial. I set the syringe down with the needle clear of the table and other surfaces."
            paragraph
                "I used a spring clamp to hold a fold of skin near my elbow and swabbed the skin with an alcohol wipe. I inserted the needle and pushed down the plunger. Afterward, I put the caps back on the syringe, placed it in the sharps container, and removed the clamp."
            paragraph
                "I returned the vials to their container and put them back in the refrigerator." ]
      ArticlePage.section
          "daily-log"
          "Daily log and findings"
          [ div {
                _class "overflow-x-auto"
                table {
                    _class "min-w-[44rem]"
                    thead {
                        tr {
                            th { _scope "col"; "Date" }
                            th { _scope "col"; "Time" }
                            th { _scope "col"; "Dose" }
                            th { _scope "col"; "Observations" }
                        }
                    }
                    tbody {
                        tr {
                            th { _scope "row"; "November 9, 2026 · Day 1" }
                            td { "7:00 AM" }
                            td { "5 units · 250 mcg" }
                            td { "First dose. Pain when attempting a palm-up curl while pressing down on that hand with my other hand." }
                        }
                        tr {
                            th { _scope "row"; "November 10, 2026 · Day 2" }
                            td { "7:00 AM" }
                            td { "5 units · 250 mcg" }
                            td { "Still painful during a palm-up resisted curl. No noticeable side effects." }
                        }
                        tr {
                            th { _scope "row"; "November 11, 2026 · Day 3" }
                            td { "7:30 AM" }
                            td { "5 units · 250 mcg" }
                            td { "Still painful during a palm-up curl. No noticeable side effects." }
                        }
                    }
                }
            } ] ]

let article =
    Article.create metadata (ArticlePage.primary metadata [] sections)
