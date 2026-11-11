module App.Index.View

open FSharp.ViewEngine
open App.Infrastructure
open App.Articles
open App.Articles.Shared
open App.Common.View
open App.Components
open type Html
open type Datastar

let metadata : PageMetadata =
    { canonicalPath = "/"
      description = "Personal notes by Andy Meier about engineering, finance, and the systems behind them."
      title = "Andy Meier" }

let aboutMe =
    p {
        _class "mt-4 text-lg text-gray-600 dark:text-gray-400"
        text "I’m an engineer and the owner of "
        fveLink { _href "https://meiermade.com"; "Meier Made, LLC" }
        text ". Originally from St. Louis, Missouri, I now live in New York City. The opinions shared here are my own."
    }

let homePage (recentArticles:Article list) =
    let content =
        div {
            _data ("home-page", "")
            _class "py-10 mx-auto max-w-5xl px-4"
            div {
                _class "grid gap-4 grid-cols-1 md:grid-cols-2"
                div {
                    _class "flex flex-col items-center"
                    img { _class "w-72 aspect-square rounded-full mb-4"; _src (Asset.fingerprinted "/images/profile.jpg") }
                    div {
                        _class "flex justify-center space-x-2"
                        for label, href, icon in [ "GitHub", "https://github.com/meiermade", IconName.GitHub; "X", "https://x.com/andymeierdev", IconName.X; "LinkedIn", "https://www.linkedin.com/in/andrew-meier/", IconName.LinkedIn ] do
                            fveLink {
                                _href href
                                _ariaLabel label
                                _variant LinkVariant.Ghost
                                _color Color.Neutral
                                fveIcon { _name icon }
                            }
                    }
                }
                div {
                    _class "md:order-first"
                    h1 { _class "text-4xl text-gray-900 dark:text-gray-100 font-medium"; "Andy Meier" }
                    aboutMe
                }
            }
            section {
                _class "mt-16"
                h2 { _class "text-2xl text-gray-900 dark:text-gray-100 font-medium"; "Recent articles" }
                div {
                    _class "mt-2 max-w-4xl"
                    for article in recentArticles do
                        ArticleCard.summary article
                }
                div {
                    _class "mt-4"
                    fveLink {
                        _href "/articles"
                        _dataOn ("click", Navigation.action "/articles")
                        "View all"
                    }
                }
            }
        }
    Page.primary content
