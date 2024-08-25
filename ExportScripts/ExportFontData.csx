// Made by mono21400

using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UndertaleModLib.Util;

EnsureDataLoaded();

if (Data.IsGameMaker2())
{
    string fntFolder = GetFolder(FilePath) + "fonts" + Path.DirectorySeparatorChar;
    TextureWorker worker = new TextureWorker();
    Directory.CreateDirectory(fntFolder);
    List<string> input = new List<string>();

    string[] arrayString = input.ToArray();

    SetProgressBar(null, "Fonts", 0, Data.Fonts.Count);
    StartProgressBarUpdater();

    await DumpFonts();
    //worker.Cleanup();

    await StopProgressBarUpdater();
    HideProgressBar();

    // Export asset
    using (StreamWriter writer = new StreamWriter(fntFolder + "asset_order.txt"))
    {
        for (int i = 0; i < Data.Fonts.Count; i++)
        {
            UndertaleFont font = Data.Fonts[i];
            writer.WriteLine(
                "    {\"id\":{\"name\":\""
                    + font.Name.Content
                    + "\",\"path\":\"fonts/"
                    + font.Name.Content
                    + "/"
                    + font.Name.Content
                    + ".yy\",},},"
            );
        }
    }

    ScriptMessage("Export Complete.\n\nLocation: " + fntFolder);

    string GetFolder(string path)
    {
        return Path.GetDirectoryName(path) + Path.DirectorySeparatorChar;
    }

    async Task DumpFonts()
    {
        await Task.Run(() => Parallel.ForEach(Data.Fonts, DumpFont));
    }

    void DumpFont(UndertaleFont font)
    {
        //if (arrayString.Contains(font.Name.ToString().Replace("\"", "")))
        //{
        System.IO.Directory.CreateDirectory(fntFolder + font.Name.Content);
        worker.ExportAsPNG(
            font.Texture,
            fntFolder + font.Name.Content + Path.DirectorySeparatorChar + font.Name.Content + ".png"
        );
        using (
            StreamWriter writer = new StreamWriter(
                fntFolder
                    + font.Name.Content
                    + Path.DirectorySeparatorChar
                    + font.Name.Content
                    + ".yy"
            )
        )
        {
            writer.WriteLine("{");
            writer.WriteLine("  \"resourceType\": \"GMFont\",");
            writer.WriteLine("  \"resourceVersion\": \"1.0\",");
            writer.WriteLine("  \"name\": \"" + font.Name.Content + "\",");
            writer.WriteLine("  \"AntiAlias\": " + font.AntiAliasing + ",");
            writer.WriteLine("  \"applyKerning\": 0,");
            writer.WriteLine("  \"ascender\": " + font.Ascender + ",");
            writer.WriteLine("  \"ascenderOffset\": " + font.AscenderOffset + ",");
            writer.WriteLine("  \"bold\": " + (font.Bold ? "true" : "false") + ",");
            writer.WriteLine("  \"canGenerateBitmap\": true,");
            writer.WriteLine("  \"charset\": " + font.Charset + ",");
            writer.WriteLine("  \"first\": 0,");
            writer.WriteLine("  \"fontName\": \"" + font.DisplayName.Content + "\",");
            writer.WriteLine("  \"glyphOperations\": 0,");
            writer.WriteLine("  \"glyphs\": {");
            foreach (var g in font.Glyphs)
            {
                writer.WriteLine(
                    "    \""
                        + g.Character
                        + "\": {\"character\":"
                        + g.Character
                        + ",\"h\":"
                        + g.SourceHeight
                        + ",\"offset\":"
                        + g.Offset
                        + ",\"shift\":"
                        + g.Shift
                        + ",\"w\":"
                        + g.SourceWidth
                        + ",\"x\":"
                        + g.SourceX
                        + ",\"y\":"
                        + g.SourceY
                        + ",},"
                );
            }
            writer.WriteLine("  },");
            writer.WriteLine("  \"hinting\": 0,");
            writer.WriteLine("  \"includeTTF\": false,");
            writer.WriteLine("  \"interpreter\": 0,");
            writer.WriteLine("  \"italic\": " + (font.Italic ? "true" : "false") + ",");
            writer.WriteLine("  \"kerningPairs\": [],");
            writer.WriteLine("  \"last\": 0,");
            writer.WriteLine("  \"maintainGms1Font\": false,");
            writer.WriteLine("  \"parent\": {");
            writer.WriteLine("    \"name\": \"Fonts\",");
            writer.WriteLine("    \"path\": \"folders/Fonts.yy\",");
            writer.WriteLine("  },");
            writer.WriteLine("  \"pointRounding\": 0,");
            writer.WriteLine("  \"ranges\": [");
            writer.WriteLine(
                "    {\"lower\":" + font.RangeStart + ",\"upper\":" + font.RangeEnd + ",},"
            );
            writer.WriteLine("  ],");
            writer.WriteLine("  \"regenerateBitmap\": false,");
            writer.WriteLine(
                "  \"sampleText\": \"abcdef ABCDEF\\n0123456789 .,<>\\\"'&!?\\nthe quick brown fox jumps over the lazy dog\\nTHE QUICK BROWN FOX JUMPS OVER THE LAZY DOG\\nDefault character: ▯ (9647)\","
            );
            writer.WriteLine("  \"size\": " + (font.EmSize + 0.0f) + ".0,");
            writer.WriteLine("  \"styleName\": \"Regular\",");
            writer.WriteLine("  \"textureGroupId\": {");
            writer.WriteLine("    \"name\": \"Default\",");
            writer.WriteLine("    \"path\": \"texturegroups/Default\",");
            writer.WriteLine("  },");
            writer.WriteLine("  \"TTFName\": \"\",");
            writer.Write("}");
        }

        IncrementProgressParallel();
    }
}
else
{
    string fntFolder = GetFolder(FilePath) + "fonts" + Path.DirectorySeparatorChar;
    TextureWorker worker = new TextureWorker();
    Directory.CreateDirectory(fntFolder);
    List<string> input = new List<string>();

    string[] arrayString = input.ToArray();

    SetProgressBar(null, "Fonts", 0, Data.Fonts.Count);
    StartProgressBarUpdater();

    await DumpFonts();
    //worker.Cleanup();

    await StopProgressBarUpdater();
    HideProgressBar();
    //
    // Export asset
    using (StreamWriter writer = new StreamWriter(fntFolder + "asset_order.txt"))
    {
        writer.WriteLine("  <fonts name=\"fonts\">");
        for (int i = 0; i < Data.Fonts.Count; i++)
        {
            UndertaleFont font = Data.Fonts[i];
            writer.WriteLine("    <font>fonts\\" + font.Name.Content + "</font>");
        }
        writer.WriteLine("  </fonts>");
    }
    ScriptMessage("Export Complete.\n\nLocation: " + fntFolder);

    string GetFolder(string path)
    {
        return Path.GetDirectoryName(path) + Path.DirectorySeparatorChar;
    }

    async Task DumpFonts()
    {
        await Task.Run(() => Parallel.ForEach(Data.Fonts, DumpFont));
    }

    void DumpFont(UndertaleFont font)
    {
        //if (arrayString.Contains(font.Name.ToString().Replace("\"", "")))
        //{
        worker.ExportAsPNG(font.Texture, fntFolder + font.Name.Content + ".png");
        using (StreamWriter writer = new StreamWriter(fntFolder + font.Name.Content + ".font.gmx"))
        {
            writer.WriteLine(
                "<!--This Document is generated by GameMaker, if you edit it by hand then you do so at your own risk!-->"
            );
            writer.WriteLine("<font>");
            writer.WriteLine("  <name>" + font.Name.Content + "</name>");
            writer.WriteLine("  <size>" + font.EmSize + "</size>");
            if (font.Bold)
            {
                writer.WriteLine("  <bold>-1</bold>");
            }
            else
            {
                writer.WriteLine("  <bold>0</bold>");
            }
            writer.WriteLine("  <renderhq>0</renderhq>");
            if (font.Italic)
            {
                writer.WriteLine("  <italic>-1</italic>");
            }
            else
            {
                writer.WriteLine("  <italic>0</italic>");
            }
            writer.WriteLine("  <charset>" + font.Charset + "</charset>");
            writer.WriteLine("  <aa>" + font.AntiAliasing + "</aa>");
            writer.WriteLine("  <includeTTF>0</includeTTF>");
            writer.WriteLine("  <TTFName></TTFName>");
            writer.WriteLine("  <texgroups>");
            writer.WriteLine("    <texgroup0>0</texgroup0>");
            writer.WriteLine("  </texgroups>");
            writer.WriteLine("  <ranges>");
            writer.WriteLine("    <range0>" + font.RangeStart + "," + font.RangeEnd + "</range0>");
            writer.WriteLine("  </ranges>");

            writer.WriteLine("  <glyphs>");
            foreach (var g in font.Glyphs)
            {
                writer.WriteLine(
                    "    <glyph character=\""
                        + g.Character
                        + "\" x=\""
                        + g.SourceX
                        + "\" y=\""
                        + g.SourceY
                        + "\" w=\""
                        + g.SourceWidth
                        + "\" h=\""
                        + g.SourceHeight
                        + "\" shift=\""
                        + g.Shift
                        + "\" offset=\""
                        + g.Offset
                        + "\"/>"
                );
            }
            writer.WriteLine("  </glyphs>");

            //writer.WriteLine(font.DisplayName + ";" + font.EmSize + ";" + font.Bold + ";" + font.Italic + ";" + font.Charset + ";" + font.AntiAliasing + ";" + font.ScaleX + ";" + font.ScaleY);

            //foreach (var g in font.Glyphs)
            //{
            //    writer.WriteLine(g.Character + ";" + g.SourceX + ";" + g.SourceY + ";" + g.SourceWidth + ";" + g.SourceHeight + ";" + g.Shift + ";" + g.Offset);
            //}

            writer.WriteLine("  <kerningPairs/>");
            writer.WriteLine("  <image>" + font.Name.Content + ".png</image>");
            writer.WriteLine("</font>");
        }
        //}

        IncrementProgressParallel();
    }
}
