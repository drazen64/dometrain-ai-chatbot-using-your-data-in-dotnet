// See https://aka.ms/new-console-template for more information
//using System.Numerics;
using Microsoft.Extensions.AI;
using OpenAI.Embeddings;
using MathNet.Numerics.LinearAlgebra;
using System.Text;

class Program
{
    static async Task Main(string[] args)
    {
        var apiKey = RequireEnv("OPENAI_API_KEY");
        var vectors = new List<(string Word, float[] Vec)>();

        var embedder = new EmbeddingClient(
            model: "text-embedding-3-small",
            apiKey: apiKey  
        ).AsIEmbeddingGenerator();

        var words = new[]
        {
            "apple",
            "banana",
            "car",
            "bus",
            "dog",
            "cat",
            "elephant",
            "grapefruit",
            "honda",
            "ford",
            "tiger",
            "lion",
            "helicopter",
            "airplane",
            "train",
            "blue",
            "red",
            "space"
        };

        for(int i = 0; i < words.Length; i++)
        {
            var word = words[i];
            var embedding = await embedder.GenerateAsync([word], 
                new Microsoft.Extensions.AI.EmbeddingGenerationOptions  
                {
                    Dimensions = 512
                })  ;
            var vector = embedding[0].Vector.ToArray();
            vectors.Add((word, vector));

        }
        SaveCsv(vectors, "keyword-embeddings-pca.csv");

    }




    static string RequireEnv(string key)
    {
        var v = Environment.GetEnvironmentVariable(key);
        if (string.IsNullOrWhiteSpace(v))
            throw new Exception($"Missing env var: {key}");
        return v!;
    }

    static void SaveCsv(List<(string Word, float[] Vec)> data, string path)
    {
        if (data == null || data.Count == 0)
        {
            System.Console.WriteLine("No vectors to project");
            return;
        }

        // Build an n x d matrix (double) and mean-center
        int n = data.Count;
        int d = data[0].Vec.Length;
        var X = Matrix<double>.Build.Dense(n, d, (i, j) => data[i].Vec[j]);

        //Mean center columns
        var means = Vector<double>.Build.Dense(d);

        for (int j = 0; j < d; j++)
        {
            means[j] = X.Column(j).Average();
            for (int i = 0; i < n; i++)
            {
                X[i, j] -= means[j];
            }
        }

        // PCA via SVD of mean-centered X
        // X = U * S * V^T, principal direction = V columns
        var svd = X.Svd(computeVectors: true);
        var V = svd.VT.Transpose(); // d x d

        // Take first two principal directions
        var V2 = V.SubMatrix(0, d, 0, 2); // d x 2
        var Y  = X * V2; // n x 2

        // writee CSV: id, title, x, y (culture-invariant)
        using var sw = new StreamWriter(path, false, Encoding.UTF8);
        sw.WriteLine("id,title,x,y");
        for (int i = 0; i < n; i++)
        {
            var x = Y[i, 0];
            var y = Y[i, 1];
            sw.WriteLine($"{i},{CsvEscape(data[i].Word)},{x.ToString(System.Globalization.CultureInfo.InvariantCulture)},{y.ToString(System.Globalization.CultureInfo.InvariantCulture)}");
        }



    }


    static string CsvEscape(string? value)
    {
        if (string.IsNullOrEmpty(value)) return "";
        var needsQuotes = value.Contains(',') || value.Contains('"') || value.Contains('\n') || value.Contains('\r');
        var escaped = value.Replace("\"", "\"\"");
        return needsQuotes ? $"\"{escaped}\"" : escaped;
    }


}