using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

public class Queue
{
	// 구조체 정의 : 데이터의 오프셋과 크기를 저장
	struct Info
	{
		public int offset;
		public int size;
	};

	// 메모리 버퍼와 Info 리스트, 오프셋 변수, 락 객체 정의
	private MemoryStream buffer;
	private List<Info> list;
	private int offset = 0;

	private Object lockObj = new Object();

	// 생성자
	public Queue()
	{
		buffer = new MemoryStream();
		list = new List<Info>();
	}

	// 데이터를 큐에 추가
	public int Add(byte[] data, int size)
	{
		// Info 구조체 생성 및 값 설정
		Info info = new Info();

		info.offset = offset;
		info.size = size;

		// 큐에 Info 추가, 버퍼에 데이터 쓰기, 오프셋 조정
		lock (lockObj)
		{
			list.Add(info);

			buffer.Position = offset;
			buffer.Write(data, 0, size);
			buffer.Flush();
			offset += size;
		}

		return size; // 추가된 데이터의 크기 반환
	}

	// 데이터를 큐에서 추출하는 메서드
	public int Pop(ref byte[] data, int size)
	{
		// 큐가 비어있으면 -1 반환
		if (list.Count <= 0)
		{
			return -1;
		}

		int iSize = 0;

		// 큐에 락을 걸고 데이터 추출 작업 수행
		lock (lockObj)
		{
			Info info = list[0];

			// 실제로 복사할 데이터 크기 계산
			int dataSize = Math.Min(size, info.size);
			// 버퍼에서 데이터 읽기
			buffer.Position = info.offset;
			iSize = buffer.Read(data, 0, dataSize);

			// 읽은 데이터가 있으면 큐에서 해당 Info 제거
			if (iSize > 0)
			{
				list.RemoveAt(0);
			}

			// 큐가 비어있으면 버퍼 초기화 및 오프셋 초기화
			if (list.Count == 0)
			{
				Clear();
				offset = 0;
			}
		}

		return iSize; // 추출된 데이터의 크기 반환
	}

	// 큐와 버퍼를 초기화
	public void Clear()
	{
		byte[] data = buffer.GetBuffer();
		Array.Clear(data, 0, data.Length);

		buffer.Position = 0;
		buffer.SetLength(0);
	}
}