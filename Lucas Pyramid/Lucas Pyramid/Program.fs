open Akka
open Akka.FSharp
open System

open System.Diagnostics

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

let proc = Process.GetCurrentProcess()
let cpu_time_stamp = proc.TotalProcessorTime
let timer = new Stopwatch()
timer.Start()

let parent (parentMailbox:Actor<ParentMessage>) =
    // parent actor spawns child - is the supervisor of the child
    let mutable startPoint = 0
    let mutable endPoint = 0
    let mutable length  = 0
    let mutable result = false
    let mutable workSize = 10
    let rec parentLoop() = actor {
        let! (msg: ParentMessage) = parentMailbox.Receive()
        if(msg.startFlag) then
            startPoint <- 1
            endPoint <- msg.endPoint
            length <- msg.length
            if endPoint <= workSize then
                workSize<- endPoint
            for i = 1 to workSize do
                let name:string = "child" + i.ToString()
                let child = spawn parentMailbox name child
                let sendMessage = new ChildMessage()
                sendMessage.start <- i
                sendMessage.length <- msg.length
                child <! sendMessage
                startPoint <- i
        
        if(msg.replyFlag) then
            if msg.replyVal then
                printfn "%i" msg.endPoint
                result <- true

            if startPoint < endPoint then
                startPoint <- startPoint + 1
                let sender = parentMailbox.Sender()
                let sendMessage = new ChildMessage()
                sendMessage.start <- startPoint
                sendMessage.length <- msg.length
                sender <! sendMessage
            else
                workSize <- workSize - 1
                if workSize = 0 then
                    if result = false then
                        printfn "No sequence found"
                    let cpu_time = (proc.TotalProcessorTime-cpu_time_stamp).TotalMilliseconds
                    printfn ""
                    printfn "CPU time = %dms" (int64 cpu_time)
                    printfn "Absolute time = %dms" timer.ElapsedMilliseconds
                    printfn ""
                    printfn "Press Any Key To Close"
                    system.Terminate()

        return! parentLoop()
    }
    parentLoop()

[<EntryPoint>]
let main(args) =    

    let mainMessage = new ParentMessage()
    mainMessage.endPoint <- args.[0] |> int
    mainMessage.length <- args.[1] |> int
    mainMessage.startFlag <- true
    mainMessage.replyFlag <- false
    mainMessage.replyVal <- false
    
    let parentActor = spawn system "parent" parent
    
    
    parentActor <! mainMessage
    
    System.Console.ReadKey() |> ignore
    
    0

                        






