using System.Text;

namespace FileSorter.MapTextValues
{
    public static class MapTextValueFiles
    {
        public static async Task<int> ProduceSetOfSortedChunks(StreamReader sourceFileStreamReader, int readingChunkBufferSize, int maximumSizeOfLine)
        {
            var tasksToWait = new List<Task>();

            var chunkNum = 0;
            var readSize = 0;

            do
            {
                var buffer = new char[readingChunkBufferSize + maximumSizeOfLine];
                readSize = await SourceFileReader.ReadChunkSizePlusCharsTillNewLineSign(sourceFileStreamReader, buffer, readingChunkBufferSize);

                var size = readSize;
                var num = chunkNum;
                tasksToWait.Add(Task
                    .Run(() => ParseChunk(buffer, size))
                    .ContinueWith(task => WriteChunk(num, task.Result)));

                chunkNum++;
            } while (readSize >= readingChunkBufferSize);

            await Task.WhenAll(tasksToWait);
            return chunkNum;
        }

        private static Dictionary<char[], Stack<long>> ParseChunk(char[] buffer, int readSize)
        {
            var lines = TextValueBufferOperations.Split(buffer, readSize);

            var dict = new Dictionary<char[], Stack<long>>(new CharArrayComparer());
            foreach (var line in lines)
            {
                var textPart = buffer.AsSpan(line.Item2).ToArray();
                dict.TryGetValue(textPart, out var list);
                if (list == null)
                {
                    list = new Stack<long>();
                    dict.Add(textPart, list);
                }

                list.Push(line.Item1);
            }

            return dict;
        }

        private static async Task WriteChunk(int chunkNum, Dictionary<char[], Stack<long>> dict)
        {
            var fi = new FileInfo(chunkNum + ".bin");
            await using var binaryFile = fi.Create();
            await using var writer = new BinaryWriter(binaryFile, Encoding.UTF8, false);

            foreach (var d in dict.OrderBy(pair => pair.Key, new CharArrayComparer()))
            {
                writer.Write(d.Key.Length);
                writer.Write(d.Key);
                writer.Write(d.Value.Count);
                var longValues = d.Value;
                foreach (var longValue in longValues)
                {
                    writer.Write(longValue);
                }
            }

            Console.WriteLine($"Chunk num {chunkNum} prepared");
        }
    }
}
