using System;
using System.Diagnostics;
using LauncherM.Domain;
using LauncherM.Infrastructure;

class LaunchItemNameResolverChecks
{
    static int Main()
    {
        try { Run(); return 0; }
        catch (Exception ex) { Console.Error.WriteLine(ex); return 1; }
    }

    static void Run()
    {
        var resolver = new LaunchItemNameResolver();
        Check(resolver.Resolve(Item(LaunchItemType.Folder, @"D:\working\TaskMemo\TaskMemoApp\", "Visual Studio Code")) == "TaskMemoApp", "Folder + VS Code");
        Check(resolver.Resolve(Item(LaunchItemType.Folder, @"D:\working\TaskMemo", "Codex")) == "TaskMemo", "Folder + Codex");
        Check(resolver.Resolve(Item(LaunchItemType.File, @"D:\Documents\設計書.xlsx")) == "設計書.xlsx", "File keeps extension");
        Check(resolver.Resolve(Item(LaunchItemType.Application, @"D:\missing\Code.exe")) == "Code", "exe filename fallback");
        Check(resolver.Resolve(Item(LaunchItemType.Url, "https://chatgpt.com/example")) == "chatgpt.com", "Web hostname");
        Check(resolver.Resolve(Item(LaunchItemType.Application, "onenote:https://d.docs.live.net/example/%E6%9D%BE%E5%B0%BE%E7%A0%94.one#section-id=1")) == "松尾研", "OneNote URI path segment");
        var explicitName = Item(LaunchItemType.Folder, @"D:\working\Changed", "Codex"); explicitName.Name = "TaskMemo 開発";
        Check(resolver.Resolve(explicitName) == "TaskMemo 開発", "Explicit name preserved");
        Check(resolver.Resolve(new LaunchItem { Type = LaunchItemType.Command, Opener = "Codex" }) == "Codex", "Opener fallback");
        Check(resolver.Resolve(new LaunchItem { Type = LaunchItemType.Command }) == "LaunchItem", "Final fallback");
        string exe = Process.GetCurrentProcess().MainModule.FileName;
        Check(!string.IsNullOrWhiteSpace(resolver.Resolve(Item(LaunchItemType.Application, exe))), "Executable version information");
        Console.WriteLine("LaunchItemNameResolverChecks: 10 checks passed.");
    }

    static LaunchItem Item(LaunchItemType type, string target, string opener = "") { return new LaunchItem { Type = type, Target = target, Opener = opener }; }
    static void Check(bool value, string name) { if (!value) throw new Exception("Failed: " + name); }
}
