namespace FileSorter.MapTextValues
{
    public static class SourceFileReader
    {
        public static async Task<int> ReadChunkSizePlusCharsTillNewLineSign(StreamReader streamReader, char[] buffer, int chunkSize)
        {
            var size = await streamReader.ReadBlockAsync(buffer, 0, chunkSize);
            if (size != chunkSize)
            {
                return size;
            }
            var line = await streamReader.ReadLineAsync();
            if (line != null)
            {
                foreach (var charFromLine in line)
                {
                    size++;
                    buffer[size] = charFromLine;
                }
            }

            return size;
        }
    }
}
