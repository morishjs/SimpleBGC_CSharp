# SimpleBGC_CSharp
This code is for operating SimpleBGC gimabal board.
Simply Read & Write the angle.

Actually, There is flawless error.
1. When write the angle to gimbal board and read the angle soon, the garbage value is returned.
-> I handled this problem by adding the code in read function. It requests read command in writing code.



