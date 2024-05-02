using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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

await DumpRooms();

await StopProgressBarUpdater();
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

    using (
        StreamWriter writer = new StreamWriter(
            roomsFolder + room.Name.Content + "\\" + room.Name.Content + ".yy"
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
                roomsFolder + room.Name.Content + "\\" + "RoomCreationCode.gml",
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

        // Composition depth
        var depthList = new SortedSet<int>();
        // Depth des objets
        foreach (var g in room.GameObjects)
        {
            depthList.Add(g.ObjectDefinition.Depth);
        }
        // Depth des Tiles
        foreach (var t in room.Tiles)
        {
            depthList.Add(t.TileDepth);
        }
        // Création des Layers
        foreach (var depth in depthList)
        {
            // depth pour Tile ou Instance ?
            var isTileLayer = false;

            foreach (var tile in room.Tiles)
            {
                if (tile.TileDepth == depth)
                {
                    isTileLayer = true;
                }
            }

            if (isTileLayer)
            {
                writer.WriteLine(
                    "    {\"resourceType\":\"GMRAssetLayer\",\"resourceVersion\":\"1.0\",\"name\":\"Compatibility_Tiles_Depth_"
                        + depth
                        + "\",\"assets\":["
                );
                // Tiles
                foreach (var t in room.Tiles)
                {
                    if (t.TileDepth == depth)
                    {
                        var resource_name = (
                            t.spriteMode
                                ? t.SpriteDefinition.Name.Content
                                : t.BackgroundDefinition.Name.Content
                        );
                        writer.WriteLine(
                            "        {\"resourceType\":\"GMRGraphic\",\"resourceVersion\":\"1.0\",\"name\":\"inst_"
                                + t.InstanceID
                                + "\",\"colour\":"
                                + t.Color
                                + ",\"frozen\":false,\"h\":"
                                + t.Height
                                + ",\"ignore\":false,\"inheritedItemId\":null,\"inheritItemSettings\":false,\"spriteId\":{\"name\":\""
                                + resource_name
                                + "\",\"path\":\"sprites/"
                                + resource_name
                                + "/"
                                + resource_name
                                + ".yy\",},\"u0\":"
                                + t.SourceX
                                + ",\"u1\":"
                                + (t.SourceX + t.Width)
                                + ",\"v0\":"
                                + t.SourceY
                                + ",\"v1\":"
                                + (t.SourceY + t.Height)
                                + ",\"w\":"
                                + t.Width
                                + ",\"x\":"
                                + t.X.ToString("0.0")
                                + ",\"y\":"
                                + t.Y.ToString("0.0")
                                + ",},"
                        );
                    }
                }
                // End Tiles
                writer.WriteLine(
                    "    ],\"depth\":"
                        + depth
                        + ",\"effectEnabled\":true,\"effectType\":null,\"gridX\":32,\"gridY\":32,\"hierarchyFrozen\":false,\"inheritLayerDepth\":false,\"inheritLayerSettings\":false,\"inheritSubLayers\":true,\"inheritVisibility\":true,\"layers\":[],\"properties\":[],\"userdefinedDepth\":true,\"visible\":true,},"
                );
            }
            else
            {
                writer.WriteLine(
                    "    {\"resourceType\":\"GMRInstanceLayer\",\"resourceVersion\":\"1.0\",\"name\":\"Compatibility_Instances_Depth_"
                        + depth
                        + "\",\"depth\":"
                        + depth
                        + ",\"effectEnabled\":true,\"effectType\":null,\"gridX\":32,\"gridY\":32,\"hierarchyFrozen\":false,\"inheritLayerDepth\":false,\"inheritLayerSettings\":false,\"inheritSubLayers\":true,\"inheritVisibility\":true,\"instances\":["
                );
                // Instances
                foreach (var g in room.GameObjects)
                {
                    if (g.ObjectDefinition.Depth == depth)
                    {
                        var resource_name = g.ObjectDefinition.Name.Content;
                        writer.WriteLine(
                            "        {\"resourceType\":\"GMRInstance\",\"resourceVersion\":\"1.0\",\"name\":\"inst_"
                                + g.InstanceID
                                + "\",\"colour\":"
                                + g.Color
                                + ",\"frozen\":false,\"hasCreationCode\":false,\"ignore\":false,\"imageIndex\":"
                                + g.ImageIndex
                                + ",\"imageSpeed\":"
                                + g.ImageSpeed
                                + ",\"inheritCode\":false,\"inheritedItemId\":null,\"inheritItemSettings\":false,\"isDnd\":false,\"objectId\":{\"name\":\""
                                + resource_name
                                + "\",\"path\":\"objects/"
                                + resource_name
                                + "/"
                                + resource_name
                                + ".yy\",},\"properties\":[],\"rotation\":"
                                + g.Rotation.ToString("0.0")
                                + ",\"scaleX\":"
                                + g.ScaleX.ToString("0.0")
                                + ",\"scaleY\":"
                                + g.ScaleY.ToString("0.0")
                                + ",\"x\":"
                                + g.X.ToString("0.0")
                                + ",\"y\":"
                                + g.Y.ToString("0.0")
                                + ",},"
                        );
                    }
                }
                // End Instances
                writer.WriteLine(
                    "      ],\"layers\":[],\"properties\":[],\"userdefinedDepth\":true,\"visible\":true,},"
                );
            }
        }

        // Background
        foreach (var b in room.Backgrounds)
        {
            if (b.BackgroundDefinition != null)
            {
                var resource_name = b.BackgroundDefinition.Name.Content;
                writer.WriteLine(
                    "    {\"resourceType\":\"GMRBackgroundLayer\",\"resourceVersion\":\"1.0\",\"name\":\"Compatibility_Colour\",\"animationFPS\":1.0,\"animationSpeedType\":1,\"colour\":4294967295,\"depth\":2147483600,\"effectEnabled\":true,\"effectType\":null,\"gridX\":32,\"gridY\":32,\"hierarchyFrozen\":false,\"hspeed\":"
                        + b.SpeedX
                        + ",\"htiled\":"
                        + (b.TiledHorizontally ? "true" : "false")
                        + ",\"inheritLayerDepth\":false,\"inheritLayerSettings\":false,\"inheritSubLayers\":true,\"inheritVisibility\":true,\"layers\":[],\"properties\":[],\"spriteId\":{\"name\":\""
                        + resource_name
                        + "\",\"path\":\"sprites/"
                        + resource_name
                        + "/"
                        + resource_name
                        + ".yy\",},\"stretch\":"
                        + (b.Stretch ? "true" : "false")
                        + ",\"userdefinedAnimFPS\":false,\"userdefinedDepth\":true,\"visible\":"
                        + (b.Enabled ? "true" : "false")
                        + ",\"vspeed\":"
                        + b.SpeedY
                        + ",\"vtiled\":"
                        + (b.TiledVertically ? "true" : "false")
                        + ",\"x\":"
                        + b.X
                        + ",\"y\":"
                        + b.Y
                        + ",},"
                );
            }
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
                    room.Flags.HasFlag(UndertaleRoom.RoomEntryFlags.ClearDisplayBuffer)
                        ? "true"
                        : "false"
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
