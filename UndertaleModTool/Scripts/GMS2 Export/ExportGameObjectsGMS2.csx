using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

EnsureDataLoaded();

string objectsFolder = GetFolder(FilePath) + "objects" + Path.DirectorySeparatorChar;
ThreadLocal<GlobalDecompileContext> DECOMPILE_CONTEXT = new ThreadLocal<GlobalDecompileContext>(
    () => new GlobalDecompileContext(Data, false)
);

// if (Directory.Exists(objectsFolder))
// {
//     ScriptError("An object export already exists. Please remove it.", "Error");
//     return;
// }

Directory.CreateDirectory(objectsFolder);

bool exportFromCache = false;
if (GMLCacheEnabled && Data.GMLCache is not null)
    exportFromCache = ScriptQuestion("Export from the cache?");

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

SetProgressBar(
    null,
    "Object Entries",
    0,
    exportFromCache ? Data.GMLCache.Count + Data.GMLCacheFailed.Count : toDump.Count
);
StartProgressBarUpdater();

await DumpCode();

await StopProgressBarUpdater();
HideProgressBar();

// Export asset
using (StreamWriter writer = new StreamWriter(objectsFolder + "asset_order.txt"))
{
    for (int i = 0; i < Data.GameObjects.Count; i++)
    {
        UndertaleGameObject game_object = Data.GameObjects[i];
        writer.WriteLine(
            "    {\"id\":{\"name\":\""
                + game_object.Name.Content
                + "\",\"path\":\"objects/"
                + game_object.Name.Content
                + "/"
                + game_object.Name.Content
                + ".yy\",},},"
        );
    }
}

ScriptMessage("Export Complete.\n\nLocation: " + objectsFolder);

string GetFolder(string path)
{
    return Path.GetDirectoryName(path) + Path.DirectorySeparatorChar;
}

async Task DumpCode()
{
    // if (Data.KnownSubFunctions is null) //if we run script before opening any code
    //     Decompiler.BuildSubFunctionCache(Data);

    await Task.Run(() => Parallel.ForEach(toDump, DumpGameObject));
}

void DumpGameObject(UndertaleGameObject game_object)
{
    Directory.CreateDirectory(objectsFolder + game_object.Name.Content);

    using (
        StreamWriter writer = new StreamWriter(
            objectsFolder + game_object.Name.Content + Path.DirectorySeparatorChar + game_object.Name.Content + ".yy"
        )
    )
    {
        writer.WriteLine("{");
        writer.WriteLine("  \"resourceType\": \"GMObject\",");
        writer.WriteLine("  \"resourceVersion\": \"1.0\",");
        writer.WriteLine("  \"name\": \"" + game_object.Name.Content + "\",");
        writer.WriteLine("  \"eventList\": [");

        var i = 0;
        foreach (var e1 in game_object.Events)
        {
            foreach (var e2 in e1)
            {
                string fileGMLName;
                if (i == 4) // Collision
                {
                    var collisionObjectName = Data.GameObjects[(int)e2.EventSubtype].Name.Content;
                    writer.WriteLine(
                        "    {\"resourceType\":\"GMEvent\",\"resourceVersion\":\"1.0\",\"name\":\"\",\"collisionObjectId\":{\"name\":\""
                            + collisionObjectName
                            + "\",\"path\":\"objects/"
                            + collisionObjectName
                            + "/"
                            + collisionObjectName
                            + ".yy\",},\"eventNum\":0,\"eventType\":"
                            + i
                            + ",\"isDnD\":false,},"
                    );

                    // Création fichier .gml : BUG
                    var enumDisplayStatus = (EventType)i;
                    string stringValue = enumDisplayStatus.ToString();
                    fileGMLName =
                        objectsFolder
                        + game_object.Name.Content
                        + Path.DirectorySeparatorChar
                        + stringValue
                        + "_"
                        + collisionObjectName
                        + ".gml";
                }
                else
                {
                    // Création fichier .gml : BUG
                    var enumDisplayStatus = (EventType)i;
                    string stringValue = enumDisplayStatus.ToString();
                    fileGMLName =
                        objectsFolder
                        + game_object.Name.Content
                        + Path.DirectorySeparatorChar
                        + stringValue
                        + "_"
                        + e2.EventSubtype
                        + ".gml";

                    if (stringValue.Equals("PreCreate"))
                    {
                        continue;
                    }

                    writer.WriteLine(
                        "    {\"resourceType\":\"GMEvent\",\"resourceVersion\":\"1.0\",\"name\":\"\",\"collisionObjectId\":null,\"eventNum\":"
                            + e2.EventSubtype
                            + ",\"eventType\":"
                            + i
                            + ",\"isDnD\":false,},"
                    );
                }

                foreach (var action in e2.Actions)
                {
                    if (action.CodeId is not null)
                    {
                        string path = fileGMLName;
                        try
                        {
                            File.WriteAllText(
                                path,
                                (
                                    action.CodeId != null
                                        ? Decompiler.Decompile(
                                            action.CodeId,
                                            DECOMPILE_CONTEXT.Value
                                        )
                                        : ""
                                )
                            );
                            using (StreamWriter sw = File.AppendText(path))
                            {
                                sw.WriteLine("");
                            }
                        }
                        catch (Exception e)
                        {
                            File.WriteAllText(
                                path,
                                "/*\nDECOMPILER FAILED!\n\n" + e.ToString() + "\n*/"
                            );
                        }
                    }
                }
            }
            i++;
        }

        writer.WriteLine("  ],");
        writer.WriteLine("  \"managed\": true,");
        writer.WriteLine("  \"overriddenProperties\": [],");
        writer.WriteLine("  \"parent\": {");
        writer.WriteLine("    \"name\": \"Objects\",");
        writer.WriteLine("    \"path\": \"folders/Objects.yy\",");
        writer.WriteLine("  },");
        /*
        writer.WriteLine("  \"parent\": {");
        writer.WriteLine("    \"name\": \"Blocks\",");
        writer.WriteLine("    \"path\": \"folders/Objects/Environment/Blocks.yy\",");
        writer.WriteLine("  },");
        */
        if (game_object.ParentId is null)
        {
            writer.WriteLine("  \"parentObjectId\": null,");
        }
        else
        {
            writer.WriteLine("  \"parentObjectId\": {");
            writer.WriteLine("    \"name\": \"" + game_object.ParentId.Name.Content + "\",");
            writer.WriteLine(
                "    \"path\": \"objects/"
                    + game_object.ParentId.Name.Content
                    + "/"
                    + game_object.ParentId.Name.Content
                    + ".yy\","
            );
            writer.WriteLine("  },");
        }
        writer.WriteLine("  \"persistent\": " + (game_object.Persistent ? "true" : "false") + ",");
        writer.WriteLine("  \"physicsAngularDamping\": 0.1,");
        writer.WriteLine("  \"physicsDensity\": 0.5,");
        writer.WriteLine("  \"physicsFriction\": 0.2,");
        writer.WriteLine("  \"physicsGroup\": " + game_object.Group + ",");
        writer.WriteLine("  \"physicsKinematic\": false,");
        writer.WriteLine("  \"physicsLinearDamping\": 0.1,");
        writer.WriteLine("  \"physicsObject\": false,");
        writer.WriteLine("  \"physicsRestitution\": 0.1,");
        writer.WriteLine("  \"physicsSensor\": false,");
        writer.WriteLine("  \"physicsShape\": " + (int)game_object.CollisionShape + ",");
        writer.WriteLine("  \"physicsShapePoints\": [],");
        writer.WriteLine("  \"physicsStartAwake\": true,");
        writer.WriteLine("  \"properties\": [],");
        writer.WriteLine("  \"solid\": " + (game_object.Solid ? "true" : "false") + ",");
        if (game_object.Sprite is null)
        {
            writer.WriteLine("  \"spriteId\": null,");
        }
        else
        {
            writer.WriteLine("  \"spriteId\": {");
            writer.WriteLine("    \"name\": \"" + game_object.Sprite.Name.Content + "\",");
            writer.WriteLine(
                "    \"path\": \"sprites/"
                    + game_object.Sprite.Name.Content
                    + "/"
                    + game_object.Sprite.Name.Content
                    + ".yy\","
            );
            writer.WriteLine("  },");
        }

        if (game_object.TextureMaskId is null)
        {
            writer.WriteLine("  \"spriteMaskId\": null,");
        }
        else
        {
            writer.WriteLine("  \"spriteMaskId\": {");
            writer.WriteLine("    \"name\": \"" + game_object.TextureMaskId.Name.Content + "\",");
            writer.WriteLine(
                "    \"path\": \"sprites/"
                    + game_object.TextureMaskId.Name.Content
                    + "/"
                    + game_object.TextureMaskId.Name.Content
                    + ".yy\","
            );
            writer.WriteLine("  },");
        }

        writer.WriteLine("  \"visible\": " + (game_object.Visible ? "true" : "false") + ",");
        writer.Write("}");
    }

    IncrementProgressParallel();
}
void DumpCachedCode(KeyValuePair<string, string> code)
{
    string path = Path.Combine(objectsFolder, code.Key + ".gml");

    File.WriteAllText(path, code.Value);

    IncrementProgressParallel();
}
