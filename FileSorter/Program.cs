using FileSorter.MapTextValues;
using FileSorter.ReducerTextValue;

// configuration
const int bytesInMegaByte = 1024 * 1024;
const ushort maximumSizeOfString = 1024;
const ushort maximumSizeOfLine = maximumSizeOfString + 20;
const int readingChunkBufferSize = bytesInMegaByte * 100;
const string workingDirectory = "../../../../";

// script parameters
const string expectedFile = "generatedFile.txt";

// body prepare folders and file streams
Directory.SetCurrentDirectory(workingDirectory);
using var sourceFileStreamReader = File.OpenText(expectedFile);
await using var resultFileStreamWriter = File.CreateText("result_" + expectedFile);

var directory = Directory.CreateDirectory(expectedFile + "_temp");
Directory.SetCurrentDirectory(directory.FullName);

// body act
var watch = System.Diagnostics.Stopwatch.StartNew();
var chunksCount = await MapTextValueFiles.ProduceSetOfSortedChunks(sourceFileStreamReader, readingChunkBufferSize, maximumSizeOfLine);
Console.WriteLine("Chunks prepared in " + watch.Elapsed.TotalSeconds);
watch.Reset();
watch.Start();
await ReduceTextValueFiles.MergeSortedChunks(resultFileStreamWriter, chunksCount);
Console.WriteLine("Chunks merged in " + watch.Elapsed.TotalSeconds);

