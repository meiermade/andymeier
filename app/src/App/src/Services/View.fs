module App.Services.View

open FSharp.ViewEngine
open App.Common.View
open type Html

let private iconSecureInfrastructure =
    raw """
    <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor" class="h-6 w-6">
      <path stroke-linecap="round" stroke-linejoin="round" d="M9 12.7498L11.25 14.9998L15 9.74985M12 2.71411C9.8495 4.75073 6.94563 5.99986 3.75 5.99986C3.69922 5.99986 3.64852 5.99955 3.59789 5.99892C3.2099 7.17903 3 8.43995 3 9.74991C3 15.3414 6.82432 20.0397 12 21.3719C17.1757 20.0397 21 15.3414 21 9.74991C21 8.43995 20.7901 7.17903 20.4021 5.99892C20.3515 5.99955 20.3008 5.99986 20.25 5.99986C17.0544 5.99986 14.1505 4.75073 12 2.71411Z" />
    </svg>
    """

let private iconPrivateNetwork =
    raw """
    <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor" class="h-6 w-6">
      <path stroke-linecap="round" stroke-linejoin="round" d="M8.28767 15.0378C10.3379 12.9875 13.662 12.9875 15.7123 15.0378M5.10569 11.8558C8.9133 8.04815 15.0867 8.04815 18.8943 11.8558M1.92371 8.67373C7.48868 3.10876 16.5113 3.10876 22.0762 8.67373M12.5303 18.2197L12 18.7501L11.4696 18.2197C11.7625 17.9268 12.2374 17.9268 12.5303 18.2197Z" />
    </svg>
    """

let private iconIngestion =
    raw """
    <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor" class="h-6 w-6">
      <path stroke-linecap="round" stroke-linejoin="round" d="M19.5 12C19.5 10.7681 19.4536 9.54699 19.3624 8.3384C19.2128 6.35425 17.6458 4.78724 15.6616 4.63757C14.453 4.54641 13.2319 4.5 12 4.5C10.7681 4.5 9.54699 4.54641 8.3384 4.63757C6.35425 4.78724 4.78724 6.35425 4.63757 8.3384C4.62097 8.55852 4.60585 8.77906 4.59222 9M19.5 12L22.5 9M19.5 12L16.5 9M4.5 12C4.5 13.2319 4.54641 14.453 4.63757 15.6616C4.78724 17.6458 6.35425 19.2128 8.3384 19.3624C9.54699 19.4536 10.7681 19.5 12 19.5C13.2319 19.5 14.453 19.4536 15.6616 19.3624C17.6458 19.2128 19.2128 17.6458 19.3624 15.6616C19.379 15.4415 19.3941 15.2209 19.4078 15M4.5 12L7.5 15M4.5 12L1.5 15" />
    </svg>
    """

let private iconWarehouse =
    raw """
    <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor" class="h-6 w-6">
      <path stroke-linecap="round" stroke-linejoin="round" d="M3.75 6.75C3.75 5.507 7.008 4.5 11.25 4.5s7.5 1.007 7.5 2.25S15.492 9 11.25 9s-7.5-1.007-7.5-2.25Zm0 0v4.5C3.75 12.493 7.008 13.5 11.25 13.5s7.5-1.007 7.5-2.25v-4.5m-15 4.5v4.5c0 1.243 3.258 2.25 7.5 2.25s7.5-1.007 7.5-2.25v-4.5" />
    </svg>
    """

let private iconFinancialModels =
    raw """
    <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor" class="h-6 w-6">
      <path stroke-linecap="round" stroke-linejoin="round" d="M7.5 14.25V16.5M10.5 12V16.5M13.5 9.75V16.5M16.5 7.5V16.5M6 20.25H18C19.2426 20.25 20.25 19.2426 20.25 18V6C20.25 4.75736 19.2426 3.75 18 3.75H6C4.75736 3.75 3.75 4.75736 3.75 6V18C3.75 19.2426 4.75736 20.25 6 20.25Z" />
    </svg>
    """

let private iconInternalAppsAi =
    raw """
    <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor" class="h-6 w-6">
      <path stroke-linecap="round" stroke-linejoin="round" d="M9 17.25V18.2574C9 19.053 8.68393 19.8161 8.12132 20.3787L7.5 21H16.5L15.8787 20.3787C15.3161 19.8161 15 19.053 15 18.2574V17.25M21 5.25V15C21 16.2426 19.9926 17.25 18.75 17.25H5.25C4.00736 17.25 3 16.2426 3 15V5.25M21 5.25C21 4.00736 19.9926 3 18.75 3H5.25C4.00736 3 3 4.00736 3 5.25M21 5.25V12C21 13.2426 19.9926 14.25 18.75 14.25H5.25C4.00736 14.25 3 13.2426 3 12V5.25" />
    </svg>
    """

let private serviceCard (icon: HtmlElement) (title: string) (description: string) : HtmlElement =
    div {
        _class "rounded-xl border border-gray-300/60 dark:border-gray-700/60 p-4"
        div {
            _class "text-emerald-600 dark:text-emerald-400"
            icon
        }
        h2 { _class "mt-3 text-xl font-semibold text-gray-900 dark:text-gray-100"; title }
        p { _class "mt-2 text-base text-gray-600 dark:text-gray-400"; description }
    }

let page =
    let content =
        div {
            _class "mx-auto max-w-5xl py-10 px-4"
            h1 { _class "text-4xl text-gray-900 dark:text-gray-100 font-medium"; "Services" }
            p {
                _class "mt-4 text-lg text-gray-600 dark:text-gray-400"
                "I help businesses build and scale their finance, accounting, and capital markets infrastructure"
            }
            div {
                _class "mt-10 grid grid-cols-1 gap-4 md:grid-cols-2"
                serviceCard iconSecureInfrastructure "Secure private infrastructure" "Design and deploy secure, cost-efficient infrastructure with Kubernetes and Pulumi. Enable private networking and access controls with Cloudflare Zero Trust."
                serviceCard iconWarehouse "Data engineering and analytics" "Build reliable ingestion and orchestration pipelines with tools like Airbyte and Dagster. Model analytics layers with dbt in BigQuery, Redshift, Snowflake or other warehouses."
                serviceCard iconFinancialModels "Financial models" "Develop operating models, borrowing base reports, investor reporting, and more in Excel and Google Sheets. Create bespoke F# and Python models for complex scenarios."
                serviceCard iconInternalAppsAi "Internal apps and AI tooling" "Build secure internal web apps and MCP servers in F# or Python. Help set up AI coding agents and skills to automate workflows."
            }
            section {
                _class "mt-10 max-w-3xl mx-auto rounded-xl border border-gray-300/60 dark:border-gray-700/60 p-6"
                div {
                    _class "flex flex-col items-start gap-4 sm:flex-row sm:items-center sm:justify-between sm:gap-6"
                    div {
                        _class "min-w-0"
                        h2 { _class "text-2xl text-gray-900 dark:text-gray-100 font-medium"; "Get in touch" }
                        p {
                            _class "mt-2 text-gray-600 dark:text-gray-400"
                            "Schedule a quick 30-minute call to see if I can help."
                        }
                    }
                    a {
                        _href "https://calendar.app.google/wQPWAYBda4r2S7o48"
                        _target "_blank"
                        _rel "noopener noreferrer"
                        _class "inline-flex shrink-0 justify-center rounded-md bg-emerald-600 hover:bg-emerald-700 text-white px-4 py-2 font-medium"
                        "Schedule 30 minutes"
                    }
                }
            }
        }

    Page.primary content
