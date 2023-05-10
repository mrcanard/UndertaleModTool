using System.Text;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

EnsureDataLoaded();

string objectsFolder = GetFolder(FilePath) + \"objects\" + Path.DirectorySeparatorChar;
ThreadLocal<GlobalDecompileContext> DECOMPILE_CONTEXT = new ThreadLocal<GlobalDecompileContext>(() => new GlobalDecompileContext(Data, false));
// if (Directory.Exists(objectsFolder))
// {
//     ScriptError(\"An object export already exists. Please remove it.\", \"Error\");
//     return;
// }

Directory.CreateDirectory(objectsFolder);

bool exportFromCache = false;
// if (GMLCacheEnabled && Data.GMLCache is not null)
//     exportFromCache = ScriptQuestion(\"Export from the cache?\");

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

SetProgressBar(null, \"Object Entries\", 0, exportFromCache ? Data.GMLCache.Count + Data.GMLCacheFailed.Count : toDump.Count);
StartProgressBarUpdater();

await DumpCode();

await StopProgressBarUpdater();
HideProgressBar();
ScriptMessage(\"Export Complete.\n\nLocation: \" + objectsFolder);


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

    using (StreamWriter writer = new StreamWriter(objectsFolder + game_object.Name.Content + \"\\\" + game_object.Name.Content + \".yy\"))
    {

	writer.WriteLine("{");
	writer.WriteLine("  \"resourceType\": \"GMObject\",");
	writer.WriteLine("  \"resourceVersion\": \"1.0\",");
	writer.WriteLine("  \"name\": \"obj_block_brick\",");

	writer.WriteLine("  \"eventList\": [");

	// writer.WriteLine("    {\"resourceType\":\"GMEvent\",\"resourceVersion\":\"1.0\",\"name\":\"\",\"collisionObjectId\":null,\"eventNum\":10,\"eventType\":7,\"isDnD\":false,},");
	// writer.WriteLine("    {\"resourceType\":\"GMEvent\",\"resourceVersion\":\"1.0\",\"name\":\"\",\"collisionObjectId\":null,\"eventNum\":0,\"eventType\":3,\"isDnD\":false,},");
	
        var i = 0;
        foreach (var e1 in game_object.Events)
        {
            foreach (var e2 in e1)
            {
                if(i == 4) // Collision
                {
                    writer.WriteLine(\"    <event eventtype=\\"\" + i + \"\\" ename=\\"\" + Data.GameObjects[(int) e2.EventSubtype].Name.Content + \"\\">\");
                } else
                {
                    writer.WriteLine(\"    <event eventtype=\\"\" + i + \"\\" enumb=\\"\" + e2.EventSubtype + \"\\">\");
                }

                foreach (var a in e2.Actions)
                {
                    writer.WriteLine(\"      <action>\");
                    writer.WriteLine(\"        <libid>\"+ a.LibID + \"</libid>\");
                    //writer.WriteLine(\"        <id>\"+ a.ID + \"</id>\");
                    writer.WriteLine(\"        <id>603</id>\"); // exécution de code
                    writer.WriteLine(\"        <kind>\"+ a.Kind + \"</kind>\");
                    writer.WriteLine(\"        <userelative>\"+ (a.UseRelative ? -1 : 0) + \"</userelative>\");
                    writer.WriteLine(\"        <isquestion>\"+ (a.IsQuestion ? -1 : 0) + \"</isquestion>\");
                    writer.WriteLine(\"        <useapplyto>\"+ (a.UseApplyTo ? -1 : 0) + \"</useapplyto>\");
                    writer.WriteLine(\"        <exetype>\"+ a.ExeType + \"</exetype>\");
                    writer.WriteLine(\"        <functionname></functionname>\");
                    writer.WriteLine(\"        <codestring></codestring>\");
                    writer.WriteLine(\"        <whoName>\"+ (a.Who == -1 ? \"self\" : \"\") + \"</whoName>\");
                    writer.WriteLine(\"        <relative>\" + (a.Relative ? -1 : 0) + \"</relative>\");
                    writer.WriteLine(\"        <isnot>\"+ (a.IsNot ? -1 : 0) + \"</isnot>\");
                    writer.WriteLine(\"        <arguments>\");
                    writer.WriteLine(\"          <argument>\");
                    writer.WriteLine(\"            <kind>\" + a.ArgumentCount + \"</kind>\");
                    if(a.CodeId == null)
                    {
                        writer.WriteLine(\"            <string></string>\");

                    }
                    else
                    {
                    	writer.WriteLine(\"<string>du contenu</string>\");
                    	/*
                        string mycode = Decompiler.Decompile(a.CodeId, DECOMPILE_CONTEXT.Value);
                        mycode = mycode.Replace(\"&\", \"&amp;\");
                        mycode = mycode.Replace(\"<\", \"&lt;\");
                        mycode = mycode.Replace(\">\", \"&gt;\");
                        mycode = mycode.Replace(\"action_set_relative\", \"// action_set_relative\");
                        writer.WriteLine(\"            <string>\" + mycode + \"</string>\");
                        */
                    }
                    writer.WriteLine(\"          </argument>\");
                    writer.WriteLine(\"        </arguments>\");
                    writer.WriteLine(\"      </action>\");
                }
                writer.WriteLine(\"    </event>\");
            }
            i++;
        }
	writer.WriteLine("  ],");
	

	writer.WriteLine("  \"managed\": true,");
	writer.WriteLine("  \"overriddenProperties\": [],");
	writer.WriteLine("  \"parent\": {");
	writer.WriteLine("    \"name\": \"Blocks\",");
	writer.WriteLine("    \"path\": \"folders/Objects/Environment/Blocks.yy\",");
	writer.WriteLine("  },");
	writer.WriteLine("  \"parentObjectId\": {");
	writer.WriteLine("    \"name\": \"obj_block_parent\",");
	writer.WriteLine("    \"path\": \"objects/obj_block_parent/obj_block_parent.yy\",");
	writer.WriteLine("  },");
	writer.WriteLine("  \"persistent\": false,");
	writer.WriteLine("  \"physicsAngularDamping\": 0.1,");
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
	writer.WriteLine("  \"spriteId\": {");
	writer.WriteLine("    \"name\": \"spr_block_brick\",");
	writer.WriteLine("    \"path\": \"sprites/spr_block_brick/spr_block_brick.yy\",");
	writer.WriteLine("  },");
	writer.WriteLine("  \"spriteMaskId\": null,");
	writer.WriteLine("  \"visible\": true,");
	writer.WriteLine("}");

		 
	writer.WriteLine(\"  \\"eventList\\": [\");
        var i = 0;
        foreach (var e1 in game_object.Events)
        {
            foreach (var e2 in e1)
            {
                if(i == 4) // Collision
                {
                    writer.WriteLine(\"    <event eventtype=\\"\" + i + \"\\" ename=\\"\" + Data.GameObjects[(int) e2.EventSubtype].Name.Content + \"\\">\");
                } else
                {
                    writer.WriteLine(\"    <event eventtype=\\"\" + i + \"\\" enumb=\\"\" + e2.EventSubtype + \"\\">\");
                }

                foreach (var a in e2.Actions)
                {
                    writer.WriteLine(\"      <action>\");
                    writer.WriteLine(\"        <libid>\"+ a.LibID + \"</libid>\");
                    //writer.WriteLine(\"        <id>\"+ a.ID + \"</id>\");
                    writer.WriteLine(\"        <id>603</id>\"); // exécution de code
                    writer.WriteLine(\"        <kind>\"+ a.Kind + \"</kind>\");
                    writer.WriteLine(\"        <userelative>\"+ (a.UseRelative ? -1 : 0) + \"</userelative>\");
                    writer.WriteLine(\"        <isquestion>\"+ (a.IsQuestion ? -1 : 0) + \"</isquestion>\");
                    writer.WriteLine(\"        <useapplyto>\"+ (a.UseApplyTo ? -1 : 0) + \"</useapplyto>\");
                    writer.WriteLine(\"        <exetype>\"+ a.ExeType + \"</exetype>\");
                    writer.WriteLine(\"        <functionname></functionname>\");
                    writer.WriteLine(\"        <codestring></codestring>\");
                    writer.WriteLine(\"        <whoName>\"+ (a.Who == -1 ? \"self\" : \"\") + \"</whoName>\");
                    writer.WriteLine(\"        <relative>\" + (a.Relative ? -1 : 0) + \"</relative>\");
                    writer.WriteLine(\"        <isnot>\"+ (a.IsNot ? -1 : 0) + \"</isnot>\");
                    writer.WriteLine(\"        <arguments>\");
                    writer.WriteLine(\"          <argument>\");
                    writer.WriteLine(\"            <kind>\" + a.ArgumentCount + \"</kind>\");
                    if(a.CodeId == null)
                    {
                        writer.WriteLine(\"            <string></string>\");

                    }
                    else
                    {
                    	writer.WriteLine(\"<string>du contenu</string>\");
                    	/*
                        string mycode = Decompiler.Decompile(a.CodeId, DECOMPILE_CONTEXT.Value);
                        mycode = mycode.Replace(\"&\", \"&amp;\");
                        mycode = mycode.Replace(\"<\", \"&lt;\");
                        mycode = mycode.Replace(\">\", \"&gt;\");
                        mycode = mycode.Replace(\"action_set_relative\", \"// action_set_relative\");
                        writer.WriteLine(\"            <string>\" + mycode + \"</string>\");
                        */
                    }
                    writer.WriteLine(\"          </argument>\");
                    writer.WriteLine(\"        </arguments>\");
                    writer.WriteLine(\"      </action>\");
                }
                writer.WriteLine(\"    </event>\");
            }
            i++;
        }
		writer.WriteLine(\"  ],\");
		writer.WriteLine(\"  \\"managed\\": true,\");
		writer.WriteLine(\"  \\"overriddenProperties\\": [],\");
		writer.WriteLine(\"  \\"parent\\": {\");
		writer.WriteLine(\"    \\"name\\": \\"Objects\\",\");
		writer.WriteLine(\"    \\"path\\": \\"folders/Objects.yy\\",\");
		writer.WriteLine(\"  },\");
		writer.WriteLine(\"  \\"parentObjectId\\": {\");
		writer.WriteLine(\"    \\"name\\": \\"obj_character_parent\\",\");
		writer.WriteLine(\"    \\"path\\": \\"objects/obj_character_parent/obj_character_parent.yy\\",\");
		writer.WriteLine(\"  },\");
		writer.WriteLine(\"  \\"persistent\\": false,\");
		writer.WriteLine(\"  \\"physicsAngularDamping\\": 0.1,\");
		writer.WriteLine(\"  \\"physicsDensity\\": 0.5,\");
		writer.WriteLine(\"  \\"physicsFriction\\": 0.2,\");
		writer.WriteLine(\"  \\"physicsGroup\\": 1,\");
		writer.WriteLine(\"  \\"physicsKinematic\\": false,\");
		writer.WriteLine(\"  \\"physicsLinearDamping\\": 0.1,\");
		writer.WriteLine(\"  \\"physicsObject\\": false,\");
		writer.WriteLine(\"  \\"physicsRestitution\\": 0.1,\");
		writer.WriteLine(\"  \\"physicsSensor\\": false,\");
		writer.WriteLine(\"  \\"physicsShape\\": 1,\");
		writer.WriteLine(\"  \\"physicsShapePoints\\": [],\");
		writer.WriteLine(\"  \\"physicsStartAwake\\": true,\");
		writer.WriteLine(\"  \\"properties\\": [],\");
		writer.WriteLine(\"  \\"solid\\": false,\");
		writer.WriteLine(\"  \\"spriteId\\": {\");
		writer.WriteLine(\"    \\"name\\": \\"spr_player_idle\\",\");
		writer.WriteLine(\"    \\"path\\": \\"sprites/spr_player_idle/spr_player_idle.yy\\",\");
		writer.WriteLine(\"  },\");
		writer.WriteLine(\"  \\"spriteMaskId\\": null,\");
		writer.WriteLine(\"  \\"visible\\": true,\");
		writer.WriteLine(\"}\");

    }

    //     if (code is not null)
    //     {
    //         if(code.Name.Content.Contains(\"gml_Script_\")) {
    //             string path = Path.Combine(objectsFolder, code.Name.Content.Substring(11) + \".gml\");
    //             try
    //             {
    //                 File.WriteAllText(path, (code != null ? Decompiler.Decompile(code, DECOMPILE_CONTEXT.Value) : \"\"));
    //             }
    //             catch (Exception e)
    //             {
    //                 File.WriteAllText(path, \"/*\nDECOMPILER FAILED!\n\n\" + e.ToString() + \"\n*/\");
    //             }        
    //         }
    //     }

    IncrementProgressParallel();
}
void DumpCachedCode(KeyValuePair<string, string> code)
{
    string path = Path.Combine(objectsFolder, code.Key + \".gml\");

    File.WriteAllText(path, code.Value);

    IncrementProgressParallel();
}
