using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

EnsureDataLoaded();

if (Data.IsGameMaker2())
{
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
    toDump = new();
    if (!exportFromCache)
    {
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
            UndertaleResourceById<
                UndertaleRoom,
                UndertaleChunkROOM
            > _room in Data.GeneralInfo.RoomOrder
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
                    "  \"creationCodeFile\": \"rooms/"
                        + room.Name.Content
                        + "/RoomCreationCode.gml\","
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
            writer.WriteLine(
                "    \"PhysicsWorldGravityX\": " + room.GravityX.ToString("0.0") + ","
            );
            writer.WriteLine(
                "    \"PhysicsWorldGravityY\": " + room.GravityY.ToString("0.0") + ","
            );
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
                            ? "true"
                            : "false"
                    )
                    + ","
            );
            writer.WriteLine("    \"clearViewBackground\": false,");
            writer.WriteLine(
                "    \"enableViews\": "
                    + (
                        room.Flags.HasFlag(UndertaleRoom.RoomEntryFlags.EnableViews)
                            ? "true"
                            : "false"
                    )
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
}
else
{
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
    toDump = new();
    if (!exportFromCache)
    {
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
        writer.WriteLine("  <rooms name=\"rooms\">");
        for (int i = 0; i < Data.Rooms.Count; i++)
        {
            UndertaleRoom room = Data.Rooms[i];
            writer.WriteLine("    <room>rooms\\" + room.Name.Content + "</room>");
        }
        writer.WriteLine("  </rooms>");
    }

    ScriptMessage("Export Complete.\n\nLocation: " + roomsFolder);

    string GetFolder(string path)
    {
        return Path.GetDirectoryName(path) + Path.DirectorySeparatorChar;
    }

    async Task DumpRooms()
    {
        if (Data.KnownSubFunctions is null) //if we run script before opening any code
            Decompiler.BuildSubFunctionCache(Data);

        await Task.Run(() => Parallel.ForEach(toDump, DumpRoom));
    }

    void DumpRoom(UndertaleRoom room)
    {
        using (
            StreamWriter writer = new StreamWriter(roomsFolder + room.Name.Content + ".room.gmx")
        )
        {
            writer.WriteLine(
                "<!--This Document is generated by GameMaker, if you edit it by hand then you do so at your own risk!-->"
            );
            writer.WriteLine("<room>");
            writer.WriteLine(
                "  <caption>" + (room.Caption == null ? "" : room.Caption.Content) + "</caption>"
            );
            writer.WriteLine("  <width>" + room.Width + "</width>");
            writer.WriteLine("  <height>" + room.Height + "</height>");
            writer.WriteLine("  <vsnap>1</vsnap>");
            writer.WriteLine("  <hsnap>1</hsnap>");
            writer.WriteLine("  <isometric>0</isometric>");
            writer.WriteLine("  <speed>" + room.Speed + "</speed>");
            writer.WriteLine("  <persistent>" + (room.Persistent ? -1 : 0) + "</persistent>");
            writer.WriteLine("  <colour>" + (room.BackgroundColor ^ 0xFF000000) + "</colour>");
            writer.WriteLine(
                "  <showcolour>" + (room.DrawBackgroundColor ? -1 : 0) + "</showcolour>"
            );
            writer.WriteLine(
                "  <code>"
                    + (
                        room.CreationCodeId != null
                            ? Decompiler.Decompile(room.CreationCodeId, DECOMPILE_CONTEXT.Value)
                            : ""
                    )
                    + "</code>"
            );

            bool flagEnableViews = room.Flags.HasFlag(UndertaleRoom.RoomEntryFlags.EnableViews);
            bool flagShowColor = room.Flags.HasFlag(UndertaleRoom.RoomEntryFlags.ShowColor);
            bool flagClearDisplayBuffer = room.Flags.HasFlag(
                UndertaleRoom.RoomEntryFlags.DoNotClearDisplayBuffer
            );

            writer.WriteLine("  <enableViews>" + (flagEnableViews ? -1 : 0) + "</enableViews>");
            writer.WriteLine(
                "  <clearViewBackground>" + (flagShowColor ? -1 : 0) + "</clearViewBackground>"
            );
            writer.WriteLine(
                "  <clearDisplayBuffer>"
                    + (flagClearDisplayBuffer ? -1 : 0)
                    + "</clearDisplayBuffer>"
            );
            writer.WriteLine("  <makerSettings>");
            writer.WriteLine("    <isSet>0</isSet>");
            writer.WriteLine("    <w>0</w>");
            writer.WriteLine("    <h>0</h>");
            writer.WriteLine("    <showGrid>0</showGrid>");
            writer.WriteLine("    <showObjects>0</showObjects>");
            writer.WriteLine("    <showTiles>0</showTiles>");
            writer.WriteLine("    <showBackgrounds>0</showBackgrounds>");
            writer.WriteLine("    <showForegrounds>0</showForegrounds>");
            writer.WriteLine("    <showViews>0</showViews>");
            writer.WriteLine("    <showForegrounds>0</showForegrounds>");
            writer.WriteLine("    <deleteUnderlyingObj>0</deleteUnderlyingObj>");
            writer.WriteLine("    <deleteUnderlyingTiles>0</deleteUnderlyingTiles>");
            writer.WriteLine("    <page>0</page>");
            writer.WriteLine("    <xoffset>0</xoffset>");
            writer.WriteLine("    <yoffset>0</yoffset>");
            writer.WriteLine("  </makerSettings>");
            writer.WriteLine("  <backgrounds>");
            foreach (var b in room.Backgrounds)
            {
                writer.WriteLine(
                    "    <background visible=\""
                        + (b.Enabled ? -1 : 0)
                        + "\" foreground=\""
                        + (b.Foreground ? -1 : 0)
                        + "\" name=\""
                        + (
                            b.BackgroundDefinition == null
                                ? ""
                                : b.BackgroundDefinition.Name.Content
                        )
                        + "\" x=\""
                        + b.X
                        + "\" y=\""
                        + b.Y
                        + "\" htiled=\""
                        + (b.TiledHorizontally ? -1 : 0)
                        + "\" vtiled=\""
                        + (b.TiledVertically ? -1 : 0)
                        + "\" hspeed=\""
                        + b.SpeedX
                        + "\" vspeed=\""
                        + b.SpeedY
                        + "\" stretch=\""
                        + (b.Stretch ? -1 : 0)
                        + "\"/>"
                );
            }
            writer.WriteLine("  </backgrounds>");
            writer.WriteLine("  <views>");
            foreach (var v in room.Views)
            {
                writer.WriteLine(
                    "    <view visible=\""
                        + (v.Enabled ? -1 : 0)
                        + "\" objName=\""
                        + (v.ObjectId == null ? "&lt;undefined&gt;" : v.ObjectId.Name.Content)
                        + "\" xview=\""
                        + v.ViewX
                        + "\" yview=\""
                        + v.ViewY
                        + "\" wview=\""
                        + v.ViewWidth
                        + "\" hview=\""
                        + v.ViewHeight
                        + "\" xport=\""
                        + v.PortX
                        + "\" yport=\""
                        + v.PortY
                        + "\" wport=\""
                        + v.PortWidth
                        + "\" hport=\""
                        + v.PortHeight
                        + "\" hborder=\""
                        + v.BorderX
                        + "\" vborder=\""
                        + v.BorderY
                        + "\" hspeed=\""
                        + v.SpeedX
                        + "\" vspeed=\""
                        + v.SpeedY
                        + "\"/>"
                );

                //writer.WriteLine("Enabled = " + v.Enabled);
                //writer.WriteLine("ViewX = " + v.ViewX);
                //writer.WriteLine("ViewY = " + v.ViewY);
                //writer.WriteLine("ViewWidth = " + v.ViewWidth);
                //writer.WriteLine("ViewHeight = " + v.ViewHeight);
                //writer.WriteLine("PortX = " + v.PortX);
                //writer.WriteLine("PortY = " + v.PortY);
                //writer.WriteLine("PortWidth = " + v.PortWidth);
                //writer.WriteLine("PortHeight = " + v.PortHeight);
                //writer.WriteLine("BorderX = " + v.BorderX);
                //writer.WriteLine("BorderY = " + v.BorderY);
                //writer.WriteLine("SpeedX = " + v.SpeedX);
                //writer.WriteLine("SpeedY = " + v.SpeedY);
            }
            writer.WriteLine("  </views>");
            writer.WriteLine("  <instances>");
            foreach (var g in room.GameObjects)
            {
                writer.WriteLine(
                    "    <instance objName=\""
                        + (
                            g.ObjectDefinition == null
                                ? "undefined"
                                : g.ObjectDefinition.Name.Content
                        )
                        + "\" x=\""
                        + g.X
                        + "\" y=\""
                        + g.Y
                        + "\" name=\""
                        + "inst_"
                        + g.InstanceID
                        + "\" locked=\""
                        + 0
                        + "\" code=\""
                        + ""
                        + "\" scaleX=\""
                        + g.ScaleX
                        + "\" scaleY=\""
                        + g.ScaleY
                        + "\" colour=\""
                        + g.Color
                        + "\" rotation=\""
                        + g.Rotation
                        + "\"/>"
                );
            }
            writer.WriteLine("  </instances>");
            writer.WriteLine("  <tiles>");
            foreach (var t in room.Tiles)
            {
                writer.WriteLine(
                    "    <tile bgName=\""
                        + t.ObjectDefinition.Name.Content
                        + "\" x=\""
                        + t.X
                        + "\" y=\""
                        + t.Y
                        + "\" w=\""
                        + t.Width
                        + "\" h=\""
                        + t.Height
                        + "\" xo=\""
                        + t.SourceX
                        + "\" yo=\""
                        + t.SourceY
                        + "\" id=\""
                        + t.InstanceID
                        + "\" name=\""
                        + "inst_"
                        + t.InstanceID
                        + "\" depth=\""
                        + t.TileDepth
                        + "\" locked=\""
                        + "0"
                        + "\" colour=\""
                        + t.Color
                        + "\" scaleX=\""
                        + t.ScaleX
                        + "\" scaleY=\""
                        + t.ScaleY
                        + "\"/>"
                );
            }
            writer.WriteLine("  </tiles>");
            writer.WriteLine("  <PhysicsWorld>" + (room.World ? -1 : 0) + "</PhysicsWorld>");
            writer.WriteLine("  <PhysicsWorldTop>" + room.Top + "</PhysicsWorldTop>");
            writer.WriteLine("  <PhysicsWorldLeft>" + room.Left + "</PhysicsWorldLeft>");
            writer.WriteLine("  <PhysicsWorldRight>" + room.Right + "</PhysicsWorldRight>");
            writer.WriteLine("  <PhysicsWorldBottom>" + room.Bottom + "</PhysicsWorldBottom>");
            writer.WriteLine(
                "  <PhysicsWorldGravityX>" + room.GravityX + "</PhysicsWorldGravityX>"
            );
            writer.WriteLine(
                "  <PhysicsWorldGravityY>" + room.GravityY + "</PhysicsWorldGravityY>"
            );
            writer.WriteLine(
                "  <PhysicsWorldPixToMeters>"
                    + room.MetersPerPixel.ToString(
                        "0.00",
                        System.Globalization.CultureInfo.InvariantCulture
                    )
                    + "</PhysicsWorldPixToMeters>"
            );

            writer.WriteLine("</room>");

            //writer.WriteLine("  <events>");
            //var i = 0;
            //foreach (var e1 in game_object.Events)
            //{
            //    foreach (var e2 in e1)
            //    {
            //        writer.WriteLine("    <event eventtype=\"" + i + "\" enumb=\"" + e2.EventSubtype + "\">");
            //        foreach (var a in e2.Actions)
            //        {
            //            writer.WriteLine("      <action>");
            //            writer.WriteLine("        <libid>"+ a.LibID + "</libid>");
            //            writer.WriteLine("        <id>"+ a.ID + "</id>");
            //            writer.WriteLine("        <kind>"+ a.Kind + "</kind>");
            //            writer.WriteLine("        <userelative>"+ (a.UseRelative ? -1 : 0) + "</userelative>");
            //            writer.WriteLine("        <isquestion>"+ (a.IsQuestion ? -1 : 0) + "</isquestion>");
            //            writer.WriteLine("        <useapplyto>"+ (a.UseApplyTo ? -1 : 0) + "</useapplyto>");
            //            writer.WriteLine("        <exetype>"+ a.ExeType + "</exetype>");
            //            writer.WriteLine("        <functionname></functionname>");
            //            writer.WriteLine("        <codestring></codestring>");
            //            writer.WriteLine("        <whoName>"+ (a.Who == -1 ? "self" : "") + "</whoName>");
            //            writer.WriteLine("        <relative>" + (a.Relative ? -1 : 0) + "</relative>");
            //            writer.WriteLine("        <isnot>"+ (a.IsNot ? -1 : 0) + "</isnot>");
            //            writer.WriteLine("        <arguments>");
            //            writer.WriteLine("          <argument>");
            //            writer.WriteLine("            <kind>" + a.ArgumentCount + "</kind>");
            //            writer.WriteLine("            <string>" + (a.CodeId != null ? Decompiler.Decompile(a.CodeId, DECOMPILE_CONTEXT.Value) : "") + "</string>");
            //            writer.WriteLine("          </argument>");
            //            writer.WriteLine("        </arguments>");
            //            writer.WriteLine("      </action>");
            //        }
            //        writer.WriteLine("    </event>");
            //    }
            //    i++;
            //}
            //writer.WriteLine("  </events>");
            //writer.WriteLine("  <PhysicsObject>" + (game_object.UsesPhysics ? -1 : 0) + "</PhysicsObject>");
            //writer.WriteLine("  <PhysicsObjectSensor>" + (game_object.IsSensor ? -1 : 0) + "</PhysicsObjectSensor>");
            //writer.WriteLine("  <PhysicsObjectShape>" + (int) game_object.CollisionShape + "</PhysicsObjectShape>");
            //writer.WriteLine("  <PhysicsObjectDensity>" + game_object.Density.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture) + "</PhysicsObjectDensity>");
            //writer.WriteLine("  <PhysicsObjectRestitution>" + game_object.Restitution.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture) + "</PhysicsObjectRestitution>");
            //writer.WriteLine("  <PhysicsObjectGroup>" + game_object.Group + "</PhysicsObjectGroup>");
            //writer.WriteLine("  <PhysicsObjectLinearDamping>" + game_object.LinearDamping.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture) + "</PhysicsObjectLinearDamping>");
            //writer.WriteLine("  <PhysicsObjectAngularDamping>" + game_object.AngularDamping.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture) + "</PhysicsObjectAngularDamping>");
            //writer.WriteLine("  <PhysicsObjectFriction>" + game_object.Friction.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture) + "</PhysicsObjectFriction>");
            //writer.WriteLine("  <PhysicsObjectAwake>" + (game_object.Awake ? -1 : 0) + "</PhysicsObjectAwake>");
            //writer.WriteLine("  <PhysicsObjectKinematic>" + (game_object.Kinematic ? -1 : 0) + "</PhysicsObjectKinematic>");
            //writer.WriteLine("  <PhysicsShapePoints/>");
            //writer.WriteLine("</object>");
        }

        IncrementProgressParallel();
    }
}
