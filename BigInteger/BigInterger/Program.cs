﻿using System.Diagnostics;
using System.Numerics;

namespace BigInterger
{
	internal class Program
	{
		static void Main(string[] args)
		{
			byte[] bytes = new byte[] { 0b11111111, 0b11110000, 0b00000000 };
			foreach (var item in bytes)
			{
				string binaryString = Convert.ToString(item, 2).PadLeft(8, '0'); // 2진수 문자열로 변환 (8비트 패딩)
				Console.WriteLine(binaryString);
			}
			Console.WriteLine("==============================");
			BigIntegerUnit unit2 = new BigIntegerUnit(123456789);
			BigIntegerUnit unit3 = new BigIntegerUnit(10);
			BigInteger cshparBigInteger1 = new BigInteger(123456789);
			BigInteger cshparBigInteger2 = new BigInteger(10);
			Console.WriteLine(unit2.ToString());
			Console.WriteLine(unit3.ToString());
			Console.WriteLine(cshparBigInteger1.ToString("X"));
			Console.WriteLine(cshparBigInteger2.ToString("X"));
			Console.WriteLine("============곱셈==================");
			Console.WriteLine((unit2*unit3).ToString());
			Console.WriteLine((cshparBigInteger1*cshparBigInteger2).ToString("X"));
			Console.WriteLine("============string==================");
			BigIntegerUnit unit5 = new BigIntegerUnit("-1234567981234567913245679");
			Console.WriteLine(unit5.ToString());
			BigInteger cshparBigInteger5 = BigInteger.Parse("-1234567981234567913245679");
			Console.WriteLine(cshparBigInteger5.ToString("X"));
			Console.WriteLine("==============대소비교================");
			Console.WriteLine((unit2>unit3));
			Console.WriteLine((unit2<unit3));
			Console.WriteLine((unit2>=unit3));
			Console.WriteLine((unit2<=unit3));
			Console.WriteLine((unit2==unit3));
			Console.WriteLine((unit2!=unit3));
			Console.WriteLine("==============================");
		}
	}
}
