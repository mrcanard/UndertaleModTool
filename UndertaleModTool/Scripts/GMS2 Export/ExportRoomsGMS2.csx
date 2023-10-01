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

string roomsFolder = GetFolder(FilePath) + "rooms" + Path.DirectorySeparatorChar;
ThreadLocal<GlobalDecompileContext> DECOMPILE_CONTEXT = new ThreadLocal<GlobalDecompileContext>(() => new GlobalDecompileContext(Data, false));
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
        writer.WriteLine("    {\"id\":{\"name\":\"" + room.Name.Content + "\",\"path\":\"rooms/" + room.Name.Content + "/" + room.Name.Content + ".yy\",},},");
    }
}

// Room order
using (StreamWriter writer = new StreamWriter(roomsFolder + "room_order.txt"))
{
    for (int i = 0; i < Data.Rooms.Count; i++)
    {
        UndertaleRoom room = Data.Rooms[i];
        writer.WriteLine("    {\"roomId\":{\"name\":\"" + room.Name.Content + "\",\"path\":\"rooms/" + room.Name.Content + "/" + room.Name.Content + ".yy\",},},");        
    }
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

    Directory.CreateDirectory(roomsFolder + room.Name.Content);

    using (StreamWriter writer = new StreamWriter(roomsFolder + room.Name.Content + "\\" + room.Name.Content + ".yy"))
    {


        writer.WriteLine("{");
        writer.WriteLine("  \"resourceType\": \"GMRoom\",");
        writer.WriteLine("  \"resourceVersion\": \"1.0\",");
        writer.WriteLine("  \"name\": \""+ room.Name.Content +"\",");
        writer.WriteLine("  \"creationCodeFile\": \"\",");
        writer.WriteLine("  \"inheritCode\": false,");
        writer.WriteLine("  \"inheritCreationOrder\": false,");
        writer.WriteLine("  \"inheritLayers\": false,");
        writer.WriteLine("  \"instanceCreationOrder\": [");
        foreach (var g in room.GameObjects)
        {
            writer.WriteLine("    {\"name\":\"inst_"+ g.InstanceID +"\",\"path\":\"rooms/"+ room.Name.Content +"/"+ room.Name.Content +".yy\",},");
        }
        writer.WriteLine("  ],");
        writer.WriteLine("  \"isDnd\": false,");
        writer.WriteLine("  \"layers\": [");
        foreach (var layer in room.Layers)
        {
            if(layer.LayerType == UndertaleRoom.LayerType.Path) {
                // Path
                // pass
            } else if(layer.LayerType == UndertaleRoom.LayerType.Instances) {
                // Instances
                writer.WriteLine("    {\"resourceType\":\"GMRInstanceLayer\",\"resourceVersion\":\"1.0\",\"name\":\""+layer.LayerName.Content+"\",\"depth\":"+layer.LayerDepth+",\"effectEnabled\":"+(layer.EffectEnabled ? "true" : "false")+",\"effectType\":"+(layer.EffectType is null ? "null" : layer.EffectType.Content)+",\"gridX\":"+room.GridWidth+",\"gridY\":"+room.GridHeight+",\"hierarchyFrozen\":false,\"inheritLayerDepth\":false,\"inheritLayerSettings\":false,\"inheritSubLayers\":true,\"inheritVisibility\":true,\"instances\":[");
                foreach (var g in layer.InstancesData.Instances) {
                    writer.WriteLine("        {\"resourceType\":\"GMRInstance\",\"resourceVersion\":\"1.0\",\"name\":\"inst_"+g.InstanceID+"\",\"colour\":4294967295,\"frozen\":false,\"hasCreationCode\":"+(g.CreationCode is null ? "false" : "true")+",\"ignore\":false,\"imageIndex\":"+g.ImageIndex+",\"imageSpeed\":"+g.ImageSpeed.ToString("0.0")+",\"inheritCode\":false,\"inheritedItemId\":null,\"inheritItemSettings\":false,\"isDnd\":false,\"objectId\":{\"name\":\""+g.ObjectDefinition.Name.Content+"\",\"path\":\"objects/"+g.ObjectDefinition.Name.Content+"/"+g.ObjectDefinition.Name.Content+".yy\",},\"properties\":[],\"rotation\":"+g.Rotation.ToString("0.0")+",\"scaleX\":"+g.ScaleX.ToString("0.0")+",\"scaleY\":"+g.ScaleY.ToString("0.0")+",\"x\":"+g.X.ToString("F01")+",\"y\":"+g.Y.ToString("F01")+",},");
                }
                writer.WriteLine("      ],\"layers\":[],\"properties\":[],\"userdefinedDepth\":false,\"visible\":"+(layer.IsVisible ? "true" : "false")+",},");
            } else if(layer.LayerType == UndertaleRoom.LayerType.Tiles) {
                // Tiles
                // pass
            } else if(layer.LayerType == UndertaleRoom.LayerType.Background) {
                // Background
                writer.Write("    {\"resourceType\":\"GMRBackgroundLayer\",");
                writer.Write("\"resourceVersion\":\"1.0\",");
                writer.Write("\"name\":\"" + layer.LayerName.Content + "\",");
                writer.Write("\"animationFPS\":" + layer.BackgroundData.AnimationSpeed.ToString("0.0") + ",");
                writer.Write("\"animationSpeedType\":" + (int)layer.BackgroundData.AnimationSpeedType + ",");
                writer.Write("\"colour\":"+ (int) (layer.BackgroundData.Color ^ 0xFF000000) +",");
                writer.Write("\"depth\":" + layer.LayerDepth + ",");
                writer.Write("\"effectEnabled\":" + (layer.EffectEnabled ? "true" : "false") + ",");
                writer.Write("\"effectType\":" + (layer.EffectType is null ? "null" : layer.EffectType.Content) + ",");
                writer.Write("\"gridX\":" + room.GridWidth + ",");
                writer.Write("\"gridY\":" + room.GridHeight + ",");
                writer.Write("\"hierarchyFrozen\":false,");
                writer.Write("\"hspeed\":" + layer.HSpeed.ToString("0.0") + ",");
                writer.Write("\"htiled\":" + (layer.BackgroundData.TiledHorizontally ? "true" : "false") + ",");
                writer.Write("\"inheritLayerDepth\":false,");
                writer.Write("\"inheritLayerSettings\":false,");
                writer.Write("\"inheritSubLayers\":true,");
                writer.Write("\"inheritVisibility\":true,");
                writer.Write("\"layers\":[],");
                writer.Write("\"properties\":[],");
                writer.Write("\"spriteId\":");
                if(layer.BackgroundData.Sprite is null)
                {
                    writer.Write("null,");
                } else
                {
                    // begin spriteId
                    writer.Write("{");
                    writer.Write("\"name\":\"" + layer.BackgroundData.Sprite.Name.Content + "\",");
                    writer.Write("\"path\":\"sprites/" + layer.BackgroundData.Sprite.Name.Content + "/" + layer.BackgroundData.Sprite.Name.Content + ".yy\",");
                    writer.Write("},");
                    // end spriteId
                }
                writer.Write("\"stretch\":" + (layer.BackgroundData.Stretch ? "true" : "false") + ",");
                writer.Write("\"userdefinedAnimFPS\":false,");
                writer.Write("\"userdefinedDepth\":false,");
                writer.Write("\"visible\":" + (layer.IsVisible ? "true" : "false") + ",");
                writer.Write("\"vspeed\":" + layer.VSpeed.ToString("0.0") + ",");
                writer.Write("\"vtiled\":" + (layer.BackgroundData.TiledVertically ? "true" : "false") + ",");
                writer.Write("\"x\":0,");
                writer.Write("\"y\":0,");
                writer.WriteLine("},");
            } else if(layer.LayerType == UndertaleRoom.LayerType.Assets) {
                // Assets
                writer.WriteLine("    {\"resourceType\":\"GMRAssetLayer\",\"resourceVersion\":\"1.0\",\"name\":\""+layer.LayerName.Content+"\",\"assets\":[");
                foreach(var g in layer.AssetsData.Sequences) {
                    // Les sequences
                    writer.WriteLine("        {\"resourceType\":\"GMRSequenceGraphic\",\"resourceVersion\":\"1.0\",\"name\":\""+g.Name.Content+"\",\"animationSpeed\":"+g.AnimationSpeed.ToString("0.0")+",\"colour\":"+g.Color+",\"frozen\":false,\"headPosition\":0.0,\"ignore\":false,\"inheritedItemId\":null,\"inheritItemSettings\":false,\"rotation\":"+g.Rotation.ToString("0.0")+",\"scaleX\":"+g.ScaleX.ToString("0.0")+",\"scaleY\":"+g.ScaleY.ToString("0.0")+",\"sequenceId\":{\"name\":\""+g.Sequence.Name.Content+"\",\"path\":\"sequences/"+g.Sequence.Name.Content+"/"+g.Sequence.Name.Content+".yy\",},\"x\":"+g.X.ToString("0.0")+",\"y\":"+g.X.ToString("0.0")+",},");
                }
                foreach(var g in layer.AssetsData.Sprites) {
                    // Les sprites
                    writer.WriteLine("        {\"resourceType\":\"GMRSpriteGraphic\",\"resourceVersion\":\"1.0\",\"name\":\""+g.Name.Content+"\",\"animationSpeed\":"+g.AnimationSpeed.ToString("0.0")+",\"colour\":"+g.Color+",\"frozen\":false,\"headPosition\":0.0,\"ignore\":false,\"inheritedItemId\":null,\"inheritItemSettings\":false,\"rotation\":"+g.Rotation.ToString("0.0")+",\"scaleX\":"+g.ScaleX.ToString("0.0")+",\"scaleY\":"+g.ScaleY.ToString("0.0")+",\"spriteId\":{\"name\":\""+g.Sprite.Name.Content+"\",\"path\":\"sprites/"+g.Sprite.Name.Content+"/"+g.Sprite.Name.Content+".yy\",},\"x\":"+g.X.ToString("F01")+",\"y\":"+g.Y.ToString("F01")+",},");
                }
                writer.WriteLine("      ],\"depth\":"+layer.LayerDepth+",\"effectEnabled\":"+(layer.EffectEnabled ? "true" : "false")+",\"effectType\":"+(layer.EffectType is null ? "null" : layer.EffectType.Content)+",\"gridX\":"+room.GridWidth+",\"gridY\":"+room.GridHeight+",\"hierarchyFrozen\":false,\"inheritLayerDepth\":false,\"inheritLayerSettings\":false,\"inheritSubLayers\":true,\"inheritVisibility\":true,\"layers\":[],\"properties\":[],\"userdefinedDepth\":false,\"visible\":true,},");

            } else if(layer.LayerType == UndertaleRoom.LayerType.Effect) {
                // Effect
                // pass
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
        writer.WriteLine("    \"PhysicsWorld\": "+(room.World ? "true" : "false")+",");
        writer.WriteLine("    \"PhysicsWorldGravityX\": "+room.GravityX.ToString("0.0")+",");
        writer.WriteLine("    \"PhysicsWorldGravityY\": "+room.GravityY.ToString("0.0")+",");
        writer.WriteLine("    \"PhysicsWorldPixToMetres\": "+room.MetersPerPixel.ToString("0.0")+",");
        writer.WriteLine("  },");
        writer.WriteLine("  \"roomSettings\": {");
        writer.WriteLine("    \"Height\": "+room.Height+",");
        writer.WriteLine("    \"inheritRoomSettings\": false,");
        writer.WriteLine("    \"persistent\": "+(room.Persistent ? "true" : "false")+",");
        writer.WriteLine("    \"Width\": "+room.Width+",");
        writer.WriteLine("  },");
        writer.WriteLine("  \"sequenceId\": null,");
        writer.WriteLine("  \"views\": [");
        foreach(var g in room.Views) {
            writer.Write("    {");
            writer.Write("\"hborder\":" + g.BorderX + ",");
            writer.Write("\"hport\":" + g.PortHeight + ",");
            writer.Write("\"hspeed\":" + g.SpeedX + ",");
            writer.Write("\"hview\":" + g.ViewHeight + ",");
            writer.Write("\"inherit\":false,");
            writer.Write("\"objectId\":" + (g.ObjectId is null ? "null" : "\"" + g.ObjectId.Name.Content + "\"") + ",");
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
        writer.WriteLine("    \"clearDisplayBuffer\": "+(room.Flags.HasFlag(UndertaleRoom.RoomEntryFlags.ClearDisplayBuffer) ? "true" : "false")+",");
        writer.WriteLine("    \"clearViewBackground\": false,");
        writer.WriteLine("    \"enableViews\": "+(room.Flags.HasFlag(UndertaleRoom.RoomEntryFlags.EnableViews) ? "true" : "false")+",");
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