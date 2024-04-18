﻿// Modified with the help of Agentalex9
using System.Text;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UndertaleModLib.Util;

EnsureDataLoaded();

// bool padded = (!ScriptQuestion("Export all sprites unpadded?"));
bool padded = true;


string texFolder = GetFolder(FilePath) + "background" + Path.DirectorySeparatorChar;
string imageFolder = texFolder + "images\\";
TextureWorker worker = new TextureWorker();
if (Directory.Exists(texFolder))
{
    Directory.Delete(texFolder, true);
}
Directory.CreateDirectory(texFolder);
Directory.CreateDirectory(imageFolder);

SetProgressBar(null, "Backgrounds", 0, Data.Backgrounds.Count);
StartProgressBarUpdater();

await DumpBackgrounds();
worker.Cleanup();

await StopProgressBarUpdater();
HideProgressBar();
ScriptMessage("Export Complete.\n\nLocation: " + texFolder);


string GetFolder(string path)
{
    return Path.GetDirectoryName(path) + Path.DirectorySeparatorChar;
}

async Task DumpBackgrounds()
{
    await Task.Run(() => Parallel.ForEach(Data.Backgrounds, DumpBackground));
}

void DumpBackground(UndertaleBackground background)
{
    if (background.Texture != null)
    {
        worker.ExportAsPNG(background.Texture, imageFolder + background.Name.Content + ".png", null, true); // Include padding to make sprites look neat!

        using (StreamWriter writer = new StreamWriter(texFolder + background.Name.Content + ".background.gmx"))
        {
            writer.WriteLine("<!--This Document is generated by GameMaker, if you edit it by hand then you do so at your own risk!-->");
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
