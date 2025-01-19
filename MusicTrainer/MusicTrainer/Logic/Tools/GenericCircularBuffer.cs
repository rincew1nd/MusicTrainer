// https://github.com/SjB/NAudio/blob/master/NAudio/Utils/CircularBuffer.cs
// Kudos to NAudio (https://github.com/SjB/NAudio)

using System;
using System.Diagnostics;

namespace MusicTrainer.Logic.Tools;

/// <summary>
/// Custom circular buffer implementation
/// </summary>
public class GenericCircularBuffer<T>
{
    private readonly object lockObject;
    
    private readonly T[] buffer;
    private int writePosition;
    private int readPosition;
    private int objCount;

    /// <summary>
    /// Create a new circular buffer
    /// </summary>
    /// <param name="size">Max buffer size</param>
    public GenericCircularBuffer(int size)
    {
        buffer = new T[size];
        lockObject = new object();
    }

    /// <summary>
    /// Write data to the buffer
    /// </summary>
    /// <param name="data">Data to write</param>
    /// <param name="offset">Offset into data</param>
    /// <param name="count">Number of objs to write</param>
    /// <returns>number of objs written</returns>
    public int Write(T[] data, int offset, int count)
    {
        lock (lockObject)
        {
            int objsWritten = 0;
            if (count > buffer.Length - objCount)
            {
                count = buffer.Length - objCount;
                //throw new ArgumentException("Not enough space in buffer");
            }
            
            // write to end
            int writeToEnd = Math.Min(buffer.Length - writePosition, count);
            Array.Copy(data, offset, buffer, writePosition, writeToEnd);
            writePosition += writeToEnd;
            writePosition %= buffer.Length;
            objsWritten += writeToEnd;
            if (objsWritten < count)
            {
                Debug.Assert(writePosition == 0);
                // must have wrapped round. Write to start
                Array.Copy(data, offset + objsWritten, buffer, writePosition, count - objsWritten);
                writePosition += (count - objsWritten);
                objsWritten = count;
            }
            objCount += objsWritten;
            return objsWritten;
        }
    }

    /// <summary>
    /// Read from the buffer
    /// </summary>
    /// <param name="data">Buffer to read into</param>
    /// <param name="offset">Offset into read buffer</param>
    /// <param name="count">objs to read</param>
    /// <returns>Number of objs actually read</returns>
    public int Read(T[] data, int offset, int count, int? advanceFor = null)
    {
        lock (lockObject)
        {
            advanceFor ??= count;
            
            if (count > objCount)
            {
                count = objCount;
            }
            int objsRead = 0;
            int readToEnd = Math.Min(buffer.Length - readPosition, count);
            Array.Copy(buffer, readPosition, data, offset, readToEnd);
            objsRead += readToEnd;
            readPosition += advanceFor.Value;
            readPosition %= buffer.Length;

            if (objsRead < count)
            {
                // must have wrapped round. Read from start
                Debug.Assert(readPosition == 0);
                Array.Copy(buffer, readPosition, data, offset + objsRead, count - objsRead);
                readPosition += (count - objsRead);
                objsRead = count;
            }

            objCount -= objsRead;
            Debug.Assert(objCount >= 0);
            return objsRead;
        }
    }

    /// <summary>
    /// Maximum length of circular buffer
    /// </summary>
    public int MaxLength
    {
        get { return buffer.Length; }
    }

    /// <summary>
    /// Number of objs currently stored in the circular buffer
    /// </summary>
    public int Count
    {
        get { return objCount; }
    }

    /// <summary>
    /// Resets the buffer
    /// </summary>
    public void Reset()
    {
        objCount = 0;
        readPosition = 0;
        writePosition = 0;
    }

    /// <summary>
    /// Advances the buffer, discarding objs
    /// </summary>
    /// <param name="count">objs to advance</param>
    public void Advance(int count)
    {
        if (count >= objCount)
        {
            Reset();
        }
        else
        {
            objCount -= count;
            readPosition += count;
            readPosition %= MaxLength;
        }

    }
}