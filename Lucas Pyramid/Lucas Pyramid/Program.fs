open Akka
open Akka.FSharp
open Akka.Configuration
open System


let check start length = 
    let mutable sum = 0

    for i = start to start+length-1 do
        sum <- sum + i*i

    if sqrt (float sum) % float 1 = 0.0 then
        true
    else
        false

let lucasPyramid endPoint length = 
    let mutable retVal = -1
    let mutable flag = true
    for i = 1 to endPoint do
        if check i length then
            if flag then
                retVal <- i
                flag <- false

    retVal

let a = lucasPyramid 100000000 4
printfn "%i"a