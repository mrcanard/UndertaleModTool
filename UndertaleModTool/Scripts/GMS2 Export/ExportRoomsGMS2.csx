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

void WriteLayerHeader(StreamWriter writer, String resourceType, UndertaleRoom.Layer layer) {
    writer.Write(
	"    {\"resourceType\":\""+resourceType+"\",\"resourceVersion\":\"1.0\",\"name\":\""
	+ layer.LayerName.Content
	+ "\",\"depth\":"
	+ layer.LayerDepth
	+ ",\"effectEnabled\":"
	+ (layer.EffectEnabled ? "true" : "false")
	+ ",\"effectType\":\""
	+ layer.EffectData.EffectType.Content
	+ "\",\"gridX\":32,\"gridY\":32,\"hierarchyFrozen\":false,\"inheritLayerDepth\":false,\"inheritLayerSettings\":false,\"inheritSubLayers\":true,\"inheritVisibility\":true,\"layers\":[");
}

void WriteLayerProperties(StreamWriter writer, UndertaleRoom.Layer layer)
{
    if(layer.EffectData.Properties.Count > 0) {
	
	foreach (var property in layer.EffectData.Properties)
	{
	    writer.Write(
		Environment.NewLine +
		"        {\"name\":\""
		+ property.Name.Content
		+ "\",\"type\":"
	    + (int)property.Kind
	    + ",\"value\":\""
	    + property.Value.Content
	    + "\",},"
	);
    }
    writer.Write(Environment.NewLine+"    ");
    }
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
		  // writer.WriteLine("  (" + layer.LayerName.Content + "," + layer.LayerType + ", " + layer.LayerDepth + ")");
		  if (layer.LayerType == UndertaleRoom.LayerType.Path)
		  {
		      writer.WriteLine(
			  "    {\"resourceType\":\"GMRPathLayer\",\"resourceVersion\":\"1.0\",\"name\":\"Path\",\"colour\":4294967295,\"depth\":"
			  + layer.LayerDepth
			  + ",\"effectEnabled\":"
			  + (layer.EffectEnabled ? "true" : "false")
			  + ",\"effectType\":"
			  + (layer.EffectType is null ? "null" : layer.EffectType)
			  + ",\"gridX\":5,\"gridY\":5,\"hierarchyFrozen\":false,\"inheritLayerDepth\":false,\"inheritLayerSettings\":false,\"inheritSubLayers\":true,\"inheritVisibility\":true,\"layers\":[],\"pathId\":null,\"properties\":[],\"userdefinedDepth\":false,\"visible\":true,},"
		      );
		  }
		  else if (layer.LayerType == UndertaleRoom.LayerType.Assets)
		  {
		      writer.Write(
			  "    {\"resourceType\":\"GMRAssetLayer\",\"resourceVersion\":\"1.0\",\"name\":\""
			  + layer.LayerName.Content
			  + "\",\"assets\":["
		      );

		      foreach (var tile in layer.AssetsData.LegacyTiles)
		      {
			  throw new InvalidOperationException("No Tile Support yet...");
		      }
		      foreach (var sprite in layer.AssetsData.Sprites)
		      {
			  // TODO GMRSpriteGraphic
			  writer.Write(
			      Environment.NewLine
			      + "        {\"resourceType\":\"GMRSpriteGraphic\",\"resourceVersion\":\"1.0\",\"name\":\""
			      + sprite.Name.Content
			      + "\",\"animationSpeed\":"
			      + sprite.AnimationSpeed.ToString("0.0")
			      + ",\"colour\":"
			      + sprite.Color
			      + ",\"frozen\":false,\"headPosition\":0.0,\"ignore\":false,\"inheritedItemId\":null,\"inheritItemSettings\":false,\"rotation\":"
			      + sprite.Rotation.ToString("0.0")
			      + ",\"scaleX\":"
			      + sprite.ScaleX.ToString("0.0")
			      + ",\"scaleY\":"
			      + sprite.ScaleY.ToString("0.0")
			      + ",\"spriteId\":{\"name\":\""
			      + sprite.Sprite.Name.Content
			      + "\",\"path\":\"sprites/"
			      + sprite.Sprite.Name.Content
			      + "/"
			      + sprite.Sprite.Name.Content
			      + ".yy\",},\"x\":"
			      + sprite.X.ToString("0.0")
			      + ",\"y\":"
			      + sprite.Y.ToString("0.0")
			      + ",},"
			  );
		      }
		      if (layer.AssetsData.Sequences.Count > 0)
		      {
			  writer.WriteLine("");
			  foreach (var sequence in layer.AssetsData.Sequences)
			  {
			      writer.WriteLine(
				  "        {\"resourceType\":\"GMRSequenceGraphic\",\"resourceVersion\":\"1.0\",\"name\":\""
				  + sequence.Name.Content
				  + "\",\"animationSpeed\":1.0,\"colour\":"
				  + sequence.Color
				  + ",\"frozen\":false,\"headPosition\":0.0,\"ignore\":false,\"inheritedItemId\":null,\"inheritItemSettings\":false,\"rotation\":0.0,\"scaleX\":"
				  + sequence.ScaleX.ToString("0.0")
				  + ",\"scaleY\":"
				  + sequence.ScaleY.ToString("0.0")
				  + ",\"sequenceId\":{\"name\":\""
				  + sequence.Sequence.Name.Content
				  + "\",\"path\":\"sequences/"
				  + sequence.Sequence.Name.Content
				  + "/"
				  + sequence.Sequence.Name.Content
				  + ".yy\",},\"x\":"
				  + sequence.X.ToString("0.0")
				  + ",\"y\":"
				  + sequence.Y.ToString("0.0")
				  + ",},"
			      );
			  }
			  writer.Write("      ");
		      }
		      if (layer.AssetsData.NineSlices != null)
		      {
			  foreach (var sprite in layer.AssetsData.NineSlices)
			  {
			      throw new InvalidOperationException("No NineSlice Support yet...");
			  }
		      }
		      foreach (var particle in layer.AssetsData.ParticleSystems)
		      {
			  throw new InvalidOperationException("No ParticleSystem Support yet...");
		      }
		      if (layer.AssetsData.TextItems != null)
		      {
			  foreach (var textitem in layer.AssetsData.TextItems)
			  {
			      throw new InvalidOperationException("No TextItem Support yet...");
			  }
		      }
		      writer.WriteLine(
			  "],\"depth\":"
			  + layer.LayerDepth
			  + ",\"effectEnabled\":"
			  + (layer.EffectEnabled ? "true" : "false")
			  + ",\"effectType\":"
			  + (layer.EffectType is null ? "null" : layer.EffectType)
			  + ",\"gridX\":20,\"gridY\":20,\"hierarchyFrozen\":false,\"inheritLayerDepth\":false,\"inheritLayerSettings\":false,\"inheritSubLayers\":true,\"inheritVisibility\":true,\"layers\":[],\"properties\":[],\"userdefinedDepth\":true,\"visible\":true,},"
		      );
		  }
		  else if (layer.LayerType == UndertaleRoom.LayerType.Instances)
		  {
		      writer.Write(
			  "    {\"resourceType\":\"GMRInstanceLayer\",\"resourceVersion\":\"1.0\",\"name\":\""
			  + layer.LayerName.Content
			  + "\",\"depth\":"
			  + layer.LayerDepth
			  + ",\"effectEnabled\":"
			  + (layer.EffectEnabled ? "true" : "false")
			  + ",\"effectType\":"
			  + (layer.EffectType is null ? "null" : layer.EffectType)
			  + ",\"gridX\":120,\"gridY\":120,\"hierarchyFrozen\":false,\"inheritLayerDepth\":false,\"inheritLayerSettings\":false,\"inheritSubLayers\":true,\"inheritVisibility\":true,\"instances\":["
		      );
		      if (layer.InstancesData.Instances != null)
		      {
			  if (layer.InstancesData.Instances.Count > 0)
			  {
			      foreach (var instance in layer.InstancesData.Instances)
			      {
				  writer.Write(
				      Environment.NewLine
				      + "        {\"resourceType\":\"GMRInstance\",\"resourceVersion\":\"1.0\",\"name\":\"inst_"
				      + instance.InstanceID
				      + "\",\"colour\":"
				      + instance.Color
				      + ",\"frozen\":false,\"hasCreationCode\":"
				      + (instance.CreationCode == null ? "false" : "true")
				      + ",\"ignore\":false,\"imageIndex\":"
				      + instance.ImageIndex
				      + ",\"imageSpeed\":"
				      + instance.ImageSpeed.ToString("0.0")
				      + ",\"inheritCode\":false,\"inheritedItemId\":null,\"inheritItemSettings\":false,\"isDnd\":false,\"objectId\":{\"name\":\""
				      + instance.ObjectDefinition.Name.Content
				      + "\",\"path\":\"objects/"
				      + instance.ObjectDefinition.Name.Content
				      + "/"
				      + instance.ObjectDefinition.Name.Content
				      + ".yy\",},\"properties\":[],\"rotation\":"
				      + instance.Rotation.ToString("0.0")
				      + ",\"scaleX\":"
				      + instance.ScaleX.ToString("0.0")
				      + ",\"scaleY\":"
				      + instance.ScaleY.ToString("0.0")
				      + ",\"x\":"
				      + instance.X.ToString("0.0")
				      + ",\"y\":"
				      + instance.Y.ToString("0.0")
				      + ",},"
				  );
			      }
            writer.Write(Environment.NewLine+"      ");
			  }
		      }
		      writer.WriteLine(
			  "],\"layers\":[],\"properties\":[],\"userdefinedDepth\":false,\"visible\":true,},"
		      );
		  }
		  else if (layer.LayerType == UndertaleRoom.LayerType.Tiles)
		  {
		      // From Match 3 Example
		      writer.WriteLine(
			  "    {\"resourceType\":\"GMRTileLayer\",\"resourceVersion\":\"1.1\",\"name\":\""
			  + layer.LayerName.Content
			  + "\",\"depth\":"
			  + layer.LayerDepth
			  + ",\"effectEnabled\":"
			  + (layer.EffectEnabled ? "true" : "false")
			  + ",\"effectType\":"
			  + (layer.EffectType is null ? "null" : layer.EffectType)
			  + ",\"gridX\":32,\"gridY\":32,\"hierarchyFrozen\":false,\"inheritLayerDepth\":false,\"inheritLayerSettings\":false,\"inheritSubLayers\":true,\"inheritVisibility\":true,\"layers\":[],\"properties\":[],\"tiles\":{\"SerialiseHeight\":30,\"SerialiseWidth\":20,\"TileCompressedData\":["
		      );

		      // Algorithme compression
		      uint i = 0;
		      uint next_line_count = 20;
		      uint lastTile = layer.TilesData.TileData[0][0];
		      foreach (var row in layer.TilesData.TileData)
		      {
			  foreach (var tile in row)
			  {
			      if (tile == lastTile)
			      {
				  i += 1;
			      }
			      else
			      {
				  writer.Write(
				      (i == 1 ? 1 : -i)
				      + ","
				      + (lastTile == 0 ? -2147483648 : lastTile)
				      + ","
				  );
				  lastTile = tile;
				  i = 1;
				  next_line_count -= 2;
				  if (next_line_count <= 0)
				  {
				      writer.WriteLine();
				      next_line_count = 20;
				  }
			      }
			  }
		      }
		      if (i > 1)
		      {
			  writer.Write(
			      (i == 1 ? 1 : -i) + "," + (lastTile == 0 ? -2147483648 : lastTile) + ","
			  );
		      }
		      writer.Write("],");

		      /* writer.WriteLine(); */
		      /* writer.WriteLine(); */
		      /* writer.WriteLine( */
		      /*     "-5,-2147483648,-9,29,-10,-2147483648,1,25,-9,0,1,17,-9,-2147483648,1,25,-9,-2147483648,1,17," */
		      /* ); */
		      /* writer.WriteLine( */
		      /*     "-9,-2147483648,1,25,-9,-2147483648,1,17,-9,-2147483648,1,25,-9,-2147483648,1,17,-9,-2147483648,1,25," */
		      /* ); */
		      /* writer.WriteLine( */
		      /*     "-9,-2147483648,1,17,-9,-2147483648,1,25,-9,-2147483648,1,17,-9,-2147483648,1,25,-9,-2147483648,1,17," */
		      /* ); */
		      /* writer.WriteLine( */
		      /*     "-9,-2147483648,1,25,-9,-2147483648,1,17,-9,-2147483648,1,25,-9,-2147483648,1,17,-9,-2147483648,1,3," */
		      /* ); */
		      /* writer.Write("-9,21,1,2,-385,-2147483648,],"); */
		      /* writer.WriteLine(); */
		      /* writer.WriteLine(); */

		      /* foreach (var row in layer.TilesData.TileData) */
		      /* { */
		      /*     foreach (var tile in row) */
		      /*     { */
		      /*         writer.Write(tile + ","); */
		      /*     } */
		      /*     writer.WriteLine(); */
		      /* } */

		      String tileset_name = layer.TilesData.Background.Name.Content;
		      writer.WriteLine(
			  "\"TileDataFormat\":1,},\"tilesetId\":{\"name\":\""
			  + tileset_name
			  + "\",\"path\":\"tilesets/"
			  + tileset_name
			  + "/"
			  + tileset_name
			  + ".yy\",},\"userdefinedDepth\":false,\"visible\":"
			  + (layer.IsVisible ? "true" : "false")
			  + ",\"x\":"
			  + layer.XOffset
			  + ",\"y\":"
			  + layer.YOffset
			  + ",},"
		      );
		  }
		  else if (layer.LayerType == UndertaleRoom.LayerType.Background)
		  {
		      UndertaleRoom.Layer.LayerBackgroundData background = layer.BackgroundData;

		      writer.WriteLine(
			  "    {\"resourceType\":\"GMRBackgroundLayer\",\"resourceVersion\":\"1.0\",\"name\":\""
			  + layer.LayerName.Content
			  + "\",\"animationFPS\":"
			  + background.AnimationSpeed.ToString("0.0")
			  + ",\"animationSpeedType\":"
			  + (int)background.AnimationSpeedType
			  + ",\"colour\":"
			  + background.Color
			  + ",\"depth\":"
			  + layer.LayerDepth
			  + ",\"effectEnabled\":"
			  + (layer.EffectEnabled ? "true" : "false")
			  + ",\"effectType\":"
			  + (layer.EffectType is null ? "null" : layer.EffectType)
			  + ",\"gridX\":20,\"gridY\":20,\"hierarchyFrozen\":false,\"hspeed\":"
			  + layer.HSpeed.ToString("0.0")
			  + ",\"htiled\":"
			  + (background.TiledHorizontally ? "true" : "false")
			  + ",\"inheritLayerDepth\":false,\"inheritLayerSettings\":false,\"inheritSubLayers\":true,\"inheritVisibility\":true,\"layers\":[],\"properties\":[],\"spriteId\":{\"name\":\""
			  + background.Sprite.Name.Content
			  + "\",\"path\":\"sprites/"
			  + background.Sprite.Name.Content
			  + "/"
			  + background.Sprite.Name.Content
			  + ".yy\",},\"stretch\":"
			  + (background.Stretch ? "true" : "false")
			  + ",\"userdefinedAnimFPS\":false,\"userdefinedDepth\":false,\"visible\":"
			  + (background.Visible ? "true" : "false")
			  + ",\"vspeed\":"
			  + layer.VSpeed.ToString("0.0")
			  + ",\"vtiled\":"
			  + (background.TiledVertically ? "true" : "false")
			  + ",\"x\":"
			  + layer.XOffset
			  + ",\"y\":"
			  + layer.YOffset
			  + ",},"
		      );
		  }
		  else if (layer.LayerType == UndertaleRoom.LayerType.Effect)
		  {
		      WriteLayerHeader(writer, "GMREffectLayer", layer);
		      writer.Write("],\"properties\":[");
		      WriteLayerProperties(writer, layer);
		      writer.WriteLine(
			  "  ],\"userdefinedDepth\":false,\"visible\":"
			  + (layer.IsVisible ? "true" : "false")
			  + ",},"
		      );
		  }
		  else
		  {
		      writer.WriteLine("Asset Layer (" + layer.LayerType + ") Unknown");
		      throw new InvalidOperationException(
			  "Asset Layer (" + layer.LayerType + ") Unknown"
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
