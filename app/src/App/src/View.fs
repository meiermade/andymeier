module App.View

open FSharp.ViewEngine
open App.Component
open App.Article
open type Html
open type Datastar

module Index =

    let aboutPage =
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
                            a { _class "p-2 text-gray-600 rounded-full hover:bg-gray-100 dark:text-gray-400 dark:hover:bg-gray-800"; _href "https://github.com/ameier38"; MiniIcon.github }
                            a { _class "p-2 text-gray-600 rounded-full hover:bg-gray-100 dark:text-gray-400 dark:hover:bg-gray-800"; _href "https://twitter.com/ameier38"; MiniIcon.twitter }
                            a { _class "p-2 text-gray-600 rounded-full hover:bg-gray-100 dark:text-gray-400 dark:hover:bg-gray-800"; _href "https://www.linkedin.com/in/andrew-meier/"; MiniIcon.linkedIn }
                        }
                    }
                    div {
                        _class "px-2 md:order-first"
                        h1 { _class "text-2xl text-gray-900 dark:text-gray-100 font-medium"; "Welcome!" }
                        div {
                            _class "mt-6 prose prose-lg dark:prose-invert"
                            p { "My name is Andrew Meier and I have worked at the intersection of finance and technology for over 10 years. I am passionate about solving problems in finance, capital markets, and accounting. This is my personal site where I share my thoughts and experiences." }
                            p {
                                text "Check out my "
                                a {
                                    _class "text-emerald-600 hover:underline hover:cursor-pointer dark:text-emerald-400"
                                    _dsOn ("click", "@get('/articles')")
                                    "articles"
                                }
                                text " to see what I've been writing about."
                            }
                        }
                    }
                }
            }
        Page.primary content

module Article =

    let private tag (text:string) =
        span {
            _class "inline-flex items-center rounded-md bg-gray-50 px-2 py-1 text-xs font-medium text-gray-600 ring-1 ring-inset ring-gray-500/10 dark:bg-gray-800 dark:text-gray-300 dark:ring-gray-600"
            text
        }

    let private articleSummary (properties:ArticleProperties) =
        let url = $"/articles/{properties.permalink}"
        article {
            _id properties.permalink
            _class "md:grid md:grid-cols-4 md:items-baseline"
            time {
                _class "hidden md:block flex items-center text-sm text-gray-400 dark:text-gray-500"
                _datetime (properties.createdAt.ToString("yyyy-MM-dd"))
                properties.createdAt.ToString("MMMM d, yyyy")
            }
            div {
                _class "md:col-span-3 flex flex-col items-start"
                time {
                    _class "md:hidden border-l border-gray-300 dark:border-gray-600 pl-3 mb-3 flex items-center text-sm text-gray-400 dark:text-gray-500"
                    _datetime (properties.createdAt.ToString("yyyy-MM-dd"))
                    properties.createdAt.ToString("MMMM d, yyyy")
                }
                a {
                    _href url
                    _dsOn ("click", $"@get('{url}')")
                    _class "w-full p-4 sm:rounded-2xl hover:bg-gray-100 dark:hover:bg-gray-800 cursor-pointer flex gap-4"
                    if not (System.String.IsNullOrEmpty(properties.thumbnail)) then
                        img {
                            _class "hidden sm:block w-24 h-24 rounded-lg object-cover shrink-0"
                            _src properties.thumbnail
                        }
                    div {
                        _class "flex flex-col"
                        h2 { _class "text-base font-semibold tracking-tight text-gray-800 dark:text-gray-100"; properties.title }
                        p { _class "mt-2 text-base text-gray-600 dark:text-gray-400"; properties.summary }
                        div {
                            _class "mt-4 flex flex-wrap gap-2"
                            for t in properties.tags do tag t
                        }
                        div {
                            _class "mt-4 flex items-start space-x-1 text-sm font-medium text-emerald-500 dark:text-emerald-400"
                            raw "Read"
                            MiniIcon.chevronRight
                        }
                    }
                }
            }
        }

    let articlesPage (articles:ArticleProperties list) =
        let content =
            div {
                _class "mx-auto max-w-5xl py-6 px-4 sm:px-8 lg:px-12"
                header {
                    h1 { _class "text-4xl text-gray-900 dark:text-gray-100 font-medium"; "Articles" }
                }
                div {
                    _class "mt-16"
                    div {
                        _class "md:border-l md:border-gray-300 dark:md:border-gray-700 md:pl-6"
                        div {
                            _class "max-w-3xl flex flex-col space-y-4"
                            for a in articles do articleSummary a
                        }
                    }
                }
            }
        Page.primary content

    let articlePage (article':Article) =
        let content =
            div {
                if not (System.String.IsNullOrEmpty(article'.properties.cover)) then
                    div {
                        _class "bg-cover bg-no-repeat bg-center bg-blend-overlay bg-gray-800"
                        _style $"background-image: url('{article'.properties.cover}')"
                        div {
                            _class "pt-28 pb-20 px-4 mx-auto max-w-4xl flex flex-col justify-end items-start text-gray-50"
                            time {
                                _class "text-base text-gray-50 border-l border-gray-300 pl-2"
                                _datetime (article'.properties.createdAt.ToString("yyyy-MM-dd"))
                                article'.properties.createdAt.ToString("MMMM d, yyyy")
                            }
                            h1 {
                                _class "mt-4 text-4xl font-bold tracking-tight text-gray-50"
                                article'.properties.title
                            }
                        }
                    }
                else
                    div {
                        _class "pt-20 pb-8 px-4 mx-auto max-w-4xl"
                        time {
                            _class "text-base text-gray-400 dark:text-gray-500 border-l border-gray-300 dark:border-gray-600 pl-2"
                            _datetime (article'.properties.createdAt.ToString("yyyy-MM-dd"))
                            article'.properties.createdAt.ToString("MMMM d, yyyy")
                        }
                        h1 {
                            _class "mt-4 text-4xl font-bold tracking-tight text-gray-900 dark:text-gray-100"
                            article'.properties.title
                        }
                    }
                article {
                    _class "mx-auto max-w-4xl px-4"
                    div {
                        _class "mt-8 prose prose-lg dark:prose-invert prose-code:before:hidden prose-code:after:hidden max-w-none"
                        _dsInit "Prism.highlightAllUnder($el)"
                        raw article'.contentHtml
                    }
                }
            }
        Page.primary content

    let notFoundPage =
        let content =
            div {
                _class "flex flex-col items-center pt-20"
                h1 { _class "text-3xl text-gray-800 dark:text-gray-100"; "Could not find page." }
                p { _class "mt-2 text-md text-gray-600 dark:text-gray-400"; "Something went wrong. Try refreshing the page." }
            }
        Page.primary content
