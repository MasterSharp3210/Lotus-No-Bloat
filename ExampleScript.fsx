
// Example Script (.fsx) for Lotus No-Bloat

// ScriptDetails class
type ScriptDetails() =
    member val Name = "ExampleScript" with get, set
    member val Author = "ReservedGalatea" with get, set
    member val Version = "1.0" with get, set

// ScriptLoad class
type ScriptLoad() =
    member val RemoveApps = [| "Xbox"; "BingNews"; "BingWeather"; "MicrosoftSolitareCollection"; "People"; "GetHelp"; "Getstarted"; "ZuneMusic"; "ZuneVideo"; "SkypeApp"; "Clipchamp"; "YourPhone"; "MicrosoftTeams" |] with get, set
    member val CleanTemp = true with get, set
    member val CleanPrefetch = true with get, set
    member val CleanINet = true with get, set
    member val CleanDXCache = true with get, set
    member val CleanWUCache = true with get, set

// Properties for C# to parse
let details = ScriptDetails()
let load = ScriptLoad()

printfn "Name:%s" details.Name
printfn "Author:%s" details.Author
printfn "Version:%s" details.Version

printfn "RemoveApps:%s" (String.concat "," load.RemoveApps)

if load.CleanTemp then printfn "CleanTemp:true"
if load.CleanPrefetch then printfn "CleanPrefetch:true"
if load.CleanINet then printfn "CleanINet:true"
if load.CleanDXCache then printfn "CleanDXCache:true"
if load.WUCache then printfn "CleanWUCache:true"