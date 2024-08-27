using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

EnsureDataLoaded();

if (Data.IsGameMaker2())
{
    string codeFolder = GetFolder(FilePath) + "scripts" + Path.DirectorySeparatorChar;
    ThreadLocal<GlobalDecompileContext> DECOMPILE_CONTEXT = new ThreadLocal<GlobalDecompileContext>(
        () => new GlobalDecompileContext(Data, false)
    );

    String EscapeLine = "\r\n";

    if (Directory.Exists(codeFolder))
    {
        Directory.Delete(codeFolder, true);
    }

    Directory.CreateDirectory(codeFolder);

    bool exportFromCache = false;
    if (GMLCacheEnabled && Data.GMLCache is not null)
        exportFromCache = ScriptQuestion("Export from the cache?");

    List<UndertaleScript> toDump;
    toDump = new();
    if (!exportFromCache)
    {
        foreach (UndertaleScript script in Data.Scripts)
        {
            //if (code.ParentEntry != null)
            //    continue;
            toDump.Add(script);
        }
    }

    bool cacheGenerated = false;
    if (exportFromCache)
    {
        cacheGenerated = await GenerateGMLCache(DECOMPILE_CONTEXT);
        await StopProgressBarUpdater();
    }

    // Export asset
    using (StreamWriter writer = new StreamWriter(codeFolder + "asset_order.txt"))
    {
        for (int i = 0; i < Data.Scripts.Count; i++)
        {
            UndertaleScript script = Data.Scripts[i];

            if (!(script.Name.Content.StartsWith("gml_")))
            {
                writer.Write(
                    "    {\"id\":{\"name\":\""
                        + script.Name.Content
                        + "\",\"path\":\"scripts/"
                        + script.Name.Content
                        + "/"
                        + script.Name.Content
                        + ".yy\",},},"
                        + EscapeLine
                );
            }
        }
    }

    SetProgressBar(
        null,
        "Code Entries",
        0,
        exportFromCache ? Data.GMLCache.Count + Data.GMLCacheFailed.Count : toDump.Count
    );
    StartProgressBarUpdater();

    await DumpCode();

    await StopProgressBarUpdater();
    HideProgressBar();

    ScriptMessage("Export Complete.\n\nLocation: " + codeFolder);

    string GetFolder(string path)
    {
        return Path.GetDirectoryName(path) + Path.DirectorySeparatorChar;
    }

    async Task DumpCode()
    {
        // if (Data.KnownSubFunctions is null) //if we run script before opening any code

        //     Decompiler.BuildSubFunctionCache(Data);

        await Task.Run(() => Parallel.ForEach(toDump, DumpCodeScript));
    }

    void DumpCodeScript(UndertaleScript script)
    {
        if (script.Code is not null)
        {
            if (!(script.Name.Content.StartsWith("gml_")))
            {
                // SetProgressBar(null, "Code Entries : " + script.Name.Content, 0, 1);
                // Extraction .gml
                Directory.CreateDirectory(Path.Combine(codeFolder, script.Name.Content));
                string path = Path.Combine(
                    codeFolder,
                    script.Name.Content,
                    script.Name.Content + ".gml"
                );
                try
                {
                    File.WriteAllText(
                        path,
                        (
                            script.Code != null
                                ? Decompiler.Decompile(script.Code, DECOMPILE_CONTEXT.Value)
                                : ""
                        )
                    );
                }
                catch (Exception e)
                {
                    File.WriteAllText(path, "/*\nDECOMPILER FAILED!\n\n" + e.ToString() + "\n*/");
                }

                // Extraction .yy
                using (
                    StreamWriter writer = new StreamWriter(
                        codeFolder
                            + script.Name.Content
                            + Path.DirectorySeparatorChar
                            + script.Name.Content
                            + ".yy"
                    )
                )
                {
                    writer.Write("{" + EscapeLine);
                    writer.Write("  \"resourceType\": \"GMScript\"," + EscapeLine);
                    writer.Write("  \"resourceVersion\": \"1.0\"," + EscapeLine);
                    writer.Write("  \"name\": \"" + script.Name.Content + "\"," + EscapeLine);
                    writer.Write("  \"isCompatibility\": false," + EscapeLine);
                    writer.Write("  \"isDnD\": false," + EscapeLine);
                    writer.Write("  \"parent\": {" + EscapeLine);
                    writer.Write("    \"name\": \"Scripts\"," + EscapeLine);
                    writer.Write("    \"path\": \"folders/Scripts.yy\"," + EscapeLine);
                    writer.Write("  }," + EscapeLine);
                    writer.Write("}");
                }
                IncrementProgressParallel();
            }
        }
    }
}
else
{
    string codeFolder = GetFolder(FilePath) + "scripts" + Path.DirectorySeparatorChar;
    ThreadLocal<GlobalDecompileContext> DECOMPILE_CONTEXT = new ThreadLocal<GlobalDecompileContext>(
        () => new GlobalDecompileContext(Data, false)
    );

    if (Directory.Exists(codeFolder))
    {
        Directory.Delete(codeFolder, true);
    }

    Directory.CreateDirectory(codeFolder);

    bool exportFromCache = false;
    if (GMLCacheEnabled && Data.GMLCache is not null)
        exportFromCache = ScriptQuestion("Export from the cache?");

    List<UndertaleScript> toDump;
    toDump = new();
    if (!exportFromCache)
    {
        foreach (UndertaleScript script in Data.Scripts)
        {
            //if (code.ParentEntry != null)
            //    continue;
            toDump.Add(script);
        }
    }

    bool cacheGenerated = false;
    if (exportFromCache)
    {
        cacheGenerated = await GenerateGMLCache(DECOMPILE_CONTEXT);
        await StopProgressBarUpdater();
    }

    SetProgressBar(
        null,
        "Code Entries",
        0,
        exportFromCache ? Data.GMLCache.Count + Data.GMLCacheFailed.Count : toDump.Count
    );
    StartProgressBarUpdater();

    await DumpCode();

    await StopProgressBarUpdater();
    HideProgressBar();

    // Export asset
    using (StreamWriter writer = new StreamWriter(codeFolder + "asset_order.txt"))
    {
        writer.WriteLine("  <scripts name=\"scripts\">");
        for (int i = 0; i < Data.Scripts.Count; i++)
        {
            UndertaleScript script = Data.Scripts[i];
            if (!script.Name.Content.Contains("gml_Script_"))
            {
                writer.WriteLine("    <script>scripts\\" + script.Name.Content + ".gml</script>");
            }
        }
        writer.WriteLine("  </scripts>");
    }

    ScriptMessage("Export Complete.\n\nLocation: " + codeFolder);

    string GetFolder(string path)
    {
        return Path.GetDirectoryName(path) + Path.DirectorySeparatorChar;
    }

    async Task DumpCode()
    {
        if (Data.KnownSubFunctions is null) //if we run script before opening any code
            Decompiler.BuildSubFunctionCache(Data);

        await Task.Run(() => Parallel.ForEach(toDump, DumpCodeScript));
    }

    void DumpCodeScript(UndertaleScript script)
    {
        if (script.Code is not null)
        {
            if (!script.Name.Content.Contains("gml_Script_"))
            {
                // Extraction .gml
                // Directory.CreateDirectory(Path.Combine(codeFolder, script.Name.Content));
                string path = Path.Combine(codeFolder, script.Name.Content + ".gml");
                try
                {
                    File.WriteAllText(
                        path,
                        (
                            script.Code != null
                                ? Decompiler.Decompile(script.Code, DECOMPILE_CONTEXT.Value)
                                : ""
                        )
                    );
                }
                catch (Exception e)
                {
                    File.WriteAllText(path, "/*\nDECOMPILER FAILED!\n\n" + e.ToString() + "\n*/");
                }
            }
        }

        IncrementProgressParallel();
    }
}
