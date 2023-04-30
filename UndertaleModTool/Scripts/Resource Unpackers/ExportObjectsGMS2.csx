using System.Text;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

EnsureDataLoaded();

// Pour avoir un "." au lieu d'une "," dans les conversion en décimal
System.Globalization.CultureInfo customCulture = (System.Globalization.CultureInfo) System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
customCulture.NumberFormat.NumberDecimalSeparator = ".";

System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;
//

string objectsFolder = GetFolder(FilePath) + "objects" + Path.DirectorySeparatorChar;
ThreadLocal<GlobalDecompileContext> DECOMPILE_CONTEXT = new ThreadLocal<GlobalDecompileContext>(() => new GlobalDecompileContext(Data, false));
if (Directory.Exists(objectsFolder))
{
  Directory.Delete(objectsFolder, true);
}
Directory.CreateDirectory(objectsFolder);

bool exportFromCache = false;
// if (GMLCacheEnabled && Data.GMLCache is not null)
//     exportFromCache = ScriptQuestion("Export from the cache?");

List<UndertaleGameObject> toDump;
if (!exportFromCache)
{
    toDump = new();
    foreach (UndertaleGameObject game_object in Data.GameObjects)
    {
        toDump.Add(game_object);
    }
}

bool cacheGenerated = false;
if (exportFromCache)
{
    cacheGenerated = await GenerateGMLCache(DECOMPILE_CONTEXT);
    await StopProgressBarUpdater();
}

SetProgressBar(null, "Object Entries", 0, exportFromCache ? Data.GMLCache.Count + Data.GMLCacheFailed.Count : toDump.Count);
StartProgressBarUpdater();

await DumpCode();

await StopProgressBarUpdater();
HideProgressBar();
ScriptMessage("Export Complete.\n\nLocation: " + objectsFolder);


string GetFolder(string path)
{
    return Path.GetDirectoryName(path) + Path.DirectorySeparatorChar;
}


async Task DumpCode()
{

    if (Data.KnownSubFunctions is null) //if we run script before opening any code
        Decompiler.BuildSubFunctionCache(Data);

    await Task.Run(() => Parallel.ForEach(toDump, DumpGameObject));
}

void DumpGameObject(UndertaleGameObject game_object)
{

    Directory.CreateDirectory(objectsFolder + game_object.Name.Content);
    using (StreamWriter writer = new StreamWriter(objectsFolder + game_object.Name.Content + "\\" + game_object.Name.Content + ".yy"))
    {

        writer.WriteLine("{");
        writer.WriteLine("  \"resourceType\": \"GMObject\",");
        writer.WriteLine("  \"resourceVersion\": \"1.0\",");
        writer.WriteLine("  \"name\": \""+game_object.Name.Content+"\",");
        writer.WriteLine("  \"eventList\": [");
        foreach(var g in game_object.Events) {
            writer.WriteLine("    {\"resourceType\":\"GMEvent\",\"resourceVersion\":\"1.0\",\"name\":\"\",\"collisionObjectId\":null,\"eventNum\":0,\"eventType\":0,\"isDnD\":false,},");
        }
        writer.WriteLine("  ],");
        if(IsVersionAtLeast(2022, 5)) {
            writer.WriteLine("  \"managed\": "+(game_object.Managed ? "true" : "false")+",");
        }
        writer.WriteLine("  \"overriddenProperties\": [],");
        writer.WriteLine("  \"parent\": {");
        writer.WriteLine("    \"name\": \"Objects\",");
        writer.WriteLine("    \"path\": \"folders/Objects.yy\",");
        writer.WriteLine("  },");

        if(game_object.ParentId is null) {
            writer.WriteLine("  \"parentObjectId\": null,");
        } else {
            writer.WriteLine("  \"parentObjectId\": {");
            writer.WriteLine("    \"name\": \""+game_object.ParentId.Name.Content+"\",");
            writer.WriteLine("    \"path\": \"objects/"+game_object.ParentId.Name.Content+"/"+game_object.ParentId.Name.Content+".yy\",");
            writer.WriteLine("  },");
        }
        writer.WriteLine("  \"persistent\": "+(game_object.Persistent ? "true" : "false")+",");
        writer.WriteLine("  \"physicsAngularDamping\": "+game_object.AngularDamping.ToString("0.0")+",");
        writer.WriteLine("  \"physicsDensity\": 0.5,");
        writer.WriteLine("  \"physicsFriction\": 0.2,");
        writer.WriteLine("  \"physicsGroup\": 1,");
        writer.WriteLine("  \"physicsKinematic\": false,");
        writer.WriteLine("  \"physicsLinearDamping\": 0.1,");
        writer.WriteLine("  \"physicsObject\": false,");
        writer.WriteLine("  \"physicsRestitution\": 0.1,");
        writer.WriteLine("  \"physicsSensor\": false,");
        writer.WriteLine("  \"physicsShape\": 1,");
        writer.WriteLine("  \"physicsShapePoints\": [],");
        writer.WriteLine("  \"physicsStartAwake\": true,");
        writer.WriteLine("  \"properties\": [],");
        writer.WriteLine("  \"solid\": false,");
        if(game_object.Sprite is null) {
            writer.WriteLine("  \"spriteId\": null,");
        } else {
            writer.WriteLine("  \"spriteId\": {");
            writer.WriteLine("    \"name\": \"spr_arrow\",");
            writer.WriteLine("    \"path\": \"sprites/spr_arrow/spr_arrow.yy\",");
            writer.WriteLine("  },");
        }
        writer.WriteLine("  \"spriteMaskId\": null,");
        writer.WriteLine("  \"visible\": true,");
        writer.WriteLine("}");

    }

    IncrementProgressParallel();
}
void DumpCachedCode(KeyValuePair<string, string> code)
{
    string path = Path.Combine(objectsFolder, code.Key + ".gml");

    File.WriteAllText(path, code.Value);

    IncrementProgressParallel();
}