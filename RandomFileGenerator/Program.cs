using System.Net;
using System.Threading.Tasks.Dataflow;

// configuration
const uint bytesInMegaByte = 1024 * 1024;
const ushort maximumSizeOfString = 1024;
const ushort stringLinesWritingChunk = 1000;
const ushort randomStringsVocabularySize = 1000;
const string allowedChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ ";
const string workingDirectory = "../../../../";

// script parameters
const uint expectedFileSizeInBytes = 1024 * bytesInMegaByte;
const string expectedFile = "generatedFile.txt";

// body
var watch = System.Diagnostics.Stopwatch.StartNew();

Directory.SetCurrentDirectory(workingDirectory);

var buffer = new BufferBlock<byte[]>();
var consumerTask = ConsumeAsync(buffer);
Produce(buffer);
await consumerTask;

Console.WriteLine("Done in " + watch.Elapsed.TotalSeconds);

static void Produce(ITargetBlock<byte[]> target)
{
    var size = 0;

    foreach (var stringToAdd in InfiniteArrayOfRandomStringWithNumbers()
                 .Chunk(stringLinesWritingChunk).Select(strings => string.Join(null, strings)))
    {
        var utfStringToAdd = System.Text.Encoding.UTF8.GetBytes(stringToAdd);

        size += utfStringToAdd.Length;
        target.Post(utfStringToAdd);
        
        if (size > expectedFileSizeInBytes)
        {
            break;
        }
    }

    target.Complete();
}

static async Task ConsumeAsync(ISourceBlock<byte[]> source)
{
    await using var file = File.Create(expectedFile);
    while (await source.OutputAvailableAsync())
    {
        byte[] data = await source.ReceiveAsync();
        var writer = file.WriteAsync(data);
        Console.WriteLine($"{expectedFileSizeInBytes}/{file.Length}");
        await writer;
    }
}

static IEnumerable<string> InfiniteArrayOfRandomStringWithNumbers()
{
    var stringsVocabulary = new string[randomStringsVocabularySize];
    var random = new Random();
    do
    {
        yield return $"{RandomLong(random)}. {RandomStringFromVocabulary(random, stringsVocabulary)}\r\n";
    } while (true);
}

static string RandomStringFromVocabulary(Random random, string[] vocabulary)
{
    var randomStringIndex = random.Next(vocabulary.Length);
    vocabulary[randomStringIndex] ??= RandomString(random);
    return vocabulary[randomStringIndex];
}

static string RandomString(Random random)
{
    var length = random.Next(1, maximumSizeOfString);
    var sb = new char[length];
    for (int i = 0; i < sb.Length; i++)
    {
        var pos = random.Next(allowedChars.Length);
        sb[i] = allowedChars[pos];
    }
    return new string(sb);
}

static long RandomLong(Random random)
{
    long result = random.Next(int.MinValue, int.MaxValue);
    result = result << 32;
    result = result | random.Next(int.MinValue, int.MaxValue);
    return result;
}