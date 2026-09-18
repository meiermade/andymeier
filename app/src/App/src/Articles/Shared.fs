module App.Articles.Shared

open App.Articles
open App.Common.View
open FSharp.ViewEngine
open type Datastar
open type Html

module ArticleCard =
    let private tag (text: string) = span {
        _class
            "inline-flex items-center rounded-md bg-gray-50 px-2 py-1 text-xs font-medium text-gray-600 ring-1 ring-inset ring-gray-500/10 dark:bg-gray-800 dark:text-gray-300 dark:ring-gray-600"

        text
    }

    let tags (tags: string[]) = div {
        _class "flex flex-wrap gap-2"

        for tag' in tags do
            tag tag'
    }

    let summary (article': Article) =
        let url = SiteUrl.article article'.permalink

        article {
            _class "py-6 border-b border-gray-300/60 dark:border-gray-700/60"

            div {
                _class "flex items-center flex-wrap gap-x-4 gap-y-1 text-sm text-gray-400 dark:text-gray-500"

                div {
                    _class "inline-flex items-center whitespace-nowrap"

                    span {
                        _class "mr-1.5"
                        MiniIcon.calendar
                    }

                    time {
                        _datetime (article'.createdAt.ToString("yyyy-MM-dd"))
                        article'.createdAt.ToString("MMMM d, yyyy")
                    }
                }
            }

            h2 {
                _class "mt-2 text-xl font-semibold tracking-tight text-gray-900 dark:text-gray-100"

                a {
                    _href url
                    _dataOn ("click", Navigation.action url)
                    _class "hover:text-emerald-600 dark:hover:text-emerald-400"
                    article'.title
                }
            }

            p {
                _class "mt-2 text-base text-gray-600 dark:text-gray-400"
                article'.summary
            }

            div {
                _class "mt-4"
                tags article'.tags
            }
        }

module ArticlePage =
    type Section =
        { id: string
          title: string
          content: HtmlElement list }

    let section id title content : Section =
        { id = id
          title = title
          content = content }

    let private contents (sections: Section list) = nav {
        _ariaLabel "On this page"
        _data ("article-toc", "")

        ul {
            _class "space-y-1"

            for section in sections do
                li {
                    a {
                        _href $"#{section.id}"
                        _dataOn ("click", "window.articleNavigation.navigate(evt)")

                        _class
                            "block rounded-md px-3 py-2 text-sm/5 text-gray-600 hover:bg-gray-200/60 hover:text-gray-950 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-emerald-600 aria-[current=location]:bg-emerald-600/10 aria-[current=location]:font-semibold aria-[current=location]:text-emerald-700 dark:text-gray-400 dark:hover:bg-gray-800 dark:hover:text-white dark:aria-[current=location]:text-emerald-400"

                        text section.title
                    }
                }
        }
    }

    let primary (metadata: ArticleMetadata) (introduction: HtmlElement list) (sections: Section list) =
        let page = div {
            _data ("article-page", "")
            _data ("article-url", SiteUrl.article metadata.permalink)
            _data ("telemetry-content-id", metadata.permalink)
            _dataOn ("scroll__window", "window.articleNavigation?.schedule()")
            _dataOn ("resize__window", "window.articleNavigation?.schedule()")

            div {
                _class "bg-cover bg-no-repeat bg-center bg-blend-overlay bg-gray-800"

                match SafeOutput.tryBackgroundImageStyle metadata.cover with
                | Some style -> _style style
                | None -> ()

                div {
                    _class
                        "pt-28 pb-20 px-4 mx-auto max-w-5xl min-[88rem]:max-w-[84rem] flex flex-col justify-end items-start text-gray-50"

                    time {
                        _class "text-base text-gray-50 border-l border-gray-300 pl-2"
                        _datetime (metadata.createdAt.ToString("yyyy-MM-dd"))
                        metadata.createdAt.ToString("MMMM d, yyyy")
                    }

                    h1 {
                        _class "mt-4 text-4xl font-bold tracking-tight text-gray-50"
                        metadata.title
                    }

                    div {
                        _class "mt-5"
                        ArticleCard.tags metadata.tags
                    }
                }
            }

            div {
                _class
                    "mx-auto max-w-5xl px-4 min-[88rem]:max-w-[84rem] min-[88rem]:grid min-[88rem]:grid-cols-[minmax(0,1fr)_15rem] min-[88rem]:gap-10"

                div {
                    _class "min-w-0"

                    if not sections.IsEmpty then
                        details {
                            _class "mt-8 rounded-lg border border-gray-300 dark:border-gray-700 min-[88rem]:hidden"
                            _data ("article-mobile-toc", "")

                            summary {
                                _class
                                    "cursor-pointer rounded-lg px-4 py-3 text-base font-semibold text-gray-900 focus-visible:outline-2 focus-visible:outline-emerald-600 dark:text-gray-100"

                                "On this page"
                            }

                            div {
                                _class "border-t border-gray-300 p-2 dark:border-gray-700"
                                contents sections
                            }
                        }

                    article {
                        _class
                            "mt-8 pb-8 prose prose-lg dark:prose-invert prose-code:before:hidden prose-code:after:hidden max-w-none"

                        _dataInit "highlightCode($el)"

                        for element in introduction do
                            element

                        for section in sections do
                            h2 {
                                _id section.id
                                _tabindex -1
                                _data ("article-section", "")
                                _class "mt-10 scroll-mt-24"
                                text section.title
                            }

                            for element in section.content do
                                element
                    }
                }

                if not sections.IsEmpty then
                    aside {
                        _class "hidden min-w-0 border-l border-gray-300 pl-5 dark:border-gray-700 min-[88rem]:block"

                        div {
                            _class "sticky top-24 mt-8 max-h-[calc(100dvh-7rem)] overflow-y-auto pb-4"

                            p {
                                _class "mb-3 px-3 text-sm font-semibold text-gray-900 dark:text-gray-100"
                                "On this page"
                            }

                            contents sections
                        }
                    }
            }

            script {
                js
                    """
window.articleNavigation = window.articleNavigation || {
    frame: 0,
    observer: null,
    schedule() {
        if (this.frame) return;
        this.frame = requestAnimationFrame(() => { this.frame = 0; this.update(); });
    },
    update() {
        const page = document.querySelector('[data-article-page]');
        if (!page) return;
        const headings = Array.from(page.querySelectorAll('[data-article-section]'));
        if (!headings.length) return;
        const offset = (document.getElementById('top-nav')?.getBoundingClientRect().bottom || 0) + 48;
        const atEnd = window.scrollY + window.innerHeight >= document.documentElement.scrollHeight - 2;
        const current = atEnd ? headings.at(-1) : headings.filter(h => h.getBoundingClientRect().top <= offset).at(-1);
        for (const link of page.querySelectorAll('[data-article-toc] a')) {
            if (current && link.hash === '#' + current.id) link.setAttribute('aria-current', 'location');
            else link.removeAttribute('aria-current');
        }
    },
    save() {
        history.replaceState({ ...history.state, meierMadeScrollX: scrollX, meierMadeScrollY: scrollY }, '', location.href);
    },
    navigate(event) {
        if (event.button !== 0 || event.metaKey || event.ctrlKey || event.shiftKey || event.altKey) return;
        const link = event.currentTarget;
        const target = document.getElementById(decodeURIComponent(link.hash.slice(1)));
        if (!target) return;
        event.preventDefault();
        this.save();
        const disclosure = link.closest('details');
        if (disclosure) disclosure.open = false;
        if (location.hash !== link.hash) history.pushState(null, '', link.hash);
        target.scrollIntoView({ block: 'start' });
        target.focus({ preventScroll: true });
        this.save();
        this.schedule();
    },
    restore() {
        const state = history.state;
        if (typeof state?.meierMadeScrollY === 'number') {
            window.scrollTo(state.meierMadeScrollX || 0, state.meierMadeScrollY);
        } else if (location.hash) {
            document.getElementById(decodeURIComponent(location.hash.slice(1)))?.scrollIntoView({ block: 'start' });
        }
        this.schedule();
    },
    observe() {
        this.observer?.disconnect();
        this.observer = new ResizeObserver(() => this.schedule());
        // The shared page container survives Datastar page morphs. Updates always query current content.
        const content = document.getElementById('page-content');
        if (content) this.observer.observe(content);
        // The site's manual history restoration also disables native fragment restoration on reload.
        if (document.readyState !== 'complete') {
            window.addEventListener('load', () => this.restore(), { once: true });
        }
        this.schedule();
    }
};
window.articleNavigation.observe();
"""
            }

            script { _src (Asset.fingerprinted "/scripts/prism.js") }
            script { js "function highlightCode(el){if(el?.querySelectorAll)Prism.highlightAllUnder(el)}" }

            script {
                js
                    "window.updateArticleProgress=window.updateArticleProgress||function(){var progress=document.getElementById('article-scroll-progress'),article=document.querySelector('[data-article-page] article');if(!progress||!article)return;var articleBottom=article.getBoundingClientRect().bottom+window.scrollY,maxScroll=Math.max(articleBottom-window.innerHeight,1),value=Math.min(Math.max(window.scrollY/maxScroll,0),1);progress.style.transform='scaleX('+value+')';progress.setAttribute('aria-valuenow',String(Math.round(value*100)))};window.scheduleArticleProgress=window.scheduleArticleProgress||function(){if(window.__articleProgressFrame)return;window.__articleProgressFrame=requestAnimationFrame(function(){window.__articleProgressFrame=0;window.updateArticleProgress()})};window.initializeArticleProgress=window.initializeArticleProgress||function(){if(!window.__articleProgressBound){window.__articleProgressBound=true;window.addEventListener('scroll',window.scheduleArticleProgress,{passive:true});window.addEventListener('resize',window.scheduleArticleProgress)}window.updateArticleProgress()};window.initializeArticleProgress()"
            }
        }

        Page.primary page
