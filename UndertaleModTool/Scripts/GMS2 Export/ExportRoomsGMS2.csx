using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using UndertaleModLib.Models;

EnsureDataLoaded();

// Pour avoir un "." au lieu d'une "," dans les conversion en décimal
System.Globalization.CultureInfo customCulture = (System.Globalization.CultureInfo)
    System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
customCulture.NumberFormat.NumberDecimalSeparator = ".";

System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;

//

string roomsFolder = GetFolder(FilePath) + "rooms" + Path.DirectorySeparatorChar;
ThreadLocal<GlobalDecompileContext> DECOMPILE_CONTEXT = new ThreadLocal<GlobalDecompileContext>(
    () => new GlobalDecompileContext(Data, false)
);
if (Directory.Exists(roomsFolder))
{
    Directory.Delete(roomsFolder, true);
}

Directory.CreateDirectory(roomsFolder);

bool exportFromCache = false;

// if (GMLCacheEnabled && Data.GMLCache is not null)
//     exportFromCache = ScriptQuestion("Export from the cache?");

List<UndertaleRoom> toDump;
if (!exportFromCache)
{
    toDump = new();
    foreach (UndertaleRoom room in Data.Rooms)
    {
        toDump.Add(room);
    }
}

SetProgressBar(null, "Room Entries", 0, toDump.Count);
StartProgressBarUpdater();

// await DumpRooms();

// await StopProgressBarUpdater();

foreach (UndertaleRoom room in Data.Rooms)
{
    DumpRoom(room);
}

HideProgressBar();

// Export asset
using (StreamWriter writer = new StreamWriter(roomsFolder + "asset_order.txt"))
{
    for (int i = 0; i < Data.Rooms.Count; i++)
    {
        UndertaleRoom room = Data.Rooms[i];
        writer.WriteLine(
            "    {\"id\":{\"name\":\""
                + room.Name.Content
                + "\",\"path\":\"rooms/"
                + room.Name.Content
                + "/"
                + room.Name.Content
                + ".yy\",},},"
        );
    }
}

// Room order
using (StreamWriter writer = new StreamWriter(roomsFolder + "room_order.txt"))
{
    foreach (
        UndertaleResourceById<UndertaleRoom, UndertaleChunkROOM> _room in Data.GeneralInfo.RoomOrder
    )
    {
        UndertaleRoom room = _room.Resource;
        writer.WriteLine(
            "    {\"roomId\":{\"name\":\""
                + room.Name.Content
                + "\",\"path\":\"rooms/"
                + room.Name.Content
                + "/"
                + room.Name.Content
                + ".yy\",},},"
        );
    }
}

ScriptMessage("Export Complete.\n\nLocation: " + roomsFolder);

string GetFolder(string path)
{
    return Path.GetDirectoryName(path) + Path.DirectorySeparatorChar;
}

async Task DumpRooms()
{
    // if (Data.KnownSubFunctions is null) //if we run script before opening any code
    //     Decompiler.BuildSubFunctionCache(Data);

    await Task.Run(() => Parallel.ForEach(toDump, DumpRoom));
}

void DumpRoom(UndertaleRoom room)
{
    Directory.CreateDirectory(roomsFolder + room.Name.Content);

    SetProgressBar(null, "Room Entries : " + room.Name.Content, 0, 1);
    using (
        StreamWriter writer = new StreamWriter(
            roomsFolder
                + room.Name.Content
                + Path.DirectorySeparatorChar
                + room.Name.Content
                + ".yy"
        )
    )
    {
        writer.WriteLine("{");
        writer.WriteLine("  \"resourceType\": \"GMRoom\",");
        writer.WriteLine("  \"resourceVersion\": \"1.0\",");
        writer.WriteLine("  \"name\": \"" + room.Name.Content + "\",");
        // Begin CreationCodeId
        if (room.CreationCodeId is null)
        {
            writer.WriteLine("  \"creationCodeFile\": \"\",");
        }
        else
        {
            writer.WriteLine(
                "  \"creationCodeFile\": \"rooms/" + room.Name.Content + "/RoomCreationCode.gml\","
            );
            File.WriteAllText(
                roomsFolder
                    + room.Name.Content
                    + Path.DirectorySeparatorChar
                    + "RoomCreationCode.gml",
                Decompiler.Decompile(room.CreationCodeId, DECOMPILE_CONTEXT.Value)
            );
        }
        // End CreationCodeId
        writer.WriteLine("  \"inheritCode\": false,");
        writer.WriteLine("  \"inheritCreationOrder\": false,");
        writer.WriteLine("  \"inheritLayers\": false,");
        writer.WriteLine("  \"instanceCreationOrder\": [");
        foreach (var g in room.GameObjects)
        {
            writer.WriteLine(
                "    {\"name\":\"inst_"
                    + g.InstanceID
                    + "\",\"path\":\"rooms/"
                    + room.Name.Content
                    + "/"
                    + room.Name.Content
                    + ".yy\",},"
            );
        }
        writer.WriteLine("  ],");
        writer.WriteLine("  \"isDnd\": false,");
        writer.WriteLine("  \"layers\": [");
        foreach (var layer in room.Layers)
        {
            writer.WriteLine("  (" + layer.LayerName.Content + "," + layer.LayerType + ", " + layer.LayerDepth + ")");
            if (layer.LayerType == UndertaleRoom.LayerType.Assets)
            {
                if (layer.LayerName.Content == "Sequence")
                {
                    writer.WriteLine("    {\"resourceType\":\"GMRAssetLayer\",\"resourceVersion\":\"1.0\",\"name\":\"Sequence\",\"assets\":[");
                    foreach (var sequence in layer.AssetsData.Sequences)
                    {
                        writer.WriteLine("        {\"resourceType\":\"GMRSequenceGraphic\",\"resourceVersion\":\"1.0\",\"name\":\"" + sequence.Name.Content + "\",\"animationSpeed\":1.0,\"colour\":" + sequence.Color + ",\"frozen\":false,\"headPosition\":0.0,\"ignore\":false,\"inheritedItemId\":null,\"inheritItemSettings\":false,\"rotation\":0.0,\"scaleX\":" + sequence.ScaleX.ToString("0.0") + ",\"scaleY\":" + sequence.ScaleY.ToString("0.0") + ",\"sequenceId\":{\"name\":\"" + sequence.Sequence.Name.Content + "\",\"path\":\"sequences/" + sequence.Sequence.Name.Content + "/" + sequence.Sequence.Name.Content + ".yy\",},\"x\":" + sequence.X.ToString("0.0") + ",\"y\":" + sequence.Y.ToString("0.0") + ",},");
                    }
                    writer.WriteLine("      ],\"depth\":" + layer.LayerDepth + ",\"effectEnabled\":" + (layer.EffectEnabled ? "true" : "false") + ",\"effectType\":null,\"gridX\":20,\"gridY\":20,\"hierarchyFrozen\":false,\"inheritLayerDepth\":false,\"inheritLayerSettings\":false,\"inheritSubLayers\":true,\"inheritVisibility\":true,\"layers\":[],\"properties\":[],\"userdefinedDepth\":false,\"visible\":true,},");
                }
                // else
                // {
                //     throw new InvalidOperationException("Asset Layer Content (" + layer.LayerName.Content + ") Unknown");
                // }
            }
            else if (layer.LayerType == UndertaleRoom.LayerType.Instances)
            {
                writer.WriteLine("    {\"resourceType\":\"GMRInstanceLayer\",\"resourceVersion\":\"1.0\",\"name\":\"" + layer.LayerName.Content + "\",\"depth\":" + layer.LayerDepth + ",\"effectEnabled\":" + (layer.EffectEnabled ? "true" : "false") + ",\"effectType\":null,\"gridX\":20,\"gridY\":20,\"hierarchyFrozen\":false,\"inheritLayerDepth\":false,\"inheritLayerSettings\":false,\"inheritSubLayers\":true,\"inheritVisibility\":true,\"instances\":[");
                foreach (var instance in layer.InstancesData.Instances)
                {
                    writer.WriteLine("        {\"resourceType\":\"GMRInstance\",\"resourceVersion\":\"1.0\",\"name\":\"inst_" + instance.InstanceID + "\",\"colour\":" + instance.Color + ",\"frozen\":false,\"hasCreationCode\":" + (instance.CreationCode == null ? "false" : "true") + ",\"ignore\":false,\"imageIndex\":" + instance.ImageIndex + ",\"imageSpeed\":" + instance.ImageSpeed.ToString("0.0") + ",\"inheritCode\":false,\"inheritedItemId\":null,\"inheritItemSettings\":false,\"isDnd\":false,\"objectId\":{\"name\":\"" + instance.ObjectDefinition.Name.Content + "\",\"path\":\"objects/" + instance.ObjectDefinition.Name.Content + "/" + instance.ObjectDefinition.Name.Content + ".yy\",},\"properties\":[],\"rotation\":" + instance.Rotation.ToString("0.0") + ",\"scaleX\":" + instance.ScaleX.ToString("0.0") + ",\"scaleY\":" + instance.ScaleY.ToString("0.0") + ",\"x\":" + instance.X.ToString("0.0") + ",\"y\":" + instance.Y.ToString("0.0") + ",},");
                }
                writer.WriteLine("      ],\"layers\":[],\"properties\":[],\"userdefinedDepth\":false,\"visible\":true,},");
            }
            // else
            // {
            //     throw new InvalidOperationException("Asset Layer (" + layer.LayerType + ") Unknown");
            // }
        }
        writer.WriteLine("  ],");
        writer.WriteLine("  \"parent\": {");
        writer.WriteLine("    \"name\": \"Rooms\",");
        writer.WriteLine("    \"path\": \"folders/Rooms.yy\",");
        writer.WriteLine("  },");
        writer.WriteLine("  \"parentRoom\": null,");
        writer.WriteLine("  \"physicsSettings\": {");
        writer.WriteLine("    \"inheritPhysicsSettings\": false,");
        writer.WriteLine("    \"PhysicsWorld\": " + (room.World ? "true" : "false") + ",");
        writer.WriteLine("    \"PhysicsWorldGravityX\": " + room.GravityX.ToString("0.0") + ",");
        writer.WriteLine("    \"PhysicsWorldGravityY\": " + room.GravityY.ToString("0.0") + ",");
        writer.WriteLine(
            "    \"PhysicsWorldPixToMetres\": " + room.MetersPerPixel.ToString("0.0") + ","
        );
        writer.WriteLine("  },");
        writer.WriteLine("  \"roomSettings\": {");
        writer.WriteLine("    \"Height\": " + room.Height + ",");
        writer.WriteLine("    \"inheritRoomSettings\": false,");
        writer.WriteLine("    \"persistent\": " + (room.Persistent ? "true" : "false") + ",");
        writer.WriteLine("    \"Width\": " + room.Width + ",");
        writer.WriteLine("  },");
        writer.WriteLine("  \"sequenceId\": null,");
        writer.WriteLine("  \"views\": [");
        foreach (var g in room.Views)
        {
            writer.Write("    {");
            writer.Write("\"hborder\":" + g.BorderX + ",");
            writer.Write("\"hport\":" + g.PortHeight + ",");
            writer.Write("\"hspeed\":" + g.SpeedX + ",");
            writer.Write("\"hview\":" + g.ViewHeight + ",");
            writer.Write("\"inherit\":false,");
            // begin objectId
            if (g.ObjectId is null)
            {
                writer.Write("\"objectId\":null,");
            }
            else
            {
                var object_name = g.ObjectId.Name.Content;
                writer.Write(
                    "\"objectId\":{\"name\":\""
                        + object_name
                        + "\",\"path\":\"objects/"
                        + object_name
                        + "/"
                        + object_name
                        + ".yy\",},"
                );
            }
            // end objectId
            writer.Write("\"vborder\":" + g.BorderY + ",");
            writer.Write("\"visible\":" + (g.Enabled ? "true" : "false") + ",");
            writer.Write("\"vspeed\":" + g.SpeedY + ",");
            writer.Write("\"wport\":" + g.PortWidth + ",");
            writer.Write("\"wview\":" + g.ViewWidth + ",");
            writer.Write("\"xport\":" + g.PortX + ",");
            writer.Write("\"xview\":" + g.ViewX + ",");
            writer.Write("\"yport\":" + g.PortY + ",");
            writer.Write("\"yview\":" + g.ViewY + ",");
            writer.WriteLine("},");
        }
        writer.WriteLine("  ],");
        writer.WriteLine("  \"viewSettings\": {");
        writer.WriteLine(
            "    \"clearDisplayBuffer\": "
                + (
                    room.Flags.HasFlag(UndertaleRoom.RoomEntryFlags.DoNotClearDisplayBuffer)
                        ? "false"
                        : "true"
                )
                + ","
        );
        writer.WriteLine("    \"clearViewBackground\": false,");
        writer.WriteLine(
            "    \"enableViews\": "
                + (room.Flags.HasFlag(UndertaleRoom.RoomEntryFlags.EnableViews) ? "true" : "false")
                + ","
        );
        writer.WriteLine("    \"inheritViewSettings\": false,");
        writer.WriteLine("  },");
        writer.WriteLine("  \"volume\": 1.0,");
        writer.Write("}");
    }

    IncrementProgressParallel();
}
void DumpCachedCode(KeyValuePair<string, string> code)
{
    string path = Path.Combine(roomsFolder, code.Key + ".gml");

    File.WriteAllText(path, code.Value);

    IncrementProgressParallel();
}
