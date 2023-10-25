using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

public class Queue
{
	// ����ü ���� : �������� �����°� ũ�⸦ ����
	struct Info
	{
		public int offset;
		public int size;
	};

	// �޸� ���ۿ� Info ����Ʈ, ������ ����, �� ��ü ����
	private MemoryStream buffer;
	private List<Info> list;
	private int offset = 0;

	private Object lockObj = new Object();

	// ������
	public Queue()
	{
		buffer = new MemoryStream();
		list = new List<Info>();
	}

	// �����͸� ť�� �߰�
	public int Add(byte[] data, int size)
	{
		// Info ����ü ���� �� �� ����
		Info info = new Info();

		info.offset = offset;
		info.size = size;

		// ť�� Info �߰�, ���ۿ� ������ ����, ������ ����
		lock (lockObj)
		{
			list.Add(info);

			buffer.Position = offset;
			buffer.Write(data, 0, size);
			buffer.Flush();
			offset += size;
		}

		return size; // �߰��� �������� ũ�� ��ȯ
	}

	// �����͸� ť���� �����ϴ� �޼���
	public int Pop(ref byte[] data, int size)
	{
		// ť�� ��������� -1 ��ȯ
		if (list.Count <= 0)
		{
			return -1;
		}

		int iSize = 0;

		// ť�� ���� �ɰ� ������ ���� �۾� ����
		lock (lockObj)
		{
			Info info = list[0];

			// ������ ������ ������ ũ�� ���
			int dataSize = Math.Min(size, info.size);
			// ���ۿ��� ������ �б�
			buffer.Position = info.offset;
			iSize = buffer.Read(data, 0, dataSize);

			// ���� �����Ͱ� ������ ť���� �ش� Info ����
			if (iSize > 0)
			{
				list.RemoveAt(0);
			}

			// ť�� ��������� ���� �ʱ�ȭ �� ������ �ʱ�ȭ
			if (list.Count == 0)
			{
				Clear();
				offset = 0;
			}
		}

		return iSize; // ����� �������� ũ�� ��ȯ
	}

	// ť�� ���۸� �ʱ�ȭ
	public void Clear()
	{
		byte[] data = buffer.GetBuffer();
		Array.Clear(data, 0, data.Length);

		buffer.Position = 0;
		buffer.SetLength(0);
	}
}