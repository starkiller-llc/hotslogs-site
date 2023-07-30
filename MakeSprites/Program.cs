// See https://aka.ms/new-console-template for more information

using SkiaSharp;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

Console.WriteLine("Hello, World!");
Console.WriteLine(Environment.CurrentDirectory);

var workList = new List<(string Name, string SrcPath, int SizeX, int SizeY)>
{
    ("portraits", "Heroes\\Portraits\\", 40, 40),
    ("talents", "Talents\\", 30, 30),
    ("maps", "Maps\\", 75, 39),
    ("roles", "Roles\\", 30, 30),
    ("awards", "Awards\\", 30, 30),
};

//byte[] ggg = null;

workList.ForEach(
    x =>
    {
        var srcPath = $@"HotsLogsApi\wwwroot\Images\{x.SrcPath}";
        var pngName = $"{x.Name}.png";
        var cssName = $"{x.Name}.css";
        var tgtPngPath = $@"Angular\projects\app\src\assets\Images\{pngName}";
        var tgtCssPath = $@"Angular\projects\app\src\assets\css\{cssName}";
        MakeSprites(srcPath, x.Name, size: x.SizeX, sizeY: x.SizeY);
        File.Copy(pngName, tgtPngPath, true);
        File.Copy(cssName, tgtCssPath, true);
        File.Delete(pngName);
        File.Delete(cssName);
    });

static void MakeSprites(
    string s,
    string outName,
    int quality = 100,
    int size = 40,
    int? sizeY = null,
    SKEncodedImageFormat format = SKEncodedImageFormat.Png,
    string extension = "png")
{
    var cacheBust = CreateMd5ForFolder(s);
    var images = Directory.GetFiles(s, "*.png");
    var sideLength = (int)Math.Ceiling(Math.Sqrt(images.Length));
    var spriteBitmap = new SKBitmap(sideLength * size, sideLength * size);
    var spriteCanvas = new SKCanvas(spriteBitmap);
    var i = 0;
    var cssList = new List<(string name, int x, int y)>();
    var test = new Dictionary<string, (string name, int x, int y)>();
    var sha = SHA512.Create();
    var sizeYDefinite = sizeY ?? size;
    foreach (var image in images)
    {
        var name = Path.GetFileNameWithoutExtension(image);
        var fileBytes = File.ReadAllBytes(image);
        var bitmap = SKBitmap.Decode(fileBytes);
        var bytes = bitmap.Bytes;
        var key = Convert.ToBase64String(sha.ComputeHash(bytes));
        //if (image.Contains("AdrenalineBoost"))
        //{
        //    ggg = bytes;
        //}

        //if (image.Contains("RegenerativeMicrobes"))
        //{
        //    var z = bytes.Zip(ggg, ((a, b) => (a,b,(a == b)))).Select((x, i) => (x, i)).ToList();
        //    var diffs = z.Where(r => !r.x.Item3).ToList();
        //    var diffs2 = z.Where(r => Math.Abs(r.x.a-r.x.b) > 6).ToList();
        //}
        if (test.ContainsKey(key))
        {
            Console.WriteLine($"Image {image} identical to {test[key]}");
            var (_, x, y) = test[key];
            cssList.Add((name, x, y));
        }
        else
        {
            var ratio = 1.0 * bitmap.Width / bitmap.Height;

            double newWidth = size;
            var newHeight = newWidth / ratio;

            if (newHeight > sizeYDefinite)
            {
                newHeight = sizeYDefinite;
                newWidth = ratio * newHeight;
            }

            var scaledBitmap = new SKBitmap((int)newWidth, (int)newHeight);
            bitmap.ScalePixels(scaledBitmap, SKFilterQuality.High);
            var x = i % sideLength * size;
            var y = i / sideLength * sizeYDefinite;
            var xoff = (int)((size - newWidth) / 2);
            var yoff = (int)((sizeYDefinite - newHeight) / 2);
            spriteCanvas.DrawBitmap(scaledBitmap, x + xoff, y + yoff);
            test[key] = (name, x, y);
            cssList.Add(test[key]);
            i++;
        }
    }

    var data = spriteBitmap.Encode(SKEncodedImageFormat.Png, 100);
    var spriteBytes = data.ToArray();
    var fn = $"{outName}.{extension}";
    File.WriteAllBytes(fn, spriteBytes);

    using var fl = File.Open($"{outName}.css", FileMode.Create);
    using var fw = new StreamWriter(fl);
    foreach (var (name, x, y) in cssList)
    {
        var xs = x == 0 ? "0" : $"{-x}px";
        var ys = y == 0 ? "0" : $"{-y}px";
        var css =
            @$".{outName}-{RemoveDiacritics(name)} {{ background: url(/assets/Images/{outName}.{extension}?v={cacheBust}) {xs} {ys} }}";
        fw.WriteLine(css);
    }
}

static string RemoveDiacritics(string text)
{
    var normalizedString = text.Normalize(NormalizationForm.FormD);
    var stringBuilder = new StringBuilder(capacity: normalizedString.Length);

    for (var i = 0; i < normalizedString.Length; i++)
    {
        var c = normalizedString[i];
        var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
        if (unicodeCategory != UnicodeCategory.NonSpacingMark)
        {
            stringBuilder.Append(c);
        }
    }

    return stringBuilder
        .ToString()
        .Normalize(NormalizationForm.FormC)
        .ToLowerInvariant();
}

// Credit to Dunc https://stackoverflow.com/a/15683147/235648
static string CreateMd5ForFolder(string path)
{
    // assuming you want to include nested folders
    var files = Directory.GetFiles(path, "*", SearchOption.AllDirectories)
        .OrderBy(p => p).ToList();

    MD5 md5 = MD5.Create();

    for (int i = 0; i < files.Count; i++)
    {
        string file = files[i];

        // hash path
        string relativePath = file[(path.Length + 1)..];
        byte[] pathBytes = Encoding.UTF8.GetBytes(relativePath.ToLower());
        md5.TransformBlock(pathBytes, 0, pathBytes.Length, pathBytes, 0);

        // hash contents
        byte[] contentBytes = File.ReadAllBytes(file);
        if (i == files.Count - 1)
            md5.TransformFinalBlock(contentBytes, 0, contentBytes.Length);
        else
            md5.TransformBlock(contentBytes, 0, contentBytes.Length, contentBytes, 0);
    }

    return BitConverter.ToString(md5.Hash ?? Array.Empty<byte>()).Replace("-", "").ToLower();
}
