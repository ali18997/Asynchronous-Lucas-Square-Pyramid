• Size of the work unit that you determined results in best performance for
your implementation and an explanation on how you determined it. Size
of the work unit refers to the number of sub-problems that a worker gets
in a single request from the boss.

The work unit is 10 and it was selected by experimenting with different sizes like 
2, 5, 10, 100, 1000, 10000 and checking which takes least time to compute results
for a large problem as well as give the highest ratio of CPU time / Real Time and 
it turns out for a size of 10 the ratio is highest and execution time is least.

• The result of running your program for
dotnet fsi proj1.fsx 1000000 4

No sequence found


• The running time for the above as reported by time for the above, i.e.
run time scala project1.scala 1000000 4 and report the time. The
ratio of CPU time to REAL TIME tells you how many cores were effectively used in the computation. If your are close to 1 you have almost no
parallelism (points will be subtracted).

CPU time = 11578ms
Absolute time = 2122ms

Cores effective = 5.45


• The largest problem you managed to solve.

The highest problem tried was 100000000 20
The result for it is as follows:
No sequence found

CPU time = 1383140ms
Absolute time = 200837ms


