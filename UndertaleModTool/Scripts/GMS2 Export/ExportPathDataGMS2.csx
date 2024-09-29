// Made by mono21400

using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
// using System.Windows.Forms;
using UndertaleModLib.Util;

EnsureDataLoaded();

// Pour avoir un "." au lieu d'une "," dans les conversion en décimal
System.Globalization.CultureInfo customCulture = (System.Globalization.CultureInfo)
    System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
customCulture.NumberFormat.NumberDecimalSeparator = ".";

System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;

//

string fntPaths = GetFolder(FilePath) + "paths" + Path.DirectorySeparatorChar;
TextureWorker worker = new TextureWorker();

if (Directory.Exists(fntPaths))
{
    Directory.Delete(fntPaths, true);
}

Directory.CreateDirectory(fntPaths);

SetProgressBar(null, "Paths", 0, Data.Paths.Count);
StartProgressBarUpdater();

await DumpPaths();
// worker.Cleanup();

await StopProgressBarUpdater();
HideProgressBar();

// Export asset
using (StreamWriter writer = new StreamWriter(fntPaths + "asset_order.txt"))
{
    for (int i = 0; i < Data.Paths.Count; i++)
    {
        UndertalePath path = Data.Paths[i];
        writer.WriteLine(
            "    {\"id\":{\"name\":\""
                + path.Name.Content
                + "\",\"path\":\"paths/"
                + path.Name.Content
                + "/"
                + path.Name.Content
                + ".yy\",},},"
        );
    }
}

ScriptMessage("Export Complete.\n\nLocation: " + fntPaths);

string GetFolder(string path)
{
    return Path.GetDirectoryName(path) + Path.DirectorySeparatorChar;
}

async Task DumpPaths()
{
    await Task.Run(() => Parallel.ForEach(Data.Paths, DumpPath));
}

void DumpPath(UndertalePath path)
{
    Directory.CreateDirectory(fntPaths + path.Name.Content);

    using (
        StreamWriter writer = new StreamWriter(
            fntPaths + path.Name.Content + Path.DirectorySeparatorChar + path.Name.Content + ".yy"
        )
    )
    {
        writer.WriteLine("{");
        writer.WriteLine("  \"resourceType\": \"GMPath\",");
        writer.WriteLine("  \"resourceVersion\": \"1.0\",");
        writer.WriteLine("  \"name\": \"" + path.Name.Content + "\",");
        writer.WriteLine("  \"closed\": " + (path.IsClosed ? "true" : "false") + ",");
        writer.WriteLine("  \"kind\": " + (path.IsSmooth ? -1 : 0) + ",");
        writer.WriteLine("  \"parent\": {");
        writer.WriteLine("    \"name\": \"Paths\",");
        writer.WriteLine("    \"path\": \"folders/Paths.yy\",");
        writer.WriteLine("  },");
        if (path.Points.Count > 1)
        {
            writer.WriteLine("  \"points\": [");
            foreach (var g in path.Points)
            {
                writer.WriteLine(
                    "    {\"speed\":"
                        + g.Speed.ToString("0.0")
                        + ",\"x\":"
                        + g.X.ToString("0.0")
                        + ",\"y\":"
                        + g.Y.ToString("0.0")
                        + ",},"
                );
            }
            writer.WriteLine("  ],");
        }
        else
        {
            writer.WriteLine("  \"points\": [],");
        }

        writer.WriteLine("  \"precision\": " + path.Precision + ",");
        writer.Write("}");
    }

    IncrementProgressParallel();
}
