namespace App.Components

open System
open System.Text
open System.Text.RegularExpressions
open FSharp.ViewEngine
open type Html
open type Datastar

module private ThemeClasses =
    let private normalize (value:string) = Regex.Replace(value, "\\s+", " ").Trim()

    let foundation = normalize """
        fve-components [color-scheme:light] dark:not-data-[fve-color-mode=light]:[color-scheme:dark] [.dark_&:not([data-fve-color-mode=light])]:[color-scheme:dark] [&.dark]:[color-scheme:dark] data-[fve-color-mode=dark]:[color-scheme:dark] data-[fve-color-mode=light]:[color-scheme:light]
        [--fve-background:oklch(98.5%_0.002_247.839)] [--fve-surface:oklch(100%_0_0)] [--fve-surface-subtle:oklch(96.7%_0.003_264.542)] [--fve-surface-hover:oklch(96.7%_0.003_264.542)] [--fve-surface-active:oklch(92.8%_0.006_264.531)]
        [--fve-text:oklch(21%_0.034_264.665)] [--fve-muted-text:oklch(44.6%_0.03_256.802)] [--fve-border:oklch(87.2%_0.01_258.338)] [--fve-overlay-backdrop:oklch(13%_0.028_261.692/55%)]
        [--fve-neutral-subtle:oklch(96.7%_0.003_264.542)] [--fve-neutral-text:oklch(37.3%_0.034_259.733)] [--fve-neutral-ring:oklch(70.7%_0.022_261.325)]
        [--fve-positive-subtle:oklch(96.2%_0.044_156.743)] [--fve-positive-text:oklch(44.8%_0.119_151.328)] [--fve-positive-ring:oklch(72.3%_0.219_149.579)]
        [--fve-warning-subtle:oklch(97.3%_0.071_103.193)] [--fve-warning-text:oklch(47.6%_0.114_61.907)] [--fve-warning-ring:oklch(76.9%_0.188_70.08)]
        [--fve-critical-subtle:oklch(97.1%_0.013_17.38)] [--fve-critical-solid:oklch(57.7%_0.245_27.325)] [--fve-critical-hover:oklch(50.5%_0.213_27.518)] [--fve-critical-active:oklch(44.4%_0.177_26.899)] [--fve-critical-text:oklch(44.4%_0.177_26.899)] [--fve-critical-ring:oklch(63.7%_0.237_25.331)]
        [--fve-info-subtle:oklch(97%_0.014_254.604)] [--fve-info-text:oklch(48.8%_0.243_264.376)] [--fve-info-ring:oklch(62.3%_0.214_259.815)]
        [--fve-radius-control:0.5rem] [--fve-radius-panel:0.75rem]
        [--fve-popup-active-background:var(--fve-brand-solid)] [--fve-popup-active-text:white]
        dark:not-data-[fve-color-mode=light]:[--fve-popup-active-background:var(--fve-brand-text)] dark:not-data-[fve-color-mode=light]:[--fve-popup-active-text:var(--fve-surface)]
        [.dark_&:not([data-fve-color-mode=light])]:[--fve-popup-active-background:var(--fve-brand-text)] [.dark_&:not([data-fve-color-mode=light])]:[--fve-popup-active-text:var(--fve-surface)]
        [&.dark]:[--fve-popup-active-background:var(--fve-brand-text)] [&.dark]:[--fve-popup-active-text:var(--fve-surface)]
        data-[fve-color-mode=dark]:[--fve-popup-active-background:var(--fve-brand-text)] data-[fve-color-mode=dark]:[--fve-popup-active-text:var(--fve-surface)]
        dark:not-data-[fve-color-mode=light]:[--fve-background:oklch(21%_0.034_264.665)] [.dark_&:not([data-fve-color-mode=light])]:[--fve-background:oklch(21%_0.034_264.665)] [&.dark]:[--fve-background:oklch(21%_0.034_264.665)] data-[fve-color-mode=dark]:[--fve-background:oklch(21%_0.034_264.665)]
        --fve-surface:oklch(24.4%_0.034_260.757) [.dark_&:not([data-fve-color-mode=light])]:[--fve-surface:oklch(24.4%_0.034_260.757)] [&.dark]:[--fve-surface:oklch(24.4%_0.034_260.757)] data-[fve-color-mode=dark]:[--fve-surface:oklch(24.4%_0.034_260.757)]
        --fve-surface-subtle:oklch(27.8%_0.033_256.848) [.dark_&:not([data-fve-color-mode=light])]:[--fve-surface-subtle:oklch(27.8%_0.033_256.848)] [&.dark]:[--fve-surface-subtle:oklch(27.8%_0.033_256.848)] data-[fve-color-mode=dark]:[--fve-surface-subtle:oklch(27.8%_0.033_256.848)]
        --fve-surface-hover:oklch(27.8%_0.033_256.848) [.dark_&:not([data-fve-color-mode=light])]:[--fve-surface-hover:oklch(27.8%_0.033_256.848)] [&.dark]:[--fve-surface-hover:oklch(27.8%_0.033_256.848)] data-[fve-color-mode=dark]:[--fve-surface-hover:oklch(27.8%_0.033_256.848)]
        --fve-surface-active:oklch(37.3%_0.034_259.733) [.dark_&:not([data-fve-color-mode=light])]:[--fve-surface-active:oklch(37.3%_0.034_259.733)] [&.dark]:[--fve-surface-active:oklch(37.3%_0.034_259.733)] data-[fve-color-mode=dark]:[--fve-surface-active:oklch(37.3%_0.034_259.733)]
        --fve-text:oklch(96.7%_0.003_264.542) [.dark_&:not([data-fve-color-mode=light])]:[--fve-text:oklch(96.7%_0.003_264.542)] [&.dark]:[--fve-text:oklch(96.7%_0.003_264.542)] data-[fve-color-mode=dark]:[--fve-text:oklch(96.7%_0.003_264.542)]
        --fve-muted-text:oklch(70.7%_0.022_261.325) [.dark_&:not([data-fve-color-mode=light])]:[--fve-muted-text:oklch(70.7%_0.022_261.325)] [&.dark]:[--fve-muted-text:oklch(70.7%_0.022_261.325)] data-[fve-color-mode=dark]:[--fve-muted-text:oklch(70.7%_0.022_261.325)]
        --fve-border:oklch(37.3%_0.034_259.733) [.dark_&:not([data-fve-color-mode=light])]:[--fve-border:oklch(37.3%_0.034_259.733)] [&.dark]:[--fve-border:oklch(37.3%_0.034_259.733)] data-[fve-color-mode=dark]:[--fve-border:oklch(37.3%_0.034_259.733)]
        --fve-neutral-subtle:oklch(27.8%_0.033_256.848) [.dark_&:not([data-fve-color-mode=light])]:[--fve-neutral-subtle:oklch(27.8%_0.033_256.848)] [&.dark]:[--fve-neutral-subtle:oklch(27.8%_0.033_256.848)] data-[fve-color-mode=dark]:[--fve-neutral-subtle:oklch(27.8%_0.033_256.848)]
        --fve-neutral-text:oklch(87.2%_0.01_258.338) [.dark_&:not([data-fve-color-mode=light])]:[--fve-neutral-text:oklch(87.2%_0.01_258.338)] [&.dark]:[--fve-neutral-text:oklch(87.2%_0.01_258.338)] data-[fve-color-mode=dark]:[--fve-neutral-text:oklch(87.2%_0.01_258.338)]
        --fve-positive-subtle:oklch(26.6%_0.065_152.934) [.dark_&:not([data-fve-color-mode=light])]:[--fve-positive-subtle:oklch(26.6%_0.065_152.934)] [&.dark]:[--fve-positive-subtle:oklch(26.6%_0.065_152.934)] data-[fve-color-mode=dark]:[--fve-positive-subtle:oklch(26.6%_0.065_152.934)]
        --fve-positive-text:oklch(87.1%_0.15_154.449) [.dark_&:not([data-fve-color-mode=light])]:[--fve-positive-text:oklch(87.1%_0.15_154.449)] [&.dark]:[--fve-positive-text:oklch(87.1%_0.15_154.449)] data-[fve-color-mode=dark]:[--fve-positive-text:oklch(87.1%_0.15_154.449)]
        --fve-warning-subtle:oklch(27.9%_0.077_45.635) [.dark_&:not([data-fve-color-mode=light])]:[--fve-warning-subtle:oklch(27.9%_0.077_45.635)] [&.dark]:[--fve-warning-subtle:oklch(27.9%_0.077_45.635)] data-[fve-color-mode=dark]:[--fve-warning-subtle:oklch(27.9%_0.077_45.635)]
        --fve-warning-text:oklch(90.5%_0.182_98.111) [.dark_&:not([data-fve-color-mode=light])]:[--fve-warning-text:oklch(90.5%_0.182_98.111)] [&.dark]:[--fve-warning-text:oklch(90.5%_0.182_98.111)] data-[fve-color-mode=dark]:[--fve-warning-text:oklch(90.5%_0.182_98.111)]
        --fve-critical-subtle:oklch(25.8%_0.092_26.042) [.dark_&:not([data-fve-color-mode=light])]:[--fve-critical-subtle:oklch(25.8%_0.092_26.042)] [&.dark]:[--fve-critical-subtle:oklch(25.8%_0.092_26.042)] data-[fve-color-mode=dark]:[--fve-critical-subtle:oklch(25.8%_0.092_26.042)]
        --fve-critical-text:oklch(80.8%_0.114_19.571) [.dark_&:not([data-fve-color-mode=light])]:[--fve-critical-text:oklch(80.8%_0.114_19.571)] [&.dark]:[--fve-critical-text:oklch(80.8%_0.114_19.571)] data-[fve-color-mode=dark]:[--fve-critical-text:oklch(80.8%_0.114_19.571)]
        --fve-info-subtle:oklch(28.2%_0.091_267.935) [.dark_&:not([data-fve-color-mode=light])]:[--fve-info-subtle:oklch(28.2%_0.091_267.935)] [&.dark]:[--fve-info-subtle:oklch(28.2%_0.091_267.935)] data-[fve-color-mode=dark]:[--fve-info-subtle:oklch(28.2%_0.091_267.935)]
        --fve-info-text:oklch(80.9%_0.105_251.813) [.dark_&:not([data-fve-color-mode=light])]:[--fve-info-text:oklch(80.9%_0.105_251.813)] [&.dark]:[--fve-info-text:oklch(80.9%_0.105_251.813)] data-[fve-color-mode=dark]:[--fve-info-text:oklch(80.9%_0.105_251.813)]
    """

    let sky = normalize """
        fve-theme-sky [--fve-brand-subtle:oklch(97.7%_0.013_236.62)] [--fve-brand-solid:oklch(50%_0.134_242.749)] [--fve-brand-hover:oklch(44.3%_0.11_240.79)] [--fve-brand-active:oklch(39.1%_0.09_240.876)] [--fve-brand-text:oklch(44.3%_0.11_240.79)] [--fve-brand-ring:oklch(55.4%_0.135_241.966)]
        --fve-brand-subtle:oklch(29.3%_0.066_243.157) [.dark_&:not([data-fve-color-mode=light])]:[--fve-brand-subtle:oklch(29.3%_0.066_243.157)] [&.dark]:[--fve-brand-subtle:oklch(29.3%_0.066_243.157)] data-[fve-color-mode=dark]:[--fve-brand-subtle:oklch(29.3%_0.066_243.157)] --fve-brand-solid:oklch(55.4%_0.135_241.966) [.dark_&:not([data-fve-color-mode=light])]:[--fve-brand-solid:oklch(55.4%_0.135_241.966)] [&.dark]:[--fve-brand-solid:oklch(55.4%_0.135_241.966)] data-[fve-color-mode=dark]:[--fve-brand-solid:oklch(55.4%_0.135_241.966)] --fve-brand-hover:oklch(65.5%_0.15_237.323) [.dark_&:not([data-fve-color-mode=light])]:[--fve-brand-hover:oklch(65.5%_0.15_237.323)] [&.dark]:[--fve-brand-hover:oklch(65.5%_0.15_237.323)] data-[fve-color-mode=dark]:[--fve-brand-hover:oklch(65.5%_0.15_237.323)] --fve-brand-active:oklch(74.6%_0.16_232.661) [.dark_&:not([data-fve-color-mode=light])]:[--fve-brand-active:oklch(74.6%_0.16_232.661)] [&.dark]:[--fve-brand-active:oklch(74.6%_0.16_232.661)] data-[fve-color-mode=dark]:[--fve-brand-active:oklch(74.6%_0.16_232.661)] --fve-brand-text:oklch(82.8%_0.111_230.318) [.dark_&:not([data-fve-color-mode=light])]:[--fve-brand-text:oklch(82.8%_0.111_230.318)] [&.dark]:[--fve-brand-text:oklch(82.8%_0.111_230.318)] data-[fve-color-mode=dark]:[--fve-brand-text:oklch(82.8%_0.111_230.318)] --fve-brand-ring:oklch(74.6%_0.16_232.661) [.dark_&:not([data-fve-color-mode=light])]:[--fve-brand-ring:oklch(74.6%_0.16_232.661)] [&.dark]:[--fve-brand-ring:oklch(74.6%_0.16_232.661)] data-[fve-color-mode=dark]:[--fve-brand-ring:oklch(74.6%_0.16_232.661)]
    """

    let emerald = normalize """
        fve-theme-emerald [--fve-brand-subtle:oklch(97.9%_0.021_166.113)] [--fve-brand-solid:oklch(59.6%_0.145_163.225)] [--fve-brand-hover:oklch(50.8%_0.118_165.612)] [--fve-brand-active:oklch(43.2%_0.095_166.913)] [--fve-brand-text:oklch(43.2%_0.095_166.913)] [--fve-brand-ring:oklch(50.8%_0.118_165.612)]
        --fve-brand-subtle:oklch(26.2%_0.051_172.552) [.dark_&:not([data-fve-color-mode=light])]:[--fve-brand-subtle:oklch(26.2%_0.051_172.552)] [&.dark]:[--fve-brand-subtle:oklch(26.2%_0.051_172.552)] data-[fve-color-mode=dark]:[--fve-brand-subtle:oklch(26.2%_0.051_172.552)] --fve-brand-solid:oklch(56.5%_0.128_163.225) [.dark_&:not([data-fve-color-mode=light])]:[--fve-brand-solid:oklch(56.5%_0.128_163.225)] [&.dark]:[--fve-brand-solid:oklch(56.5%_0.128_163.225)] data-[fve-color-mode=dark]:[--fve-brand-solid:oklch(56.5%_0.128_163.225)] --fve-brand-hover:oklch(66.5%_0.154_162.48) [.dark_&:not([data-fve-color-mode=light])]:[--fve-brand-hover:oklch(66.5%_0.154_162.48)] [&.dark]:[--fve-brand-hover:oklch(66.5%_0.154_162.48)] data-[fve-color-mode=dark]:[--fve-brand-hover:oklch(66.5%_0.154_162.48)] --fve-brand-active:oklch(76.5%_0.177_163.223) [.dark_&:not([data-fve-color-mode=light])]:[--fve-brand-active:oklch(76.5%_0.177_163.223)] [&.dark]:[--fve-brand-active:oklch(76.5%_0.177_163.223)] data-[fve-color-mode=dark]:[--fve-brand-active:oklch(76.5%_0.177_163.223)] --fve-brand-text:oklch(84.5%_0.143_164.978) [.dark_&:not([data-fve-color-mode=light])]:[--fve-brand-text:oklch(84.5%_0.143_164.978)] [&.dark]:[--fve-brand-text:oklch(84.5%_0.143_164.978)] data-[fve-color-mode=dark]:[--fve-brand-text:oklch(84.5%_0.143_164.978)] --fve-brand-ring:oklch(76.5%_0.177_163.223) [.dark_&:not([data-fve-color-mode=light])]:[--fve-brand-ring:oklch(76.5%_0.177_163.223)] [&.dark]:[--fve-brand-ring:oklch(76.5%_0.177_163.223)] data-[fve-color-mode=dark]:[--fve-brand-ring:oklch(76.5%_0.177_163.223)]
    """

    let amber = normalize """
        fve-theme-amber [--fve-brand-subtle:oklch(98.7%_0.022_95.277)] [--fve-brand-solid:oklch(66.6%_0.179_58.318)] [--fve-brand-hover:oklch(55.5%_0.163_48.998)] [--fve-brand-active:oklch(47.3%_0.137_46.201)] [--fve-brand-text:oklch(47.3%_0.137_46.201)] [--fve-brand-ring:oklch(55.5%_0.163_48.998)]
        --fve-brand-subtle:oklch(27.9%_0.077_45.635) [.dark_&:not([data-fve-color-mode=light])]:[--fve-brand-subtle:oklch(27.9%_0.077_45.635)] [&.dark]:[--fve-brand-subtle:oklch(27.9%_0.077_45.635)] data-[fve-color-mode=dark]:[--fve-brand-subtle:oklch(27.9%_0.077_45.635)] --fve-brand-solid:oklch(61%_0.17_55.5) [.dark_&:not([data-fve-color-mode=light])]:[--fve-brand-solid:oklch(61%_0.17_55.5)] [&.dark]:[--fve-brand-solid:oklch(61%_0.17_55.5)] data-[fve-color-mode=dark]:[--fve-brand-solid:oklch(61%_0.17_55.5)] --fve-brand-hover:oklch(70%_0.18_65) [.dark_&:not([data-fve-color-mode=light])]:[--fve-brand-hover:oklch(70%_0.18_65)] [&.dark]:[--fve-brand-hover:oklch(70%_0.18_65)] data-[fve-color-mode=dark]:[--fve-brand-hover:oklch(70%_0.18_65)] --fve-brand-active:oklch(80%_0.17_75) [.dark_&:not([data-fve-color-mode=light])]:[--fve-brand-active:oklch(80%_0.17_75)] [&.dark]:[--fve-brand-active:oklch(80%_0.17_75)] data-[fve-color-mode=dark]:[--fve-brand-active:oklch(80%_0.17_75)] --fve-brand-text:oklch(87.9%_0.169_91.605) [.dark_&:not([data-fve-color-mode=light])]:[--fve-brand-text:oklch(87.9%_0.169_91.605)] [&.dark]:[--fve-brand-text:oklch(87.9%_0.169_91.605)] data-[fve-color-mode=dark]:[--fve-brand-text:oklch(87.9%_0.169_91.605)] --fve-brand-ring:oklch(76.9%_0.188_70.08) [.dark_&:not([data-fve-color-mode=light])]:[--fve-brand-ring:oklch(76.9%_0.188_70.08)] [&.dark]:[--fve-brand-ring:oklch(76.9%_0.188_70.08)] data-[fve-color-mode=dark]:[--fve-brand-ring:oklch(76.9%_0.188_70.08)]
    """

    let cyan = normalize """
        fve-theme-cyan [--fve-brand-subtle:oklch(98.4%_0.019_200.873)] [--fve-brand-solid:oklch(60.9%_0.126_221.723)] [--fve-brand-hover:oklch(52%_0.105_223.128)] [--fve-brand-active:oklch(45%_0.085_224.283)] [--fve-brand-text:oklch(45%_0.085_224.283)] [--fve-brand-ring:oklch(52%_0.105_223.128)]
        --fve-brand-subtle:oklch(30.2%_0.056_229.695) [.dark_&:not([data-fve-color-mode=light])]:[--fve-brand-subtle:oklch(30.2%_0.056_229.695)] [&.dark]:[--fve-brand-subtle:oklch(30.2%_0.056_229.695)] data-[fve-color-mode=dark]:[--fve-brand-subtle:oklch(30.2%_0.056_229.695)] --fve-brand-solid:oklch(60.9%_0.126_221.723) [.dark_&:not([data-fve-color-mode=light])]:[--fve-brand-solid:oklch(60.9%_0.126_221.723)] [&.dark]:[--fve-brand-solid:oklch(60.9%_0.126_221.723)] data-[fve-color-mode=dark]:[--fve-brand-solid:oklch(60.9%_0.126_221.723)] --fve-brand-hover:oklch(71.5%_0.143_215.221) [.dark_&:not([data-fve-color-mode=light])]:[--fve-brand-hover:oklch(71.5%_0.143_215.221)] [&.dark]:[--fve-brand-hover:oklch(71.5%_0.143_215.221)] data-[fve-color-mode=dark]:[--fve-brand-hover:oklch(71.5%_0.143_215.221)] --fve-brand-active:oklch(78.9%_0.154_211.53) [.dark_&:not([data-fve-color-mode=light])]:[--fve-brand-active:oklch(78.9%_0.154_211.53)] [&.dark]:[--fve-brand-active:oklch(78.9%_0.154_211.53)] data-[fve-color-mode=dark]:[--fve-brand-active:oklch(78.9%_0.154_211.53)] --fve-brand-text:oklch(86.5%_0.127_207.078) [.dark_&:not([data-fve-color-mode=light])]:[--fve-brand-text:oklch(86.5%_0.127_207.078)] [&.dark]:[--fve-brand-text:oklch(86.5%_0.127_207.078)] data-[fve-color-mode=dark]:[--fve-brand-text:oklch(86.5%_0.127_207.078)] --fve-brand-ring:oklch(71.5%_0.143_215.221) [.dark_&:not([data-fve-color-mode=light])]:[--fve-brand-ring:oklch(71.5%_0.143_215.221)] [&.dark]:[--fve-brand-ring:oklch(71.5%_0.143_215.221)] data-[fve-color-mode=dark]:[--fve-brand-ring:oklch(71.5%_0.143_215.221)]
    """

    let neutral = normalize """
        fve-theme-neutral [--fve-brand-subtle:oklch(96.7%_0.003_264.542)] [--fve-brand-solid:oklch(44.6%_0.03_256.802)] [--fve-brand-hover:oklch(37.3%_0.034_259.733)] [--fve-brand-active:oklch(27.8%_0.033_256.848)] [--fve-brand-text:oklch(27.8%_0.033_256.848)] [--fve-brand-ring:oklch(55.4%_0.046_257.417)]
        --fve-brand-subtle:oklch(27.8%_0.033_256.848) [.dark_&:not([data-fve-color-mode=light])]:[--fve-brand-subtle:oklch(27.8%_0.033_256.848)] [&.dark]:[--fve-brand-subtle:oklch(27.8%_0.033_256.848)] data-[fve-color-mode=dark]:[--fve-brand-subtle:oklch(27.8%_0.033_256.848)] --fve-brand-solid:oklch(70.7%_0.022_261.325) [.dark_&:not([data-fve-color-mode=light])]:[--fve-brand-solid:oklch(70.7%_0.022_261.325)] [&.dark]:[--fve-brand-solid:oklch(70.7%_0.022_261.325)] data-[fve-color-mode=dark]:[--fve-brand-solid:oklch(70.7%_0.022_261.325)] --fve-brand-hover:oklch(87.2%_0.01_258.338) [.dark_&:not([data-fve-color-mode=light])]:[--fve-brand-hover:oklch(87.2%_0.01_258.338)] [&.dark]:[--fve-brand-hover:oklch(87.2%_0.01_258.338)] data-[fve-color-mode=dark]:[--fve-brand-hover:oklch(87.2%_0.01_258.338)] --fve-brand-active:oklch(92.8%_0.006_264.531) [.dark_&:not([data-fve-color-mode=light])]:[--fve-brand-active:oklch(92.8%_0.006_264.531)] [&.dark]:[--fve-brand-active:oklch(92.8%_0.006_264.531)] data-[fve-color-mode=dark]:[--fve-brand-active:oklch(92.8%_0.006_264.531)] --fve-brand-text:oklch(96.7%_0.003_264.542) [.dark_&:not([data-fve-color-mode=light])]:[--fve-brand-text:oklch(96.7%_0.003_264.542)] [&.dark]:[--fve-brand-text:oklch(96.7%_0.003_264.542)] data-[fve-color-mode=dark]:[--fve-brand-text:oklch(96.7%_0.003_264.542)] --fve-brand-ring:oklch(70.7%_0.022_261.325) [.dark_&:not([data-fve-color-mode=light])]:[--fve-brand-ring:oklch(70.7%_0.022_261.325)] [&.dark]:[--fve-brand-ring:oklch(70.7%_0.022_261.325)] data-[fve-color-mode=dark]:[--fve-brand-ring:oklch(70.7%_0.022_261.325)]
    """

/// <summary>
/// Shared announcement policy for persistent notices and dismissible notifications.
/// </summary>
/// <category>notice</category>
[<RequireQualifiedAccess>]
type LiveAnnouncement =
    | Static
    | Polite
    | Assertive

/// <category>theming</category>
[<RequireQualifiedAccess>]
type Size =
    | ExtraSmall
    | Small
    | Medium
    | Large
    | ExtraLarge

/// <category>theming</category>
[<RequireQualifiedAccess>]
module Size =
    let internal name = function
        | Size.ExtraSmall -> "extra-small" | Size.Small -> "small" | Size.Medium -> "medium"
        | Size.Large -> "large" | Size.ExtraLarge -> "extra-large"

    let internal tryParse = function
        | "extra-small" -> Some Size.ExtraSmall | "small" -> Some Size.Small | "medium" -> Some Size.Medium
        | "large" -> Some Size.Large | "extra-large" -> Some Size.ExtraLarge | _ -> None

    /// Apply the 24/28/32/36/40px control scale independently of layout density.
    let className = function
        | Size.ExtraSmall -> "fve-control-extra-small [--fve-control-min-height:1.5rem] [--fve-control-icon-size:0.75rem] [--fve-control-padding-block:0.25rem] [--fve-control-padding-inline:0.5rem] [--fve-control-font-size:0.75rem] [--fve-control-line-height:1rem]"
        | Size.Small -> "fve-control-small [--fve-control-min-height:1.75rem] [--fve-control-icon-size:1rem] [--fve-control-padding-block:0.25rem] [--fve-control-padding-inline:0.5rem] [--fve-control-font-size:0.875rem] [--fve-control-line-height:1.25rem]"
        | Size.Medium -> "fve-control-medium [--fve-control-min-height:2rem] [--fve-control-icon-size:1rem] [--fve-control-padding-block:0.375rem] [--fve-control-padding-inline:0.625rem] [--fve-control-font-size:0.875rem] [--fve-control-line-height:1.25rem]"
        | Size.Large -> "fve-control-large [--fve-control-min-height:2.25rem] [--fve-control-icon-size:1.25rem] [--fve-control-padding-block:0.5rem] [--fve-control-padding-inline:0.75rem] [--fve-control-font-size:0.875rem] [--fve-control-line-height:1.25rem]"
        | Size.ExtraLarge -> "fve-control-extra-large [--fve-control-min-height:2.5rem] [--fve-control-icon-size:1.25rem] [--fve-control-padding-block:0.625rem] [--fve-control-padding-inline:0.875rem] [--fve-control-font-size:0.875rem] [--fve-control-line-height:1.25rem]"

/// <category>theming</category>
[<RequireQualifiedAccess>]
type Color = Primary | Secondary | Success | Warning | Error | Info | Neutral | Custom

/// <category>button</category>
[<RequireQualifiedAccess>]
type ButtonVariant = Solid | Soft | Outline | Ghost

/// <category>link</category>
[<RequireQualifiedAccess>]
type LinkVariant = Text | Solid | Soft | Outline | Ghost

/// <category>badge</category>
[<RequireQualifiedAccess>]
type BadgeVariant = Solid | Soft | Outline | Ghost

/// <category>theming</category>
[<RequireQualifiedAccess>]
type Orientation = Horizontal | Vertical

/// <category>theming</category>
[<RequireQualifiedAccess>]
type Radius =
    | None
    | Medium
    | Large
    | Full

/// <category>theming</category>
[<RequireQualifiedAccess>]
type Density =
    | Compact
    | Comfortable

/// <summary>Typed component values serialized as ordinary HTML attributes. Native HTML overloads remain available.</summary>
/// <category>theming</category>
[<AutoOpen>]
module ComponentAttributes =
    type Html with
        static member _color(value:Color) =
            Html._attr("color", match value with Color.Primary -> "primary" | Color.Secondary -> "secondary" | Color.Success -> "success" | Color.Warning -> "warning" | Color.Error -> "error" | Color.Info -> "info" | Color.Neutral -> "neutral" | Color.Custom -> "custom")
        static member _variant(value:ButtonVariant) =
            Html._attr("variant", match value with ButtonVariant.Solid -> "solid" | ButtonVariant.Soft -> "soft" | ButtonVariant.Outline -> "outline" | ButtonVariant.Ghost -> "ghost")
        static member _variant(value:LinkVariant) =
            Html._attr("variant", match value with LinkVariant.Text -> "text" | LinkVariant.Solid -> "solid" | LinkVariant.Soft -> "soft" | LinkVariant.Outline -> "outline" | LinkVariant.Ghost -> "ghost")
        static member _variant(value:BadgeVariant) =
            Html._attr("variant", match value with BadgeVariant.Solid -> "solid" | BadgeVariant.Soft -> "soft" | BadgeVariant.Outline -> "outline" | BadgeVariant.Ghost -> "ghost")
        static member _size(value:Size) =
            Html._attr("size", Size.name value)
        static member _values(value:string list) = Html._attr("values", System.Text.Json.JsonSerializer.Serialize value)
        static member _expanded(value:bool) = Html._attr("expanded", if value then "true" else "false")
        static member _clearable(value:bool) = Html._attr("clearable", if value then "true" else "false")
        static member _announcement(value:LiveAnnouncement) = Html._attr("announcement", match value with LiveAnnouncement.Static -> "static" | LiveAnnouncement.Polite -> "polite" | LiveAnnouncement.Assertive -> "assertive")
        static member _description(value:string) = Html._attr("description", value)
        static member _trend(value:string) = Html._attr("trend", value)
        static member _orientation(value:Orientation) =
            Html._attr("orientation", match value with Orientation.Horizontal -> "horizontal" | Orientation.Vertical -> "vertical")
        static member _density(value:Density) =
            Html._attr("density", match value with Density.Compact -> "compact" | Density.Comfortable -> "comfortable")

/// <category>theming</category>
[<NoEquality; NoComparison>]
type ComponentsTheme =
    private
        { paletteClass:string
          radiusClass:string
          densityClass:string
          controlSizeClass:string }

/// <category>theming</category>
[<RequireQualifiedAccess>]
module ComponentsTheme =
    let private comfortableDensity = "fve-density-comfortable [--fve-navigation-padding-block:0.5rem] [--fve-navigation-min-height:2.25rem] [--fve-shell-bar-min-height:4rem]"

    let sky =
        { paletteClass = ThemeClasses.sky
          radiusClass = "fve-radius-large"
          densityClass = comfortableDensity
          controlSizeClass = Size.className Size.Medium }

    let emerald =
        { paletteClass = ThemeClasses.emerald
          radiusClass = "fve-radius-large"
          densityClass = comfortableDensity
          controlSizeClass = Size.className Size.Medium }

    let amber =
        { paletteClass = ThemeClasses.amber
          radiusClass = "fve-radius-large"
          densityClass = comfortableDensity
          controlSizeClass = Size.className Size.Medium }

    let cyan =
        { paletteClass = ThemeClasses.cyan
          radiusClass = "fve-radius-large"
          densityClass = comfortableDensity
          controlSizeClass = Size.className Size.Medium }

    let neutral =
        { paletteClass = ThemeClasses.neutral
          radiusClass = "fve-radius-large"
          densityClass = comfortableDensity
          controlSizeClass = Size.className Size.Medium }

    let custom paletteClass =
        if String.IsNullOrWhiteSpace paletteClass || Regex.IsMatch(paletteClass, "\\s") then
            invalidArg (nameof paletteClass) "A custom theme requires one non-empty CSS class."
        { paletteClass = paletteClass
          radiusClass = "fve-radius-large"
          densityClass = comfortableDensity
          controlSizeClass = Size.className Size.Medium }

    let withRadius radius theme =
        let radiusClass =
            match radius with
            | Radius.None -> "fve-radius-none [--fve-radius-control:0] [--fve-radius-panel:0]"
            | Radius.Medium -> "fve-radius-medium [--fve-radius-control:0.375rem] [--fve-radius-panel:0.5rem]"
            | Radius.Large -> "fve-radius-large [--fve-radius-control:0.5rem] [--fve-radius-panel:0.75rem]"
            | Radius.Full -> "fve-radius-full [--fve-radius-control:9999px] [--fve-radius-panel:1rem]"
        { theme with radiusClass = radiusClass }

    let withDensity density theme =
        let densityClass =
            match density with
            | Density.Compact -> "fve-density-compact [--fve-navigation-padding-block:0.25rem] [--fve-navigation-min-height:1.75rem] [--fve-shell-bar-min-height:3rem]"
            | Density.Comfortable -> comfortableDensity
        { theme with densityClass = densityClass }

    let withControlSize size theme =
        { theme with controlSizeClass = Size.className size }

    let className theme =
        [ ThemeClasses.foundation; theme.paletteClass; theme.radiusClass; theme.densityClass; theme.controlSizeClass ]
        |> String.concat " "

    let attributes theme =
        [ _class (className theme) ]

/// <summary>
/// Explicit CSS colors for one mode. Check contrast for every foreground/background pair.
/// </summary>
/// <category>theming</category>
type ColorPaletteMode =
    { solid:string
      solidHover:string
      solidActive:string
      onSolid:string
      soft:string
      softHover:string
      softActive:string
      text:string
      border:string
      focus:string }

/// <summary>
/// A custom palette supplies both modes; component-owned Color unions select it with Custom.
/// </summary>
/// <category>theming</category>
type ColorPalette =
    { light:ColorPaletteMode
      dark:ColorPaletteMode }

/// <category>theming</category>
[<AutoOpen>]
module PaletteAttributes =
    type Html with
        static member _palette(value:ColorPalette) = Html._attr("palette", System.Text.Json.JsonSerializer.Serialize value)

module internal ComponentColors =
    let private mode solid onSolid soft mix =
        { solid = solid
          solidHover = $"color-mix(in srgb, {solid} 85%%, {mix})"
          solidActive = $"color-mix(in srgb, {solid} 70%%, {mix})"
          onSolid = onSolid
          soft = soft
          softHover = $"color-mix(in srgb, {soft} 90%%, {solid})"
          softActive = $"color-mix(in srgb, {soft} 80%%, {solid})"
          text = solid
          border = solid
          focus = solid }

    // Defaults are contrast-aware pairs, not arbitrary user colors. Each token can be
    // overridden independently by a consumer's theme; Custom requires explicit pairs.
    let private tokens prefix light dark =
        let apply (value:ColorPaletteMode) =
            let token name fallback = $"var(--fve-{prefix}-{name}, {fallback})"
            { solid = token "solid" value.solid
              solidHover = token "hover" value.solidHover
              solidActive = token "active" value.solidActive
              onSolid = token "on-solid" value.onSolid
              soft = token "soft" value.soft
              softHover = token "soft-hover" value.softHover
              softActive = token "soft-active" value.softActive
              text = token "text" value.text
              border = token "border" value.border
              focus = token "focus" value.focus }
        { light = apply light; dark = apply dark }

    let primary = tokens "primary" (mode "var(--fve-brand-text)" "#ffffff" "var(--fve-brand-subtle)" "#000000") (mode "var(--fve-brand-text)" "#101828" "var(--fve-brand-subtle)" "#ffffff")
    let secondary = tokens "secondary" (mode "#6d28d9" "#ffffff" "#f5f3ff" "#000000") (mode "#c4b5fd" "#101828" "#2e1065" "#ffffff")
    let success = tokens "success" (mode "#15803d" "#ffffff" "#f0fdf4" "#000000") (mode "#86efac" "#101828" "#052e16" "#ffffff")
    let warning = tokens "warning" (mode "#92400e" "#ffffff" "#fffbeb" "#000000") (mode "#fde68a" "#101828" "#451a03" "#ffffff")
    let error = tokens "error" (mode "#b91c1c" "#ffffff" "#fef2f2" "#000000") (mode "#fca5a5" "#101828" "#450a0a" "#ffffff")
    let info = tokens "info" (mode "#1d4ed8" "#ffffff" "#eff6ff" "#000000") (mode "#93c5fd" "#101828" "#172554" "#ffffff")
    let neutral = tokens "neutral" (mode "#374151" "#ffffff" "var(--fve-surface-subtle)" "#000000") (mode "#e5e7eb" "#101828" "var(--fve-surface-subtle)" "#ffffff")

    let style (palette:ColorPalette) (attributes:HtmlAttribute list) =
        let values (colors:ColorPaletteMode) =
            [ "solid", colors.solid; "hover", colors.solidHover; "active", colors.solidActive
              "on-solid", colors.onSolid; "soft", colors.soft; "soft-hover", colors.softHover
              "soft-active", colors.softActive; "text", colors.text; "border", colors.border; "focus", colors.focus ]
        let consumerStyle =
            attributes
            |> List.choose (fun attribute ->
                if String.Equals(attribute.Name, "style", StringComparison.OrdinalIgnoreCase) then
                    attribute.Value |> ValueOption.toOption
                else None)
        let colors =
            List.zip (values palette.light) (values palette.dark)
            |> List.map (fun ((name, light), (_, dark)) -> $"--fve-color-{name}:light-dark({light},{dark})")
        String.concat ";" (consumerStyle @ colors)

    let solid = "bg-[var(--fve-color-solid)] text-[var(--fve-color-on-solid)]"
    let soft = "bg-[var(--fve-color-soft)] text-[var(--fve-color-text)]"
    let outline = "bg-transparent text-[var(--fve-color-text)] ring-1 ring-inset ring-[color-mix(in_srgb,var(--fve-color-border)_20%,transparent)]"
    let ghost = "bg-transparent text-[var(--fve-color-text)]"
    let solidInteraction = "enabled:hover:bg-[var(--fve-color-hover)] enabled:active:bg-[var(--fve-color-active)]"
    let softInteraction = "enabled:hover:bg-[var(--fve-color-soft-hover)] enabled:active:bg-[var(--fve-color-soft-active)]"
    let focus = "focus-visible:ring-[var(--fve-color-focus)] focus-visible:ring-offset-[var(--fve-surface)]"
module internal ActionStyles =
    let geometryClasses = "min-h-[var(--fve-control-min-height)] min-w-[var(--fve-control-min-height)] p-[max(0px,calc((var(--fve-control-min-height)-var(--fve-control-line-height))/2-var(--fve-control-border-width,0px)))] text-[length:var(--fve-control-font-size)] leading-[var(--fve-control-line-height)]"

    let iconClasses = "[&_svg]:size-[var(--fve-control-icon-size,1rem)] [&_svg]:shrink-0"

    let baseClasses =
        iconClasses + " inline-flex items-center justify-center gap-2 rounded-[var(--fve-radius-control)] font-medium shadow-xs outline-none transition-[color,background-color,border-color,box-shadow,translate] duration-100 motion-reduce:transition-none motion-safe:[&:not(:disabled):not([aria-disabled=true]):not([aria-busy=true]):not([aria-haspopup]):not([data-fve-keyboard-pressed])]:active:translate-y-px motion-safe:[&:not(:disabled):not([aria-disabled=true]):not([aria-busy=true]):not([aria-haspopup])]:data-[fve-keyboard-pressed=true]:translate-y-px focus-visible:ring-2 focus-visible:ring-offset-2 disabled:pointer-events-none disabled:opacity-50"

module internal ControlGroup =
    // A logical segment exports geometry to marked native controls, regardless of wrapper depth.
    // Popovers and dialogs are independent surfaces, not segments of their opener's group.
    let private controls =
        "[&_[data-fve-group-control]]:relative [&_[data-fve-group-control]]:shrink-0 [&_[data-fve-group-control]:focus-visible]:z-10 [&_[data-fve-group-control]]:[border-start-start-radius:var(--fve-group-radius-ss,var(--fve-radius-control))] [&_[data-fve-group-control]]:[border-start-end-radius:var(--fve-group-radius-se,var(--fve-radius-control))] [&_[data-fve-group-control]]:[border-end-start-radius:var(--fve-group-radius-es,var(--fve-radius-control))] [&_[data-fve-group-control]]:[border-end-end-radius:var(--fve-group-radius-ee,var(--fve-radius-control))] [&_[data-fve-group-control=outline]]:[border-inline-start-width:var(--fve-group-border-inline-start,1px)] [&_[data-fve-group-control=outline]]:[border-block-start-width:var(--fve-group-border-block-start,1px)]"

    let private surfaces =
        "[&_:is([popover],dialog)]:[--fve-group-radius-ss:initial] [&_:is([popover],dialog)]:[--fve-group-radius-se:initial] [&_:is([popover],dialog)]:[--fve-group-radius-es:initial] [&_:is([popover],dialog)]:[--fve-group-radius-ee:initial] [&_:is([popover],dialog)]:[--fve-group-border-inline-start:initial] [&_:is([popover],dialog)]:[--fve-group-border-block-start:initial]"

    let private geometry = function
        | "vertical" ->
            "flex-col [&>*]:[--fve-group-radius-ss:0px] [&>*]:[--fve-group-radius-se:0px] [&>*]:[--fve-group-radius-es:0px] [&>*]:[--fve-group-radius-ee:0px] [&>:first-child]:[--fve-group-radius-ss:var(--fve-radius-control)] [&>:first-child]:[--fve-group-radius-se:var(--fve-radius-control)] [&>:last-child]:[--fve-group-radius-es:var(--fve-radius-control)] [&>:last-child]:[--fve-group-radius-ee:var(--fve-radius-control)] [&>:not(:first-child)]:[--fve-group-border-block-start:0px]"
        | _ ->
            "[&>*]:[--fve-group-radius-ss:0px] [&>*]:[--fve-group-radius-se:0px] [&>*]:[--fve-group-radius-es:0px] [&>*]:[--fve-group-radius-ee:0px] [&>:first-child]:[--fve-group-radius-ss:var(--fve-radius-control)] [&>:first-child]:[--fve-group-radius-es:var(--fve-radius-control)] [&>:last-child]:[--fve-group-radius-se:var(--fve-radius-control)] [&>:last-child]:[--fve-group-radius-ee:var(--fve-radius-control)] [&>:not(:first-child)]:[--fve-group-border-inline-start:0px]"

    let classes orientation =
        "isolate inline-flex items-stretch [&>*]:shrink-0 [&>*:focus-within]:relative [&>*:focus-within]:z-10 "
        + controls + " " + surfaces + " " + geometry orientation

module internal ComponentHtml =
    let classes values = values |> List.filter (String.IsNullOrWhiteSpace >> not) |> String.concat " "

    let horizontalScrollClasses = "min-w-0 max-w-full overflow-x-auto p-1 scroll-p-1 outline-none focus-visible:outline-2 focus-visible:outline-solid focus-visible:-outline-offset-2 focus-visible:outline-[var(--fve-brand-ring)] forced-colors:focus-visible:outline-[Highlight]"

    let horizontalScrollAttributes =
        [ _tabindex 0; _attr("scroll-step", "80"); _attr("follow-focus", "") ]

    let popupClasses = "border border-[var(--fve-border)] bg-[var(--fve-surface)] shadow-lg outline-none forced-colors:border-[CanvasText]"
    let popupFieldClasses = "outline-none focus-visible:outline-solid focus-visible:outline-2 focus-visible:-outline-offset-2 focus-visible:outline-[var(--fve-brand-ring)] forced-colors:focus-visible:outline-solid forced-colors:focus-visible:outline-[Highlight]"
    let popupControlClasses = "outline-none focus-visible:outline-solid focus-visible:outline-2 focus-visible:-outline-offset-2 focus-visible:outline-[var(--fve-brand-ring)] forced-colors:focus-visible:outline-solid forced-colors:focus-visible:outline-[Highlight]"
    let popupItemClasses =
        classes [
            "outline-none aria-selected:bg-[var(--fve-brand-subtle)] aria-selected:text-[var(--fve-brand-text)] aria-selected:font-semibold"
            "not-disabled:not-aria-disabled:focus:bg-[var(--fve-popup-active-background)] not-disabled:not-aria-disabled:focus:text-[var(--fve-popup-active-text)] not-disabled:not-aria-disabled:data-[active=true]:bg-[var(--fve-popup-active-background)] not-disabled:not-aria-disabled:data-[active=true]:text-[var(--fve-popup-active-text)]"
            "aria-selected:focus:bg-[var(--fve-popup-active-background)] aria-selected:focus:text-[var(--fve-popup-active-text)] aria-selected:data-[active=true]:bg-[var(--fve-popup-active-background)] aria-selected:data-[active=true]:text-[var(--fve-popup-active-text)]"
            "forced-colors:focus:outline-solid forced-colors:focus:outline-2 forced-colors:focus:-outline-offset-2 forced-colors:focus:outline-[Highlight] forced-colors:data-[active=true]:outline-solid forced-colors:data-[active=true]:outline-2 forced-colors:data-[active=true]:-outline-offset-2 forced-colors:data-[active=true]:outline-[Highlight]" ]

    let safeAttributes reservedNames attributes =
        attributes
        |> List.filter (fun attribute ->
            reservedNames
            |> List.exists (fun reservedName ->
                String.Equals(attribute.Name, reservedName, StringComparison.OrdinalIgnoreCase)
                || (reservedName.EndsWith(':') && attribute.Name.StartsWith(reservedName, StringComparison.OrdinalIgnoreCase)))
            |> not)

    // Structural Rocket hosts enhance caller HTML; declarations remain in the outer page scope.
    let outerSignals (attributes: HtmlAttribute list) =
        attributes |> List.map (fun attribute ->
            if attribute.Name = "data-signals" then { attribute with Name = "data-signals__root" }
            else attribute)

    let javascriptString value = System.Text.Json.JsonSerializer.Serialize(value)

    let optionToken (value:string) =
        value
        |> Encoding.UTF8.GetBytes
        |> Array.map (fun character -> character.ToString("x2"))
        |> String.concat ""
        |> (+) "v"

    let signalToken (value:string) =
        // Keep conventional wire names readable. All other IDs use a disjoint, lossless namespace.
        if Regex.IsMatch(value, "^[a-z][a-z0-9]*(?:-[a-z0-9]+)*$") then value.Replace('-', '_')
        else "_" + optionToken value

    let controlSizeClass size = size |> Option.map Size.className |> Option.defaultValue ""

    let sizeClasses size =
        classes [ controlSizeClass size; "min-h-[var(--fve-control-min-height)] px-[max(0px,calc(var(--fve-control-padding-inline)-var(--fve-control-border-width,0px)))] py-[max(0px,calc(var(--fve-control-padding-block)-var(--fve-control-border-width,0px)))] text-[length:var(--fve-control-font-size)] leading-[var(--fve-control-line-height)]" ]

    let loadingGlyph size =
        let sizeClass =
            match size with
            | Size.ExtraSmall -> "size-3"
            | Size.Small | Size.Medium -> "size-4"
            | Size.Large | Size.ExtraLarge -> "size-5"
        span {
            _ariaHidden "true"
            _class (classes [ "shrink-0 animate-spin rounded-full border-2 border-current border-r-transparent motion-reduce:animate-none"; sizeClass ])
        }

module internal ComponentElement =
    let attributes (items:HtmlAttribute list) =
        items
        |> List.filter (fun item -> not (String.IsNullOrEmpty item.Name))
        |> List.groupBy (fun item -> item.Name.ToLowerInvariant())
        |> List.map (fun (name, values) ->
            if name = "class" || name = "style" then
                Html._attr(name, values |> List.choose (fun item -> ValueOption.toOption item.Value) |> String.concat (if name = "style" then ";" else " "))
            else List.last values)

    let attribute name (items:HtmlAttribute list) =
        items |> List.tryFindBack (fun item -> String.Equals(item.Name, name, StringComparison.OrdinalIgnoreCase))

    let value name items = attribute name items |> Option.bind (fun item -> ValueOption.toOption item.Value)

    let size fallback items =
        match attribute "size" items with
        | None -> fallback
        | Some { Value = ValueSome value } ->
            match Size.tryParse value with
            | Some size -> Some size
            | None -> invalidArg "attributes" "Unsupported component size."
        | Some _ -> invalidArg "attributes" "A component size is required."

    let required name items =
        match value name items with
        | Some value when not (String.IsNullOrWhiteSpace value) -> value
        | _ -> invalidArg "attributes" $"A non-empty {name} attribute is required."

    let choice name fallback allowed items =
        match attribute name items with
        | None -> fallback
        | Some { Value = ValueSome value } when List.contains value allowed -> value
        | _ -> invalidArg "attributes" $"Unsupported {name} attribute."

    let render tag (defaults:HtmlAttribute list) (items:HtmlAttribute list) (children:HtmlElement list) =
        (Html.el tag) {
            for item in attributes (defaults @ items) do item
            children
        }

    let palette items =
        match choice "color" "neutral" ["primary"; "secondary"; "success"; "warning"; "error"; "info"; "neutral"; "custom"] items with
        | "primary" -> ComponentColors.primary
        | "secondary" -> ComponentColors.secondary
        | "success" -> ComponentColors.success
        | "warning" -> ComponentColors.warning
        | "error" -> ComponentColors.error
        | "info" -> ComponentColors.info
        | "custom" ->
            try
                let palette = System.Text.Json.JsonSerializer.Deserialize<ColorPalette>(required "palette" items)
                for colors in [palette.light; palette.dark] do
                    for color in [colors.solid; colors.solidHover; colors.solidActive; colors.onSolid; colors.soft; colors.softHover; colors.softActive; colors.text; colors.border; colors.focus] do
                        if String.IsNullOrWhiteSpace color then invalidArg "attributes" "A complete light/dark palette is required."
                palette
            with _ -> invalidArg "attributes" "A complete light/dark palette is required."
        | _ -> ComponentColors.neutral

module internal ComponentTrigger =
    let control owner authored (children: HtmlElement list) =
        let children =
            children
            |> List.filter (function
                | :? NoopElement -> false
                | _ -> true)

        let unsupported () =
            invalidArg
                "children"
                $"{owner} requires one fveButton control or an attribute-forwarding render function for a native/custom control."

        match children with
        | [ child ] ->
            match box child with
            | :? ComponentControlElement as target when target.TagName = "button" || target.TagName = "fve-button" ->
                target, ComponentElement.attributes (target.Attributes @ authored)
            | _ -> unsupported ()
        | _ -> unsupported ()

    let flag name attributes =
        match ComponentElement.attribute name attributes with
        | Some { Value = ValueNone } -> true
        | Some { Value = ValueSome value } -> value <> "false"
        | None -> false

    let unavailable attributes =
        flag "readonly" attributes || flag "aria-disabled" attributes

    let rec render (target: ComponentControlElement) attributes =
        let attributes = ComponentElement.attributes attributes

        let attributes =
            if target.TagName = "fve-button" then
                let id = ComponentElement.required "id" attributes
                // The host remains a Datastar boundary; IDs, focus and anchoring name its actual native control.
                Html._id (id + "-host")
                :: Html._data ("fve-control-id", id)
                :: ComponentHtml.safeAttributes [ "id"; "data-fve-control-id" ] attributes
            else
                attributes

        let element = target.WithAttributes attributes

        match element with
        | :? ComponentControlElement as control when control.TagName = "fve-button" -> render control control.Attributes
        | _ -> element
