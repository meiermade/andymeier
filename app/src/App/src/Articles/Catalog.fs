module App.Articles.Catalog

open App.Articles

let all: Article list =
    [ Posts.FSharpSemanticKernel.article
      Posts.PersonalInfrastructure.article
      Posts.DevelopmentEnvironment.article
      Posts.Bpc157.article
      Posts.GeneratingRealisticImages.article
      Posts.WhyImBuildingAthleneum.article ]
    |> Article.sort NewestPublished

let tryFind permalink =
    all |> List.tryFind (fun article -> article.permalink = permalink)
