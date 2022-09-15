namespace FileSorter;

public class CharArrayComparer : IEqualityComparer<char[]>, IComparer<char[]>
{
    public bool Equals(char[] x, char[] y)
    {
        if (x.Length != y.Length)
        {
            return false;
        }
        for (int i = 0; i < x.Length; i++)
        {
            if (x[i] != y[i])
            {
                return false;
            }
        }
        return true;
    }
    
    public int Compare(char[] x, char[] y)
    {
        var lengthCompare = x.Length.CompareTo(y.Length);

        if (lengthCompare != 0)
        {
            return lengthCompare;
        }

        for (int i = 0; i < x.Length; i++)
        {
            var valueCompare = x[i].CompareTo(y[i]);
            if (valueCompare != 0)
            {
                return valueCompare;
            }
        }
        return 0;
    }

    public int GetHashCode(char[] obj)
    {
        if (obj.Length == 0)
        {
            return 0;
        }

        var result = obj.Length;
        result = result << 8;
        result += obj[0];
        result = result << 8;
        if (obj.Length > 1)
        {
            result += obj[1];
        }
        return result;
    }
}