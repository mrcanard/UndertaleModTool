// Modified with the help of Agentalex9
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UndertaleModLib.Util;

EnsureDataLoaded();

if (Data.IsGameMaker2())
{
    // Pour avoir un "." au lieu d'une "," dans les conversion en décimal
    System.Globalization.CultureInfo customCulture = (System.Globalization.CultureInfo)
        System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
    customCulture.NumberFormat.NumberDecimalSeparator = ".";

    System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;

    String EscapeLine = "\r\n";

    bool padded = true;

    string texFolder = GetFolder(FilePath) + "sprites" + Path.DirectorySeparatorChar;
    TextureWorker worker = new TextureWorker();
    if (Directory.Exists(texFolder))
    {
        Directory.Delete(texFolder, true);
    }
    Directory.CreateDirectory(texFolder);

    // Sprites
    SetProgressBar(null, "Sprites", 0, Data.Sprites.Count);
    StartProgressBarUpdater();

    await DumpSprites();

    await StopProgressBarUpdater();
    HideProgressBar();

    // Backgrounds
    SetProgressBar(null, "Backgrounds", 0, Data.Backgrounds.Count);
    StartProgressBarUpdater();

    await DumpBackgrounds();
    // worker.Cleanup();

    await StopProgressBarUpdater();
    HideProgressBar();

    // Export asset
    using (StreamWriter writer = new StreamWriter(texFolder + "asset_order.txt"))
    {
        for (int i = 0; i < Data.Sprites.Count; i++)
        {
            UndertaleSprite sprite = Data.Sprites[i];
            writer.Write(
                "    {\"id\":{\"name\":\""
                    + sprite.Name.Content
                    + "\",\"path\":\"sprites/"
                    + sprite.Name.Content
                    + "/"
                    + sprite.Name.Content
                    + ".yy\",},},"
                    + EscapeLine
            );
        }
        for (int i = 0; i < Data.Backgrounds.Count; i++)
        {
            UndertaleBackground background = Data.Backgrounds[i];
            writer.Write(
                "    {\"id\":{\"name\":\""
                    + background.Name.Content
                    + "\",\"path\":\"sprites/"
                    + background.Name.Content
                    + "/"
                    + background.Name.Content
                    + ".yy\",},},"
                    + EscapeLine
            );
        }
    }

    ScriptMessage("Export Complete.\n\nLocation: " + texFolder);

    // --------------------------------------------------------------------- Functions ---------------------------------------------------------------------------
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
        using (
            StreamWriter writer = new StreamWriter(
                texFolder
                    + sprite.Name.Content
                    + Path.DirectorySeparatorChar
                    + sprite.Name.Content
                    + ".yy"
            )
        )
        {
            Console.WriteLine(sprite.Name.Content);
            // BEGIN : Extraction Images
            string layer_directory =
                texFolder + sprite.Name.Content + Path.DirectorySeparatorChar + "layers";
            Directory.CreateDirectory(layer_directory);
            for (int i = 0; i < sprite.Textures.Count; i++)
            {
                if (sprite.Textures[i]?.Texture != null)
                {
                    // Extraction de l'image à la base du répertoire
                    worker.ExportAsPNG(
                        sprite.Textures[i].Texture,
                        texFolder
                            + sprite.Name.Content
                            + Path.DirectorySeparatorChar
                            + sprite.Name.Content
                            + "_"
                            + i
                            + ".png",
                        null,
                        padded
                    ); // Include padding to make sprites look neat!

                    // Création du répertoire dans "layers"
                    Directory.CreateDirectory(
                        texFolder
                            + sprite.Name.Content
                            + Path.DirectorySeparatorChar
                            + "layers"
                            + Path.DirectorySeparatorChar
                            + sprite.Name.Content
                            + "_"
                            + i
                    );

                    // Extraction de l'image "layer"
                    worker.ExportAsPNG(
                        sprite.Textures[i].Texture,
                        texFolder
                            + sprite.Name.Content
                            + Path.DirectorySeparatorChar
                            + "layers"
                            + Path.DirectorySeparatorChar
                            + sprite.Name.Content
                            + "_"
                            + i
                            + Path.DirectorySeparatorChar
                            + sprite.Name.Content
                            + "_"
                            + "layer"
                            + ".png",
                        null,
                        padded
                    ); // Include padding to make sprites look neat!
                }
            }
            // END : Extraction Images

            writer.Write("{" + EscapeLine);
            writer.Write("  \"resourceType\": \"GMSprite\"," + EscapeLine);
            writer.Write("  \"resourceVersion\": \"1.0\"," + EscapeLine);
            writer.Write("  \"name\": \"" + sprite.Name.Content + "\"," + EscapeLine);
            writer.Write("  \"bbox_bottom\": " + sprite.MarginBottom + "," + EscapeLine);
            writer.Write("  \"bbox_left\": " + sprite.MarginLeft + "," + EscapeLine);
            writer.Write("  \"bbox_right\": " + sprite.MarginRight + "," + EscapeLine);
            writer.Write("  \"bbox_top\": " + sprite.MarginTop + "," + EscapeLine);
            writer.Write("  \"bboxMode\": " + sprite.BBoxMode + "," + EscapeLine);
            if (sprite.SepMasks == UndertaleSprite.SepMaskType.Precise)
            {
                writer.Write("  \"collisionKind\": 4," + EscapeLine);
            }
            else if (sprite.SepMasks == UndertaleSprite.SepMaskType.AxisAlignedRect)
            {
                writer.Write("  \"collisionKind\": 0," + EscapeLine);
            }
            else
            {
                writer.Write("  \"collisionKind\": 1," + EscapeLine);
            }
            writer.Write("  \"collisionTolerance\": 0," + EscapeLine);
            writer.Write("  \"DynamicTexturePage\": false," + EscapeLine);
            writer.Write("  \"edgeFiltering\": false," + EscapeLine);
            writer.Write("  \"For3D\": false," + EscapeLine);
            // BEGIN : frames
            writer.Write("  \"frames\": [" + EscapeLine);
            for (int i = 0; i < sprite.Textures.Count; i++)
            {
                if (sprite.Textures[i]?.Texture != null)
                {
                    writer.Write(
                        "    {\"resourceType\":\"GMSpriteFrame\",\"resourceVersion\":\"1.1\",\"name\":\""
                            + sprite.Name.Content
                            + "_"
                            + i
                            + "\",},"
                            + EscapeLine
                    );
                }
            }
            writer.Write("  ]," + EscapeLine);
            // END : frames
            writer.Write("  \"gridX\": 0," + EscapeLine);
            writer.Write("  \"gridY\": 0," + EscapeLine);
            writer.Write("  \"height\": " + sprite.Height + "," + EscapeLine);
            writer.Write("  \"HTile\": false," + EscapeLine);
            // BEGIN : layers
            writer.Write("  \"layers\": [" + EscapeLine);
            writer.Write(
                "    {\"resourceType\":\"GMImageLayer\",\"resourceVersion\":\"1.0\",\"name\":\""
                    + sprite.Name.Content
                    + "_"
                    + "layer"
                    + "\",\"blendMode\":0,\"displayName\":\"default\",\"isLocked\":false,\"opacity\":100.0,\"visible\":true,},"
                    + EscapeLine
            );
            writer.Write("  ]," + EscapeLine);
            // END : layers
            writer.Write("  \"nineSlice\": null," + EscapeLine);
            writer.Write("  \"origin\": 0," + EscapeLine);
            writer.Write("  \"parent\": {" + EscapeLine);
            writer.Write("    \"name\": \"Sprites\"," + EscapeLine);
            writer.Write("    \"path\": \"folders/Sprites.yy\"," + EscapeLine);
            writer.Write("  }," + EscapeLine);
            writer.Write("  \"preMultiplyAlpha\": false," + EscapeLine);
            writer.Write("  \"sequence\": {" + EscapeLine);
            writer.Write("    \"resourceType\": \"GMSequence\"," + EscapeLine);
            writer.Write("    \"resourceVersion\": \"1.4\"," + EscapeLine);
            writer.Write("    \"name\": \"" + sprite.Name.Content + "\"," + EscapeLine);
            writer.Write("    \"autoRecord\": true," + EscapeLine);
            writer.Write("    \"backdropHeight\": 1080," + EscapeLine);
            writer.Write("    \"backdropImageOpacity\": 0.5," + EscapeLine);
            writer.Write("    \"backdropImagePath\": \"\"," + EscapeLine);
            writer.Write("    \"backdropWidth\": 1920," + EscapeLine);
            writer.Write("    \"backdropXOffset\": 0.0," + EscapeLine);
            writer.Write("    \"backdropYOffset\": 0.0," + EscapeLine);
            writer.Write(
                "    \"events\": {\"resourceType\":\"KeyframeStore<MessageEventKeyframe>\",\"resourceVersion\":\"1.0\",\"Keyframes\":[],},"
                    + EscapeLine
            );
            writer.Write("    \"eventStubScript\": null," + EscapeLine);
            writer.Write("    \"eventToFunction\": {}," + EscapeLine);
            writer.Write(
                "    \"length\": " + sprite.Textures.Count.ToString("0.0") + "," + EscapeLine
            );
            writer.Write("    \"lockOrigin\": false," + EscapeLine);
            writer.Write(
                "    \"moments\": {\"resourceType\":\"KeyframeStore<MomentsEventKeyframe>\",\"resourceVersion\":\"1.0\",\"Keyframes\":[],},"
                    + EscapeLine
            );
            writer.Write("    \"playback\": 1," + EscapeLine);
            writer.Write(
                "    \"playbackSpeed\": "
                    + sprite.GMS2PlaybackSpeed.ToString("0.0")
                    + ","
                    + EscapeLine
            );
            writer.Write(
                "    \"playbackSpeedType\": "
                    + (ushort)sprite.GMS2PlaybackSpeedType
                    + ","
                    + EscapeLine
            );
            writer.Write("    \"showBackdrop\": true," + EscapeLine);
            writer.Write("    \"showBackdropImage\": false," + EscapeLine);
            writer.Write("    \"timeUnits\": 1," + EscapeLine);
            // BEGIN : tracks
            writer.Write("    \"tracks\": [" + EscapeLine);
            writer.Write(
                "      {\"resourceType\":\"GMSpriteFramesTrack\",\"resourceVersion\":\"1.0\",\"name\":\"frames\",\"builtinName\":0,\"events\":[],\"inheritsTrackColour\":true,\"interpolation\":1,\"isCreationTrack\":false,\"keyframes\":{\"resourceType\":\"KeyframeStore<SpriteFrameKeyframe>\",\"resourceVersion\":\"1.0\",\"Keyframes\":["
                    + EscapeLine
            );
            for (int i = 0; i < sprite.Textures.Count; i++)
            {
                if (sprite.Textures[i]?.Texture != null)
                {
                    writer.Write(
                        "            {\"resourceType\":\"Keyframe<SpriteFrameKeyframe>\",\"resourceVersion\":\"1.0\",\"Channels\":{\"0\":{\"resourceType\":\"SpriteFrameKeyframe\",\"resourceVersion\":\"1.0\",\"Id\":{\"name\":\""
                            + sprite.Name.Content
                            + "_"
                            + i
                            + "\",\"path\":\"sprites/"
                            + sprite.Name.Content
                            + "/"
                            + sprite.Name.Content
                            + ".yy\",},},},\"Disabled\":false,\"IsCreationKey\":false,\"Key\":"
                            + i.ToString("0.0")
                            + ",\"Length\":1.0,\"Stretch\":false,},"
                            + EscapeLine
                    );
                }
            }
            writer.Write(
                "      ],},\"modifiers\":[],\"spriteId\":null,\"trackColour\":0,\"tracks\":[],\"traits\":0,},"
                    + EscapeLine
            );
            writer.Write("    ]," + EscapeLine);
            // END : tracks
            writer.Write("    \"visibleRange\": null," + EscapeLine);
            writer.Write("    \"volume\": 1.0," + EscapeLine);
            writer.Write("    \"xorigin\": " + sprite.OriginX + "," + EscapeLine);
            writer.Write("    \"yorigin\": " + sprite.OriginY + "," + EscapeLine);
            writer.Write("  }," + EscapeLine);
            writer.Write("  \"swatchColours\": null," + EscapeLine);
            writer.Write("  \"swfPrecision\": 2.525," + EscapeLine);
            writer.Write("  \"textureGroupId\": {" + EscapeLine);
            writer.Write("    \"name\": \"Default\"," + EscapeLine);
            writer.Write("    \"path\": \"texturegroups/Default\"," + EscapeLine);
            writer.Write("  }," + EscapeLine);
            writer.Write("  \"type\": 0," + EscapeLine);
            writer.Write("  \"VTile\": false," + EscapeLine);
            writer.Write("  \"width\": " + sprite.Width + "," + EscapeLine);
            writer.Write("}");
        }

        IncrementProgressParallel();
    }

    // Backgrounds
    async Task DumpBackgrounds()
    {
        await Task.Run(() => Parallel.ForEach(Data.Backgrounds, DumpBackground));
    }

    void DumpBackground(UndertaleBackground background)
    {
        if (background.Texture == null)
        {
            return;
        }

        Directory.CreateDirectory(texFolder + background.Name.Content);
        using (
            StreamWriter writer = new StreamWriter(
                texFolder
                    + background.Name.Content
                    + Path.DirectorySeparatorChar
                    + background.Name.Content
                    + ".yy"
            )
        )
        {
            Console.WriteLine(background.Name.Content);

            // BEGIN : Extraction Images
            string layer_directory =
                texFolder + background.Name.Content + Path.DirectorySeparatorChar + "layers";
            Directory.CreateDirectory(layer_directory);
            // for (int i = 0; i < background.Textures.Count; i++)
            // {
            if (background.Texture != null)
            {
                // Extraction de l'image à la base du répertoire
                worker.ExportAsPNG(
                    background.Texture,
                    texFolder
                        + background.Name.Content
                        + Path.DirectorySeparatorChar
                        + background.Name.Content
                        + ".png",
                    null,
                    padded
                ); // Include padding to make sprites look neat!

                // Création du répertoire dans "layers"
                Directory.CreateDirectory(
                    texFolder
                        + background.Name.Content
                        + Path.DirectorySeparatorChar
                        + "layers"
                        + Path.DirectorySeparatorChar
                        + background.Name.Content
                );

                // Extraction de l'image "layer"
                worker.ExportAsPNG(
                    background.Texture,
                    texFolder
                        + background.Name.Content
                        + Path.DirectorySeparatorChar
                        + "layers"
                        + Path.DirectorySeparatorChar
                        + background.Name.Content
                        + Path.DirectorySeparatorChar
                        + background.Name.Content
                        + ".png",
                    null,
                    padded
                ); // Include padding to make sprites look neat!
            }
            // }
            // END : Extraction Images

            writer.Write("{" + EscapeLine);
            writer.Write("  \"resourceType\": \"GMSprite\"," + EscapeLine);
            writer.Write("  \"resourceVersion\": \"1.0\"," + EscapeLine);
            writer.Write("  \"name\": \"" + background.Name.Content + "\"," + EscapeLine);
            writer.Write("  \"bbox_bottom\": 63," + EscapeLine);
            writer.Write("  \"bbox_left\": 0," + EscapeLine);
            writer.Write("  \"bbox_right\": 63," + EscapeLine);
            writer.Write("  \"bbox_top\": 0," + EscapeLine);
            writer.Write("  \"bboxMode\": 0," + EscapeLine);
            writer.Write("  \"collisionKind\": 1," + EscapeLine);
            writer.Write("  \"collisionTolerance\": 0," + EscapeLine);
            writer.Write("  \"DynamicTexturePage\": false," + EscapeLine);
            writer.Write("  \"edgeFiltering\": false," + EscapeLine);
            writer.Write("  \"For3D\": false," + EscapeLine);
            writer.Write("  \"frames\": [" + EscapeLine);
            writer.Write(
                "    {\"resourceType\":\"GMSpriteFrame\",\"resourceVersion\":\"1.1\",\"name\":\""
                    + background.Name.Content
                    + "\",},"
                    + EscapeLine
            );
            writer.Write("  ]," + EscapeLine);
            writer.Write("  \"gridX\": 0," + EscapeLine);
            writer.Write("  \"gridY\": 0," + EscapeLine);
            writer.Write("  \"height\": " + background.Texture.TargetHeight + "," + EscapeLine);
            writer.Write("  \"HTile\": false," + EscapeLine);
            writer.Write("  \"layers\": [" + EscapeLine);
            writer.Write(
                "    {\"resourceType\":\"GMImageLayer\",\"resourceVersion\":\"1.0\",\"name\":\""
                    + background.Name.Content
                    + "\",\"blendMode\":0,\"displayName\":\"default\",\"isLocked\":false,\"opacity\":100.0,\"visible\":true,},"
                    + EscapeLine
            );
            writer.Write("  ]," + EscapeLine);
            writer.Write("  \"nineSlice\": null," + EscapeLine);
            writer.Write("  \"origin\": 0," + EscapeLine);
            writer.Write("  \"parent\": {" + EscapeLine);
            writer.Write("    \"name\": \"tilesets\"," + EscapeLine);
            writer.Write("    \"path\": \"folders/Sprites/tilesets.yy\"," + EscapeLine);
            writer.Write("  }," + EscapeLine);
            writer.Write("  \"preMultiplyAlpha\": false," + EscapeLine);
            writer.Write("  \"sequence\": {" + EscapeLine);
            writer.Write("    \"resourceType\": \"GMSequence\"," + EscapeLine);
            writer.Write("    \"resourceVersion\": \"1.4\"," + EscapeLine);
            writer.Write("    \"name\": \"\"," + EscapeLine);
            writer.Write("    \"autoRecord\": true," + EscapeLine);
            writer.Write("    \"backdropHeight\": 1080," + EscapeLine);
            writer.Write("    \"backdropImageOpacity\": 0.5," + EscapeLine);
            writer.Write("    \"backdropImagePath\": \"\"," + EscapeLine);
            writer.Write("    \"backdropWidth\": 1920," + EscapeLine);
            writer.Write("    \"backdropXOffset\": 0.0," + EscapeLine);
            writer.Write("    \"backdropYOffset\": 0.0," + EscapeLine);
            writer.Write(
                "    \"events\": {\"resourceType\":\"KeyframeStore<MessageEventKeyframe>\",\"resourceVersion\":\"1.0\",\"Keyframes\":[],},"
                    + EscapeLine
            );
            writer.Write("    \"eventStubScript\": null," + EscapeLine);
            writer.Write("    \"eventToFunction\": {}," + EscapeLine);
            writer.Write("    \"length\": 1.0," + EscapeLine);
            writer.Write("    \"lockOrigin\": false," + EscapeLine);
            writer.Write(
                "    \"moments\": {\"resourceType\":\"KeyframeStore<MomentsEventKeyframe>\",\"resourceVersion\":\"1.0\",\"Keyframes\":[],},"
                    + EscapeLine
            );
            writer.Write("    \"playback\": 1," + EscapeLine);
            writer.Write("    \"playbackSpeed\": 1.0," + EscapeLine);
            writer.Write("    \"playbackSpeedType\": 1," + EscapeLine);
            writer.Write("    \"showBackdrop\": true," + EscapeLine);
            writer.Write("    \"showBackdropImage\": false," + EscapeLine);
            writer.Write("    \"timeUnits\": 1," + EscapeLine);
            writer.Write("    \"tracks\": [" + EscapeLine);
            writer.Write(
                "      {\"resourceType\":\"GMSpriteFramesTrack\",\"resourceVersion\":\"1.0\",\"name\":\"frames\",\"builtinName\":0,\"events\":[],\"inheritsTrackColour\":true,\"interpolation\":1,\"isCreationTrack\":false,\"keyframes\":{\"resourceType\":\"KeyframeStore<SpriteFrameKeyframe>\",\"resourceVersion\":\"1.0\",\"Keyframes\":["
                    + EscapeLine
            );
            writer.Write(
                "            {\"resourceType\":\"Keyframe<SpriteFrameKeyframe>\",\"resourceVersion\":\"1.0\",\"Channels\":{\"0\":{\"resourceType\":\"SpriteFrameKeyframe\",\"resourceVersion\":\"1.0\",\"Id\":{\"name\":\""
                    + background.Name.Content
                    + "\",\"path\":\"sprites/"
                    + background.Name.Content
                    + "/"
                    + background.Name.Content
                    + ".yy\",},},},\"Disabled\":false,\"IsCreationKey\":false,\"Key\":0.0,\"Length\":1.0,\"Stretch\":false,},"
                    + EscapeLine
            );
            writer.Write(
                "          ],},\"modifiers\":[],\"spriteId\":null,\"trackColour\":0,\"tracks\":[],\"traits\":0,},"
                    + EscapeLine
            );
            writer.Write("    ]," + EscapeLine);
            writer.Write("    \"visibleRange\": null," + EscapeLine);
            writer.Write("    \"volume\": 1.0," + EscapeLine);
            writer.Write("    \"xorigin\": 0," + EscapeLine);
            writer.Write("    \"yorigin\": 0," + EscapeLine);
            writer.Write("  }," + EscapeLine);
            writer.Write("  \"swatchColours\": null," + EscapeLine);
            writer.Write("  \"swfPrecision\": 2.525," + EscapeLine);
            writer.Write("  \"textureGroupId\": {" + EscapeLine);
            writer.Write("    \"name\": \"Default\"," + EscapeLine);
            writer.Write("    \"path\": \"texturegroups/Default\"," + EscapeLine);
            writer.Write("  }," + EscapeLine);
            writer.Write("  \"type\": 0," + EscapeLine);
            writer.Write("  \"VTile\": false," + EscapeLine);
            writer.Write("  \"width\": " + background.Texture.TargetWidth + "," + EscapeLine);
            writer.Write("}");
        }

        IncrementProgressParallel();
    }
}
else
{
    // Sprites
    // Pour avoir un "." au lieu d'une "," dans les conversion en décimal
    System.Globalization.CultureInfo customCulture = (System.Globalization.CultureInfo)
        System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
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

    // layer
    string layer_directory = texFolder + "images";
    Directory.CreateDirectory(layer_directory);

    // Sprites
    SetProgressBar(null, "Sprites", 0, Data.Sprites.Count);
    StartProgressBarUpdater();

    await DumpSprites();

    await StopProgressBarUpdater();
    HideProgressBar();

    // // Backgrounds
    // SetProgressBar(null, "Backgrounds", 0, Data.Backgrounds.Count);
    // StartProgressBarUpdater();

    // await DumpBackgrounds();
    // worker.Cleanup();

    // await StopProgressBarUpdater();
    // HideProgressBar();

    // Export asset
    using (StreamWriter writer = new StreamWriter(texFolder + "asset_order.txt"))
    {
        writer.WriteLine("  <sprites name=\"sprites\">");
        for (int i = 0; i < Data.Sprites.Count; i++)
        {
            UndertaleSprite sprite = Data.Sprites[i];
            writer.WriteLine("    <sprite>sprites\\" + sprite.Name.Content + "</sprite>");
        }
        writer.WriteLine("  </sprites>");
    }

    ScriptMessage("Export Complete.\n\nLocation: " + texFolder);

    // --------------------------------------------------------------------- Functions ---------------------------------------------------------------------------
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
        // BEGIN : Extraction Images
        for (int i = 0; i < sprite.Textures.Count; i++)
        {
            if (sprite.Textures[i]?.Texture != null)
            {
                worker.ExportAsPNG(
                    sprite.Textures[i].Texture,
                    texFolder
                        + "images"
                        + Path.DirectorySeparatorChar
                        + sprite.Name.Content
                        + "_"
                        + i
                        + ".png",
                    null,
                    padded
                ); // Include padding to make sprites look neat!
            }
        }
        // END : Extraction Images

        // Directory.CreateDirectory(texFolder + sprite.Name.Content);
        using (
            StreamWriter writer = new StreamWriter(texFolder + sprite.Name.Content + ".sprite.gmx")
        )
        {
            writer.WriteLine(
                "<!--This Document is generated by GameMaker, if you edit it by hand then you do so at your own risk!-->"
            );
            writer.WriteLine("<sprite>");
            writer.WriteLine("  <type>0</type>"); // TODO type ??
            writer.WriteLine("  <xorig>" + sprite.OriginX + "</xorig>");
            writer.WriteLine("  <yorigin>" + sprite.OriginY + "</yorigin>");
            if (sprite.SepMasks == UndertaleSprite.SepMaskType.Precise)
            { // Precise Mask
                writer.WriteLine("  <colkind>0</colkind>");
            }
            else if (sprite.SepMasks == UndertaleSprite.SepMaskType.AxisAlignedRect)
            { // Rectangle Mask
                writer.WriteLine("  <colkind>1</colkind>");
                // } else if(sprite.SepMasks == UndertaleSprite.SepMaskType.Ellipse) { // Ellipse Mask
                //     writer.WriteLine("  <colkind>2</colkind>");
            }
            else if (sprite.SepMasks == UndertaleSprite.SepMaskType.RotatedRect)
            { // Diamond Mask
                writer.WriteLine("  <colkind>3</colkind>");
            }
            else
            {
                writer.WriteLine("  <colkind>4</colkind>");
            }
            writer.WriteLine("  <coltolerance>0</coltolerance>");
            if (sprite.SepMasks == UndertaleSprite.SepMaskType.Precise)
            { // Si Precise alors -1 sinon 0
                writer.WriteLine("  <sepmasks>-1</sepmasks>");
            }
            else
            {
                writer.WriteLine("  <sepmasks>0</sepmasks>");
            }
            writer.WriteLine("  <bboxmode>" + sprite.BBoxMode + "</bboxmode>");
            writer.WriteLine("  <bbox_left>" + sprite.MarginLeft + "</bbox_left>");
            writer.WriteLine("  <bbox_right>" + sprite.MarginRight + "</bbox_right>");
            writer.WriteLine("  <bbox_top>" + sprite.MarginTop + "</bbox_top>");
            writer.WriteLine("  <bbox_bottom>" + sprite.MarginBottom + "</bbox_bottom>");
            writer.WriteLine("  <HTile>0</HTile>");
            writer.WriteLine("  <VTile>0</VTile>");
            writer.WriteLine("  <TextureGroups>");
            writer.WriteLine("    <TextureGroup0>0</TextureGroup0>");
            writer.WriteLine("  </TextureGroups>");
            writer.WriteLine("  <For3D>0</For3D>");
            writer.WriteLine("  <width>" + sprite.Width + "</width>");
            writer.WriteLine("  <height>" + sprite.Height + "</height>");
            writer.WriteLine("  <frames>");
            for (int i = 0; i < sprite.Textures.Count; i++)
            {
                if (sprite.Textures[i]?.Texture != null)
                {
                    writer.WriteLine(
                        "    <frame index=\""
                            + i
                            + "\">images\\"
                            + sprite.Name.Content
                            + "_"
                            + i
                            + ".png</frame>"
                    );
                }
            }
            writer.WriteLine("  </frames>");
            writer.WriteLine("</sprite>");
        }

        IncrementProgressParallel();
    }

    // Background
    // bool padded = (!ScriptQuestion("Export all sprites unpadded?"));
    padded = true;

    texFolder = GetFolder(FilePath) + "background" + Path.DirectorySeparatorChar;
    string imageFolder = texFolder + "images" + Path.DirectorySeparatorChar;
    if (Directory.Exists(texFolder))
    {
        Directory.Delete(texFolder, true);
    }
    Directory.CreateDirectory(texFolder);
    Directory.CreateDirectory(imageFolder);

    SetProgressBar(null, "Backgrounds", 0, Data.Backgrounds.Count);
    StartProgressBarUpdater();

    await DumpBackgrounds();
    // worker.Cleanup();

    await StopProgressBarUpdater();
    HideProgressBar();

    // Export asset
    using (StreamWriter writer = new StreamWriter(texFolder + "asset_order.txt"))
    {
        writer.WriteLine("  <backgrounds name=\"background\">");
        for (int i = 0; i < Data.Backgrounds.Count; i++)
        {
            UndertaleBackground background = Data.Backgrounds[i];
            writer.WriteLine(
                "    <background>background\\" + background.Name.Content + "</background>"
            );
        }
        writer.WriteLine("  </backgrounds>");
    }

    ScriptMessage("Export Complete.\n\nLocation: " + texFolder);

    async Task DumpBackgrounds()
    {
        await Task.Run(() => Parallel.ForEach(Data.Backgrounds, DumpBackground));
    }

    void DumpBackground(UndertaleBackground background)
    {
        if (background.Texture != null)
        {
            worker.ExportAsPNG(
                background.Texture,
                imageFolder + background.Name.Content + ".png",
                null,
                true
            ); // Include padding to make sprites look neat!

            using (
                StreamWriter writer = new StreamWriter(
                    texFolder + background.Name.Content + ".background.gmx"
                )
            )
            {
                writer.WriteLine(
                    "<!--This Document is generated by GameMaker, if you edit it by hand then you do so at your own risk!-->"
                );
                writer.WriteLine("<background>");
                writer.WriteLine("  <istileset>0</istileset>");
                writer.WriteLine("  <tilewidth>0</tilewidth>");
                writer.WriteLine("  <tileheight>0</tileheight>");
                writer.WriteLine("  <tilexoff>0</tilexoff>");
                writer.WriteLine("  <tileyoff>0</tileyoff>");
                writer.WriteLine("  <tilehsep>0</tilehsep>");
                writer.WriteLine("  <tilevsep>0</tilevsep>");
                writer.WriteLine("  <HTile>0</HTile>");
                writer.WriteLine("  <VTile>0</VTile>");
                writer.WriteLine("  <TextureGroups>");
                writer.WriteLine("    <TextureGroup0>0</TextureGroup0>");
                writer.WriteLine("  </TextureGroups>");
                writer.WriteLine("  <For3D>0</For3D>");
                writer.WriteLine("  <width>" + background.Texture.SourceWidth + "</width>");
                writer.WriteLine("  <height>" + background.Texture.SourceHeight + "</height>");
                writer.WriteLine("  <data>images\\" + background.Name.Content + ".png</data>");
                writer.WriteLine("</background>");
            }

            IncrementProgressParallel();
        }
    }
}
