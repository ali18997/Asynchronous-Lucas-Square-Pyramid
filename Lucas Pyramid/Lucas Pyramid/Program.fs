﻿open Akka
open Akka.FSharp
open Akka.Configuration
open System
open Akka.Actor

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
    

//time (fun () -> lucasPyramid 1000000 4)



type ParentMessage() = 
    [<DefaultValue>] val mutable endPoint: int
    [<DefaultValue>] val mutable length: int
    [<DefaultValue>] val mutable startFlag: bool
    [<DefaultValue>] val mutable replyFlag: bool
    [<DefaultValue>] val mutable replyVal: bool
  

type ChildMessage() = 
    [<DefaultValue>] val mutable start: int
    [<DefaultValue>] val mutable length: int

let system = System.create "system" <| Configuration.defaultConfig()

let child (childMailbox:Actor<ChildMessage>) = 
    let rec childLoop() = actor {
        let! msg = childMailbox.Receive()

        let mutable sum = 0
        let start = msg.start
        let length = msg.length
        let mutable retVal = false

        for i = start to start+length-1 do
                sum <- sum + i*i
        
        if sqrt (float sum) % float 1 = 0.0 then
            retVal <- true


        let sender = childMailbox.Sender()
        let reply = new ParentMessage()

        reply.replyFlag <- true
        reply.startFlag <- false
        reply.endPoint <- start
        reply.length <- length
        reply.replyVal <- retVal

        sender <! reply

        return! childLoop()
    }
    childLoop()

let parent (parentMailbox:Actor<ParentMessage>) =
    // parent actor spawns child - is the supervisor of the child
    let mutable startPoint = 0
    let mutable endPoint = 0
    let mutable length  = 0
    let workSize = 10000
    let rec parentLoop() = actor {
        let! (msg: ParentMessage) = parentMailbox.Receive()
        if(msg.startFlag) then
            startPoint <- 1
            endPoint <- msg.endPoint
            length <- msg.length
            let mutable tillPoint = 0
            if endPoint > workSize then
                tillPoint <- workSize
            else 
                tillPoint <- endPoint
            for i = 1 to tillPoint do
                printfn "Send %i" i
                let name:string = "child" + i.ToString()
                let child = spawn parentMailbox name child
                let sendMessage = new ChildMessage()
                sendMessage.start <- i
                sendMessage.length <- msg.length
                child <! sendMessage
                startPoint <- i
        
        if(msg.replyFlag) then
            printf "Receive %i" msg.endPoint
            printfn " %b" msg.replyVal
            if startPoint < endPoint then
                startPoint <- startPoint + 1
                printfn "Send %i" startPoint
                let sender = parentMailbox.Sender()
                let sendMessage = new ChildMessage()
                sendMessage.start <- startPoint
                sendMessage.length <- msg.length
                sender <! sendMessage
            if msg.replyVal or startPoint = endPoint then
                printfn "%b" msg.replyVal

        return! parentLoop()
    }
    parentLoop()
                        

let mainMessage = new ParentMessage()
mainMessage.endPoint <- 100000
mainMessage.length <- 4
mainMessage.startFlag <- true
mainMessage.replyFlag <- false
mainMessage.replyVal <- false

let parentActor = spawn system "parent" parent
parentActor <! mainMessage

//time (fun () -> parentActor <! mainMessage)
System.Console.ReadKey() |> ignore
system.Terminate()
