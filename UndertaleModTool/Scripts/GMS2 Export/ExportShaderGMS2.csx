// Made by mono21400

using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UndertaleModLib.Util;

EnsureDataLoaded();

// Pour avoir un "." au lieu d'une "," dans les conversion en décimal
System.Globalization.CultureInfo customCulture = (System.Globalization.CultureInfo)
    System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
customCulture.NumberFormat.NumberDecimalSeparator = ".";

System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;

string shaderPaths = GetFolder(FilePath) + "shaders" + Path.DirectorySeparatorChar;
TextureWorker worker = new TextureWorker();

if (Directory.Exists(shaderPaths))
{
    Directory.Delete(shaderPaths, true);
}

Directory.CreateDirectory(shaderPaths);

SetProgressBar(null, "Shaders", 0, Data.Shaders.Count);
StartProgressBarUpdater();

await DumpShaders();
// worker.Cleanup();

await StopProgressBarUpdater();
HideProgressBar();

// Export asset
using (StreamWriter writer = new StreamWriter(shaderPaths + "asset_order.txt"))
{
    for (int i = 0; i < Data.Shaders.Count; i++)
    {
        UndertaleShader shader = Data.Shaders[i];
        writer.WriteLine(
            "    {\"id\":{\"name\":\""
                + shader.Name.Content
                + "\",\"path\":\"shaders/"
                + shader.Name.Content
                + "/"
                + shader.Name.Content
                + ".yy\",},},"
        );
    }
}

ScriptMessage("Export Complete.\n\nLocation: " + shaderPaths);

string GetFolder(string path)
{
    return Path.GetDirectoryName(path) + Path.DirectorySeparatorChar;
}

async Task DumpShaders()
{
    await Task.Run(() => Parallel.ForEach(Data.Shaders, DumpShader));
}

void DumpShader(UndertaleShader shader)
{
    Directory.CreateDirectory(shaderPaths + shader.Name.Content);

    using (
        StreamWriter writer = new StreamWriter(
            shaderPaths + shader.Name.Content + Path.DirectorySeparatorChar + shader.Name.Content + ".yy"
        )
    )
    {
        writer.WriteLine("{");
        writer.WriteLine("  \"resourceType\": \"GMShader\",");
        writer.WriteLine("  \"resourceVersion\": \"1.0\",");
        writer.WriteLine("  \"name\": \"" + shader.Name.Content + "\",");
        writer.WriteLine("  \"parent\": {");
        writer.WriteLine("    \"name\": \"Shaders\",");
        writer.WriteLine("    \"path\": \"folders/Shaders.yy\",");
        writer.WriteLine("  },");
        writer.WriteLine("  \"type\": " + (int)shader.Type + ",");
        writer.WriteLine("}");
    }

    switch (shader.Type)
    {
        case UndertaleShader.ShaderType.GLSL_ES:
            // Vertex Source
            string path_vsh =
                shaderPaths + shader.Name.Content + Path.DirectorySeparatorChar + shader.Name.Content + ".vsh";
            File.WriteAllText(path_vsh, shader.GLSL_ES_Vertex.Content);
            // Fragment Source
            string path_fsh =
                shaderPaths + shader.Name.Content + Path.DirectorySeparatorChar + shader.Name.Content + ".fsh";
            File.WriteAllText(path_fsh, shader.GLSL_ES_Fragment.Content);
            break;

        default:
            throw new InvalidOperationException("Type de shader non supporté");
    }

    IncrementProgressParallel();
}
