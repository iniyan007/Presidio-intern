using System;
using System.IO;

class Program {
    static void Main() {
        Console.WriteLine(new FileInfo("test.cs").Length);
    }
}
