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
            writer.WriteLine(
                "    {\"id\":{\"name\":\""
                    + sprite.Name.Content
                    + "\",\"path\":\"sprites/"
                    + sprite.Name.Content
                    + "/"
                    + sprite.Name.Content
                    + ".yy\",},},"
            );
        }
        for (int i = 0; i < Data.Backgrounds.Count; i++)
        {
            UndertaleBackground background = Data.Backgrounds[i];
            writer.WriteLine(
                "    {\"id\":{\"name\":\""
                    + background.Name.Content
                    + "\",\"path\":\"sprites/"
                    + background.Name.Content
                    + "/"
                    + background.Name.Content
                    + ".yy\",},},"
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

            writer.WriteLine("{");
            writer.WriteLine("  \"resourceType\": \"GMSprite\",");
            writer.WriteLine("  \"resourceVersion\": \"1.0\",");
            writer.WriteLine("  \"name\": \"" + sprite.Name.Content + "\",");
            writer.WriteLine("  \"bbox_bottom\": " + sprite.MarginBottom + ",");
            writer.WriteLine("  \"bbox_left\": " + sprite.MarginLeft + ",");
            writer.WriteLine("  \"bbox_right\": " + sprite.MarginRight + ",");
            writer.WriteLine("  \"bbox_top\": " + sprite.MarginTop + ",");
            writer.WriteLine("  \"bboxMode\": " + sprite.BBoxMode + ",");
            if (sprite.SepMasks == UndertaleSprite.SepMaskType.Precise)
            {
                writer.WriteLine("  \"collisionKind\": 4,");
            }
            else if (sprite.SepMasks == UndertaleSprite.SepMaskType.AxisAlignedRect)
            {
                writer.WriteLine("  \"collisionKind\": 0,");
            }
            else
            {
                writer.WriteLine("  \"collisionKind\": 1,");
            }
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
                    writer.WriteLine(
                        "    {\"resourceType\":\"GMSpriteFrame\",\"resourceVersion\":\"1.1\",\"name\":\""
                            + sprite.Name.Content
                            + "_"
                            + i
                            + "\",},"
                    );
                }
            }
            writer.WriteLine("  ],");
            // END : frames
            writer.WriteLine("  \"gridX\": 0,");
            writer.WriteLine("  \"gridY\": 0,");
            writer.WriteLine("  \"height\": " + sprite.Height + ",");
            writer.WriteLine("  \"HTile\": false,");
            // BEGIN : layers
            writer.WriteLine("  \"layers\": [");
            writer.WriteLine(
                "    {\"resourceType\":\"GMImageLayer\",\"resourceVersion\":\"1.0\",\"name\":\""
                    + sprite.Name.Content
                    + "_"
                    + "layer"
                    + "\",\"blendMode\":0,\"displayName\":\"default\",\"isLocked\":false,\"opacity\":100.0,\"visible\":true,},"
            );
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
            writer.WriteLine("    \"name\": \"" + sprite.Name.Content + "\",");
            writer.WriteLine("    \"autoRecord\": true,");
            writer.WriteLine("    \"backdropHeight\": 1080,");
            writer.WriteLine("    \"backdropImageOpacity\": 0.5,");
            writer.WriteLine("    \"backdropImagePath\": \"\",");
            writer.WriteLine("    \"backdropWidth\": 1920,");
            writer.WriteLine("    \"backdropXOffset\": 0.0,");
            writer.WriteLine("    \"backdropYOffset\": 0.0,");
            writer.WriteLine(
                "    \"events\": {\"resourceType\":\"KeyframeStore<MessageEventKeyframe>\",\"resourceVersion\":\"1.0\",\"Keyframes\":[],},"
            );
            writer.WriteLine("    \"eventStubScript\": null,");
            writer.WriteLine("    \"eventToFunction\": {},");
            writer.WriteLine("    \"length\": " + sprite.Textures.Count.ToString("0.0") + ",");
            writer.WriteLine("    \"lockOrigin\": false,");
            writer.WriteLine(
                "    \"moments\": {\"resourceType\":\"KeyframeStore<MomentsEventKeyframe>\",\"resourceVersion\":\"1.0\",\"Keyframes\":[],},"
            );
            writer.WriteLine("    \"playback\": 1,");
            writer.WriteLine(
                "    \"playbackSpeed\": " + sprite.GMS2PlaybackSpeed.ToString("0.0") + ","
            );
            writer.WriteLine(
                "    \"playbackSpeedType\": " + (ushort)sprite.GMS2PlaybackSpeedType + ","
            );
            writer.WriteLine("    \"showBackdrop\": true,");
            writer.WriteLine("    \"showBackdropImage\": false,");
            writer.WriteLine("    \"timeUnits\": 1,");
            // BEGIN : tracks
            writer.WriteLine("    \"tracks\": [");
            writer.WriteLine(
                "      {\"resourceType\":\"GMSpriteFramesTrack\",\"resourceVersion\":\"1.0\",\"name\":\"frames\",\"builtinName\":0,\"events\":[],\"inheritsTrackColour\":true,\"interpolation\":1,\"isCreationTrack\":false,\"keyframes\":{\"resourceType\":\"KeyframeStore<SpriteFrameKeyframe>\",\"resourceVersion\":\"1.0\",\"Keyframes\":["
            );
            for (int i = 0; i < sprite.Textures.Count; i++)
            {
                if (sprite.Textures[i]?.Texture != null)
                {
                    writer.WriteLine(
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
                    );
                }
            }
            writer.WriteLine(
                "      ],},\"modifiers\":[],\"spriteId\":null,\"trackColour\":0,\"tracks\":[],\"traits\":0,},"
            );
            writer.WriteLine("    ],");
            // END : tracks
            writer.WriteLine("    \"visibleRange\": null,");
            writer.WriteLine("    \"volume\": 1.0,");
            writer.WriteLine("    \"xorigin\": " + sprite.OriginX + ",");
            writer.WriteLine("    \"yorigin\": " + sprite.OriginY + ",");
            writer.WriteLine("  },");
            writer.WriteLine("  \"swatchColours\": null,");
            writer.WriteLine("  \"swfPrecision\": 2.525,");
            writer.WriteLine("  \"textureGroupId\": {");
            writer.WriteLine("    \"name\": \"Default\",");
            writer.WriteLine("    \"path\": \"texturegroups/Default\",");
            writer.WriteLine("  },");
            writer.WriteLine("  \"type\": 0,");
            writer.WriteLine("  \"VTile\": false,");
            writer.WriteLine("  \"width\": " + sprite.Width + ",");
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

            writer.WriteLine("{");
            writer.WriteLine("  \"resourceType\": \"GMSprite\",");
            writer.WriteLine("  \"resourceVersion\": \"1.0\",");
            writer.WriteLine("  \"name\": \"" + background.Name.Content + "\",");
            writer.WriteLine("  \"bbox_bottom\": 63,");
            writer.WriteLine("  \"bbox_left\": 0,");
            writer.WriteLine("  \"bbox_right\": 63,");
            writer.WriteLine("  \"bbox_top\": 0,");
            writer.WriteLine("  \"bboxMode\": 0,");
            writer.WriteLine("  \"collisionKind\": 1,");
            writer.WriteLine("  \"collisionTolerance\": 0,");
            writer.WriteLine("  \"DynamicTexturePage\": false,");
            writer.WriteLine("  \"edgeFiltering\": false,");
            writer.WriteLine("  \"For3D\": false,");
            writer.WriteLine("  \"frames\": [");
            writer.WriteLine(
                "    {\"resourceType\":\"GMSpriteFrame\",\"resourceVersion\":\"1.1\",\"name\":\""
                    + background.Name.Content
                    + "\",},"
            );
            writer.WriteLine("  ],");
            writer.WriteLine("  \"gridX\": 0,");
            writer.WriteLine("  \"gridY\": 0,");
            writer.WriteLine("  \"height\": " + background.Texture.TargetHeight + ",");
            writer.WriteLine("  \"HTile\": false,");
            writer.WriteLine("  \"layers\": [");
            writer.WriteLine(
                "    {\"resourceType\":\"GMImageLayer\",\"resourceVersion\":\"1.0\",\"name\":\""
                    + background.Name.Content
                    + "\",\"blendMode\":0,\"displayName\":\"default\",\"isLocked\":false,\"opacity\":100.0,\"visible\":true,},"
            );
            writer.WriteLine("  ],");
            writer.WriteLine("  \"nineSlice\": null,");
            writer.WriteLine("  \"origin\": 0,");
            writer.WriteLine("  \"parent\": {");
            writer.WriteLine("    \"name\": \"tilesets\",");
            writer.WriteLine("    \"path\": \"folders/Sprites/tilesets.yy\",");
            writer.WriteLine("  },");
            writer.WriteLine("  \"preMultiplyAlpha\": false,");
            writer.WriteLine("  \"sequence\": {");
            writer.WriteLine("    \"resourceType\": \"GMSequence\",");
            writer.WriteLine("    \"resourceVersion\": \"1.4\",");
            writer.WriteLine("    \"name\": \"\",");
            writer.WriteLine("    \"autoRecord\": true,");
            writer.WriteLine("    \"backdropHeight\": 1080,");
            writer.WriteLine("    \"backdropImageOpacity\": 0.5,");
            writer.WriteLine("    \"backdropImagePath\": \"\",");
            writer.WriteLine("    \"backdropWidth\": 1920,");
            writer.WriteLine("    \"backdropXOffset\": 0.0,");
            writer.WriteLine("    \"backdropYOffset\": 0.0,");
            writer.WriteLine(
                "    \"events\": {\"resourceType\":\"KeyframeStore<MessageEventKeyframe>\",\"resourceVersion\":\"1.0\",\"Keyframes\":[],},"
            );
            writer.WriteLine("    \"eventStubScript\": null,");
            writer.WriteLine("    \"eventToFunction\": {},");
            writer.WriteLine("    \"length\": 1.0,");
            writer.WriteLine("    \"lockOrigin\": false,");
            writer.WriteLine(
                "    \"moments\": {\"resourceType\":\"KeyframeStore<MomentsEventKeyframe>\",\"resourceVersion\":\"1.0\",\"Keyframes\":[],},"
            );
            writer.WriteLine("    \"playback\": 1,");
            writer.WriteLine("    \"playbackSpeed\": 1.0,");
            writer.WriteLine("    \"playbackSpeedType\": 1,");
            writer.WriteLine("    \"showBackdrop\": true,");
            writer.WriteLine("    \"showBackdropImage\": false,");
            writer.WriteLine("    \"timeUnits\": 1,");
            writer.WriteLine("    \"tracks\": [");
            writer.WriteLine(
                "      {\"resourceType\":\"GMSpriteFramesTrack\",\"resourceVersion\":\"1.0\",\"name\":\"frames\",\"builtinName\":0,\"events\":[],\"inheritsTrackColour\":true,\"interpolation\":1,\"isCreationTrack\":false,\"keyframes\":{\"resourceType\":\"KeyframeStore<SpriteFrameKeyframe>\",\"resourceVersion\":\"1.0\",\"Keyframes\":["
            );
            writer.WriteLine(
                "            {\"resourceType\":\"Keyframe<SpriteFrameKeyframe>\",\"resourceVersion\":\"1.0\",\"Channels\":{\"0\":{\"resourceType\":\"SpriteFrameKeyframe\",\"resourceVersion\":\"1.0\",\"Id\":{\"name\":\""
                    + background.Name.Content
                    + "\",\"path\":\"sprites/"
                    + background.Name.Content
                    + "/"
                    + background.Name.Content
                    + ".yy\",},},},\"Disabled\":false,\"IsCreationKey\":false,\"Key\":0.0,\"Length\":1.0,\"Stretch\":false,},"
            );
            writer.WriteLine(
                "          ],},\"modifiers\":[],\"spriteId\":null,\"trackColour\":0,\"tracks\":[],\"traits\":0,},"
            );
            writer.WriteLine("    ],");
            writer.WriteLine("    \"visibleRange\": null,");
            writer.WriteLine("    \"volume\": 1.0,");
            writer.WriteLine("    \"xorigin\": 0,");
            writer.WriteLine("    \"yorigin\": 0,");
            writer.WriteLine("  },");
            writer.WriteLine("  \"swatchColours\": null,");
            writer.WriteLine("  \"swfPrecision\": 2.525,");
            writer.WriteLine("  \"textureGroupId\": {");
            writer.WriteLine("    \"name\": \"Default\",");
            writer.WriteLine("    \"path\": \"texturegroups/Default\",");
            writer.WriteLine("  },");
            writer.WriteLine("  \"type\": 0,");
            writer.WriteLine("  \"VTile\": false,");
            writer.WriteLine("  \"width\": " + background.Texture.TargetWidth + ",");
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
