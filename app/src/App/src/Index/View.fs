module App.Index.View

open FSharp.ViewEngine
open App.Infrastructure
open Domain.Article
open App.Common.View
open type Html
open type Datastar
    
let aboutMe = markdown """
Engineer building systems for finance, accounting, and capital markets.

Currently working at [Aplazo](https://aplazo.mx/).
"""

let homePage (recentArticles:ArticleProperties list) =
    let content =
        div {
            _class "pt-20 pb-16 mx-auto max-w-4xl px-4"
            div {
                _class "grid gap-4 grid-cols-1 md:grid-cols-2"
                div {
                    _class "flex flex-col items-center"
                    img { _class "w-72 aspect-square rounded-full mb-4"; _src "/images/profile.jpg" }
                    div {
                        _class "flex justify-center space-x-2"
                        a { _class "p-2 text-gray-600 rounded-full hover:bg-gray-100 dark:text-gray-400 dark:hover:bg-gray-800"; _href "https://github.com/meiermade"; MiniIcon.github }
                        a { _class "p-2 text-gray-600 rounded-full hover:bg-gray-100 dark:text-gray-400 dark:hover:bg-gray-800"; _href "https://x.com/andrewmeierdev"; MiniIcon.twitter }
                        a { _class "p-2 text-gray-600 rounded-full hover:bg-gray-100 dark:text-gray-400 dark:hover:bg-gray-800"; _href "https://www.linkedin.com/in/andrew-meier/"; MiniIcon.linkedIn }
                    }
                }
                div {
                    _class "px-2 md:order-first"
                    h1 { _class "text-2xl text-gray-900 dark:text-gray-100 font-medium"; "Andrew Meier" }
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
                    a {
                        _class "text-sm text-emerald-600 hover:underline hover:cursor-pointer dark:text-emerald-400"
                        _dsOn ("click", "@get('/articles')")
                        "View all"
                    }
                }
            }
        }
    Page.primary content
