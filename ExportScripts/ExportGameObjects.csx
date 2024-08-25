﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

EnsureDataLoaded();

if (Data.IsGameMaker2())
{
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
    toDump = new();
    if (!exportFromCache)
    {
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
                objectsFolder + game_object.Name.Content + "\\" + game_object.Name.Content + ".yy"
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
                        var collisionObjectName = Data.GameObjects[(int)e2.EventSubtype]
                            .Name
                            .Content;
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
                            + "\\"
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
                            + "\\"
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
            writer.WriteLine(
                "  \"persistent\": " + (game_object.Persistent ? "true" : "false") + ","
            );
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
                writer.WriteLine(
                    "    \"name\": \"" + game_object.TextureMaskId.Name.Content + "\","
                );
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
}
else
{
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

    // if (GMLCacheEnabled && Data.GMLCache is not null)
    //     exportFromCache = ScriptQuestion("Export from the cache?");

    List<UndertaleGameObject> toDump;
    toDump = new();
    if (!exportFromCache)
    {
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
        writer.WriteLine("  <objects name=\"objects\">");
        for (int i = 0; i < Data.GameObjects.Count; i++)
        {
            UndertaleGameObject game_object = Data.GameObjects[i];
            writer.WriteLine("    <object>objects\\" + game_object.Name.Content + "</object>");
        }
        writer.WriteLine("  </objects>");
    }

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
        using (
            StreamWriter writer = new StreamWriter(
                objectsFolder + game_object.Name.Content + ".object.gmx"
            )
        )
        {
            writer.WriteLine(
                "<!--This Document is generated by GameMaker, if you edit it by hand then you do so at your own risk!-->"
            );
            writer.WriteLine("<object>");
            if (game_object.Sprite != null)
            {
                writer.WriteLine(
                    "  <spriteName>" + game_object.Sprite.Name.Content + "</spriteName>"
                );
            }
            else
            {
                writer.WriteLine("  <spriteName>&lt;undefined&gt;</spriteName>");
            }
            writer.WriteLine("  <solid>" + (game_object.Solid ? -1 : 0) + "</solid>");
            writer.WriteLine("  <visible>" + (game_object.Visible ? -1 : 0) + "</visible>");
            writer.WriteLine("  <depth>" + game_object.Depth + "</depth>");
            writer.WriteLine(
                "  <persistent>" + (game_object.Persistent ? -1 : 0) + "</persistent>"
            );

            if (game_object.ParentId != null)
            {
                writer.WriteLine(
                    "  <parentName>" + game_object.ParentId.Name.Content + "</parentName>"
                );
            }
            else
            {
                writer.WriteLine("  <parentName>&lt;undefined&gt;</parentName>");
            }
            if (game_object.TextureMaskId != null)
            {
                writer.WriteLine(
                    "  <maskName>" + game_object.TextureMaskId.Name.Content + "</maskName>"
                );
            }
            else
            {
                writer.WriteLine("  <maskName>&lt;undefined&gt;</maskName>");
            }
            writer.WriteLine("  <events>");
            var i = 0;
            foreach (var e1 in game_object.Events)
            {
                foreach (var e2 in e1)
                {
                    if (i == 4) // Collision
                    {
                        writer.WriteLine(
                            "    <event eventtype=\""
                                + i
                                + "\" ename=\""
                                + Data.GameObjects[(int)e2.EventSubtype].Name.Content
                                + "\">"
                        );
                    }
                    else
                    {
                        writer.WriteLine(
                            "    <event eventtype=\"" + i + "\" enumb=\"" + e2.EventSubtype + "\">"
                        );
                    }

                    foreach (var a in e2.Actions)
                    {
                        writer.WriteLine("      <action>");
                        writer.WriteLine("        <libid>" + a.LibID + "</libid>");
                        //writer.WriteLine("        <id>"+ a.ID + "</id>");
                        writer.WriteLine("        <id>603</id>"); // exécution de code
                        writer.WriteLine("        <kind>" + a.Kind + "</kind>");
                        writer.WriteLine(
                            "        <userelative>" + (a.UseRelative ? -1 : 0) + "</userelative>"
                        );
                        writer.WriteLine(
                            "        <isquestion>" + (a.IsQuestion ? -1 : 0) + "</isquestion>"
                        );
                        writer.WriteLine(
                            "        <useapplyto>" + (a.UseApplyTo ? -1 : 0) + "</useapplyto>"
                        );
                        writer.WriteLine("        <exetype>" + a.ExeType + "</exetype>");
                        writer.WriteLine("        <functionname></functionname>");
                        writer.WriteLine("        <codestring></codestring>");
                        writer.WriteLine(
                            "        <whoName>" + (a.Who == -1 ? "self" : "") + "</whoName>"
                        );
                        writer.WriteLine(
                            "        <relative>" + (a.Relative ? -1 : 0) + "</relative>"
                        );
                        writer.WriteLine("        <isnot>" + (a.IsNot ? -1 : 0) + "</isnot>");
                        writer.WriteLine("        <arguments>");
                        writer.WriteLine("          <argument>");
                        writer.WriteLine("            <kind>" + a.ArgumentCount + "</kind>");
                        if (a.CodeId == null)
                        {
                            writer.WriteLine("            <string></string>");
                        }
                        else
                        {
                            string mycode = Decompiler.Decompile(a.CodeId, DECOMPILE_CONTEXT.Value);
                            mycode = mycode.Replace("&", "&amp;");
                            mycode = mycode.Replace("<", "&lt;");
                            mycode = mycode.Replace(">", "&gt;");
                            mycode = mycode.Replace(
                                "action_set_relative",
                                "// action_set_relative"
                            );
                            writer.WriteLine("            <string>" + mycode + "</string>");
                        }
                        writer.WriteLine("          </argument>");
                        writer.WriteLine("        </arguments>");
                        writer.WriteLine("      </action>");
                    }
                    writer.WriteLine("    </event>");
                }
                i++;
            }
            writer.WriteLine("  </events>");
            writer.WriteLine(
                "  <PhysicsObject>" + (game_object.UsesPhysics ? -1 : 0) + "</PhysicsObject>"
            );
            writer.WriteLine(
                "  <PhysicsObjectSensor>"
                    + (game_object.IsSensor ? -1 : 0)
                    + "</PhysicsObjectSensor>"
            );
            writer.WriteLine(
                "  <PhysicsObjectShape>" + (int)game_object.CollisionShape + "</PhysicsObjectShape>"
            );
            writer.WriteLine(
                "  <PhysicsObjectDensity>"
                    + game_object.Density.ToString(
                        "0.00",
                        System.Globalization.CultureInfo.InvariantCulture
                    )
                    + "</PhysicsObjectDensity>"
            );
            writer.WriteLine(
                "  <PhysicsObjectRestitution>"
                    + game_object.Restitution.ToString(
                        "0.00",
                        System.Globalization.CultureInfo.InvariantCulture
                    )
                    + "</PhysicsObjectRestitution>"
            );
            writer.WriteLine(
                "  <PhysicsObjectGroup>" + game_object.Group + "</PhysicsObjectGroup>"
            );
            writer.WriteLine(
                "  <PhysicsObjectLinearDamping>"
                    + game_object.LinearDamping.ToString(
                        "0.00",
                        System.Globalization.CultureInfo.InvariantCulture
                    )
                    + "</PhysicsObjectLinearDamping>"
            );
            writer.WriteLine(
                "  <PhysicsObjectAngularDamping>"
                    + game_object.AngularDamping.ToString(
                        "0.00",
                        System.Globalization.CultureInfo.InvariantCulture
                    )
                    + "</PhysicsObjectAngularDamping>"
            );
            writer.WriteLine(
                "  <PhysicsObjectFriction>"
                    + game_object.Friction.ToString(
                        "0.00",
                        System.Globalization.CultureInfo.InvariantCulture
                    )
                    + "</PhysicsObjectFriction>"
            );
            writer.WriteLine(
                "  <PhysicsObjectAwake>" + (game_object.Awake ? -1 : 0) + "</PhysicsObjectAwake>"
            );
            writer.WriteLine(
                "  <PhysicsObjectKinematic>"
                    + (game_object.Kinematic ? -1 : 0)
                    + "</PhysicsObjectKinematic>"
            );
            writer.WriteLine("  <PhysicsShapePoints/>");
            writer.WriteLine("</object>");
        }

        //     if (code is not null)
        //     {
        //         if(code.Name.Content.Contains("gml_Script_")) {
        //             string path = Path.Combine(objectsFolder, code.Name.Content.Substring(11) + ".gml");
        //             try
        //             {
        //                 File.WriteAllText(path, (code != null ? Decompiler.Decompile(code, DECOMPILE_CONTEXT.Value) : ""));
        //             }
        //             catch (Exception e)
        //             {
        //                 File.WriteAllText(path, "/*\nDECOMPILER FAILED!\n\n" + e.ToString() + "\n*/");
        //             }
        //         }
        //     }

        IncrementProgressParallel();
    }
    void DumpCachedCode(KeyValuePair<string, string> code)
    {
        string path = Path.Combine(objectsFolder, code.Key + ".gml");

        File.WriteAllText(path, code.Value);

        IncrementProgressParallel();
    }
}
