using System.Text;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

EnsureDataLoaded();

string codeFolder = GetFolder(FilePath) + "scripts" + Path.DirectorySeparatorChar;
ThreadLocal<GlobalDecompileContext> DECOMPILE_CONTEXT = new ThreadLocal<GlobalDecompileContext>(() => new GlobalDecompileContext(Data, false));

if (Directory.Exists(codeFolder))
{
    Directory.Delete(codeFolder, true);
}

Directory.CreateDirectory(codeFolder);

bool exportFromCache = false;
if (GMLCacheEnabled && Data.GMLCache is not null)
    exportFromCache = ScriptQuestion("Export from the cache?");

List<UndertaleScript> toDump;
if (!exportFromCache)
{
    toDump = new();
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

SetProgressBar(null, "Code Entries", 0, exportFromCache ? Data.GMLCache.Count + Data.GMLCacheFailed.Count : toDump.Count);
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
    if (Data.KnownSubFunctions is null) //if we run script before opening any code
        Decompiler.BuildSubFunctionCache(Data);

    await Task.Run(() => Parallel.ForEach(toDump, DumpCode));
}

void DumpCode(UndertaleScript script)
{

    if (script.Code is not null)
    {
        if(! script.Name.Content.Contains("gml_Script_")) {

            // Extraction .gml
            Directory.CreateDirectory(Path.Combine(codeFolder, script.Name.Content));
            string path = Path.Combine(codeFolder, script.Name.Content, script.Name.Content + ".gml");
            try
            {
                File.WriteAllText(path, (script.Code != null ? Decompiler.Decompile(script.Code, DECOMPILE_CONTEXT.Value) : ""));
            }
            catch (Exception e)
            {
                File.WriteAllText(path, "/*\nDECOMPILER FAILED!\n\n" + e.ToString() + "\n*/");
            }        

            // Extraction .yy
            using (StreamWriter writer = new StreamWriter(codeFolder + script.Name.Content + "\\" + script.Name.Content + ".yy"))
            {

                writer.WriteLine("{");
                writer.WriteLine("  \"resourceType\": \"GMScript\",");
                writer.WriteLine("  \"resourceVersion\": \"1.0\",");
                writer.WriteLine("  \"name\": \""+script.Name.Content+"\",");
                writer.WriteLine("  \"isCompatibility\": false,");
                writer.WriteLine("  \"isDnD\": false,");
                writer.WriteLine("  \"parent\": {");
                writer.WriteLine("    \"name\": \"Scripts\",");
                writer.WriteLine("    \"path\": \"folders/Scripts.yy\",");
                writer.WriteLine("  },");
                writer.WriteLine("}");

            }
        }
    }

    IncrementProgressParallel();
}
void DumpCachedCode(KeyValuePair<string, string> code)
{
    string path = Path.Combine(codeFolder, code.Key + ".gml");

    File.WriteAllText(path, code.Value);

    IncrementProgressParallel();
}