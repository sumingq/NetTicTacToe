using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

public class Queue
{
	struct Info
	{
		public int offset;
		public int size;
	};

	private MemoryStream buffer;
	private List<Info> list;
	private int offset = 0;

	private Object lockObj = new Object();

	public Queue()
	{
		buffer = new MemoryStream();
		list = new List<Info>();
	}

	public int Add(byte[] data, int size)
	{
		Info info = new Info();

		info.offset = offset;
		info.size = size;

		lock (lockObj)
		{
			list.Add(info);

			buffer.Position = offset;
			buffer.Write(data, 0, size);
			buffer.Flush();
			offset += size;
		}

		return size;
	}

	public int Pop(ref byte[] data, int size)
	{
		if (list.Count <= 0)
		{
			return -1;
		}

		int iSize = 0;
		lock (lockObj)
		{
			Info info = list[0];

			int dataSize = Math.Min(size, info.size);
			buffer.Position = info.offset;
			iSize = buffer.Read(data, 0, dataSize);

			if (iSize > 0)
			{
				list.RemoveAt(0);
			}

			if (list.Count == 0)
			{
				Clear();
				offset = 0;
			}
		}

		return iSize;
	}

	public void Clear()
	{
		byte[] data = buffer.GetBuffer();
		Array.Clear(data, 0, data.Length);

		buffer.Position = 0;
		buffer.SetLength(0);
	}
}