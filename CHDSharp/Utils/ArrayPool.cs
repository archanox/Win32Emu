using System.Collections.Generic;

namespace CHDSharpLib.Utils;

public class ArrayPool
{
    private uint _arraySize;
    private List<byte[]> _array;
    private int _count;
    private int _issuedArraysTotal;

    public ArrayPool(uint arraySize)
    {
        _array = new List<byte[]>();
        _arraySize = arraySize;
        _count = 0;
        _issuedArraysTotal = 0;
    }

    public byte[] Rent()
    {
        byte[] ret;
        lock (_array)
        {
            if (_count == 0)
            {
                ret = new byte[_arraySize];
                _issuedArraysTotal++;
            }
            else
            {
                _count--;
                ret = _array[_count];
                _array.RemoveAt(_count);
            }
        }
        return ret;

    }

    public void Return(byte[] ret)
    {
        lock (_array)
        {
            _array.Add(ret);
            _count++;
        }
    }

    public void ReadStats(out int issuedArraysTotal, out int returnedArraysTotal)
    {
        issuedArraysTotal = _issuedArraysTotal;
        returnedArraysTotal = _count;
    }
}
