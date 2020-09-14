open Akka
open Akka.FSharp
open Akka.Configuration
open System

open System.Diagnostics

let time f = 
    let proc = Process.GetCurrentProcess()
    let cpu_time_stamp = proc.TotalProcessorTime
    let timer = new Stopwatch()
    timer.Start()
    try
        f()
    finally
        let cpu_time = (proc.TotalProcessorTime-cpu_time_stamp).TotalMilliseconds
        printfn "CPU time = %dms" (int64 cpu_time)
        printfn "Absolute time = %dms" timer.ElapsedMilliseconds


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
    

time (fun () -> lucasPyramid 1000000 4)

