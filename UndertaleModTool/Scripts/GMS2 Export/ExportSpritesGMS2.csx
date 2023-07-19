// Modified with the help of Agentalex9
using System.Text;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UndertaleModLib.Util;

EnsureDataLoaded();

// Pour avoir un "." au lieu d'une "," dans les conversion en décimal
System.Globalization.CultureInfo customCulture = (System.Globalization.CultureInfo) System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
customCulture.NumberFormat.NumberDecimalSeparator = ".";

System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;
//
// bool padded = (!ScriptQuestion("Export all sprites unpadded?"));

bool padded = true;


string texFolder = GetFolder(FilePath) + "sprites" + Path.DirectorySeparatorChar;
TextureWorker worker = new TextureWorker();
if (Directory.Exists(texFolder))
{
  Directory.Delete(texFolder, true);
}
Directory.CreateDirectory(texFolder);

SetProgressBar(null, "Sprites", 0, Data.Sprites.Count);
StartProgressBarUpdater();

await DumpSprites();
worker.Cleanup();

await StopProgressBarUpdater();
HideProgressBar();
ScriptMessage("Export Complete.\n\nLocation: " + texFolder);


string GetFolder(string path)
{
  return Path.GetDirectoryName(path) + Path.DirectorySeparatorChar;
}

async Task DumpSprites()
{
  await Task.Run(() => Parallel.ForEach(Data.Sprites, DumpSprite));
}

void DumpSprite(UndertaleSprite sprite)
{
  Directory.CreateDirectory(texFolder + sprite.Name.Content);
  using (StreamWriter writer = new StreamWriter(texFolder + sprite.Name.Content + "\\" + sprite.Name.Content + ".yy"))
  {

    // BEGIN : Extraction Images
    string layer_directory = texFolder + sprite.Name.Content + "\\" + "layers";
    Directory.CreateDirectory(layer_directory);
    for (int i = 0; i < sprite.Textures.Count; i++)
    {
      if (sprite.Textures[i]?.Texture != null)
      {
        // Extraction de l'image à la base du répertoire
        worker.ExportAsPNG(sprite.Textures[i].Texture,texFolder + sprite.Name.Content + "\\" + sprite.Name.Content + "_"+ i + ".png", null, padded); // Include padding to make sprites look neat!

        // Création du répertoire dans "layers"
        Directory.CreateDirectory(texFolder + sprite.Name.Content + "\\" + "layers" + "\\" + sprite.Name.Content + "_" + i);

        // Extraction de l'image "layer"
        worker.ExportAsPNG(sprite.Textures[i].Texture,texFolder + sprite.Name.Content + "\\" + "layers" + "\\" + sprite.Name.Content + "_"+ i + "\\" + sprite.Name.Content + "_" + "layer" + ".png", null, padded); // Include padding to make sprites look neat!
      }
    }
    // END : Extraction Images

    writer.WriteLine("{");
    writer.WriteLine("  \"resourceType\": \"GMSprite\",");
    writer.WriteLine("  \"resourceVersion\": \"1.0\",");
    writer.WriteLine("  \"name\": \""+ sprite.Name.Content +"\",");
    writer.WriteLine("  \"bbox_bottom\": "+ sprite.MarginBottom +",");
    writer.WriteLine("  \"bbox_left\": "+ sprite.MarginLeft +",");
    writer.WriteLine("  \"bbox_right\": "+ sprite.MarginRight +",");
    writer.WriteLine("  \"bbox_top\": "+ sprite.MarginTop +",");
    writer.WriteLine("  \"bboxMode\": "+ sprite.BBoxMode +",");
    writer.WriteLine("  \"collisionKind\": 1,");
    writer.WriteLine("  \"collisionTolerance\": 0,");
    writer.WriteLine("  \"DynamicTexturePage\": false,");
    writer.WriteLine("  \"edgeFiltering\": false,");
    writer.WriteLine("  \"For3D\": false,");
    // BEGIN : frames
    writer.WriteLine("  \"frames\": [");
    for (int i = 0; i < sprite.Textures.Count; i++)
    {
        if (sprite.Textures[i]?.Texture != null)
        {
            writer.WriteLine("    {\"resourceType\":\"GMSpriteFrame\",\"resourceVersion\":\"1.1\",\"name\":\"" + sprite.Name.Content + "_" + i + "\",},");
        }
    }
    writer.WriteLine("  ],");
    // END : frames
    writer.WriteLine("  \"gridX\": 0,");
    writer.WriteLine("  \"gridY\": 0,");
    writer.WriteLine("  \"height\": "+ sprite.Height +",");
    writer.WriteLine("  \"HTile\": false,");
    // BEGIN : layers
    writer.WriteLine("  \"layers\": [");
    writer.WriteLine("    {\"resourceType\":\"GMImageLayer\",\"resourceVersion\":\"1.0\",\"name\":\"" + sprite.Name.Content + "_" + "layer" +"\",\"blendMode\":0,\"displayName\":\"default\",\"isLocked\":false,\"opacity\":100.0,\"visible\":true,},");
    writer.WriteLine("  ],");
    // END : layers
    writer.WriteLine("  \"nineSlice\": null,");
    writer.WriteLine("  \"origin\": 0,");
    writer.WriteLine("  \"parent\": {");
    writer.WriteLine("    \"name\": \"Sprites\",");
    writer.WriteLine("    \"path\": \"folders/Sprites.yy\",");
    writer.WriteLine("  },");
    writer.WriteLine("  \"preMultiplyAlpha\": false,");
    writer.WriteLine("  \"sequence\": {");
    writer.WriteLine("    \"resourceType\": \"GMSequence\",");
    writer.WriteLine("    \"resourceVersion\": \"1.4\",");
    writer.WriteLine("    \"name\": \""+ sprite.Name.Content +"\",");
    writer.WriteLine("    \"autoRecord\": true,");
    writer.WriteLine("    \"backdropHeight\": 768,");
    writer.WriteLine("    \"backdropImageOpacity\": 0.5,");
    writer.WriteLine("    \"backdropImagePath\": \"\",");
    writer.WriteLine("    \"backdropWidth\": 1366,");
    writer.WriteLine("    \"backdropXOffset\": 0.0,");
    writer.WriteLine("    \"backdropYOffset\": 0.0,");
    writer.WriteLine("    \"events\": {\"resourceType\":\"KeyframeStore<MessageEventKeyframe>\",\"resourceVersion\":\"1.0\",\"Keyframes\":[],},");
    writer.WriteLine("    \"eventStubScript\": null,");
    writer.WriteLine("    \"eventToFunction\": {},");
    writer.WriteLine("    \"length\": "+ sprite.Textures.Count.ToString("0.0") +",");
    writer.WriteLine("    \"lockOrigin\": false,");
    writer.WriteLine("    \"moments\": {\"resourceType\":\"KeyframeStore<MomentsEventKeyframe>\",\"resourceVersion\":\"1.0\",\"Keyframes\":[],},");
    writer.WriteLine("    \"playback\": 1,");
    writer.WriteLine("    \"playbackSpeed\": "+sprite.GMS2PlaybackSpeed.ToString("0.0")+",");
    writer.WriteLine("    \"playbackSpeedType\": "+(ushort) sprite.GMS2PlaybackSpeedType+",");
    writer.WriteLine("    \"showBackdrop\": true,");
    writer.WriteLine("    \"showBackdropImage\": false,");
    writer.WriteLine("    \"timeUnits\": 1,");
    // BEGIN : tracks
    writer.WriteLine("    \"tracks\": [");
    writer.WriteLine("      {\"resourceType\":\"GMSpriteFramesTrack\",\"resourceVersion\":\"1.0\",\"name\":\"frames\",\"builtinName\":0,\"events\":[],\"inheritsTrackColour\":true,\"interpolation\":1,\"isCreationTrack\":false,\"keyframes\":{\"resourceType\":\"KeyframeStore<SpriteFrameKeyframe>\",\"resourceVersion\":\"1.0\",\"Keyframes\":[");
    for (int i = 0; i < sprite.Textures.Count; i++)
    {
        if (sprite.Textures[i]?.Texture != null)
        {
            writer.WriteLine("            {\"resourceType\":\"Keyframe<SpriteFrameKeyframe>\",\"resourceVersion\":\"1.0\",\"Channels\":{\"0\":{\"resourceType\":\"SpriteFrameKeyframe\",\"resourceVersion\":\"1.0\",\"Id\":{\"name\":\""+ sprite.Name.Content + "_" + i +"\",\"path\":\"sprites/"+ sprite.Name.Content +"/"+ sprite.Name.Content +".yy\",},},},\"Disabled\":false,\"IsCreationKey\":false,\"Key\":"+i.ToString("0.0")+",\"Length\":1.0,\"Stretch\":false,},");
        }
    }
    writer.WriteLine("      ],},\"modifiers\":[],\"spriteId\":null,\"trackColour\":0,\"tracks\":[],\"traits\":0,},");
    writer.WriteLine("    ],");
    // END : tracks
    writer.WriteLine("    \"visibleRange\": null,");
    writer.WriteLine("    \"volume\": 1.0,");
    writer.WriteLine("    \"xorigin\": "+ sprite.OriginX +",");
    writer.WriteLine("    \"yorigin\": "+ sprite.OriginY +",");
    writer.WriteLine("  },");
    writer.WriteLine("  \"swatchColours\": null,");
    writer.WriteLine("  \"swfPrecision\": 2.525,");
    writer.WriteLine("  \"textureGroupId\": {");
    writer.WriteLine("    \"name\": \"Default\",");
    writer.WriteLine("    \"path\": \"texturegroups/Default\",");
    writer.WriteLine("  },");
    writer.WriteLine("  \"type\": 0,");
    writer.WriteLine("  \"VTile\": false,");
    writer.WriteLine("  \"width\": "+ sprite.Width +",");
    writer.Write("}");

  }

  IncrementProgressParallel();
}